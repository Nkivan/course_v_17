using Domain.Core.Enums;
using Domain.Core.Exceptions;
using Domain.Core.Interfaces;
using Domain.Core.Models;

namespace Domain.Core.Services
{
   
    public class ProjectManagementService : IProjectManagementService
    {
        private readonly IRepository<Project> _projects;
        private readonly IRepository<WorkTask> _tasks;
        private readonly IRepository<Employee> _employees;
        private readonly IRepository<TimeLog> _timeLogs;

        public IRepository<Project> Projects => _projects;
        public IRepository<WorkTask> Tasks => _tasks;
        public IRepository<Employee> Employees => _employees;
        public IRepository<TimeLog> TimeLogs => _timeLogs;

        public ProjectManagementService(
            IRepository<Project> projects,
            IRepository<WorkTask> tasks,
            IRepository<Employee> employees,
            IRepository<TimeLog> timeLogs)
        {
            _projects = projects ?? throw new ArgumentNullException(nameof(projects));
            _tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
            _employees = employees ?? throw new ArgumentNullException(nameof(employees));
            _timeLogs = timeLogs ?? throw new ArgumentNullException(nameof(timeLogs));
        }

        public Project CreateProject(string name, string description, DateTime startDate, DateTime deadline, ProjectStatus status = ProjectStatus.Planned)
        {
            var project = new Project(name, description, startDate, deadline, status);
            _projects.Add(project);
            return project;
        }

        public Employee CreateEmployee(
            string fullName,
            string role,
            double maxWeeklyHours = Employee.DefaultMaxWeeklyHours,
            int maxConcurrentTasks = Employee.DefaultMaxConcurrentTasks)
        {
            var employee = new Employee(fullName, role, maxWeeklyHours, maxConcurrentTasks);
            _employees.Add(employee);
            return employee;
        }

        public WorkTask CreateTask(
            Guid projectId,
            string title,
            string description,
            double estimatedHours,
            TaskPriority priority,
            DateTime deadline,
            Guid? assigneeId = null)
        {
            Project project = _projects.GetById(projectId)
                ?? throw new EntityNotFoundException(nameof(Project), projectId);

            if (deadline.Date > project.Deadline.Date)
            {
                throw new DeadlineExceededException(
                    $"Дедлайн завдання '{title}' перевищує дедлайн проєкту '{project.Name}'",
                    project.Deadline,
                    deadline);
            }

            if (assigneeId.HasValue)
            {
                Employee employee = _employees.GetById(assigneeId.Value)
                    ?? throw new EntityNotFoundException(nameof(Employee), assigneeId.Value);

                ValidateEmployeeWorkload(employee, estimatedHours, taskBeingReassignedId: null);
            }

            var task = new WorkTask(projectId, title, description, estimatedHours, priority, deadline, assigneeId, WorkTaskStatus.New);
            _tasks.Add(task);
            return task;
        }

        public void AssignTaskToEmployee(Guid taskId, Guid employeeId)
        {
            WorkTask task = _tasks.GetById(taskId)
                ?? throw new EntityNotFoundException(nameof(WorkTask), taskId);

            Employee employee = _employees.GetById(employeeId)
                ?? throw new EntityNotFoundException(nameof(Employee), employeeId);

            if (!task.IsActive)
            {
                throw new DomainValidationException($"Неможливо призначити виконавця на вже завершене завдання '{task.Title}'.");
            }

            ValidateEmployeeWorkload(employee, task.EstimatedHours, taskBeingReassignedId: task.Id);

            task.AssigneeId = employee.Id;
            if (task.Status == WorkTaskStatus.New)
            {
                task.Status = WorkTaskStatus.InProgress;
            }
        }

        public void ChangeTaskStatus(Guid taskId, WorkTaskStatus newStatus)
        {
            WorkTask task = _tasks.GetById(taskId)
                ?? throw new EntityNotFoundException(nameof(WorkTask), taskId);

            task.Status = newStatus;
        }

        public TimeLog LogTime(Guid taskId, Guid employeeId, DateTime logDate, double hours, string note = "")
        {
            WorkTask task = _tasks.GetById(taskId)
                ?? throw new EntityNotFoundException(nameof(WorkTask), taskId);

            Employee employee = _employees.GetById(employeeId)
                ?? throw new EntityNotFoundException(nameof(Employee), employeeId);

            if (!task.AssigneeId.HasValue)
            {
                throw new DomainValidationException($"Завдання '{task.Title}' ще не призначене жодному виконавцю.");
            }

            if (task.AssigneeId.Value != employee.Id)
            {
                throw new DomainValidationException(
                    $"Виконавець '{employee.FullName}' не призначений на завдання '{task.Title}'.");
            }

            if (task.Status == WorkTaskStatus.Completed)
            {
                throw new DomainValidationException($"Неможливо записати час у вже завершене завдання '{task.Title}'.");
            }

            if (logDate.Date > task.Deadline.Date)
            {
                throw new DeadlineExceededException(
                    $"Неможливо записати час у завдання '{task.Title}' після його дедлайну",
                    task.Deadline,
                    logDate);
            }

            var log = new TimeLog(task.Id, employee.Id, logDate, hours, note);
            _timeLogs.Add(log);

            if (task.Status == WorkTaskStatus.New)
            {
                task.Status = WorkTaskStatus.InProgress;
            }

            return log;
        }

        public double GetTotalLoggedHoursForTask(Guid taskId)
        {
            _ = _tasks.GetById(taskId) ?? throw new EntityNotFoundException(nameof(WorkTask), taskId);
            return _timeLogs.Find(log => log.TaskId == taskId).Sum(log => log.Hours);
        }

        public double GetTotalLoggedHoursForEmployee(Guid employeeId)
        {
            _ = _employees.GetById(employeeId) ?? throw new EntityNotFoundException(nameof(Employee), employeeId);
            return _timeLogs.Find(log => log.EmployeeId == employeeId).Sum(log => log.Hours);
        }

        public double GetTotalLoggedHoursForProject(Guid projectId)
        {
            _ = _projects.GetById(projectId) ?? throw new EntityNotFoundException(nameof(Project), projectId);
            HashSet<Guid> projectTaskIds = _tasks.Find(t => t.ProjectId == projectId).Select(t => t.Id).ToHashSet();
            return _timeLogs.Find(log => projectTaskIds.Contains(log.TaskId)).Sum(log => log.Hours);
        }

        public double GetActivePlannedHoursForEmployee(Guid employeeId)
        {
            return _tasks
                .Find(t => t.AssigneeId == employeeId && t.IsActive)
                .Sum(t => t.EstimatedHours);
        }

        public int GetActiveTasksCountForEmployee(Guid employeeId)
        {
            return _tasks.Find(t => t.AssigneeId == employeeId && t.IsActive).Count;
        }

        public List<WorkTask> GetOverdueTasks(DateTime? referenceDate = null)
        {
            DateTime checkDate = (referenceDate ?? DateTime.Today).Date;
            return _tasks.Find(t => t.IsOverdue(checkDate));
        }

        public List<Project> GetOverdueProjects(DateTime? referenceDate = null)
        {
            DateTime checkDate = (referenceDate ?? DateTime.Today).Date;
            return _projects.Find(p => p.IsOverdue(checkDate));
        }

        public List<Project> GetProjectsSorted(IComparer<Project>? comparer = null) => _projects.GetSorted(comparer);
        public List<WorkTask> GetTasksSorted(IComparer<WorkTask>? comparer = null) => _tasks.GetSorted(comparer);
        public List<Employee> GetEmployeesSorted(IComparer<Employee>? comparer = null) => _employees.GetSorted(comparer);
        public List<TimeLog> GetTimeLogsSorted(IComparer<TimeLog>? comparer = null) => _timeLogs.GetSorted(comparer);

        public bool DeleteProject(Guid projectId)
        {
            List<WorkTask> relatedTasks = _tasks.Find(t => t.ProjectId == projectId);
            foreach (WorkTask task in relatedTasks)
            {
                DeleteTask(task.Id);
            }
            return _projects.Remove(projectId);
        }

        public bool DeleteTask(Guid taskId)
        {
            List<TimeLog> relatedLogs = _timeLogs.Find(l => l.TaskId == taskId);
            foreach (TimeLog log in relatedLogs)
            {
                _timeLogs.Remove(log.Id);
            }
            return _tasks.Remove(taskId);
        }

        public bool DeleteEmployee(Guid employeeId)
        {
            List<WorkTask> assignedTasks = _tasks.Find(t => t.AssigneeId == employeeId);
            foreach (WorkTask task in assignedTasks)
            {
                task.AssigneeId = null;
            }
            return _employees.Remove(employeeId);
        }

        private void ValidateEmployeeWorkload(Employee employee, double additionalHours, Guid? taskBeingReassignedId)
        {
            List<WorkTask> activeTasks = _tasks.Find(t => t.AssigneeId == employee.Id && t.IsActive);

            bool alreadyAssigned = taskBeingReassignedId.HasValue &&
                                   activeTasks.Any(t => t.Id == taskBeingReassignedId.Value);

            double currentHours = activeTasks.Sum(t => t.EstimatedHours);
            int currentCount = activeTasks.Count;

            double projectedHours = alreadyAssigned ? currentHours : currentHours + additionalHours;
            int projectedCount = alreadyAssigned ? currentCount : currentCount + 1;

            if (projectedHours > employee.MaxWeeklyHours)
            {
                throw new EmployeeOverloadedException(
                    employee.FullName,
                    projectedHours,
                    employee.MaxWeeklyHours,
                    projectedCount,
                    employee.MaxConcurrentTasks,
                    $"сума планових годин активних завдань перевищує ліміт ({employee.MaxWeeklyHours:F0} год)");
            }

            if (projectedCount > employee.MaxConcurrentTasks)
            {
                throw new EmployeeOverloadedException(
                    employee.FullName,
                    projectedHours,
                    employee.MaxWeeklyHours,
                    projectedCount,
                    employee.MaxConcurrentTasks,
                    $"кількість одночасних активних завдань перевищує ліміт ({employee.MaxConcurrentTasks})");
            }
        }
    }
}

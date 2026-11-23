using Domain.Core.Enums;
using Domain.Core.Models;

namespace Domain.Core.Interfaces
{
    
    public interface IProjectManagementService
    {
        Project CreateProject(string name, string description, DateTime startDate, DateTime deadline, ProjectStatus status = ProjectStatus.Planned);
        Employee CreateEmployee(string fullName, string role, double maxWeeklyHours = Employee.DefaultMaxWeeklyHours, int maxConcurrentTasks = Employee.DefaultMaxConcurrentTasks);
        WorkTask CreateTask(Guid projectId, string title, string description, double estimatedHours, TaskPriority priority, DateTime deadline, Guid? assigneeId = null);

        void AssignTaskToEmployee(Guid taskId, Guid employeeId);
        void ChangeTaskStatus(Guid taskId, WorkTaskStatus newStatus);
        TimeLog LogTime(Guid taskId, Guid employeeId, DateTime logDate, double hours, string note = "");

        double GetTotalLoggedHoursForTask(Guid taskId);
        double GetTotalLoggedHoursForEmployee(Guid employeeId);
        double GetTotalLoggedHoursForProject(Guid projectId);
        double GetActivePlannedHoursForEmployee(Guid employeeId);
        int GetActiveTasksCountForEmployee(Guid employeeId);

        List<WorkTask> GetOverdueTasks(DateTime? referenceDate = null);
        List<Project> GetOverdueProjects(DateTime? referenceDate = null);

        List<Project> GetProjectsSorted(IComparer<Project>? comparer = null);
        List<WorkTask> GetTasksSorted(IComparer<WorkTask>? comparer = null);
        List<Employee> GetEmployeesSorted(IComparer<Employee>? comparer = null);
        List<TimeLog> GetTimeLogsSorted(IComparer<TimeLog>? comparer = null);

        bool DeleteProject(Guid projectId);
        bool DeleteTask(Guid taskId);
        bool DeleteEmployee(Guid employeeId);
    }
}

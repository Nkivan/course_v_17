using System.Text;
using Domain.Core.Comparers;
using Domain.Core.Enums;
using Domain.Core.Exceptions;
using Domain.Core.Models;
using Domain.Core.Services;

namespace Domain.UI
{
    internal class Program
    {
        private const string DataFolderName = "Data";

        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            string dataDir = Path.Combine(AppContext.BaseDirectory, DataFolderName);
            var projectStore = new JsonFileService<Project>(Path.Combine(dataDir, "projects.json"));
            var taskStore = new JsonFileService<WorkTask>(Path.Combine(dataDir, "tasks.json"));
            var employeeStore = new JsonFileService<Employee>(Path.Combine(dataDir, "employees.json"));
            var timeLogStore = new JsonFileService<TimeLog>(Path.Combine(dataDir, "timelogs.json"));

            var service = new ProjectManagementService(
                new Domain.Core.Repositories.Repository<Project>(),
                new Domain.Core.Repositories.Repository<WorkTask>(),
                new Domain.Core.Repositories.Repository<Employee>(),
                new Domain.Core.Repositories.Repository<TimeLog>());

            LoadStateOrSeed(service, projectStore, taskStore, employeeStore, timeLogStore);

            bool running = true;
            while (running)
            {
                PrintMainMenu();
                int choice = ConsoleInput.ReadInt("Оберіть пункт меню: ", 0, 14);
                Console.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case 1: CreateProjectMenu(service); break;
                        case 2: CreateEmployeeMenu(service); break;
                        case 3: CreateTaskMenu(service); break;
                        case 4: AssignTaskMenu(service); break;
                        case 5: LogTimeMenu(service); break;
                        case 6: ChangeTaskStatusMenu(service); break;
                        case 7: ListEntitiesMenu(service); break;
                        case 8: ListTasksSortedMenu(service); break;
                        case 9: ShowHoursReport(service); break;
                        case 10: ShowOverdueReport(service); break;
                        case 11: DeleteEntityMenu(service); break;
                        case 12: RunExceptionsDemo(service); break;
                        case 13: SaveAll(service, projectStore, taskStore, employeeStore, timeLogStore); break;
                        case 14: ResetToSeedData(service, projectStore, taskStore, employeeStore, timeLogStore); break;
                        case 0:
                            SaveAll(service, projectStore, taskStore, employeeStore, timeLogStore);
                            running = false;
                            break;
                    }
                }
                catch (DeadlineExceededException ex)
                {
                    PrintError($"[DeadlineExceededException] {ex.Message}");
                }
                catch (EmployeeOverloadedException ex)
                {
                    PrintError($"[EmployeeOverloadedException] {ex.Message}");
                }
                catch (EntityNotFoundException ex)
                {
                    PrintError($"[EntityNotFoundException] {ex.Message}");
                }
                catch (DomainValidationException ex)
                {
                    PrintError($"[DomainValidationException] {ex.Message}");
                }
                catch (ArgumentException ex)
                {
                    PrintError($"[Помилка валідації даних] {ex.Message}");
                }
                catch (Exception ex)
                {
                    PrintError($"[Неочікувана помилка] {ex.Message}");
                }

                if (running)
                {
                    Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
                    Console.ReadKey(intercept: true);
                    Console.Clear();
                }
            }
        }

        private static void PrintMainMenu()
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("  КУРСОВА РОБОТА (Варіант № 17): УПРАВЛІННЯ IT-ПРОЄКТАМИ");
            Console.WriteLine("=================================================================");
            Console.WriteLine("  1. Створити проєкт");
            Console.WriteLine("  2. Додати виконавця");
            Console.WriteLine("  3. Створити завдання (з контролем дедлайну проєкту та лімітів)");
            Console.WriteLine("  4. Призначити / перепризначити виконавця на завдання");
            Console.WriteLine("  5. Записати витрачений час (TimeLog, 0..24 год, контроль дедлайну)");
            Console.WriteLine("  6. Змінити статус завдання");
            Console.WriteLine("  7. Переглянути всі сутності (Проєкти, Виконавці, Завдання, Логи)");
            Console.WriteLine("  8. Переглянути завдання (сортування за дедлайном або пріоритетом)");
            Console.WriteLine("  9. Звіт: залучений час і завантаженість виконавців");
            Console.WriteLine(" 10. Звіт: прострочені завдання та проєкти");
            Console.WriteLine(" 11. Видалити сутність");
            Console.WriteLine(" 12. Демонстрація роботи власних винятків (сценарії)");
            Console.WriteLine(" 13. Зберегти дані в JSON зараз");
            Console.WriteLine(" 14. Скинути дані до початкових тестових записів");
            Console.WriteLine("  0. Зберегти та вийти");
            Console.WriteLine("=================================================================");
        }

        private static void LoadStateOrSeed(
            ProjectManagementService service,
            JsonFileService<Project> projectStore,
            JsonFileService<WorkTask> taskStore,
            JsonFileService<Employee> employeeStore,
            JsonFileService<TimeLog> timeLogStore)
        {
            bool anyFileExists = File.Exists(projectStore.FilePath)
                                 || File.Exists(taskStore.FilePath)
                                 || File.Exists(employeeStore.FilePath)
                                 || File.Exists(timeLogStore.FilePath);

            if (!anyFileExists)
            {
                SeedData.Populate(service);
                SaveAll(service, projectStore, taskStore, employeeStore, timeLogStore);
                Console.WriteLine("JSON-файли не знайдено — створено початкові тестові дані.");
                return;
            }

            service.Projects.ReplaceAll(projectStore.Load());
            service.Employees.ReplaceAll(employeeStore.Load());
            service.Tasks.ReplaceAll(taskStore.Load());
            service.TimeLogs.ReplaceAll(timeLogStore.Load());
            Console.WriteLine($"Завантажено: проєктів — {service.Projects.GetAll().Count}, " +
                              $"виконавців — {service.Employees.GetAll().Count}, " +
                              $"завдань — {service.Tasks.GetAll().Count}, " +
                              $"записів часу — {service.TimeLogs.GetAll().Count}.");
        }

        private static void SaveAll(
            ProjectManagementService service,
            JsonFileService<Project> projectStore,
            JsonFileService<WorkTask> taskStore,
            JsonFileService<Employee> employeeStore,
            JsonFileService<TimeLog> timeLogStore)
        {
            projectStore.Save(service.Projects.GetAll());
            employeeStore.Save(service.Employees.GetAll());
            taskStore.Save(service.Tasks.GetAll());
            timeLogStore.Save(service.TimeLogs.GetAll());
            Console.WriteLine($"Дані успішно збережено у JSON ({Path.GetDirectoryName(projectStore.FilePath)}).");
        }

        private static void ResetToSeedData(
            ProjectManagementService service,
            JsonFileService<Project> projectStore,
            JsonFileService<WorkTask> taskStore,
            JsonFileService<Employee> employeeStore,
            JsonFileService<TimeLog> timeLogStore)
        {
            service.Projects.ReplaceAll(Array.Empty<Project>());
            service.Employees.ReplaceAll(Array.Empty<Employee>());
            service.Tasks.ReplaceAll(Array.Empty<WorkTask>());
            service.TimeLogs.ReplaceAll(Array.Empty<TimeLog>());

            SeedData.Populate(service);
            SaveAll(service, projectStore, taskStore, employeeStore, timeLogStore);
            Console.WriteLine("Стан скинуто до початкових тестових даних.");
        }

        private static void CreateProjectMenu(ProjectManagementService service)
        {
            string name = ConsoleInput.ReadNonEmptyString("Назва проєкту: ");
            string description = ConsoleInput.ReadOptionalString("Опис проєкту: ");
            DateTime startDate = ConsoleInput.ReadDate("Дата старту (РРРР-ММ-ДД): ");
            DateTime deadline = ConsoleInput.ReadDate("Дедлайн проєкту (РРРР-ММ-ДД): ");
            ProjectStatus status = ConsoleInput.ReadEnum<ProjectStatus>("Оберіть статус проєкту:");

            Project project = service.CreateProject(name, description, startDate, deadline, status);
            Console.WriteLine($"Створено проєкт: {project.GetSummary()}");
        }

        private static void CreateEmployeeMenu(ProjectManagementService service)
        {
            string fullName = ConsoleInput.ReadNonEmptyString("ПІБ виконавця: ");
            string role = ConsoleInput.ReadNonEmptyString("Посада / роль: ");
            double maxHours = ConsoleInput.ReadDouble(
                $"Ліміт годин (за замовчуванням {Employee.DefaultMaxWeeklyHours}): ",
                0, 168, inclusiveMin: false);
            int maxTasks = ConsoleInput.ReadInt(
                $"Макс. одночасних активних завдань (за замовчуванням {Employee.DefaultMaxConcurrentTasks}): ",
                1, 50);

            Employee emp = service.CreateEmployee(fullName, role, maxHours, maxTasks);
            Console.WriteLine($"Додано виконавця: {emp.GetSummary()}");
        }

        private static void CreateTaskMenu(ProjectManagementService service)
        {
            Project? project = SelectProject(service);
            if (project is null) return;

            string title = ConsoleInput.ReadNonEmptyString("Назва завдання: ");
            string description = ConsoleInput.ReadOptionalString("Опис завдання: ");
            double estimatedHours = ConsoleInput.ReadDouble("Планові години (> 0): ", 0, 2000, inclusiveMin: false);
            TaskPriority priority = ConsoleInput.ReadEnum<TaskPriority>("Оберіть пріоритет завдання:");
            DateTime deadline = ConsoleInput.ReadDate($"Дедлайн завдання (не пізніше {project.Deadline:yyyy-MM-dd}): ");

            Guid? assigneeId = null;
            int assignNow = ConsoleInput.ReadInt("Призначити виконавця одразу? (1 — Так, 0 — Ні): ", 0, 1);
            if (assignNow == 1)
            {
                Employee? emp = SelectEmployee(service);
                if (emp is null) return;
                assigneeId = emp.Id;
            }

            WorkTask task = service.CreateTask(project.Id, title, description, estimatedHours, priority, deadline, assigneeId);
            Console.WriteLine($"Створено завдання: {task.GetSummary()}");
        }

        private static void AssignTaskMenu(ProjectManagementService service)
        {
            WorkTask? task = SelectTask(service);
            if (task is null) return;

            Employee? employee = SelectEmployee(service);
            if (employee is null) return;

            service.AssignTaskToEmployee(task.Id, employee.Id);
            Console.WriteLine($"Завдання '{task.Title}' призначено виконавцю '{employee.FullName}'.");
        }

        private static void LogTimeMenu(ProjectManagementService service)
        {
            WorkTask? task = SelectTask(service);
            if (task is null) return;

            Employee? employee = SelectEmployee(service);
            if (employee is null) return;

            DateTime logDate = ConsoleInput.ReadDate($"Дата роботи (не пізніше дедлайну задачі {task.Deadline:yyyy-MM-dd}): ");
            double hours = ConsoleInput.ReadDouble("Витрачені години (від 0 до 24): ", 0, 24, inclusiveMin: false);
            string note = ConsoleInput.ReadOptionalString("Коментар до виконаної роботи: ");

            TimeLog log = service.LogTime(task.Id, employee.Id, logDate, hours, note);
            Console.WriteLine($"Запис часу додано: {log.GetSummary()}");
        }

        private static void ChangeTaskStatusMenu(ProjectManagementService service)
        {
            WorkTask? task = SelectTask(service);
            if (task is null) return;

            WorkTaskStatus status = ConsoleInput.ReadEnum<WorkTaskStatus>("Оберіть новий статус завдання:");
            service.ChangeTaskStatus(task.Id, status);
            Console.WriteLine($"Статус завдання '{task.Title}' змінено на {status}.");
        }

        private static void ListEntitiesMenu(ProjectManagementService service)
        {
            Console.WriteLine("--- ПРОЄКТИ ---");
            foreach (var p in service.GetProjectsSorted())
                Console.WriteLine("  " + p.GetSummary());

            Console.WriteLine("\n--- ВИКОНАВЦІ ---");
            foreach (var e in service.GetEmployeesSorted())
                Console.WriteLine("  " + e.GetSummary());

            Console.WriteLine("\n--- ЗАВДАННЯ ---");
            foreach (var t in service.GetTasksSorted())
                Console.WriteLine("  " + t.GetSummary());

            Console.WriteLine("\n--- ЗАПИСИ ЧАСУ (TIMELOG) ---");
            foreach (var l in service.GetTimeLogsSorted())
                Console.WriteLine("  " + l.GetSummary());
        }

        private static void ListTasksSortedMenu(ProjectManagementService service)
        {
            Console.WriteLine("Оберіть спосіб сортування завдань:");
            Console.WriteLine("  1. За дедлайном (IComparer<WorkTask> — TaskDeadlineComparer)");
            Console.WriteLine("  2. За пріоритетом (IComparer<WorkTask> — TaskPriorityComparer)");
            Console.WriteLine("  3. Стандартне сортування (IComparable<WorkTask>)");
            int mode = ConsoleInput.ReadInt("Ваш вибір: ", 1, 3);

            List<WorkTask> sorted = mode switch
            {
                1 => service.GetTasksSorted(new TaskDeadlineComparer()),
                2 => service.GetTasksSorted(new TaskPriorityComparer()),
                _ => service.GetTasksSorted()
            };

            Console.WriteLine("\n--- ВІДСОРТОВАНІ ЗАВДАННЯ ---");
            foreach (var t in sorted)
                Console.WriteLine("  " + t.GetSummary());
        }

        private static void ShowHoursReport(ProjectManagementService service)
        {
            Console.WriteLine("=== ЗВІТ: ЗАЛУЧЕНИЙ ЧАС ТА ЗАВАНТАЖЕНІСТЬ ===");
            Console.WriteLine("\n-- За проєктами --");
            foreach (var p in service.Projects.GetAll())
            {
                double logged = service.GetTotalLoggedHoursForProject(p.Id);
                Console.WriteLine($"  {p.Name}: залучено {logged:F1} год (статус: {p.Status}, дедлайн: {p.Deadline:yyyy-MM-dd})");
            }

            Console.WriteLine("\n-- За виконавцями --");
            foreach (var e in service.Employees.GetAll())
            {
                double activePlanned = service.GetActivePlannedHoursForEmployee(e.Id);
                int activeCount = service.GetActiveTasksCountForEmployee(e.Id);
                double totalLogged = service.GetTotalLoggedHoursForEmployee(e.Id);
                Console.WriteLine($"  {e.FullName}: план активних {activePlanned:F1}/{e.MaxWeeklyHours:F0} год, " +
                                  $"активних задач {activeCount}/{e.MaxConcurrentTasks}, фактично залоговано {totalLogged:F1} год");
            }

            Console.WriteLine("\n-- За завданнями --");
            foreach (var t in service.Tasks.GetAll())
            {
                double logged = service.GetTotalLoggedHoursForTask(t.Id);
                Console.WriteLine($"  {t.Title}: факт {logged:F1} год / план {t.EstimatedHours:F1} год (статус: {t.Status})");
            }
        }

        private static void ShowOverdueReport(ProjectManagementService service)
        {
            Console.WriteLine($"=== ЗВІТ ПРО ПРОСТРОЧЕНІ СУТНОСТІ (станом на {DateTime.Today:yyyy-MM-dd}) ===");

            var overdueProjects = service.GetOverdueProjects();
            Console.WriteLine($"\n-- Прострочені проєкти ({overdueProjects.Count}) --");
            if (overdueProjects.Count == 0)
                Console.WriteLine("  Прострочених проєктів немає.");
            else
                foreach (var p in overdueProjects)
                    Console.WriteLine("  " + p.GetSummary());

            var overdueTasks = service.GetOverdueTasks();
            Console.WriteLine($"\n-- Прострочені завдання ({overdueTasks.Count}) --");
            if (overdueTasks.Count == 0)
                Console.WriteLine("  Прострочених завдань немає.");
            else
                foreach (var t in overdueTasks)
                    Console.WriteLine("  " + t.GetSummary());
        }

        private static void DeleteEntityMenu(ProjectManagementService service)
        {
            Console.WriteLine("Що видалити? 1 — Проєкт, 2 — Завдання, 3 — Виконавця");
            int type = ConsoleInput.ReadInt("Ваш вибір: ", 1, 3);

            if (type == 1)
            {
                Project? p = SelectProject(service);
                if (p is not null && service.DeleteProject(p.Id))
                    Console.WriteLine($"Проєкт '{p.Name}' видалено.");
            }
            else if (type == 2)
            {
                WorkTask? t = SelectTask(service);
                if (t is not null && service.DeleteTask(t.Id))
                    Console.WriteLine($"Завдання '{t.Title}' видалено.");
            }
            else
            {
                Employee? e = SelectEmployee(service);
                if (e is not null && service.DeleteEmployee(e.Id))
                    Console.WriteLine($"Виконавця '{e.FullName}' видалено.");
            }
        }

        private static void RunExceptionsDemo(ProjectManagementService service)
        {
            Console.WriteLine("=== ДЕМОНСТРАЦІЯ ВЛАСНИХ ВИНЯТКІВ ===");
            Project? project = service.Projects.GetAll().FirstOrDefault();
            Employee? emp = service.Employees.GetAll().FirstOrDefault();

            if (project is null || emp is null)
            {
                Console.WriteLine("Недостатньо даних для автодемонстрації.");
                return;
            }

            try
            {
                Console.WriteLine("1. Спроба створити завдання з дедлайном пізніше за дедлайн проєкту...");
                service.CreateTask(project.Id, "Тест простроченого дедлайну", "Демо", 4, TaskPriority.Low, project.Deadline.AddDays(5));
            }
            catch (DeadlineExceededException ex)
            {
                Console.WriteLine($"   Спіймано: {ex.Message}");
            }

            try
            {
                Console.WriteLine("2. Спроба призначити важке завдання понад ліміт годин виконавця...");
                service.CreateTask(project.Id, "Надважке завдання", "Демо перевантаження", 35, TaskPriority.High, project.Deadline, emp.Id);
            }
            catch (EmployeeOverloadedException ex)
            {
                Console.WriteLine($"   Спіймано: {ex.Message}");
            }

            try
            {
                Console.WriteLine("3. Спроба звернення до неіснуючого ID...");
                service.GetTotalLoggedHoursForTask(Guid.NewGuid());
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"   Спіймано: {ex.Message}");
            }
        }

        private static Project? SelectProject(ProjectManagementService service)
        {
            List<Project> items = service.Projects.GetAll();
            if (items.Count == 0)
            {
                Console.WriteLine("Список проєктів порожній.");
                return null;
            }

            for (int i = 0; i < items.Count; i++)
                Console.WriteLine($"  {i + 1}. {items[i].GetSummary()}");

            int idx = ConsoleInput.ReadInt("Оберіть проєкт (0 — скасувати): ", 0, items.Count);
            return idx == 0 ? null : items[idx - 1];
        }

        private static Employee? SelectEmployee(ProjectManagementService service)
        {
            List<Employee> items = service.Employees.GetAll();
            if (items.Count == 0)
            {
                Console.WriteLine("Список виконавців порожній.");
                return null;
            }

            for (int i = 0; i < items.Count; i++)
                Console.WriteLine($"  {i + 1}. {items[i].GetSummary()}");

            int idx = ConsoleInput.ReadInt("Оберіть виконавця (0 — скасувати): ", 0, items.Count);
            return idx == 0 ? null : items[idx - 1];
        }

        private static WorkTask? SelectTask(ProjectManagementService service)
        {
            List<WorkTask> items = service.Tasks.GetAll();
            if (items.Count == 0)
            {
                Console.WriteLine("Список завдань порожній.");
                return null;
            }

            for (int i = 0; i < items.Count; i++)
                Console.WriteLine($"  {i + 1}. {items[i].GetSummary()}");

            int idx = ConsoleInput.ReadInt("Оберіть завдання (0 — скасувати): ", 0, items.Count);
            return idx == 0 ? null : items[idx - 1];
        }

        private static void PrintError(string message)
        {
            ConsoleColor prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = prev;
        }
    }
}

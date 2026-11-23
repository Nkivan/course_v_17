# Діаграми архітектури та алгоритмів проєкту (Варіант № 17 — Управління IT-проєктами)

## 1. Діаграми класів

### 1.1. Предметна область: сутності та зв'язки (`Domain.Core/Models`)

```mermaid
classDiagram
    class IComparable~T~ {
        <<interface>>
        +CompareTo(T other) int
    }

    class EntityBase {
        <<abstract>>
        -Guid _id
        +Guid Id
        +GetSummary()* string
    }

    class Project {
        -string _name
        -string _description
        -DateTime _startDate
        -DateTime _deadline
        -ProjectStatus _status
        +string Name
        +string Description
        +DateTime StartDate
        +DateTime Deadline
        +ProjectStatus Status
        +IsOverdue(DateTime referenceDate) bool
        +CompareTo(Project other) int
        +GetSummary() string
    }

    class Employee {
        -string _fullName
        -string _role
        -double _maxWeeklyHours
        -int _maxConcurrentTasks
        +string FullName
        +string Role
        +double MaxWeeklyHours
        +int MaxConcurrentTasks
        +CompareTo(Employee other) int
        +GetSummary() string
    }

    class WorkTask {
        -Guid _projectId
        -Guid? _assigneeId
        -string _title
        -string _description
        -double _estimatedHours
        -TaskPriority _priority
        -DateTime _deadline
        -WorkTaskStatus _status
        +Guid ProjectId
        +Guid? AssigneeId
        +string Title
        +double EstimatedHours
        +TaskPriority Priority
        +DateTime Deadline
        +WorkTaskStatus Status
        +bool IsActive
        +IsOverdue(DateTime referenceDate) bool
        +CompareTo(WorkTask other) int
        +GetSummary() string
    }

    class TimeLog {
        -Guid _taskId
        -Guid _employeeId
        -DateTime _logDate
        -double _hours
        -string _note
        +Guid TaskId
        +Guid EmployeeId
        +DateTime LogDate
        +double Hours
        +string Note
        +CompareTo(TimeLog other) int
        +GetSummary() string
    }

    EntityBase <|-- Project : успадковує
    EntityBase <|-- Employee : успадковує
    EntityBase <|-- WorkTask : успадковує
    EntityBase <|-- TimeLog : успадковує

    IComparable~T~ <|.. Project : реалізує
    IComparable~T~ <|.. Employee : реалізує
    IComparable~T~ <|.. WorkTask : реалізує
    IComparable~T~ <|.. TimeLog : реалізує

    Project "1" --> "0..*" WorkTask : містить завдання
    Employee "0..1" --> "0..*" WorkTask : виконує
    WorkTask "1" --> "0..*" TimeLog : має записи часу
    Employee "1" --> "0..*" TimeLog : фіксує час
```

---

### 1.2. Збереження даних: репозиторій, компаратори, серіалізація та винятки (`Domain.Core`)

```mermaid
classDiagram
    class IRepository~T~ {
        <<interface>>
        +Add(T item)
        +Remove(Guid id) bool
        +GetById(Guid id) T?
        +GetAll() List~T~
        +Find(Predicate~T~ match) List~T~
        +GetSorted(IComparer~T~ comparer) List~T~
        +ReplaceAll(IEnumerable~T~ items)
    }

    class Repository~T~ {
        -List~T~ _items
        +Add(T item)
        +Remove(Guid id) bool
        +GetById(Guid id) T?
        +GetAll() List~T~
        +Find(Predicate~T~ match) List~T~
        +GetSorted(IComparer~T~ comparer) List~T~
        +ReplaceAll(IEnumerable~T~ items)
    }

    class JsonFileService~T~ {
        -string _filePath
        -JsonSerializerOptions _options
        +string FilePath
        +Save(List~T~ data)
        +Load() List~T~
    }

    class IComparer~WorkTask~ {
        <<interface>>
        +Compare(WorkTask x, WorkTask y) int
    }

    class TaskDeadlineComparer {
        +Compare(WorkTask x, WorkTask y) int
    }

    class TaskPriorityComparer {
        +Compare(WorkTask x, WorkTask y) int
    }

    class Exception {
        <<system>>
    }

    class DeadlineExceededException {
        -DateTime _deadline
        -DateTime _attemptedDate
        +DateTime Deadline
        +DateTime AttemptedDate
    }

    class EmployeeOverloadedException {
        -string _employeeName
        -double _currentHours
        -double _maxHours
        -int _currentTasks
        -int _maxTasks
    }

    class EntityNotFoundException {
        -string _entityType
        -Guid _entityId
    }

    class DomainValidationException

    IRepository~T~ <|.. Repository~T~ : реалізує
    IComparer~WorkTask~ <|.. TaskDeadlineComparer : реалізує
    IComparer~WorkTask~ <|.. TaskPriorityComparer : реалізує

    Exception <|-- DeadlineExceededException : успадковує
    Exception <|-- EmployeeOverloadedException : успадковує
    Exception <|-- EntityNotFoundException : успадковує
    Exception <|-- DomainValidationException : успадковує
```

---

### 1.3. Керування: меню, валідація вводу та бізнес-логіка (`Domain.UI` + `Domain.Core`)

```mermaid
classDiagram
    class Program {
        -string DataFolderName
        -Main()$
        -PrintMainMenu()$
        -LoadStateOrSeed(...)$
        -SaveAll(...)$
        -CreateProjectMenu(...)$
        -CreateTaskMenu(...)$
        -AssignTaskMenu(...)$
        -LogTimeMenu(...)$
        -ShowHoursReport(...)$
        -ShowOverdueReport(...)$
        -RunExceptionsDemo(...)$
    }

    class ConsoleInput {
        <<static>>
        +ReadNonEmptyString(prompt) string$
        +ReadOptionalString(prompt) string$
        +ReadInt(prompt, min, max) int$
        +ReadDouble(prompt, min, max) double$
        +ReadDate(prompt) DateTime$
        +ReadEnum~TEnum~(prompt) TEnum$
    }

    class SeedData {
        <<static>>
        +Populate(ProjectManagementService service)$
    }

    class IProjectManagementService {
        <<interface>>
        +CreateProject(...) Project
        +CreateEmployee(...) Employee
        +CreateTask(...) WorkTask
        +AssignTaskToEmployee(taskId, employeeId)
        +LogTime(...) TimeLog
        +GetTotalLoggedHoursForProject(projectId) double
        +GetOverdueTasks(referenceDate) List~WorkTask~
        +GetOverdueProjects(referenceDate) List~Project~
    }

    class ProjectManagementService {
        -IRepository~Project~ _projects
        -IRepository~WorkTask~ _tasks
        -IRepository~Employee~ _employees
        -IRepository~TimeLog~ _timeLogs
        +CreateProject(...) Project
        +CreateEmployee(...) Employee
        +CreateTask(...) WorkTask
        +AssignTaskToEmployee(taskId, employeeId)
        +LogTime(...) TimeLog
        -ValidateEmployeeWorkload(...)
    }

    Program --> ConsoleInput : перевіряє ввід
    Program --> SeedData : завантажує тестові дані
    Program --> ProjectManagementService : викликає бізнес-логіку
    Program --> JsonFileService~T~ : зберігає/читає JSON
    IProjectManagementService <|.. ProjectManagementService : реалізує
    ProjectManagementService --> IRepository~T~ : працює з колекціями
```

Класи `ProjectManagementService`, `Repository<T>`, `JsonFileService<T>`, моделі та винятки винесені в окрему бібліотеку `Domain.Core` (.dll). Консольне меню, `ConsoleInput` та `SeedData` належать проєкту `Domain.UI`.

---

## 2. Взаємодія об'єктів: призначення завдання виконавцю

```mermaid
sequenceDiagram
    actor Користувач
    participant Program
    participant ConsoleInput
    participant Service as ProjectManagementService
    participant TaskRepo as Repository~WorkTask~
    participant EmpRepo as Repository~Employee~

    Користувач->>Program: пункт 4 — Призначити виконавця
    Program->>Service: Tasks.GetAll() та Employees.GetAll()
    Service-->>Program: списки завдань і виконавців
    Program->>ConsoleInput: ReadInt("Оберіть завдання")
    ConsoleInput-->>Program: taskId
    Program->>ConsoleInput: ReadInt("Оберіть виконавця")
    ConsoleInput-->>Program: employeeId

    Program->>Service: AssignTaskToEmployee(taskId, employeeId)
    Service->>TaskRepo: GetById(taskId)
    TaskRepo-->>Service: WorkTask
    Service->>EmpRepo: GetById(employeeId)
    EmpRepo-->>Service: Employee

    Service->>TaskRepo: Find(t => t.AssigneeId == employeeId && t.IsActive)
    TaskRepo-->>Service: список активних завдань

    alt Перевищено 40 год або ліміт активних завдань
        Service--xProgram: throw EmployeeOverloadedException
        Program-->>Користувач: Повідомлення про перевантаження
    else Навантаження в межах норми
        Service->>Service: task.AssigneeId = employeeId, Status = InProgress
        Service-->>Program: успішно призначено
        Program-->>Користувач: Завдання призначено виконавцю
    end
```

---

## 3. Блок-схеми алгоритмів бізнес-логіки

### 3.1. Перевірка навантаження виконавця (`ValidateEmployeeWorkload`)

```mermaid
flowchart TD
    Start([Початок]) --> GetActive["Знайти всі активні завдання<br/>виконавця (Status != Completed)"]
    GetActive --> CalcCurrent["Обчислити currentHours = сума годин<br/>та currentCount = кількість завдань"]
    CalcCurrent --> CheckSame{"Це перепризначення<br/>тієї самої задачі?"}

    CheckSame -- "так" --> SameProj["projectedHours = currentHours<br/>projectedCount = currentCount"]
    CheckSame -- "ні" --> AddProj["projectedHours = currentHours + additionalHours<br/>projectedCount = currentCount + 1"]

    SameProj --> CheckHours
    AddProj --> CheckHours

    CheckHours{"projectedHours ><br/>MaxWeeklyHours (40)?"}
    CheckHours -- "так" --> ThrowHours["EmployeeOverloadedException:<br/>перевищено ліміт годин"]
    CheckHours -- "ні" --> CheckTasks{"projectedCount ><br/>MaxConcurrentTasks (3)?"}

    CheckTasks -- "так" --> ThrowTasks["EmployeeOverloadedException:<br/>перевищено ліміт завдань"]
    CheckTasks -- "ні" --> End([Успішне завершення перевірки])
```

---

### 3.2. Алгоритм обліку витраченого часу (`LogTime`)

```mermaid
flowchart TD
    Start([Початок]) --> FindEntities["Знайти WorkTask та Employee за ID"]
    FindEntities --> Exists{"Обидві сутності<br/>існують?"}
    Exists -- "ні" --> ThrowNotFound["EntityNotFoundException:<br/>сутність не знайдено"]
    Exists -- "так" --> CheckAssigned{"Завдання призначене<br/>цьому виконавцю?"}

    CheckAssigned -- "ні" --> ThrowValidation1["DomainValidationException:<br/>невірний виконавець"]
    CheckAssigned -- "так" --> CheckCompleted{"Status ==<br/>Completed?"}

    CheckCompleted -- "так" --> ThrowValidation2["DomainValidationException:<br/>завдання вже завершене"]
    CheckCompleted -- "ні" --> CheckDeadline{"logDate ><br/>task.Deadline?"}

    CheckDeadline -- "так" --> ThrowDeadline["DeadlineExceededException:<br/>логування після дедлайну"]
    CheckDeadline -- "ні" --> CreateLog["Створити TimeLog (0 < hours <= 24)<br/>та додати в Repository~TimeLog~"]

    CreateLog --> UpdateStatus["Якщо Status == New,<br/>змінити на InProgress"]
    UpdateStatus --> End([Повернути створений TimeLog])
```

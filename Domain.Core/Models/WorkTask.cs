using Domain.Core.Enums;

namespace Domain.Core.Models
{
   
    public class WorkTask : EntityBase, IComparable<WorkTask>
    {
        private Guid _projectId;
        private Guid? _assigneeId;
        private string _title = string.Empty;
        private string _description = string.Empty;
        private double _estimatedHours;
        private TaskPriority _priority;
        private DateTime _deadline;
        private WorkTaskStatus _status;

        public Guid ProjectId
        {
            get => _projectId;
            set
            {
                if (value == Guid.Empty)
                    throw new ArgumentException("Завдання має бути прив'язане до коректного проєкту.", nameof(ProjectId));
                _projectId = value;
            }
        }

        public Guid? AssigneeId
        {
            get => _assigneeId;
            set => _assigneeId = value;
        }

        public string Title
        {
            get => _title;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Назва завдання не може бути порожньою.", nameof(Title));
                _title = value.Trim();
            }
        }

        public string Description
        {
            get => _description;
            set => _description = value?.Trim() ?? string.Empty;
        }

        public double EstimatedHours
        {
            get => _estimatedHours;
            set
            {
                if (value <= 0 || value > 2000)
                    throw new ArgumentException("Планові години завдання мають бути в межах (0; 2000].", nameof(EstimatedHours));
                _estimatedHours = value;
            }
        }

        public TaskPriority Priority
        {
            get => _priority;
            set => _priority = value;
        }

        public DateTime Deadline
        {
            get => _deadline;
            set => _deadline = value.Date;
        }

        public WorkTaskStatus Status
        {
            get => _status;
            set => _status = value;
        }

        public WorkTask()
        {
            _priority = TaskPriority.Medium;
            _deadline = DateTime.Today.AddDays(7);
            _status = WorkTaskStatus.New;
        }

        public WorkTask(
            Guid projectId,
            string title,
            string description,
            double estimatedHours,
            TaskPriority priority,
            DateTime deadline,
            Guid? assigneeId = null,
            WorkTaskStatus status = WorkTaskStatus.New)
        {
            ProjectId = projectId;
            Title = title;
            Description = description;
            EstimatedHours = estimatedHours;
            Priority = priority;
            _deadline = deadline.Date;
            AssigneeId = assigneeId;
            Status = status;
        }

        public bool IsActive => Status != WorkTaskStatus.Completed;

        public bool IsOverdue(DateTime referenceDate)
        {
            return IsActive && referenceDate.Date > Deadline.Date;
        }

        public int CompareTo(WorkTask? other)
        {
            if (other is null) return 1;
            int byDeadline = Deadline.CompareTo(other.Deadline);
            return byDeadline != 0 ? byDeadline : string.Compare(Title, other.Title, StringComparison.OrdinalIgnoreCase);
        }

        public override string GetSummary()
        {
            string assigneeInfo = AssigneeId.HasValue ? AssigneeId.Value.ToString()[..8] : "Не призначено";
            return $"[{Id.ToString()[..8]}] {Title} | Пріоритет: {Priority} | Статус: {Status} | " +
                   $"План: {EstimatedHours:F1} год | Дедлайн: {Deadline:yyyy-MM-dd} | Виконавець: {assigneeInfo}";
        }
    }
}

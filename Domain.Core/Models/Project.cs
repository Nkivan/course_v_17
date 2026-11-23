using Domain.Core.Enums;

namespace Domain.Core.Models
{
    
    public class Project : EntityBase, IComparable<Project>
    {
        private string _name = string.Empty;
        private string _description = string.Empty;
        private DateTime _startDate;
        private DateTime _deadline;
        private ProjectStatus _status;

        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Назва проєкту не може бути порожньою.", nameof(Name));
                _name = value.Trim();
            }
        }

        public string Description
        {
            get => _description;
            set => _description = value?.Trim() ?? string.Empty;
        }

        public DateTime StartDate
        {
            get => _startDate;
            set => _startDate = value.Date;
        }

        public DateTime Deadline
        {
            get => _deadline;
            set
            {
                if (value.Date < _startDate)
                    throw new ArgumentException("Дедлайн проєкту не може бути ранішим за дату старту.", nameof(Deadline));
                _deadline = value.Date;
            }
        }

        public ProjectStatus Status
        {
            get => _status;
            set => _status = value;
        }

        public Project()
        {
            _startDate = DateTime.Today;
            _deadline = DateTime.Today.AddDays(30);
            _status = ProjectStatus.Planned;
        }

        public Project(string name, string description, DateTime startDate, DateTime deadline, ProjectStatus status = ProjectStatus.Planned)
        {
            Name = name;
            Description = description;
            _startDate = startDate.Date;
            Deadline = deadline.Date;
            Status = status;
        }

        public bool IsOverdue(DateTime referenceDate)
        {
            return Status != ProjectStatus.Completed && referenceDate.Date > Deadline.Date;
        }

        public int CompareTo(Project? other)
        {
            if (other is null) return 1;
            int byDeadline = Deadline.CompareTo(other.Deadline);
            return byDeadline != 0 ? byDeadline : string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override string GetSummary()
        {
            return $"[{Id.ToString()[..8]}] {Name} | Статус: {Status} | Дедлайн: {Deadline:yyyy-MM-dd}";
        }
    }
}

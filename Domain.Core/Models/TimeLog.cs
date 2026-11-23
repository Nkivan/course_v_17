namespace Domain.Core.Models
{
   
    public class TimeLog : EntityBase, IComparable<TimeLog>
    {
        private Guid _taskId;
        private Guid _employeeId;
        private DateTime _logDate;
        private double _hours;
        private string _note = string.Empty;

        public Guid TaskId
        {
            get => _taskId;
            set
            {
                if (value == Guid.Empty)
                    throw new ArgumentException("Запис часу має бути прив'язаний до завдання.", nameof(TaskId));
                _taskId = value;
            }
        }

        public Guid EmployeeId
        {
            get => _employeeId;
            set
            {
                if (value == Guid.Empty)
                    throw new ArgumentException("Запис часу має бути прив'язаний до виконавця.", nameof(EmployeeId));
                _employeeId = value;
            }
        }

        public DateTime LogDate
        {
            get => _logDate;
            set => _logDate = value.Date;
        }

        public double Hours
        {
            get => _hours;
            set
            {
                if (value <= 0 || value > 24)
                    throw new ArgumentException("Кількість годин за один запис має бути більшою за 0 та не перевищувати 24.", nameof(Hours));
                _hours = value;
            }
        }

        public string Note
        {
            get => _note;
            set => _note = value?.Trim() ?? string.Empty;
        }

        public TimeLog()
        {
            _logDate = DateTime.Today;
        }

        public TimeLog(Guid taskId, Guid employeeId, DateTime logDate, double hours, string note = "")
        {
            TaskId = taskId;
            EmployeeId = employeeId;
            _logDate = logDate.Date;
            Hours = hours;
            Note = note;
        }

        public int CompareTo(TimeLog? other)
        {
            if (other is null) return 1;
            return LogDate.CompareTo(other.LogDate);
        }

        public override string GetSummary()
        {
            return $"[{Id.ToString()[..8]}] {LogDate:yyyy-MM-dd} | {Hours:F1} год | Задача: {TaskId.ToString()[..8]} | " +
                   $"Виконавець: {EmployeeId.ToString()[..8]} | {Note}";
        }
    }
}

namespace Domain.Core.Models
{
    
    public class Employee : EntityBase, IComparable<Employee>
    {
        public const double DefaultMaxWeeklyHours = 40.0;
        public const int DefaultMaxConcurrentTasks = 3;

        private string _fullName = string.Empty;
        private string _role = string.Empty;
        private double _maxWeeklyHours;
        private int _maxConcurrentTasks;

        public string FullName
        {
            get => _fullName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("ПІБ виконавця не може бути порожнім.", nameof(FullName));
                _fullName = value.Trim();
            }
        }

        public string Role
        {
            get => _role;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Посада/роль виконавця не може бути порожньою.", nameof(Role));
                _role = value.Trim();
            }
        }

        public double MaxWeeklyHours
        {
            get => _maxWeeklyHours;
            set
            {
                if (value <= 0 || value > 168)
                    throw new ArgumentException("Ліміт годин виконавця має бути в межах (0; 168].", nameof(MaxWeeklyHours));
                _maxWeeklyHours = value;
            }
        }

        public int MaxConcurrentTasks
        {
            get => _maxConcurrentTasks;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Максимальна кількість одночасних завдань має бути більшою за 0.", nameof(MaxConcurrentTasks));
                _maxConcurrentTasks = value;
            }
        }

        public Employee()
        {
            _maxWeeklyHours = DefaultMaxWeeklyHours;
            _maxConcurrentTasks = DefaultMaxConcurrentTasks;
        }

        public Employee(
            string fullName,
            string role,
            double maxWeeklyHours = DefaultMaxWeeklyHours,
            int maxConcurrentTasks = DefaultMaxConcurrentTasks)
        {
            FullName = fullName;
            Role = role;
            MaxWeeklyHours = maxWeeklyHours;
            MaxConcurrentTasks = maxConcurrentTasks;
        }

        public int CompareTo(Employee? other)
        {
            if (other is null) return 1;
            return string.Compare(FullName, other.FullName, StringComparison.OrdinalIgnoreCase);
        }

        public override string GetSummary()
        {
            return $"[{Id.ToString()[..8]}] {FullName} ({Role}) | Ліміт: {MaxWeeklyHours:F0} год, макс. задач: {MaxConcurrentTasks}";
        }
    }
}

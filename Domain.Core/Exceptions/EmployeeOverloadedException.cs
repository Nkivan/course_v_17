namespace Domain.Core.Exceptions
{
    
    public class EmployeeOverloadedException : Exception
    {
        private readonly string _employeeName;
        private readonly double _currentHours;
        private readonly double _maxHours;
        private readonly int _currentTasks;
        private readonly int _maxTasks;

        public string EmployeeName => _employeeName;
        public double CurrentHours => _currentHours;
        public double MaxHours => _maxHours;
        public int CurrentTasks => _currentTasks;
        public int MaxTasks => _maxTasks;

        public EmployeeOverloadedException(
            string employeeName,
            double currentHours,
            double maxHours,
            int currentTasks,
            int maxTasks,
            string reason)
            : base($"Перевантаження виконавця '{employeeName}': {reason} " +
                   $"[Години: {currentHours:F1}/{maxHours:F1}, Активні завдання: {currentTasks}/{maxTasks}].")
        {
            _employeeName = employeeName;
            _currentHours = currentHours;
            _maxHours = maxHours;
            _currentTasks = currentTasks;
            _maxTasks = maxTasks;
        }
    }
}

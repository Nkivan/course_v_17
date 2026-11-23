namespace Domain.Core.Exceptions
{
    
    public class DeadlineExceededException : Exception
    {
        private readonly DateTime _deadline;
        private readonly DateTime _attemptedDate;

        public DateTime Deadline => _deadline;
        public DateTime AttemptedDate => _attemptedDate;

        public DeadlineExceededException(string message, DateTime deadline, DateTime attemptedDate)
            : base($"{message} (Дедлайн: {deadline:yyyy-MM-dd}, вказана/поточна дата: {attemptedDate:yyyy-MM-dd}).")
        {
            _deadline = deadline.Date;
            _attemptedDate = attemptedDate.Date;
        }
    }
}

namespace Domain.Core.Models
{
    
    public abstract class EntityBase
    {
        private Guid _id;

        public Guid Id
        {
            get => _id;
            set
            {
                if (value == Guid.Empty)
                    throw new ArgumentException("Ідентифікатор сутності не може бути порожнім (Guid.Empty).", nameof(Id));
                _id = value;
            }
        }

        protected EntityBase()
        {
            _id = Guid.NewGuid();
        }

        protected EntityBase(Guid id)
        {
            Id = id;
        }

        public abstract string GetSummary();
    }
}

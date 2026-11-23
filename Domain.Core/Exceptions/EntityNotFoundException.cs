namespace Domain.Core.Exceptions
{
    
    public class EntityNotFoundException : Exception
    {
        private readonly string _entityType;
        private readonly Guid _entityId;

        public string EntityType => _entityType;
        public Guid EntityId => _entityId;

        public EntityNotFoundException(string entityType, Guid entityId)
            : base($"Сутність '{entityType}' з ідентифікатором '{entityId}' не знайдено.")
        {
            _entityType = entityType;
            _entityId = entityId;
        }
    }
}

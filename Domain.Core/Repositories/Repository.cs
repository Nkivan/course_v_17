using Domain.Core.Interfaces;
using Domain.Core.Models;

namespace Domain.Core.Repositories
{
    
    public class Repository<T> : IRepository<T> where T : EntityBase
    {
        private readonly List<T> _items = new();

        public void Add(T item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (_items.Any(existing => existing.Id == item.Id))
                throw new ArgumentException($"Елемент з ідентифікатором '{item.Id}' уже існує в репозиторії.", nameof(item));

            _items.Add(item);
        }

        public bool Remove(Guid id)
        {
            T? existing = GetById(id);
            if (existing is null)
                return false;

            return _items.Remove(existing);
        }

        public T? GetById(Guid id)
        {
            return _items.FirstOrDefault(item => item.Id == id);
        }

        public List<T> GetAll()
        {
            return new List<T>(_items);
        }

        public List<T> Find(Predicate<T> match)
        {
            if (match is null)
                throw new ArgumentNullException(nameof(match));

            return _items.FindAll(match);
        }

        public List<T> GetSorted(IComparer<T>? comparer = null)
        {
            List<T> copy = new(_items);
            if (comparer is not null)
                copy.Sort(comparer);
            else
                copy.Sort();

            return copy;
        }

        public void ReplaceAll(IEnumerable<T> items)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            _items.Clear();
            _items.AddRange(items);
        }
    }
}

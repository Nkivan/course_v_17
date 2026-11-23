using Domain.Core.Models;

namespace Domain.Core.Interfaces
{
    
    public interface IRepository<T> where T : EntityBase
    {
        void Add(T item);
        bool Remove(Guid id);
        T? GetById(Guid id);
        List<T> GetAll();
        List<T> Find(Predicate<T> match);
        List<T> GetSorted(IComparer<T>? comparer = null);
        void ReplaceAll(IEnumerable<T> items);
    }
}

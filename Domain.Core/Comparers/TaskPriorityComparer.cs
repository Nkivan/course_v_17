using Domain.Core.Models;

namespace Domain.Core.Comparers
{
    
    public class TaskPriorityComparer : IComparer<WorkTask>
    {
        public int Compare(WorkTask? x, WorkTask? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return 1;
            if (y is null) return -1;

            int priorityComparison = y.Priority.CompareTo(x.Priority);
            if (priorityComparison != 0)
                return priorityComparison;

            return x.Deadline.CompareTo(y.Deadline);
        }
    }
}

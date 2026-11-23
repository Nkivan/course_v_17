using Domain.Core.Models;

namespace Domain.Core.Comparers
{

    public class TaskDeadlineComparer : IComparer<WorkTask>
    {
        public int Compare(WorkTask? x, WorkTask? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;

            int deadlineComparison = x.Deadline.CompareTo(y.Deadline);
            if (deadlineComparison != 0)
                return deadlineComparison;

            return y.Priority.CompareTo(x.Priority);
        }
    }
}

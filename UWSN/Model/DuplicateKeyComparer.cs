using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model;

public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
{
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).

    public int Compare(TKey x, TKey y)
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    {
        int result = x.CompareTo(y);
        if (result == 0)
            return 1; // Handle equality as being greater
        else
            return result;
    }
}
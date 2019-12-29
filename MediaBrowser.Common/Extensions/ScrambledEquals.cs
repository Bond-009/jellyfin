#nullable enable

using System.Collections.Generic;

namespace MediaBrowser.Common.Extensions
{
    /// <summary>
    /// Provides <c>ScrambledEquals</c> extensions methods for <see cref="IEnumerable{T}" />.
    /// </summary>
    public static class ScrambledEqualsExtensions
    {
        /// <summary>
        /// Compares the items in <c>list1</c> and <c>list2</c> regardless of order.
        /// </summary>
        /// <param name="first">The first list.</param>
        /// <param name="second">The second list.</param>
        /// <param name="comparer">The item comparer.</param>
        /// <typeparam name="T">The item type.</typeparam>
        /// <returns><c>true</c> if the items match; otherwise, <c>false</c></returns>
        public static bool ScrambledEquals<T>(this IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T>? comparer = null)
        {
            if (first is ICollection<T> firstCol && second is ICollection<T> secondCol)
            {
                // Do an early return if they don't have the same amount of elements
                if (firstCol.Count != secondCol.Count)
                {
                    return false;
                }

                if (firstCol is IList<T> firstList && secondCol is IList<T> secondList)
                {
                    var count = firstList.Count;
                    var cnt1 = new Dictionary<T, int>(comparer);
                    for (int i = 0; i < count; i++)
                    {
                        var key = firstList[i];
                        if (!cnt1.TryAdd(key, 1))
                        {
                            cnt1[key]++;
                        }
                    }

                    for (int i = 0; i < count; i++)
                    {
                        var key = secondList[i];
                        if (cnt1.ContainsKey(key))
                        {
                            if (--cnt1[key] < 0)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            var cnt2 = new Dictionary<T, int>(comparer);
            foreach (T s in first)
            {
                if (!cnt2.TryAdd(s, 1))
                {
                    cnt2[s]++;
                }
            }

            foreach (T s in second)
            {
                if (!cnt2.ContainsKey(s))
                {
                    if (--cnt2[s] < 0)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}

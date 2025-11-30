using CK3BeagleServer.Core.Domain.Entities;
using System;
using System.Collections.Generic;

namespace CK3BeagleServer.Core.Comparing
{
    public sealed class ComparisonHelpers
    {

        public static bool StringMatches(string sourceString, string editString)
        {
            float similarity;
            if (sourceString == editString)
                similarity = 1;
            else
                similarity = CalcBigramSimilarity(sourceString, editString);

            return similarity >= 0.6;
        }


        public static (HashSet<string> added, HashSet<string> removed, HashSet<string> changed, HashSet<string> untouched)
            SimpleListComparison<T>(Dictionary<string, T> sourceDict, Dictionary<string, T> editDict, Func<T, T, bool> comparator)
           where T : Node
        {
            var added = new HashSet<string>();
            var removed = new HashSet<string>();
            var changed = new HashSet<string>();
            var untouched = new HashSet<string>();

            foreach (var item in sourceDict)
            {
                if (!editDict.TryGetValue(item.Key, out T editedItem))
                {
                    removed.Add(item.Key);
                }
                else
                {
                    if (comparator(item.Value, editedItem))
                    {
                        untouched.Add(item.Key);
                    }
                    else
                    {
                        changed.Add(item.Key);
                    }
                }
            }
            foreach (var file in editDict)
            {
                if (!sourceDict.TryGetValue(file.Key, out T _))
                {
                    added.Add(file.Key);
                }
            }

            return (added, removed, changed, untouched);
        }

        public static IEnumerable<(T, T)> GetIdenticalPairs<T, TKey>(
            IEnumerable<T> first,
            IEnumerable<T> second,
            Func<T, TKey> keySelector)
            where TKey : IComparable<TKey>
        {
            using var enum1 = first.GetEnumerator();
            using var enum2 = second.GetEnumerator();

            bool has1 = enum1.MoveNext();
            bool has2 = enum2.MoveNext();

            while (has1 && has2)
            {
                var key1 = keySelector(enum1.Current);
                var key2 = keySelector(enum2.Current);

                int cmp = key1.CompareTo(key2);

                if (cmp == 0)
                {
                    yield return (enum1.Current, enum2.Current);
                    has1 = enum1.MoveNext();
                    has2 = enum2.MoveNext();
                }
                else if (cmp < 0)
                {
                    has1 = enum1.MoveNext();
                }
                else
                {
                    has2 = enum2.MoveNext();
                }
            }
        }

        public static float CalcBigramSimilarity(string sourceString, string editString)
        {
            if (sourceString.Length <= 1 || editString.Length <= 1)
                return sourceString == editString ? 1 : 0;

            var bigrams = new HashSet<string>();
            for (int i = 0; i < sourceString.Length - 1; i++)
            {
                var substring = sourceString.Substring(i, 2);
                bigrams.Add(substring);
            }

            int sharedBigramCount = 0;
            for (int i = 0; i < editString.Length - 1; i++)
            {
                var substring = editString.Substring(i, 2);
                if (!bigrams.Add(substring))
                {
                    sharedBigramCount++;
                }
            }

            var totalNoOfBigrams = sourceString.Length - 1 + editString.Length - 1;

            return 2 * (float)sharedBigramCount / totalNoOfBigrams;
        }
    }
}

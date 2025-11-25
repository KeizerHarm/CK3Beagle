using CK3Analyser.Core.Domain.Entities;
using System;
using System.Collections.Generic;

namespace CK3Analyser.Analysing.Comparing
{
    public sealed class ComparisonHelpers
    {
        public static (HashSet<string> added, HashSet<string> removed, HashSet<string> changed, HashSet<string> untouched)
            SimpleListComparison<T>(Dictionary<string, T> baseDict, Dictionary<string, T> editDict, Func<T, T, bool> comparator)
           where T : Node
        {
            var added = new HashSet<string>();
            var removed = new HashSet<string>();
            var changed = new HashSet<string>();
            var untouched = new HashSet<string>();

            foreach (var item in baseDict)
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
                if (!baseDict.TryGetValue(file.Key, out T _))
                {
                    added.Add(file.Key);
                }
            }

            return (added, removed, changed, untouched);
        }
    }
}

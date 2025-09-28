using CK3Analyser.Core.Domain.Entities;
using System.Collections.Generic;

namespace CK3Analyser.Analysis.Comparing
{
    public sealed class ComparisonHelpers
    {
        public static (HashSet<string> added, HashSet<string> removed, HashSet<string> changed, HashSet<string> untouched)
            SimpleListComparison<T>(Dictionary<string, T> baseDict, Dictionary<string, T> editDict)
           where T : Node
        {
            var added = new HashSet<string>();
            var removed = new HashSet<string>();
            var changed = new HashSet<string>();
            var untouched = new HashSet<string>();

            foreach (var item in baseDict)
            {
                if (!editDict.TryGetValue(item.Key, out T editedFile))
                {
                    removed.Add(item.Key);
                }
                else
                {
                    if (item.Value.Raw == editedFile.Raw)
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

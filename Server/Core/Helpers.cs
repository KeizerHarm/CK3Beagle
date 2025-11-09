using System;
using System.Collections.Generic;
using System.Linq;

namespace CK3Analyser.Core
{
    public static class Helpers
    {
        public static void AddToDictCollection<KEY, VALUE, LISTTYPE>(this Dictionary<KEY, LISTTYPE> dictionary, VALUE obj, KEY key)
            where LISTTYPE : ICollection<VALUE>, new()
        {
            if (!dictionary.TryGetValue(key, out var collection))
            {
                collection = new LISTTYPE();
                dictionary.Add(key, collection);
            }
            collection.Add(obj);
        }

        public static void ForEachWithProgress<T>(
            this IEnumerable<T> source,
            Action<T> action,
            Action<int> log,
            int progressStepPercent = 10)
        {
            var collection = source as ICollection<T> ?? source.ToList();
            int total = collection.Count;
            if (total == 0)
                return;

            // Trivial case, slowdown of writing becomes notable
            if (total <= 20)
            {
                foreach (var item in collection)
                {
                    action(item);
                }
                log(100);
                return;
            }

            // No logging delegate provided
            if (log == null)
            {
                foreach (var item in collection)
                {
                    action(item);
                }
                return;
            }

            int nextPercent = progressStepPercent;
            int nextThreshold = total * nextPercent / 100;
            int index = 0;

            foreach (var item in collection)
            {
                action(item);
                index++;

                if (index >= nextThreshold)
                {
                    log(nextPercent);
                    nextPercent += progressStepPercent;
                    nextThreshold = total * nextPercent / 100;
                }
            }

            // Ensure 100% is logged once, even if total isn't a clean multiple
            if (nextPercent > 100)
                log(100);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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
            int progressStepPercent = 25)
        {
            source.ForEachWithProgress<T>(action,
                (progress) =>
                {
                    log(progress);
                    return Task.CompletedTask;
                },
                progressStepPercent
                ).RunSynchronously();
        }

        public static async Task ForEachWithProgress<T>(
            this IEnumerable<T> source,
            Action<T> action,
            Func<int, Task> log,
            int progressStepPercent = 25)
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
                await log(100);
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
                    await log(nextPercent);
                    nextPercent += Math.Min(100, progressStepPercent);
                    nextThreshold = total * nextPercent / 100;
                }
            }
        }

        public static async Task ParallelForEachWithProgress<T>(
            this IEnumerable<T> source,
            Action<T> action,
            Func<int, Task> log,
            int progressStepPercent = 25,
            ParallelOptions? parallelOptions = null)
        {
            var collection = source as ICollection<T> ?? source.ToList();
            int total = collection.Count;
            if (total == 0)
                return;

            // Trivial case, slowdown of writing becomes notable
            if (total <= 20)
            {
                Parallel.ForEach(collection, parallelOptions ?? new ParallelOptions(), action);
                await log(100);
                return;
            }

            // No logging delegate provided
            if (log == null)
            {
                Parallel.ForEach(collection, parallelOptions ?? new ParallelOptions(), action);
                return;
            }

            int nextPercent = progressStepPercent;
            int nextThreshold = total * nextPercent / 100;
            int index = 0;

            var logTasks = new List<Task>();
            var lockObj = new object();

            Parallel.ForEach(
                collection,
                parallelOptions ?? new ParallelOptions(),
                item =>
                {
                    action(item);

                    var current = Interlocked.Increment(ref index);

                    if (current >= nextThreshold)
                    {
                        lock (lockObj)
                        {
                            // Recheck inside the lock
                            if (current >= nextThreshold)
                            {
                                int percent = nextPercent;
                                logTasks.Add(Task.Run(async () => await log(percent)));

                                nextPercent = Math.Min(100, nextPercent + progressStepPercent);
                                nextThreshold = total * nextPercent / 100;
                            }
                        }
                    }
                });

            await Task.WhenAll(logTasks);
        }


        public static string GenericToString(this object obj)
        {
            Type type = obj.GetType();
            FieldInfo[] fields = type.GetFields();
            PropertyInfo[] properties = type.GetProperties();

            Dictionary<string, object> values = new Dictionary<string, object>();
            Array.ForEach(fields, field => values.Add(field.Name, field.GetValue(obj)));
            Array.ForEach(properties, property =>
            {
                if (property.CanRead)
                    values.Add(property.Name, property.GetValue(obj, null));
            });

            string FormatValue(object v)
            {
                if (v == null)
                    return "null";

                if (v is IEnumerable enumerable && v is not string)
                {
                    var items = new List<string>();
                    foreach (var item in enumerable)
                        items.Add(item?.ToString() ?? "null");
                    return "[" + string.Join(", ", items) + "]";
                }

                return v.ToString();
            }

            return string.Join(", ",
                values.Select(kvp => kvp.Key + "=" + FormatValue(kvp.Value)));
        }

    }
}

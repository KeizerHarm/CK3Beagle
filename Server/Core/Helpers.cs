using System.Collections.Generic;

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
    }
}

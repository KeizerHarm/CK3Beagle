using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace CK3Analyser.Core.Resources.Storage
{
    public class StringTable
    {
        private readonly ConcurrentDictionary<string, int> _stringTable = [];
        private int _nextId = 0;
        public List<string> STRINGTABLE = [];
        private readonly object lockObject = new();

        public int GetId(string s)
        {
            return _stringTable.GetOrAdd(s, key =>
            {
                int id;
                lock (lockObject)
                {
                    id = _nextId++;
                    STRINGTABLE.Add(key);
                }
                return id;
            });
        }

        public string GetString(int i)
        {
            lock (lockObject)
            {
                return STRINGTABLE[i];
            }
            
        }
    }
}

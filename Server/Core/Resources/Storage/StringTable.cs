using System.Collections.Generic;

namespace CK3Analyser.Core.Resources.Storage
{
    public class StringTable
    {
        private readonly Dictionary<string, int> _stringTable = [];
        private int _nextId = 0;
        public List<string> STRINGTABLE = [];
        private readonly object lockObject = new();

        public int GetId(string s)
        {
            lock (lockObject)
            {
                if (!_stringTable.TryGetValue(s, out int id))
                {
                    id = _nextId++;
                    _stringTable[s] = id;
                    STRINGTABLE.Add(s);
                }
                return id;
            }
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

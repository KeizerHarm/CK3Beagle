using CK3BeagleServer.Core.Domain.Symbols;
using System.Collections.Generic;

namespace CK3BeagleServer.Core.Resources.Storage
{
    public interface ISymbolStore
    {
        ISymbol Get(int id);
        int Count { get; }
        void Add(ISymbol symbol);
    }

    public sealed class SymbolStore<T> : ISymbolStore where T : ISymbol
    {
        public List<T> Items { get; } = new();

        public ISymbol Get(int id) => Items[id];
        public int Count => Items.Count;
        public void Add(ISymbol symbol) => Items.Add((T)symbol);
    }
}

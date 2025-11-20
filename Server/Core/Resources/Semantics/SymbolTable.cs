using CK3Analyser.Core.Domain.Symbols;
using CK3Analyser.Core.Generated;
using System;
using System.Collections.Generic;

namespace CK3Analyser.Core.Resources.Semantics
{

    public sealed class SymbolTable
    {
        private readonly Dictionary<SymbolType, ISymbolStore> _stores = new(Enum.GetValues<SymbolType>().Length);

        public SymbolTable()
        {
            GeneratedSymbolExtensions.RegisterAllTypes(this);
        }

        public void Register<T>(SymbolType symbolType) where T : ISymbol
        {
            _stores[symbolType] = new SymbolStore<T>();
        }

        public int Insert(SymbolType kind, ISymbol symbol)
        {
            var s = _stores[kind];
            var id = s.Count;
            s.Add(symbol);
            return id;
        }

        public ISymbol Lookup(SymbolType kind, int? id)
        {
            if (!id.HasValue || id.Value == -1)
                return null;

            return _stores[kind].Get(id.Value);
        }

        public List<T> GetList<T>(SymbolType kind) where T : ISymbol
        {
            return ((SymbolStore<T>)_stores[kind]).Items;
        }
    }

}

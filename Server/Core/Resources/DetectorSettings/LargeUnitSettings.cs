namespace CK3Analyser.Core.Resources.DetectorSettings
{
    public readonly struct LargeUnitSettings : IGenericSettings
    {
        public bool Enabled { get; init; }
        public int NonMacroBlock_MaxSize { get; init; }
        public Severity NonMacroBlock_Severity { get; init; }

        public int File_MaxSize { get; init; }
        public Severity File_Severity { get; init; }

        public int Macro_MaxSize { get; init; }
        public Severity Macro_Severity { get; init; }

        public override string ToString() => this.GenericToString();
    }
}

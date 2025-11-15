using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CK3Analyser.Core.Resources.DetectorSettings
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum IndentationType
    {
        Tab,
        [JsonStringEnumMemberName("Two Spaces")]
        TwoSpaces,
        [JsonStringEnumMemberName("Three Spaces")]
        ThreeSpaces,
        [JsonStringEnumMemberName("Four Spaces")]
        FourSpaces, 
        Inconclusive
    }

    public enum CommentHandling
    {
        CommentedBracketsCount,
        NoSpecialTreatment,
        CommentsIgnored
    }
    public readonly struct InconsistentIndentationSettings
    {
        public bool Enabled { get; init; }
        public HashSet<IndentationType> AllowedIndentationTypes { get; init; }
        public Severity UnexpectedType_Severity { get; init; }
        public CommentHandling CommentHandling { get; init; }
        public Severity AbberatingLines_Severity { get; init; }

        public override string ToString() => this.GenericToString();
    }
}

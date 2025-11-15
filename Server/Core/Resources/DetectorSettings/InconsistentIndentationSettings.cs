using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CK3Analyser.Core.Resources.DetectorSettings
{
    public class IndentationTypeJsonConverter : JsonConverter<IndentationType>
    {
        public override IndentationType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var read = reader.GetString();
            return read switch
            {
                "Tab" => IndentationType.Tab,
                "Two Spaces" => IndentationType.TwoSpaces,
                "Three Spaces" => IndentationType.ThreeSpaces,
                "Four Spaces" => IndentationType.FourSpaces,
                _ => IndentationType.Inconclusive,
            };
        }

        public override void Write(Utf8JsonWriter writer, IndentationType value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
    }

    [JsonConverter(typeof(IndentationTypeJsonConverter))]
    public enum IndentationType
    {
        Tab, TwoSpaces, ThreeSpaces, FourSpaces, Inconclusive
    }
    public readonly struct InconsistentIndentationSettings
    {
        public bool Enabled { get; init; }
        public HashSet<IndentationType> AllowedIndentationTypes { get; init; }
        public Severity UnexpectedType_Severity { get; init; }
        public bool AccountCommentedBrackets { get; init; }
        public Severity AbberatingLines_Severity { get; init; }

        public override string ToString() => this.GenericToString();
    }
}

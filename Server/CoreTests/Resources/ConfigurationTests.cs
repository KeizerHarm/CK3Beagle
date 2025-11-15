using CK3Analyser.Core.Resources.DetectorSettings;
using System.Text.Json;

namespace CK3Analyser.Core.Resources
{
    public class ConfigurationTests
    {
        [Fact]
        public void ParsesSettings()
        {
			var inputJson = @"{
	""largeUnit"": {
		""enabled"": true,
		""file"": {
			""maxSize"": 10000,
			""severity"": 1
		},
		""macro"": {
			""maxSize"": 75,
			""severity"": 2
		},
		""nonMacroBlock"": {
			""maxSize"": 50,
			""severity"": 3
		}
	}
}";
			var input = JsonDocument.Parse(inputJson);

			var expectedSettings = new LargeUnitSettings
			{
				Enabled = true,
				File_MaxSize = 10000,
				File_Severity = Severity.Info,
				Macro_MaxSize = 75,
				Macro_Severity = Severity.Warning,
				NonMacroBlock_MaxSize = 50,
				NonMacroBlock_Severity = Severity.Critical
			};

			var config = new Configuration(input.RootElement);
			var actualSettings = config.LargeUnitSettings;

			Assert.Equal(expectedSettings, actualSettings);
        }
		[Fact]
        public void ParsesEnumSettings()
        {
			var inputJson = @"{""inconsistentIndentation"":{""enabled"":true,""accountCommentedBrackets"":true,""allowedIndentationTypes"":[""Tab"",""Two Spaces""],""unexpectedType"":{""severity"":2},""abberatingLines"":{""severity"":2}}}";
			var input = JsonDocument.Parse(inputJson);

			var expectedSettings = new InconsistentIndentationSettings
			{
				Enabled = true,
				AccountCommentedBrackets = true,
				UnexpectedType_Severity = Severity.Warning,
				AbberatingLines_Severity = Severity.Warning,
				AllowedIndentationTypes = [IndentationType.Tab, IndentationType.TwoSpaces]
			};

			var config = new Configuration(input.RootElement);
			var actualSettings = config.InconsistentIndentationSettings;

			Assert.Equal(expectedSettings.Enabled, actualSettings.Enabled);
			Assert.Equal(expectedSettings.AccountCommentedBrackets, actualSettings.AccountCommentedBrackets);
			Assert.Equal(expectedSettings.UnexpectedType_Severity, actualSettings.UnexpectedType_Severity);
			Assert.Equal(expectedSettings.AbberatingLines_Severity, actualSettings.AbberatingLines_Severity);
			Assert.Equal(expectedSettings.AllowedIndentationTypes, actualSettings.AllowedIndentationTypes);
        }

    }
}

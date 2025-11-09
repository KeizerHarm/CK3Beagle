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

    }
}

using CK3Analyser.Analysing.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;
using CK3Analyser.Core.Resources;
using CK3Analyser.Core.Resources.DetectorSettings;
using CK3Analyser.Core.Generated;

namespace CK3Analyser.Analysing.Common.Detectors
{
    public class LargeUnitDetectorTests : BaseDetectorTest
    {
        [Theory]
        [InlineData(1, 3)]
        [InlineData(5, 2)]
        [InlineData(10, 1)]
        [InlineData(15, 0)]
        public void DetectsLargeNonMacroBlocks(int maxSize, int numberOfExpectedErrors)
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("LargeUnit/LargeUnit_NonMacroBlock", DeclarationType.Event);
            var visitor = GetDetector(logger, testcase.Context, NonMacroBlock_severity: Severity.Critical, NonMacroBlock_maxSize: maxSize);

            //act
            visitor.Visit(testcase);

            //assert
            Assert.Equal(numberOfExpectedErrors, logger.LogEntries.Where(x => x.Smell == Smell.LargeUnit_NonMacroBlock).Count());
        }

        [Theory]
        [InlineData(1, 3)]
        [InlineData(5, 2)]
        [InlineData(10, 1)]
        [InlineData(15, 0)]
        public void DetectsLargeMacro(int maxSize, int numberOfExpectedErrors)
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("LargeUnit/LargeUnit_Macro", DeclarationType.ScriptedTrigger);
            var visitor = GetDetector(logger, testcase.Context, Macro_severity: Severity.Critical, Macro_maxSize: maxSize);

            //act
            visitor.Visit(testcase);

            //assert
            Assert.Equal(numberOfExpectedErrors, logger.LogEntries.Where(x => x.Smell == Smell.LargeUnit_Macro).Count());
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(5, 0)]
        [InlineData(10, 0)]
        [InlineData(15, 0)]
        public void DetectsLargeMacro_IgnoresNonMacroType(int maxSize, int numberOfExpectedErrors)
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase("LargeUnit/LargeUnit_Macro", DeclarationType.Debug);
            var visitor = GetDetector(logger, testcase.Context, Macro_severity: Severity.Critical, Macro_maxSize: maxSize);

            //act
            visitor.Visit(testcase);

            //assert
            Assert.Equal(numberOfExpectedErrors, logger.LogEntries.Where(x => x.Smell == Smell.LargeUnit_Macro).Count());
        }


        [Theory]
        [InlineData("LargeUnit/LargeFile_1Line", 40, false)]
        [InlineData("LargeUnit/LargeFile_40Lines", 40, true)]
        [InlineData("LargeUnit/LargeFile_41Lines", 40, true)]
        [InlineData("LargeUnit/LargeFile_10000Lines", 40, true)]
        [InlineData("LargeUnit/LargeFile_1Line", 41, false)]
        [InlineData("LargeUnit/LargeFile_40Lines", 41, false)]
        [InlineData("LargeUnit/LargeFile_41Lines", 41, true)]
        [InlineData("LargeUnit/LargeFile_10000Lines", 41, true)]
        public void DetectsLargeFile(string file, int maxSize, bool shouldError)
        {
            //arrange
            var logger = new Logger();
            ScriptFile testcase = GetTestCase(file);
            var visitor = GetDetector(logger, testcase.Context, File_severity: Severity.Critical, File_maxSize: maxSize);


            //act
            visitor.Visit(testcase);

            //assert
            if (shouldError)
            {
                Assert.Single(logger.LogEntries);
                Assert.Single(logger.LogEntries, x => x.Smell == Smell.LargeUnit_File);
            }
            else
            {
                Assert.Empty(logger.LogEntries);
            }
        }


        //[Theory]
        //[InlineData("LargeUnit/LargeFile_SmallUnits", 5, 0)]
        //[InlineData("LargeUnit/LargeUnits", 5, 2)]
        //[InlineData("LargeUnit/LargeFile_SmallUnits", 15, 0)]
        //[InlineData("LargeUnit/LargeUnits", 15, 1)]
        //[InlineData("LargeUnit/LargeFile_SmallUnits", 25, 0)]
        //[InlineData("LargeUnit/LargeUnits", 25, 0)]
        //public void DetectsLargeFile(string file, int maxSize, int numberOfExpectedErrors)
        //{
        //    //arrange
        //    var logger = new Logger();
        //    ScriptFile testcase = GetTestCase(file);
        //    var visitor = GetDetector(logger, testcase.Context, severity: Severity.Critical, maxSize: maxSize);


        //    //act
        //    visitor.Visit(testcase);

        //    //assert
        //    Assert.Equal(numberOfExpectedErrors, logger.LogEntries.Where(x => x.Smell == Smell.LargeUnit_File).Count());
        //}

        private static AnalysisVisitor GetDetector(
            Logger logger,
            Context context,
            Severity NonMacroBlock_severity = Severity.Warning,
            int NonMacroBlock_maxSize = 40,
            Severity Macro_severity = Severity.Warning,
            int Macro_maxSize = 40,
            Severity File_severity = Severity.Warning,
            int File_maxSize = 10000
            )
        {
            var settings = new LargeUnitSettings
            {
                NonMacroBlock_Severity = NonMacroBlock_severity,
                NonMacroBlock_MaxSize = NonMacroBlock_maxSize,
                File_Severity = File_severity,
                File_MaxSize = File_maxSize,
                Macro_Severity = Macro_severity,
                Macro_MaxSize = Macro_maxSize
            };

            var visitor = new AnalysisVisitor();
            var detector = new LargeUnitDetector(logger, context, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}

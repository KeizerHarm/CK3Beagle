using CK3Analyser.Analysis;
using CK3Analyser.Analysis.Detectors;
using CK3Analyser.Analysis.Logging;
using CK3Analyser.Core.Domain;
using CK3Analyser.Core.Domain.Entities;

namespace CK3Analyser.Analysing
{
    public class LargeUnitDetectorTests : BaseTest
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
            ScriptFile testcase = GetTestCase("LargeUnit/LargeUnit_NonMacroBlock");
            var visitor = GetDetector(logger, testcase.Context, severity_NonMacroBlock: Severity.Critical, maxSize_NonMacroBlock: maxSize);

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
            var visitor = GetDetector(logger, testcase.Context, severity_Macro: Severity.Critical, maxSize_Macro: maxSize);

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
            var visitor = GetDetector(logger, testcase.Context, severity_Macro: Severity.Critical, maxSize_Macro: maxSize);

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
            var visitor = GetDetector(logger, testcase.Context, severity_File: Severity.Critical, maxSize_File: maxSize);


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
            Severity severity_NonMacroBlock = Severity.Warning,
            int maxSize_NonMacroBlock = 40,
            Severity severity_Macro = Severity.Warning,
            int maxSize_Macro = 40,
            Severity severity_File = Severity.Warning,
            int maxSize_File = 10000
            )
        {
            var settings = new LargeUnitDetector.Settings
            {
                Severity_NonMacroBlock = severity_NonMacroBlock,
                MaxSize_NonMacroBlock = maxSize_NonMacroBlock,
                Severity_File = severity_File,
                MaxSize_File = maxSize_File,
                Severity_Macro = severity_Macro,
                MaxSize_Macro = maxSize_Macro
            };

            var visitor = new AnalysisVisitor();
            var detector = new LargeUnitDetector(logger, context, settings);
            visitor.Detectors.Add(detector);
            return visitor;
        }
    }
}

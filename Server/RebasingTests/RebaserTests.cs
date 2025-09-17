//using CK3Analyser.Parser.Domain;
//using CK3Analyser.Parser.Parsing.Fast;
//using CK3Analyser.Parser.Rebasing;
//using CK3AnalyserAntlr.Parser;

//namespace CK3Analyser.Parser
//{
//    public class RebaserTests
//    {
//        private static void GatherTestCase(string caseName, out Context oldContext, out Context newContext, out Context moddedContext, out Context expectedRebased)
//        {
//            var root = Path.Combine(AppContext.BaseDirectory, "Testcases", caseName);
//            var fastParser = new FastParser();
//            oldContext = new Context(Path.Combine(root, "Old"), ContextType.Old);
//            Program.GatherDeclarations(fastParser, oldContext);
//            newContext = new Context(Path.Combine(root, "New"), ContextType.New);
//            Program.GatherDeclarations(fastParser, newContext);
//            moddedContext = new Context(Path.Combine(root, "Modded"), ContextType.Modded);
//            Program.GatherDeclarations(fastParser, moddedContext);
//            expectedRebased = new Context(Path.Combine(root, "Rebased"), ContextType.ModdedRebased);
//            Program.GatherDeclarations(fastParser, expectedRebased);
//        }

//        private static void AssertContextsEqual(Context expected, Context actual)
//        {
//            Assert.Equal(expected.Files.Count, actual.Files.Count);

//            foreach (var expectedFile in expected.Files)
//            {
//                var actualFile = actual.Files[expectedFile.Key];
//                Assert.NotNull(actualFile);

//                Assert.Equal(expectedFile.Value.Raw, actualFile.Raw);
//            }
//        }


//        [Theory]
//        [InlineData("Rebasing/NoChanges")]
//        [InlineData("Rebasing/VanillaChange")]
//        [InlineData("Rebasing/VanillaChange_ModdedChange")]
//        [InlineData("Rebasing/VanillaChange_ModdedAddition")]
//        [InlineData("Rebasing/VanillaAddition")]
//        [InlineData("Rebasing/VanillaAddition_ModdedChange")]
//        [InlineData("Rebasing/VanillaAddition_ModdedAddition")]
//        public void RunTestcases(string testcase)
//        {
//            //arrange
//            Context oldContext, newContext, moddedContext, expectedRebased;
//            GatherTestCase(testcase, out oldContext, out newContext, out moddedContext, out expectedRebased);

//            var rebaser = new Rebaser();

//            //act
//            var rebased = rebaser.DoRebase(oldContext, newContext, moddedContext);

//            //assert
//            AssertContextsEqual(expectedRebased, rebased);
//        }
//    }
//}

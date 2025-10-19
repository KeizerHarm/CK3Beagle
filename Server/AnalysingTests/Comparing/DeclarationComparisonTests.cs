namespace CK3Analyser.Analysing.Comparing
{
    public class DeclarationComparisonTests : BaseComparisonTest
    {
        [Fact]
        public void TryMatch()
        {
            //arrange
            (var old, var @new) = GetTestCase("Basic");

            //var comparison = new DeclarationComparison().MatchSubtrees(old, @new);
        }
    }
}

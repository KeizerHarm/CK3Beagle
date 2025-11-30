using CK3BeagleServer.Core.Comparing.Building;

namespace CK3BeagleServer.Core.Comparing
{
    public class LcsCalculatorTests
    {
        [Fact]
        public void ShortLcsCorrect()
        {
            //arrange
            var a = "human";
            var b = "chimpanzee";

            //act
            var lcs = LcsCalculator.ComputeLcs(a, b, EqualityComparer<char>.Default);

            //assert
            Assert.Equal("hman".ToCharArray(), lcs);
        }

        [Fact]
        public void LongLcsCorrect()
        {
            //arrange
            var a = "In computer science, Hirschberg's algorithm, named after its inventor, Dan Hirschberg, is a dynamic programming algorithm that finds the optimal sequence alignment between two strings. Optimality is measured with the Levenshtein distance, defined to be the sum of the costs of insertions, replacements, deletions, and null actions needed to change one string into the other.";
            var b = "Hirschberg's algorithm is simply described as a more space-efficient version of the Needleman-Wunsch algorithm that uses dynamic programming. Hirschberg's algorithm is commonly used in computational biology to find maximal global alignments of DNA and protein sequences.";

            //act
            var lcs = LcsCalculator.ComputeLcs(a, b, EqualityComparer<char>.Default);

            //assert
            Assert.Equal(138, lcs.Count());
        }
    }
}

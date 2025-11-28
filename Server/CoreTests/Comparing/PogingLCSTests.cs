using CK3Analyser.Core.Comparing.Domain;
using CK3Analyser.Core.Comparing.Building;

namespace CK3Analyser.Core.Comparing
{
    public class PogingLCSTests : BaseComparisonTest
    {
        [Fact]
        public void NoChanges()
        {
            //arrange
            (var old, var @new) = GetTestCase("NoChanges");

            //act
            var comparison = new FileComparisonBuilder().BuildFileComparison(old, @new);

            //assert
            Assert.Equal(DeltaKind.Unchanged, comparison.Kind);
            Assert.Null(comparison.Children);
        }

        [Fact]
        public void SingleUpdatedValue()
        {
            //arrange
            (var old, var @new) = GetTestCase("Update/SingleValue");

            //act
            var comparison = new FileComparisonBuilder().BuildFileComparison(old, @new);

            //assert
            Assert.Single(comparison.Children);
        }

        [Fact]
        public void ChangedScoper()
        {
            //arrange
            (var old, var @new) = GetTestCase("Update/ChangedScoper");

            //act
            var comparison = new FileComparisonBuilder().BuildFileComparison(old, @new);

            //assert
            Assert.Single(comparison.Children);
        }

        [Fact]
        public void SingleDeletedLeaf()
        {
            //arrange
            (var old, var @new) = GetTestCase("Delete/SingleLeaf");

            //act
            var comparison = new FileComparisonBuilder().BuildFileComparison(old, @new);

            //assert
            Assert.Single(comparison.Children);
        }

        [Fact]
        public void LongFile()
        {
            //arrange
            (var old, var @new) = GetTestCase("Insert/LongFileManyInserts");

            //act
            var comparison = new FileComparisonBuilder().BuildFileComparison(old, @new);

            //assert
            Assert.Equal(7, comparison.Children.Count);
        }
    }
}

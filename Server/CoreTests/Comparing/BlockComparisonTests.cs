using CK3Analyser.Core.Comparing.Domain;
using CK3Analyser.Core.Domain.Entities;

namespace CK3Analyser.Core.Comparing
{
    public class BlockComparisonTests : BaseComparisonTest
    {
        [Fact]
        public void NoChanges()
        {
            //arrange
            (var old, var @new) = GetTestCase("NoChanges");

            //act
            var comparison = new FileComparison(old, @new);

            //assert
            Assert.Empty(comparison.EditScript);
        }

        [Fact]
        public void SingleUpdatedValue()
        {
            //arrange
            (var old, var @new) = GetTestCase("Update/SingleValue");

            //act
            var comparison = new FileComparison(old, @new);

            //assert
            Assert.Single(comparison.EditScript);
            Assert.IsType<UpdateOperation>(comparison.EditScript.First());
            var updateOpderation = comparison.EditScript.OfType<UpdateOperation>().First();

            Assert.Equal("20", updateOpderation.NewValue);
            Assert.Equal("value", ((BinaryExpression)updateOpderation.UpdatedNode).Key);
        }

        [Fact]
        public void ThreeUpdatedValues()
        {
            //arrange
            (var old, var @new) = GetTestCase("Update/ThreeValues");

            //act
            var comparison = new FileComparison(old, @new);

            //assert
            Assert.Equal(3, comparison.EditScript.Count);
            Assert.Equal(3, comparison.EditScript.OfType<UpdateOperation>().Count());


            var update1 = comparison.EditScript.OfType<UpdateOperation>().ElementAt(0);
            Assert.Equal("ambitious", update1.NewValue);
            Assert.Equal("has_trait", ((BinaryExpression)update1.UpdatedNode).Key);

            var update2 = comparison.EditScript.OfType<UpdateOperation>().ElementAt(1);
            Assert.Equal("ambitious", update2.NewValue);
            Assert.Equal("trait", ((BinaryExpression)update2.UpdatedNode).Key);

            var update3 = comparison.EditScript.OfType<UpdateOperation>().ElementAt(2);
            Assert.Equal("20", update3.NewValue);
            Assert.Equal("value", ((BinaryExpression)update3.UpdatedNode).Key);
        }
    }
}

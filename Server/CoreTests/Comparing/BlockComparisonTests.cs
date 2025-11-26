using CK3Analyser.Core.Comparing.Domain;
using CK3Analyser.Core.Domain.Entities;

namespace CK3Analyser.Core.Comparing
{
    public class BlockComparisonTests : BaseComparisonTest
    {
        [Fact]
        public void TryMatch()
        {
            //arrange
            (var old, var @new) = GetTestCase("UpdatedValue");

            //act
            var comparison = new FileComparison(old, @new);

            //assert
            Assert.Single(comparison.EditScript);
            Assert.IsType<UpdateOperation>(comparison.EditScript.First());
            var updateOpderation = comparison.EditScript.OfType<UpdateOperation>().First();

            Assert.Equal("20", updateOpderation.NewValue);
            Assert.Equal("value", ((BinaryExpression)updateOpderation.UpdatedNode).Key);
        }
    }
}

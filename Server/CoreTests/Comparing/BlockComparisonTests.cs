//using CK3Analyser.Core.Comparing.Domain;
//using CK3Analyser.Core.Comparing.PreviousAttempts.Domain;
//using CK3Analyser.Core.Domain.Entities;

//namespace CK3Analyser.Core.Comparing
//{
//    public class BlockComparisonTests : BaseComparisonTest
//    {
//        [Fact]
//        public void NoChanges()
//        {
//            //arrange
//            (var old, var @new) = GetTestCase("NoChanges");

//            //act
//            var comparison = new FileComparison(old, @new);

//            //assert
//            Assert.Empty(comparison.EditScript);
//        }

//        [Fact]
//        public void SingleUpdatedValue()
//        {
//            //arrange
//            (var old, var @new) = GetTestCase("Update/SingleValue");

//            //act
//            var comparison = new FileComparison(old, @new);

//            //assert
//            Assert.Single(comparison.EditScript);
//            Assert.IsType<UpdateOperation>(comparison.EditScript.First());
//            var updateOperation = comparison.EditScript.OfType<UpdateOperation>().First();

//            Assert.Equal("value = 20", updateOperation.NewValue);
//            Assert.Equal("value", ((BinaryExpression)updateOperation.UpdatedNode).Key);
//        }

//        [Fact]
//        public void ThreeUpdatedValues()
//        {
//            //arrange
//            (var old, var @new) = GetTestCase("Update/ThreeValues");

//            //act
//            var comparison = new FileComparison(old, @new);

//            //assert
//            Assert.Equal(3, comparison.EditScript.Count);
//            Assert.Equal(3, comparison.EditScript.OfType<UpdateOperation>().Count());

//            var update1 = comparison.EditScript.OfType<UpdateOperation>().ElementAt(0);
//            Assert.Equal("has_trait = ambitious", update1.NewValue);
//            Assert.Equal("has_trait", ((BinaryExpression)update1.UpdatedNode).Key);

//            var update2 = comparison.EditScript.OfType<UpdateOperation>().ElementAt(1);
//            Assert.Equal("trait = ambitious", update2.NewValue);
//            Assert.Equal("trait", ((BinaryExpression)update2.UpdatedNode).Key);

//            var update3 = comparison.EditScript.OfType<UpdateOperation>().ElementAt(2);
//            Assert.Equal("value = 20", update3.NewValue);
//            Assert.Equal("value", ((BinaryExpression)update3.UpdatedNode).Key);
//        }

//        [Fact]
//        public void ChangedScoper()
//        {
//            //arrange
//            (var old, var @new) = GetTestCase("Update/ChangedScoper");

//            //act
//            var comparison = new FileComparison(old, @new);

//            //assert
//            Assert.Single(comparison.EditScript);
//            Assert.IsType<UpdateOperation>(comparison.EditScript.First());
//            var updateOperation = comparison.EditScript.OfType<UpdateOperation>().First();

//            Assert.Equal("prev ?=", updateOperation.NewValue);
//            Assert.Equal("prev", ((NamedBlock)updateOperation.UpdatedNode).Key);
//        }

//        [Fact]
//        public void SingleInsertedLeaf()
//        {
//            //arrange
//            (var old, var @new) = GetTestCase("Insert/SingleLeaf");

//            //act
//            var comparison = new FileComparison(old, @new);

//            //assert
//            Assert.Single(comparison.EditScript);
//            Assert.IsType<InsertOperation>(comparison.EditScript.First());
//            var insertOperation = comparison.EditScript.OfType<InsertOperation>().First();

//            Assert.Equal("Test", ((Comment)insertOperation.InsertedNode).RawWithoutHashtag);
//            Assert.Equal("nerge_check_commander_trait_xp_effect", ((Declaration)insertOperation.NewParent).Key);
//        }

//        [Fact]
//        public void ThreeInsertedLeaves()
//        {
//            //arrange
//            (var old, var @new) = GetTestCase("Insert/ThreeLeaves");

//            //act
//            var comparison = new FileComparison(old, @new);

//            //assert
//            Assert.Equal(3, comparison.EditScript.Count);
//            Assert.Equal(3, comparison.EditScript.OfType<InsertOperation>().Count());

//            var insertOperation1 = comparison.EditScript.OfType<InsertOperation>().ElementAt(0);
//            Assert.Equal("Test", ((Comment)insertOperation1.InsertedNode).RawWithoutHashtag);
//            Assert.Equal("nerge_check_commander_trait_xp_effect", ((Declaration)insertOperation1.NewParent).Key);

//            var insertOperation2 = comparison.EditScript.OfType<InsertOperation>().ElementAt(1);
//            Assert.Equal("Test 2", ((Comment)insertOperation2.InsertedNode).RawWithoutHashtag);
//            Assert.Equal("if", ((NamedBlock)insertOperation2.NewParent).Key);

//            var insertOperation3 = comparison.EditScript.OfType<InsertOperation>().ElementAt(2);
//            Assert.Equal("NOT", ((NamedBlock)insertOperation3.InsertedNode).Key);
//            Assert.Equal("limit", ((NamedBlock)insertOperation3.NewParent).Key);
//        }

//        [Fact]
//        public void SingleInsertedBlock()
//        {
//            //arrange
//            (var old, var @new) = GetTestCase("Insert/SingleBlock");

//            //act
//            var comparison = new FileComparison(old, @new);

//            //assert
//            Assert.Single(comparison.EditScript);
//            Assert.IsType<InsertOperation>(comparison.EditScript.First());
//            var insertOperation = comparison.EditScript.OfType<InsertOperation>().First();

//            Assert.Equal("root", ((NamedBlock)insertOperation.InsertedNode).Key);
//            Assert.Equal("prev", ((NamedBlock)insertOperation.NewParent).Key);
//        }

//        [Fact]
//        public void LongFile()
//        {
//            //arrange
//            (var old, var @new) = GetTestCase("Insert/LongFileManyInserts");

//            //act
//            var comparison = new FileComparison(old, @new);

//            //assert
//            var insertOperations = comparison.EditScript.OfType<InsertOperation>();
//            Assert.Equal(comparison.EditScript.Count, insertOperations.Count());
//        }


//        //[Fact]
//        //public void SingleDeletedLeaf()
//        //{
//        //    //arrange
//        //    (var old, var @new) = GetTestCase("Delete/SingleLeaf");

//        //    //act
//        //    var comparison = new FileComparison(old, @new);

//        //    //assert
//        //    Assert.Single(comparison.EditScript);
//        //    Assert.IsType<DeleteOperation>(comparison.EditScript.First());
//        //    var DeleteOperation = comparison.EditScript.OfType<DeleteOperation>().First();

//        //    Assert.Equal("value = 10", DeleteOperation.DeletedNode.StringRepresentation);
//        //}

//        //[Fact]
//        //public void ThreeDeletedLeaves()
//        //{
//        //    //arrange
//        //    (var old, var @new) = GetTestCase("Delete/ThreeLeaves");

//        //    //act
//        //    var comparison = new FileComparison(old, @new);

//        //    //assert
//        //    Assert.Equal(3, comparison.EditScript.Count);
//        //    Assert.Equal(3, comparison.EditScript.OfType<DeleteOperation>().Count());

//        //    var DeleteOperation1 = comparison.EditScript.OfType<DeleteOperation>().ElementAt(0);
//        //    Assert.Equal("Test", DeleteOperation1.DeletedNode.StringRepresentation);

//        //    var DeleteOperation2 = comparison.EditScript.OfType<DeleteOperation>().ElementAt(1);
//        //    Assert.Equal("Test 2", DeleteOperation2.DeletedNode.StringRepresentation);

//        //    var DeleteOperation3 = comparison.EditScript.OfType<DeleteOperation>().ElementAt(2);
//        //    Assert.Equal("NOT", DeleteOperation3.DeletedNode.Key);
//        //}

//        //[Fact]
//        //public void SingleDeletedBlock()
//        //{
//        //    //arrange
//        //    (var old, var @new) = GetTestCase("Delete/SingleBlock");

//        //    //act
//        //    var comparison = new FileComparison(old, @new);

//        //    //assert
//        //    Assert.Single(comparison.EditScript);
//        //    Assert.IsType<DeleteOperation>(comparison.EditScript.First());
//        //    var DeleteOperation = comparison.EditScript.OfType<DeleteOperation>().First();

//        //    Assert.Equal("root", DeleteOperation.DeletedNode.Key);
//        //}
//    }
//}

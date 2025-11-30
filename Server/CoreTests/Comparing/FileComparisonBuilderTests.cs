using CK3BeagleServer.Core.Comparing.Building;
using CK3BeagleServer.Core.Comparing.Domain;

namespace CK3BeagleServer.Core.Comparing
{
    public class FileComparisonBuilderTests : BaseComparisonTest
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
            var expectedDelta =
                GetDelta(DeltaKind.ChangedInChildren,
                    GetDelta(DeltaKind.ChangedInChildren,
                        GetDelta(DeltaKind.ChangedInChildren,
                            GetDelta(DeltaKind.ChangedInChildren,
                                GetDelta(DeltaKind.ChangedInChildren,
                                    GetDelta(DeltaKind.Changed))))));

            //act
            var comparison = new FileComparisonBuilder().BuildFileComparison(old, @new);

            //assert
            AssertDeltasEqual(expectedDelta, comparison);
        }

        [Fact]
        public void ThreeUpdatedValues()
        {
            //arrange
            (var old, var @new) = GetTestCase("Update/ThreeValues");
            var expectedDelta =
                GetDelta(DeltaKind.ChangedInChildren,
                    GetDelta(DeltaKind.ChangedInChildren,
                        GetDelta(DeltaKind.ChangedInChildren,
                            GetDelta(DeltaKind.ChangedInChildren,
                                GetDelta(DeltaKind.ChangedInChildren,
                                    GetDelta(DeltaKind.Changed)),
                                GetDelta(DeltaKind.ChangedInChildren,
                                    GetDelta(DeltaKind.Changed),
                                    GetDelta(DeltaKind.Changed))))));

            //act
            var comparison = new FileComparisonBuilder().BuildFileComparison(old, @new);

            //assert
            AssertDeltasEqual(expectedDelta, comparison);
        }


        [Fact]
        public void ChangedScoper()
        {
            //arrange
            (var old, var @new) = GetTestCase("Update/ChangedScoper");
            var expectedDelta = GetDelta(DeltaKind.ChangedInChildren,
                GetDelta(DeltaKind.ChangedInChildren,
                    GetDelta(DeltaKind.Changed)));

            //act
            var comparison = new FileComparisonBuilder().BuildFileComparison(old, @new);

            //assert
            AssertDeltasEqual(expectedDelta, comparison);
        }

        [Fact]
        public void SingleInsertedLeaf()
        {
            //arrange
            (var old, var @new) = GetTestCase("Insert/SingleLeaf");
            var expectedDelta = GetDelta(DeltaKind.ChangedInChildren,
                GetDelta(DeltaKind.ChangedInChildren,
                    GetDelta(DeltaKind.Added)));

            //act
            var comparison = new FileComparisonBuilder().BuildFileComparison(old, @new);

            //assert
            AssertDeltasEqual(expectedDelta, comparison);
        }

        [Fact]
        public void SingleDeletedLeaf()
        {
            //arrange
            (var old, var @new) = GetTestCase("Delete/SingleLeaf");
            var expectedDelta =
                GetDelta(DeltaKind.ChangedInChildren,
                    GetDelta(DeltaKind.ChangedInChildren,
                        GetDelta(DeltaKind.ChangedInChildren,
                            GetDelta(DeltaKind.ChangedInChildren,
                                GetDelta(DeltaKind.ChangedInChildren,
                                    GetDelta(DeltaKind.Deleted))))));

            //act
            var comparison = new FileComparisonBuilder().BuildFileComparison(old, @new);

            //assert
            AssertDeltasEqual(expectedDelta, comparison);
        }

        [Fact]
        public void ThreeDeletedLeaves()
        {
            //arrange
            (var old, var @new) = GetTestCase("Delete/ThreeLeaves");
            var expectedDelta =
                GetDelta(DeltaKind.ChangedInChildren,
                    GetDelta(DeltaKind.ChangedInChildren,
                        GetDelta(DeltaKind.ChangedInChildren,
                            GetDelta(DeltaKind.ChangedInChildren,
                                GetDelta(DeltaKind.ChangedInChildren,
                                    GetDelta(DeltaKind.Deleted)),
                                GetDelta(DeltaKind.ChangedInChildren,
                                    GetDelta(DeltaKind.Deleted),
                                    GetDelta(DeltaKind.Deleted))))));

            //act
            var comparison = new FileComparisonBuilder().BuildFileComparison(old, @new);

            //assert
            AssertDeltasEqual(expectedDelta, comparison);
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

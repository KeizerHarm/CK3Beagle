using CK3BeagleServer.Core.Comparing.Building;
using CK3BeagleServer.Core.Comparing.Domain;

namespace CK3BeagleServer.Core.Comparing
{
    public class FileComparisonBuilderTests : BaseComparisonTest
    {
        public class Changes : FileComparisonBuilderTests
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
        }

        public class Inserts : FileComparisonBuilderTests
        {

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

                var child1 = comparison.Children[0];
                var expectedChild1 =
                    GetDelta(DeltaKind.ChangedInChildren,
                        GetDelta(DeltaKind.ChangedInChildren,
                            GetDelta(DeltaKind.ChangedInChildren,
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added))));

                AssertDeltasEqual(expectedChild1, child1);

                var child2 = comparison.Children[1];
                var expectedChild2 =
                    GetDelta(DeltaKind.ChangedInChildren,
                        GetDelta(DeltaKind.ChangedInChildren,
                            GetDelta(DeltaKind.ChangedInChildren,
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added))));

                AssertDeltasEqual(expectedChild2, child2);

                var child3 = comparison.Children[2];
                var expectedChild3 =
                    GetDelta(DeltaKind.ChangedInChildren,
                        GetDelta(DeltaKind.ChangedInChildren,
                            GetDelta(DeltaKind.ChangedInChildren,
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added))));

                AssertDeltasEqual(expectedChild3, child3);

                var child4 = comparison.Children[3];
                var expectedChild4 =
                    GetDelta(DeltaKind.ChangedInChildren,
                        GetDelta(DeltaKind.ChangedInChildren,
                            GetDelta(DeltaKind.ChangedInChildren,
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added))));

                AssertDeltasEqual(expectedChild4, child4);

                var child5 = comparison.Children[4];
                var expectedChild5 = GetDelta(DeltaKind.Added);
                AssertDeltasEqual(expectedChild5, child5);

                var child6 = comparison.Children[5];
                var expectedChild6 = GetDelta(DeltaKind.Added);
                AssertDeltasEqual(expectedChild6, child6);

                var child7 = comparison.Children[6];
                var expectedChild7 = GetDelta(DeltaKind.Added);
                AssertDeltasEqual(expectedChild7, child7);
            }
        }

        public class Deletions : FileComparisonBuilderTests
        {
            [Fact]
            public void SingleDeletedValue()
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
            public void ThreeDeletedValues()
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
            public void SingleDeletedBlock()
            {
                //arrange
                (var old, var @new) = GetTestCase("Delete/SingleBlock");
                var expectedDelta =
                    GetDelta(DeltaKind.ChangedInChildren,
                        GetDelta(DeltaKind.ChangedInChildren,
                            GetDelta(DeltaKind.ChangedInChildren,
                                GetDelta(DeltaKind.ChangedInChildren,
                                    GetDelta(DeltaKind.Deleted)))));

                //act
                var comparison = new FileComparisonBuilder().BuildFileComparison(old, @new);

                //assert
                AssertDeltasEqual(expectedDelta, comparison);
            }
        }

        public class ExtraTests : FileComparisonBuilderTests
        {
            [Fact]
            public void CharacterInteraction()
            {
                //arrange
                (var old, var @new) = GetTestCase("ExtraTests/CharacterInteraction");
                var expectedDelta =
                    GetDelta(DeltaKind.ChangedInChildren,
                        GetDelta(DeltaKind.ChangedInChildren,
                            GetDelta(DeltaKind.ChangedInChildren,
                                GetDelta(DeltaKind.ChangedInChildren,
                                    GetDelta(DeltaKind.ChangedInChildren,
                                        GetDelta(DeltaKind.ChangedInChildren,
                                            GetDelta(DeltaKind.Deleted),
                                            GetDelta(DeltaKind.Added))))),
                            GetDelta(DeltaKind.ChangedInChildren,
                                GetDelta(DeltaKind.Deleted),
                                GetDelta(DeltaKind.Added))));

                //act
                var comparison = new FileComparisonBuilder().BuildFileComparison(old, @new);

                //assert
                AssertDeltasEqual(expectedDelta, comparison);
            }

            [Fact]
            public void Event()
            {
                //arrange
                (var old, var @new) = GetTestCase("ExtraTests/Event");
                var expectedDelta =
                    GetDelta(DeltaKind.ChangedInChildren,
                        GetDelta(DeltaKind.ChangedInChildren,
                            GetDelta(DeltaKind.ChangedInChildren,
                                GetDelta(DeltaKind.ChangedInChildren,
                                    GetDelta(DeltaKind.Added),
                                    GetDelta(DeltaKind.Added),
                                    GetDelta(DeltaKind.Added)),
                                GetDelta(DeltaKind.ChangedInChildren,
                                    GetDelta(DeltaKind.Added),
                                    GetDelta(DeltaKind.Added),
                                    GetDelta(DeltaKind.Added)),
                                GetDelta(DeltaKind.ChangedInChildren,
                                    GetDelta(DeltaKind.Added),
                                    GetDelta(DeltaKind.Added),
                                    GetDelta(DeltaKind.Added)),
                                GetDelta(DeltaKind.ChangedInChildren,
                                    GetDelta(DeltaKind.Added),
                                    GetDelta(DeltaKind.Added),
                                    GetDelta(DeltaKind.Added)),
                                GetDelta(DeltaKind.ChangedInChildren,
                                    GetDelta(DeltaKind.Added),
                                    GetDelta(DeltaKind.Added),
                                    GetDelta(DeltaKind.Added)),
                                GetDelta(DeltaKind.ChangedInChildren,
                                    GetDelta(DeltaKind.Added),
                                    GetDelta(DeltaKind.Added),
                                    GetDelta(DeltaKind.Added))
                                )),
                        GetDelta(DeltaKind.ChangedInChildren,
                            GetDelta(DeltaKind.ChangedInChildren,
                                GetDelta(DeltaKind.ChangedInChildren,
                                    GetDelta(DeltaKind.ChangedInChildren,
                                        GetDelta(DeltaKind.Deleted),
                                        GetDelta(DeltaKind.Added)))),
                            GetDelta(DeltaKind.ChangedInChildren,
                                GetDelta(DeltaKind.Deleted),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Deleted),
                                GetDelta(DeltaKind.ChangedInChildren,
                                    GetDelta(DeltaKind.ChangedInChildren,
                                        GetDelta(DeltaKind.Deleted),
                                        GetDelta(DeltaKind.Added))))));

                //act
                var comparison = new FileComparisonBuilder().BuildFileComparison(old, @new);

                //assert
                AssertDeltasEqual(expectedDelta, comparison);
            }


            [Fact]
            public void Imprison()
            {
                //arrange
                (var old, var @new) = GetTestCase("ExtraTests/Imprison");
                var expectedDelta =
                    GetDelta(DeltaKind.ChangedInChildren,
                        GetDelta(DeltaKind.ChangedInChildren,
                            GetDelta(DeltaKind.ChangedInChildren,
                                GetDelta(DeltaKind.Deleted),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Deleted),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Deleted),
                                GetDelta(DeltaKind.Deleted)),
                        GetDelta(DeltaKind.ChangedInChildren,
                            GetDelta(DeltaKind.ChangedInChildren,
                                GetDelta(DeltaKind.ChangedInChildren,
                                    GetDelta(DeltaKind.Deleted),
                                    GetDelta(DeltaKind.Added),
                                    GetDelta(DeltaKind.Deleted)
                                    )))));

                //act
                var comparison = new FileComparisonBuilder().BuildFileComparison(old, @new);

                //assert
                AssertDeltasEqual(expectedDelta, comparison);
            }

            [Fact]
            public void DuplicatedItem()
            {
                //arrange
                (var old, var @new) = GetTestCase("ExtraTests/DuplicatedItem");
                var expectedDelta =
                    GetDelta(DeltaKind.ChangedInChildren,
                        GetDelta(DeltaKind.Added));

                //act
                var comparison = new FileComparisonBuilder().BuildFileComparison(old, @new);

                //assert
                AssertDeltasEqual(expectedDelta, comparison);
            }

            [Fact]
            public void ScriptedEffect()
            {
                //arrange
                (var old, var @new) = GetTestCase("ExtraTests/ScriptedEffect");
                var expectedDelta =
                    GetDelta(DeltaKind.ChangedInChildren,
                        GetDelta(DeltaKind.ChangedInChildren,
                            GetDelta(DeltaKind.ChangedInChildren,
                                GetDelta(DeltaKind.ChangedInChildren,
                                    GetDelta(DeltaKind.Changed))),
                            GetDelta(DeltaKind.Deleted)));

                //act
                var comparison = new FileComparisonBuilder().BuildFileComparison(old, @new);

                //assert
                AssertDeltasEqual(expectedDelta, comparison);
            }

            [Fact]
            public void ScriptedEffect2()
            {
                //arrange
                (var old, var @new) = GetTestCase("ExtraTests/ScriptedEffect2");
                var expectedDelta =
                    GetDelta(DeltaKind.ChangedInChildren,
                        GetDelta(DeltaKind.ChangedInChildren,
                            GetDelta(DeltaKind.ChangedInChildren,
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added),
                                GetDelta(DeltaKind.Added)
                                )));

                //act
                var comparison = new FileComparisonBuilder().BuildFileComparison(old, @new);

                //assert
                AssertDeltasEqual(expectedDelta, comparison);
            }
        }
    }
}

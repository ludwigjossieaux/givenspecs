using FluentAssertions;
using GivenSpecs.Application.Tables;
using GivenSpecs.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace GivenSpecs.Tests.Helpers
{
    public class TableItem
    {
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }
        public string Prop3 { get; set; }
        public int PropInt { get; set; }
    }

    public class TableHelpers_Tests
    {
        public TableHelpers_Tests() { }

        #region CreateInstance

        [Fact]
        public void CreateInstance_When_EmptyTable_ItReturnDefault()
        {
            // Arrange
            var headers = new[] { "Prop1", "Prop2", "Prop3" };
            var table = new Table(headers);

            // Act
            var res = table.CreateInstance<TableItem>();

            // Assert
            res.Should().BeNull();
        }

        [Fact]
        public void CreateInstance_When_ItRuns_ItReturnsAFilledObject()
        {
            // Arrange
            var headers = new[] { "Prop1", "Prop2", "Prop3" };
            var table = new Table(headers);
            table.AddRow(new[] { "Val1", "Val2", "Val3" });

            // Act
            var res = table.CreateInstance<TableItem>();

            // Assert
            res.Should().NotBeNull();
            res.Prop1.Should().Be("Val1");
            res.Prop2.Should().Be("Val2");
            res.Prop3.Should().Be("Val3");
        }

        [Fact]
        public void CreateInstance_When_DataIsDisplayedInColumn_ItReturnsAFilledObject()
        {
            // Arrange
            var headers = new[] { "Property", "Value" };
            var table = new Table(headers);
            table.AddRow(new[] { "Prop1", "Val1" });
            table.AddRow(new[] { "Prop2", "Val2" });
            table.AddRow(new[] { "Prop3", "Val3" });

            // Act
            var res = table.CreateInstance<TableItem>(columnData: true);

            // Assert
            res.Should().NotBeNull();
            res.Prop1.Should().Be("Val1");
            res.Prop2.Should().Be("Val2");
            res.Prop3.Should().Be("Val3");
        }

        [Fact]
        public void CreateInstance_When_ATransformIsDefined_ItIsCalled()
        {
            // Arrange
            var headers = new[] { "PropInt" };
            var table = new Table(headers);
            table.AddRow(new[] { "100" });

            var tf = new List<(string property, Action<TableItem, string> action)>
            {
                ("PropInt", (obj, value) => obj.PropInt = int.Parse(value))
            };

            // Act
            var res = table.CreateInstance<TableItem>(transforms: tf);

            // Assert
            res.Should().NotBeNull();
            res.PropInt.Should().Be(100);
        }

        #endregion

        #region CreateSet

        [Fact]
        public void CreateSet_When_EmptyTable_ItReturnDefault()
        {
            // Arrange
            var headers = new[] { "Prop1", "Prop2", "Prop3" };
            var table = new Table(headers);

            // Act
            var res = table.CreateSet<TableItem>();

            // Assert
            res.Should().BeOfType<List<TableItem>>();
            res.Should().BeEmpty();
        }

        [Fact]
        public void CreateSet_When_ItContainsRows_ItReturnsAsMuchObject()
        {
            // Arrange
            var headers = new[] { "Prop1", "Prop2", "Prop3" };
            var table = new Table(headers);
            table.AddRow(new[] { "Val1", "Val2", "Val3" });
            table.AddRow(new[] { "Val4", "Val5", "Val6" });

            // Act
            var res = table.CreateSet<TableItem>();

            // Assert
            res.Should().HaveCount(2);
            res[0].Prop1.Should().Be("Val1");
            res[0].Prop2.Should().Be("Val2");
            res[0].Prop3.Should().Be("Val3");
            res[1].Prop1.Should().Be("Val4");
            res[1].Prop2.Should().Be("Val5");
            res[1].Prop3.Should().Be("Val6");
        }

        #endregion

        #region CompareToInstance

        [Fact]
        public void CompareToInstance_When_ItRuns_ItChecksPropertyDefined()
        {
            // Arrange
            var headers = new[] { "Prop1", "Prop2", "Prop3" };
            var table = new Table(headers);
            table.AddRow(new[] { "Val1", "Val2", "Val3" });

            var item = new TableItem() { 
                Prop1 = "Val1", 
                Prop2 = "Val2", 
                Prop3 = "Val3" 
            };

            // Act
            var res = table.CompareToInstance(item);

            // Assert
            res.Result.Should().BeTrue();
            res.Message.Should().BeNullOrEmpty();
        }

        [Fact]
        public void CompareToInstance_When_EmptyTable_ItReturnsFalse()
        {
            // Arrange
            var headers = new[] { "Prop1", "Prop2", "Prop3" };
            var table = new Table(headers);
            var item = new TableItem()
            {
                Prop1 = "Val1",
                Prop2 = "Val2",
                Prop3 = "Val3"
            };

            // Act
            var res = table.CompareToInstance(item);

            // Assert
            res.Result.Should().BeFalse();
            res.Message.Should().Be("Empty table");
        }

        [Fact]
        public void CompareToInstance_When_DataIsDisplayedInColumn_ItChecksPropertyDefined()
        {
            // Arrange
            var headers = new[] { "Property", "Value" };
            var table = new Table(headers);
            table.AddRow(new[] { "Prop1", "Val1" });
            table.AddRow(new[] { "Prop2", "Val2" });
            table.AddRow(new[] { "Prop3", "Val3" });

            var item = new TableItem()
            {
                Prop1 = "Val1",
                Prop2 = "Val2",
                Prop3 = "Val3"
            };

            // Act
            var res = table.CompareToInstance(item, columnData: true);

            // Assert
            res.Result.Should().BeTrue();
            res.Message.Should().BeNullOrEmpty();
        }

        [Fact]
        public void CompareToInstance_When_ItHasADifferentProperty_ItReturnsFalse()
        {
            // Arrange
            var headers = new[] { "Prop1", "Prop2", "Prop3" };
            var table = new Table(headers);
            table.AddRow(new[] { "Val1", "Val2", "Val3" });

            var item = new TableItem()
            {
                Prop1 = "Val1",
                Prop2 = "Different",
                Prop3 = "Val3"
            };

            // Act
            var res = table.CompareToInstance(item);

            // Assert
            res.Result.Should().BeFalse();
            res.Message.Should().Be("Property: Prop2, Is: Different, Expected: Val2");
        }

        [Fact]
        public void CompareToInstance_When_ItHasComparatorDefined_ItUsesIt()
        {
            // Arrange
            var headers = new[] { "PropInt" };
            var table = new Table(headers);
            table.AddRow(new[] { "100" });

            var comparators = new List<(string property, Func<TableItem, string, (bool, string)> func)>
            {
                ("PropInt", (TableItem obj, string value) => {
                    return (obj.PropInt == int.Parse(value), obj.PropInt.ToString());
                })
            };

            var item = new TableItem()
            {
                PropInt = 100
            };

            // Act
            var res = table.CompareToInstance(item, comparators);

            // Assert
            res.Result.Should().BeTrue();
            res.Message.Should().BeNullOrEmpty();
        }

        [Fact]
        public void CompareToInstance_When_ItHasComparatorDefinedWithDifferentValue_ItReturnsFalse()
        {
            // Arrange
            var headers = new[] { "PropInt" };
            var table = new Table(headers);
            table.AddRow(new[] { "200" });

            var comparators = new List<(string property, Func<TableItem, string, (bool, string)> func)>
            {
                ("PropInt", (TableItem obj, string value) => {
                    return (obj.PropInt == int.Parse(value), obj.PropInt.ToString());
                })
            };

            var item = new TableItem()
            {
                PropInt = 100
            };

            // Act
            var res = table.CompareToInstance(item, comparators);

            // Assert
            res.Result.Should().BeFalse();
            res.Message.Should().Be("Property: PropInt, Is: 100, Expected: 200");
        }

        #endregion

        #region CompareToSet

        [Fact]
        public void CompareToSet_When_ItIsSimilar_ItReturnsTrue()
        {
            // Arrange
            var headers = new[] { "Prop1", "Prop2", "Prop3" };
            var table = new Table(headers);
            table.AddRow(new[] { "Val1", "Val2", "Val3" });
            table.AddRow(new[] { "Val4", "Val5", "Val6" });

            var list = new List<TableItem>()
            {
                new TableItem() { Prop1 = "Val1", Prop2 = "Val2", Prop3 = "Val3" },
                new TableItem() { Prop1 = "Val4", Prop2 = "Val5", Prop3 = "Val6" },
            };

            // Act
            var res = table.CompareToSet(list);

            // Assert
            res.Result.Should().BeTrue();
            res.Message.Should().BeEmpty();

        }

        [Fact]
        public void CompareToSet_When_NotTheSameNumberOfItems_ItReturnsFalse()
        {
            // Arrange
            var headers = new[] { "Prop1", "Prop2", "Prop3" };
            var table = new Table(headers);
            table.AddRow(new[] { "Val1", "Val2", "Val3" });

            var list = new List<TableItem>()
            {
                new TableItem() { Prop1 = "Val1", Prop2 = "Val2", Prop3 = "Val3" },
                new TableItem() { Prop1 = "Val4", Prop2 = "Different", Prop3 = "Val6" },
            };

            // Act
            var res = table.CompareToSet(list);

            // Assert
            res.Result.Should().BeFalse();
            res.Message.Should().Be("Number of items mismatch");
        }

        [Fact]
        public void CompareToSet_When_TableIsEmpty_ItReturnsFalse()
        {
            // Arrange
            var headers = new[] { "Prop1", "Prop2", "Prop3" };
            var table = new Table(headers);

            var list = new List<TableItem>()
            {
                new TableItem() { Prop1 = "Val1", Prop2 = "Val2", Prop3 = "Val3" },
                new TableItem() { Prop1 = "Val4", Prop2 = "Val5", Prop3 = "Val6" },
            };

            // Act
            var res = table.CompareToSet(list);

            // Assert
            res.Result.Should().BeFalse();
            res.Message.Should().Be("Empty table");
        }

        [Fact]
        public void CompareToSet_When_ItTheyAreDifferent_ItReturnsFalse()
        {
            // Arrange
            var headers = new[] { "Prop1", "Prop2", "Prop3" };
            var table = new Table(headers);
            table.AddRow(new[] { "Val1", "Val2", "Val3" });
            table.AddRow(new[] { "Val4", "Val5", "Val6" });

            var list = new List<TableItem>()
            {
                new TableItem() { Prop1 = "Val1", Prop2 = "Val2", Prop3 = "Val3" },
                new TableItem() { Prop1 = "Val4", Prop2 = "Different", Prop3 = "Val6" },
            };

            // Act
            var res = table.CompareToSet(list);

            // Assert
            res.Result.Should().BeFalse();
            res.Message.Should().Be("Index: 1, Property: Prop2, Is: Different, Expected: Val5");
        }

        #endregion

    }
}

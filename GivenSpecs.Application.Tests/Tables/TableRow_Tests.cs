using FluentAssertions;
using GivenSpecs.Application.Exceptions;
using GivenSpecs.Application.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;

namespace GivenSpecs.Application.Tests.Tables
{
    public class TableRow_Tests
    {
        public TableRow_Tests()
        {

        }

        #region constructor

        [Fact]
        public void Ctr_When_ItRuns_ItSetsHeaders()
        {
            // Arrange
            var h = new List<string>() { "header1", "header2" };

            // Act
            var result = new TableRow(h);

            // Assert
            result.GetHeaders().Should().BeEquivalentTo(h);
        }

        public static IEnumerable<object[]> Ctr_When_HeadersAreNull_ItRaisesAnException_Data
        {
            get
            {
                return new[]
                {
                    new object[] { null },
                    new object[] { new List<string>() },
                    new object[] { new List<string>() { "", "header2" } },
                    new object[] { new List<string>() { "  ", "header2" } },
                    new object[] { new List<string>() { " header1", "header2" } },
                    new object[] { new List<string>() { "header1 ", "header2" } },
                };
            }
        }

        [Theory, MemberData(nameof(Ctr_When_HeadersAreNull_ItRaisesAnException_Data))]
        public void Ctor_When_HeadersAreInvalid_ItRaisesAnException(List<string> headers)
        {
            // Arrange

            // Act
            Action act = () => new TableRow(headers);

            // Assert
            act.Should().Throw<TableRowInvalidHeadersException>()
                .WithMessage("TableRow should be initialized with valid headers");
        }

        [Fact]
        public void Ctor_When_ItRuns_ItInitializesEmptyCellsTable()
        {
            // Arrange
            var h = new List<string>() { "header1", "header2" };

            // Act
            var result = new TableRow(h);

            // Assert
            result.Cells.Should().NotBeNull();
            result.Cells.Should().BeEmpty();
        }

        #endregion

        #region Cells (get)

        [Fact]
        public void Cells_Get()
        {
            // Arrange
            var h = new List<string>() { "header1", "header2" };
            var tr = new TableRow(h);
            var cells = new List<TableCell>();
            cells.Add(new TableCell() { Value = "a" });
            cells.Add(new TableCell() { Value = "b" });
            tr.Cells = cells;

            // Act
            var result = tr.Cells;

            // Assert
            result.Should().BeEquivalentTo(cells);
        }

        #endregion

        #region Cells (set)

        [Fact]
        public void Cells_Set()
        {
            // Arrange
            var h = new List<string>() { "header1", "header2" };
            var tr = new TableRow(h);
            var cells = new List<TableCell>();
            cells.Add(new TableCell() { Value = "a" });
            cells.Add(new TableCell() { Value = "b" });
            tr.Cells.Should().BeEmpty();

            // Act
            tr.Cells = cells;

            // Assert
            var result = tr.Cells;
            result.Should().BeEquivalentTo(cells);
        }

        #endregion

        #region GetHeaders

        [Fact]
        public void GetHeader_When_ItRuns_ItRetrievesTheHeadersAtCreation()
        {
            // Arrange
            var h = new List<string>() { "header1", "header2" };
            var tr = new TableRow(h);

            // Act
            var result = tr.GetHeaders();

            // Assert
            result.Should().BeEquivalentTo(h);
        }

        #endregion

        #region GetValuesAsArray

        [Fact]
        public void GetValuesAsArray_When_ItRuns_ItReturnsCellsValuesAsArray()
        {
            // Arrange
            var h = new List<string>() { "header1", "header2" };
            var tr = new TableRow(h);
            var cells = new List<TableCell>();
            cells.Add(new TableCell() { Value = "a" });
            cells.Add(new TableCell() { Value = "b" });
            tr.Cells = cells;

            // Act
            var result = tr.GetValuesAsArray();

            // Assert
            result.Should().HaveCount(2);
            result[0].Should().Be("a");
            result[1].Should().Be("b");
        }

        #endregion

        #region Get(idx)

        [Fact]
        public void GetIdx_When_ItRuns_ItRetrievesTheCellValueAtIndex()
        {
            // Arrange
            var h = new List<string>() { "header1", "header2" };
            var tr = new TableRow(h);
            var cells = new List<TableCell>();
            cells.Add(new TableCell() { Value = "a" });
            cells.Add(new TableCell() { Value = "b" });
            tr.Cells = cells;

            // Act
            var result1 = tr.Get(0);
            var result2 = tr.Get(1);

            // Assert
            result1.Should().Be("a");
            result2.Should().Be("b");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(5)]
        public void GetIdx_When_IndexDoesntExist_ItReturnsNull(int index)
        {
            // Arrange
            var h = new List<string>() { "header1", "header2" };
            var tr = new TableRow(h);
            var cells = new List<TableCell>();
            cells.Add(new TableCell() { Value = "a" });
            cells.Add(new TableCell() { Value = "b" });
            tr.Cells = cells;

            // Act
            var result = tr.Get(index);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Get(header)

        [Fact]
        public void GetHeader_When_ItRuns_ItRetrievesTheCellValueForHeader()
        {
            // Arrange
            var h = new List<string>() { "header1", "header2" };
            var tr = new TableRow(h);
            var cells = new List<TableCell>();
            cells.Add(new TableCell() { Value = "a" });
            cells.Add(new TableCell() { Value = "b" });
            tr.Cells = cells;

            // Act
            var result1 = tr.Get("header1");
            var result2 = tr.Get("header2");

            // Assert
            result1.Should().Be("a");
            result2.Should().Be("b");
        }

        [Fact]
        public void GetHeader_When_HeaderDoesntExist_ItReturnsNull()
        {
            // Arrange
            var h = new List<string>() { "header1", "header2" };
            var tr = new TableRow(h);
            var cells = new List<TableCell>();
            cells.Add(new TableCell() { Value = "a" });
            cells.Add(new TableCell() { Value = "b" });
            tr.Cells = cells;

            // Act
            var result = tr.Get("header5");

            // Assert
            result.Should().BeNull();
        }

        #endregion
    }
}

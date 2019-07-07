using FluentAssertions;
using GivenSpecs.Application.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace GivenSpecs.Application.Tests.Tables
{
    public class Table_Tests
    {
        public Table_Tests()
        {

        }

        #region GetHeaders

        [Fact]
        public void GetHeaders_When_ItRuns_ItReturnsTheDefinedColumns()
        {
            // Arrange
            var table = new Table(new List<string>() {
                "header1", "header2", "header3"
            }.ToArray());

            // Act
            var result = table.GetHeaders();

            // Assert
            result.Should().BeOfType<List<string>>();
            result[0].Should().Be("header1");
            result[1].Should().Be("header2");
            result[2].Should().Be("header3");
        }

        #endregion

        #region AddRow

        [Fact]
        public void AddRow_When_ItRuns_ItAddsARowToTheTable()
        {
            // Arrange
            var table = new Table(new List<string>() {
                "header1", "header2", "header3"
            }.ToArray());

            // Act
            table.AddRow(new List<string>()
            {
                "value1", "value2", "value3"
            }.ToArray());

            // Assert
            var rows = table.GetRows();
            rows.Count().Should().Be(1);
            var row = rows.First();
            row.Cells[0].Value.Should().Be("value1");
            row.Cells[1].Value.Should().Be("value2");
            row.Cells[2].Value.Should().Be("value3");
        }

        #endregion

        #region GetRows

        [Fact]
        public void GetRows_When_ItRuns_ItReturnsTheDefinedColumns()
        {
            // Arrange
            var table = new Table(new List<string>() {
                "header1", "header2", "header3"
            }.ToArray());
            table.AddRow(new List<string>()
            {
                "value1.1", "value1.2", "value1.3"
            }.ToArray());
            table.AddRow(new List<string>()
            {
                "value2.1", "value2.2", "value2.3"
            }.ToArray());

            // Act
            var result = table.GetRows();

            // Assert
            result.Count().Should().Be(2);
            var row = result.First();
            row.Cells[0].Value.Should().Be("value1.1");
            row.Cells[1].Value.Should().Be("value1.2");
            row.Cells[2].Value.Should().Be("value1.3");
            row = result.Skip(1).First();
            row.Cells[0].Value.Should().Be("value2.1");
            row.Cells[1].Value.Should().Be("value2.2");
            row.Cells[2].Value.Should().Be("value2.3");
        }

        #endregion

        #region ApplyReplacements

        [Fact]
        public void ApplyReplacements_When_ItRuns_ItReturnsTheDefinedColumns()
        {
            // Arrange
            var table = new Table(new List<string>() {
                "header1", "header2", "header3"
            }.ToArray());
            table.AddRow(new List<string>()
            {
                "<Value>", "<DifferentValue>", "<AnotherValue>"
            }.ToArray());
            var replacementFunc = new Func<string, string>((string input) =>
            {
                return input.Replace("<Value>", "123")
                            .Replace("<DifferentValue>", "abc")
                            .Replace("<AnotherValue>", "789");
            });

            // Act
            table.ApplyReplacements(replacementFunc);

            // Assert
            var rows = table.GetRows();
            rows.Count().Should().Be(1);
            var row = rows.First();
            row.Cells[0].Value.Should().Be("123");
            row.Cells[1].Value.Should().Be("abc");
            row.Cells[2].Value.Should().Be("789");
        }

        #endregion
    }
}

using FluentAssertions;
using GivenSpecs.Application.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GivenSpecs.Application.Tests.Tables
{
    public class TableCell_Tests
    {
        public TableCell_Tests()
        {

        }

        [Fact]
        public async Task Value_Get()
        {
            // Arrange
            var config = new TableCell();
            config.Value = "Value";

            // Act
            var value = config.Value;

            // Assert
            value.Should().Be("Value");
        }

        [Fact]
        public async Task Value_Set()
        {
            // Arrange
            var config = new TableCell();

            // Act
            config.Value = "Value";

            // Assert
            config.Value.Should().Be("Value");
        }
    }
}

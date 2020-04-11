using FluentAssertions;
using GivenSpecs.Application.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GivenSpecs.Application.Tests.Configuration
{
    public class GivenSpecsAppConfiguration_Tests
    {
        public GivenSpecsAppConfiguration_Tests()
        {
        }

        [Fact]
        public void FeatureNamespace_Get()
        {
            // Arrange
            var config = new GivenSpecsAppConfiguration();
            config.FeatureNamespace = "Feature1";

            // Act
            var value = config.FeatureNamespace;

            // Assert
            value.Should().Be("Feature1");
        }

        [Fact]
        public void FeatureNamespace_Set()
        {
            // Arrange
            var config = new GivenSpecsAppConfiguration();

            // Act
            config.FeatureNamespace = "Feature1";

            // Assert
            config.FeatureNamespace.Should().Be("Feature1");
        }
    }
}

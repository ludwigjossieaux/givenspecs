using FluentAssertions;
using GivenSpecs.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GivenSpecs.Application.Tests.Services
{
    public class StringHelperService_Tests
    {
        private readonly StringHelperService _service;

        public StringHelperService_Tests()
        {
            _service = new StringHelperService();
        }

        #region ToMethodString

        [Fact]
        public async Task ToMethodString_When_ItRuns_ItPreparesAString()
        {
            // Arrange
            var cases = new List<string>()
            {
                "A sentence with words",
                "A sentence with a Proper Name",
                "A sentence with an ACRONYM",
                "A. sentence/ with $ special characters ?!",
                "A sentence with 123 numbers"
            };

            // Act
            var tasks = cases.Select(x => _service.ToMethodString(x));
            var results = await Task.WhenAll(tasks);

            // Assert
            results[0].Should().Be("ASentenceWithWords");
            results[1].Should().Be("ASentenceWithAProperName");
            results[2].Should().Be("ASentenceWithAnACRONYM");
            results[3].Should().Be("ASentenceWithSpecialCharacters");
            results[4].Should().Be("ASentenceWith123Numbers");
        }

        #endregion

        #region ToParamString

        [Fact]
        public async Task ToParamString_When_ItRuns_ItPreparesAString()
        {
            // Arrange
            var cases = new List<string>()
            {
                "A sentence with words",
                "A sentence with a Proper Name",
                "A sentence with an ACRONYM",
                "A. sentence/ with $ special characters ?!",
                "A sentence with 123 numbers"
            };

            // Act
            var tasks = cases.Select(x => _service.ToParamString(x));
            var results = await Task.WhenAll(tasks);

            // Assert
            results[0].Should().Be("aSentenceWithWords");
            results[1].Should().Be("aSentenceWithAProperName");
            results[2].Should().Be("aSentenceWithAnACRONYM");
            results[3].Should().Be("aSentenceWithSpecialCharacters");
            results[4].Should().Be("aSentenceWith123Numbers");
        }

        #endregion

        #region ToIdString

        [Fact]
        public async Task ToIdString_When_ItRuns_ItPreparesAString()
        {
            // Arrange
            var cases = new List<string>()
            {
                "A sentence with words",
                "A sentence with a Proper Name",
                "A sentence with an ACRONYM",
                "A. sentence/ with $ special characters ?!",
                "A sentence with 123 numbers"
            };

            // Act
            var tasks = cases.Select(x => _service.ToIdString(x));
            var results = await Task.WhenAll(tasks);

            // Assert
            results[0].Should().Be("a-sentence-with-words");
            results[1].Should().Be("a-sentence-with-a-proper-name");
            results[2].Should().Be("a-sentence-with-an-acronym");
            results[3].Should().Be("a--sentence--with---special-characters");
            results[4].Should().Be("a-sentence-with-123-numbers");
        }

        #endregion

        #region GetEmbeddedFile
        #endregion
    }
}

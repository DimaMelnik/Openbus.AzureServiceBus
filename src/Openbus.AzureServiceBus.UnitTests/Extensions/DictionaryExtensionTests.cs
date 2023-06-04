using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Openbus.AzureServiceBus.Extensions;
using Xunit;

namespace Openbus.AzureServiceBus.UnitTests.Extensions
{
    public class DictionaryExtensionTests
    {
        [Fact]
        public void ContainsKeyValuePair_ShouldReturnFalse_WhenPropertyIsMissing()
        {
            // Arrange
            IReadOnlyDictionary<string, object> dictionary = new Dictionary<string,object>();

            // Act & Assert
            dictionary.ContainsKeyValuePair("key", "value").Should().BeFalse();
        }

        [Fact]
        public void ContainsKeyValuePair_ShouldHandleNullValues()
        {
            // Arrange
            IReadOnlyDictionary<string, object> dictionary = new Dictionary<string, object>();

            // Act & Assert
            dictionary.ContainsKeyValuePair("key", (string)null).Should().BeFalse();
        }

        [Theory]
        [InlineData("KEY", "VALUE")]
        [InlineData("Key", "Value")]
        [InlineData("key", "value")]
        public void ContainsKeyValuePair_ShouldIgnoreCase(string key, string value)
        {
            // Arrange
            IReadOnlyDictionary<string, object> dictionary = new Dictionary<string, object>
            {
                { key, value }
            };

            // Act & Assert
            dictionary.ContainsKeyValuePair("key", "value").Should().BeTrue();
        }

        [Fact]
        public void ContainsKeyValuePair_ShouldReturnTrue_WhenCheckConditionPasses()
        {
            // Arrange
            IReadOnlyDictionary<string, object> dictionary = new Dictionary<string, object>
            {
                { "key", "value" }
            };

            // Act & Assert
            dictionary.ContainsKeyValuePair("key", v => v.ToString().EndsWith("ue")).Should().BeTrue();
        }

        [Fact]
        public void ContainsKeyValuePair_ShouldReturnFalse_WhenCheckConditionFails()
        {
            // Arrange
            IReadOnlyDictionary<string, object> dictionary = new Dictionary<string, object>
            {
                { "key", "value" }
            };

            // Act & Assert
            dictionary.ContainsKeyValuePair("key", v => v.ToString().StartsWith("z")).Should().BeFalse();
        }
    }
}

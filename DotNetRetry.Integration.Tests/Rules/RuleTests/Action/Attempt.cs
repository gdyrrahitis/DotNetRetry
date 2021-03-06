﻿namespace DotNetRetry.Integration.Tests.Rules.RuleTests.Action
{
    using System;
    using DotNetRetry.Rules;
    using DotNetRetry.Rules.Configuration;
    using DotNetRetry.Tests.Common;
    using Xunit;
    using static Xunit.Assert;

    public class Attempt
    {
        [Theory]
        [MemberData(nameof(RulesDataSource.Data), MemberType = typeof(RulesDataSource))]
        public void SuccessAtFirstTry(Strategy input)
        {
            // Arrange
            var actual = 0;
            var rules = Rule.Setup(input).Config(options =>
            {
                options.Attempts = 3;
                options.Time = TimeSpan.FromMilliseconds(100);
            });
            Action successFullAction = () =>
            {
                const string intAsString = "15";
                var stringToInt = int.Parse(intAsString);
                actual = stringToInt;
            };

            // Act
            rules.Attempt(successFullAction);

            // Assert
            Equal(15, actual);
        }

        [Theory]
        [MemberData(nameof(RulesDataSource.Data), MemberType = typeof(RulesDataSource))]
        public void SuccessAtSecondTry(Strategy input)
        {
            // Arrange
            var actual = 0;
            var tries = 0;
            var rules = Rule.Setup(input).Config(options =>
            {
                options.Attempts = 5;
                options.Time = TimeSpan.FromMilliseconds(100);
            });
            Action successAtSecondTryAction = () =>
            {
                const string invalidNumber = "ab123";
                if (tries == 2)
                {
                    var validNumber = invalidNumber.Replace("ab", "");
                    actual = int.Parse(validNumber);
                }
                else
                {
                    tries++;
                    actual = int.Parse(invalidNumber);
                }
            };

            // Act
            rules.Attempt(successAtSecondTryAction);

            // Assert
            Equal(2, tries);
            Equal(123, actual);
        }

        [Theory]
        [MemberData(nameof(RulesDataSource.Data), MemberType = typeof(RulesDataSource))]
        public void SuccessAtThirdTry(Strategy input)
        {
            // Arrange
            var actual = 0;
            var tries = 0;
            var rules = Rule.Setup(input).Config(options =>
            {
                options.Attempts = 5;
                options.Time = TimeSpan.FromMilliseconds(100);
            });
            Action successAtThirdTryAction = () =>
            {
                const string invalidNumber = "ab123";
                if (tries == 3)
                {
                    var validNumber = invalidNumber.Replace("ab", "");
                    actual = int.Parse(validNumber);
                }
                else
                {
                    tries++;
                    actual = int.Parse(invalidNumber);
                }
            };

            // Act
            rules.Attempt(successAtThirdTryAction);

            // Assert
            Equal(3, tries);
            Equal(123, actual);
        }

        [Theory]
        [MemberData(nameof(RulesDataSource.Data), MemberType = typeof(RulesDataSource))]
        public void FailureAfterAllTriesReturnsAggregateExceptionWithAllTheExceptionsOccurred(Strategy input)
        {
            // Arrange
            var actual = 0;
            var tries = 0;
            var rules = Rule.Setup(input).Config(options =>
            {
                options.Attempts = 3;
                options.Time = TimeSpan.FromMilliseconds(100);
            });
            Action failureAction = () =>
            {
                tries++;
                const string invalidNumber = "123abc";
                actual = int.Parse(invalidNumber);
            };

            // Act
            var exception = Throws<AggregateException>(() => rules.Attempt(failureAction));

            // Assert
            Equal(3, exception.InnerExceptions.Count);
            Equal(3, tries);
            Equal(0, actual);
        }

        [Theory]
        [MemberData(nameof(RulesDataSource.Data), MemberType = typeof(RulesDataSource))]
        public void SuccessAtFirstTryWithParameterPassed(Strategy input)
        {
            // Arrange
            var actual = 0;
            const string parameter = "123456";
            var rules = Rule.Setup(input).Config(options =>
            {
                options.Attempts = 3;
                options.Time = TimeSpan.FromMilliseconds(100);
            });
            Action<string> convertToIntAction = s => actual = int.Parse(s);

            // Act
            rules.Attempt(() => convertToIntAction(parameter));

            // Assert
            Equal(123456, actual);
        }

        [Theory]
        [MemberData(nameof(RulesDataSource.Data), MemberType = typeof(RulesDataSource))]
        public void SuccessAtSecondTryWithParameterPassed(Strategy input)
        {
            // Arrange
            var actual = 0;
            var tries = 0;
            const string parameter = "abc123456";
            var rules = Rule.Setup(input).Config(options =>
            {
                options.Attempts = 5;
                options.Time = TimeSpan.FromMilliseconds(100);
            });
            Action<string> convertToIntAction = s =>
            {
                if (tries == 2)
                {
                    s = s.Replace("abc", "");
                    actual = int.Parse(s);
                }
                else
                {
                    tries++;
                    actual = int.Parse(s);
                }
            };

            // Act
            rules.Attempt(() => convertToIntAction(parameter));

            // Assert
            Equal(2, tries);
            Equal(123456, actual);
        }

        [Theory]
        [MemberData(nameof(RulesDataSource.Data), MemberType = typeof(RulesDataSource))]
        public void FailureAfterAllTriesWithParameterPassed(Strategy input)
        {
            // Arrange
            var actual = 0;
            var tries = 0;
            const string parameter = "abcd123";
            var rules = Rule.Setup(input).Config(options =>
            {
                options.Attempts = 3;
                options.Time = TimeSpan.FromMilliseconds(100);
            });
            Action<string> failureAction = s =>
            {
                tries++;
                actual = int.Parse(s);
            };

            // Act
            var exception = Throws<AggregateException>(() => 
                rules.Attempt(() => failureAction(parameter)));

            // Assert
            Equal(3, exception.InnerExceptions.Count);
            Equal(3, tries);
            Equal(0, actual);
        }
    }
}

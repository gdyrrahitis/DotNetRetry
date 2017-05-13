﻿namespace DotNetRetry.Tests.RetryTests
{
    using System;
    using DotNetRetry;
    using NUnit.Framework;
    using static NUnit.Framework.Assert;

    [TestFixture]
    public class Attempt
    {
        private IRetry _retry;

        [SetUp]
        public void Setup()
        {
            _retry = new Retry();
        }

        [Test]
        public void SuccessAtFirstTry()
        {
            // Arrange
            var actual = 0;
            Action successFullAction = () =>
            {
                const string intAsString = "15";
                var stringToInt = int.Parse(intAsString);
                actual = stringToInt;
            };

            // Act
            _retry.Attempt(successFullAction, 3, TimeSpan.FromSeconds(2));

            // Assert
            AreEqual(15, actual);
        }

        [Test]
        public void SuccessAtSecondTry()
        {
            // Arrange
            var actual = 0;
            var tries = 0;
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
            _retry.Attempt(successAtSecondTryAction, 5, TimeSpan.FromSeconds(1));

            // Assert
            AreEqual(2, tries);
            AreEqual(123, actual);
        }

        [Test]
        public void SuccessAtThirdTry()
        {
            // Arrange
            var actual = 0;
            var tries = 0;
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
            _retry.Attempt(successAtThirdTryAction, 5, TimeSpan.FromSeconds(1));

            // Assert
            AreEqual(3, tries);
            AreEqual(123, actual);
        }

        [Test]
        public void FailureAfterAllTriesReturnsAggregateExceptionWithAllTheExceptionsOccurred()
        {
            // Arrange
            var actual = 0;
            var tries = 0;
            Action failureAction = () =>
            {
                tries++;
                const string invalidNumber = "123abc";
                actual = int.Parse(invalidNumber);
            };

            // Act
            TestDelegate action = () => _retry.Attempt(failureAction, 3, TimeSpan.FromSeconds(1));

            // Assert
            Throws<AggregateException>(action);
            AreEqual(3, tries);
            AreEqual(0, actual);
        }

        [Test]
        public void SuccessAtFirstTryWithParameterPassed()
        {
            // Arrange
            const string parameter = "123456";
            var actual = 0;
            Action<string> convertToIntAction = s => actual = int.Parse(s);

            // Act
            _retry.Attempt(() => convertToIntAction(parameter), 3, TimeSpan.FromSeconds(1));

            // Assert
            AreEqual(123456, actual);
        }

        [Test]
        public void SuccessAtSecondTryWithParameterPassed()
        {
            // Arrange
            const string parameter = "abc123456";
            var actual = 0;
            var tries = 0;
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
            _retry.Attempt(() => convertToIntAction(parameter), 6, TimeSpan.FromSeconds(1));

            // Assert
            AreEqual(2, tries);
            AreEqual(123456, actual);
        }

        [Test]
        public void FailureAfterAllTriesWithParameterPassed()
        {
            // Arrange
            const string parameter = "abcd123";
            var actual = 0;
            var tries = 0;
            Action<string> failureAction = s =>
            {
                tries++;
                actual = int.Parse(s);
            };

            // Act
            TestDelegate action = () => _retry.Attempt(() => failureAction(parameter), 3, TimeSpan.FromSeconds(1));

            // Assert
            Throws<AggregateException>(action);
            AreEqual(3, tries);
            AreEqual(0, actual);
        }
    }
}
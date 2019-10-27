using System;
using System.Collections.Generic;
using Xunit;
using Moq;

namespace CanonicalizeRequest.Tests
{
    public class RequestAuthenticatorTest
    {
        [Fact]
        public void RequestNotAuthenticWhenMakeFromRequestErrors()
        {
            var auth = RequestAuthenticator.New(null, 0);
            Assert.False(auth.IsRequestAuthentic(MockHttpRequest.MakeWith(new Dictionary<string, IEnumerable<string>>())));
        }
        [Fact]
        public void RequestNotAuthenticIfTimestampTooFarAhead()
        {
            var tolerance = 10;
            var auth = RequestAuthenticator.New(null, tolerance);
            var now = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds + tolerance * 2;
            Assert.False(auth.IsRequestAuthentic(MockHttpRequest.MakeWith(new Dictionary<string, IEnumerable<string>>()
            {
                { "SignedHeaders", new List<string>() { "foo" } },
                { "Authorization", new List<string>() { $"a:b:{now}:d" } }
            })));
        }
        [Fact]
        public void RequestNotAuthenticIfTimestampTooFarBehind()
        {
            var tolerance = 10;
            var auth = RequestAuthenticator.New(null, tolerance);
            var now = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds - tolerance * 2;
            Assert.False(auth.IsRequestAuthentic(MockHttpRequest.MakeWith(new Dictionary<string, IEnumerable<string>>()
            {
                { "SignedHeaders", new List<string>() { "foo" } },
                { "Authorization", new List<string>() { $"a:b:{now}:d" } }
            })));
        }
        [Fact]
        public void RequestCallsVerifyIfPassingTimestampCheck()
        {
            var mockVerifier = new MockCryptoVerifier(() => false);
            var tolerance = 10;
            var auth = RequestAuthenticator.New(mockVerifier, tolerance);
            var now = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            Assert.False(auth.IsRequestAuthentic(MockHttpRequest.MakeWith(
                "GET", "/", new Dictionary<string, IEnumerable<string>>(), new Dictionary<string, IEnumerable<string>>()
            {
                { "SignedHeaders", new List<string>() { "foo" } },
                { "Authorization", new List<string>() { $"a:b:{now}:d" } }
            }, "")));
            Assert.Equal("a", mockVerifier.CalledWithAlgorithm);
            Assert.Equal("b", mockVerifier.CalledWithKey);
            Assert.NotNull(mockVerifier.CalledWithStringToSign);
            Assert.NotNull(mockVerifier.CalledWithSignature);
            Assert.Equal(1, mockVerifier.CallCount);
        }
        [Fact]
        public void RequestIsAuthenticIfPassingEverything()
        {
            var mockVerifier = new MockCryptoVerifier(() => true);
            var tolerance = 10;
            var auth = RequestAuthenticator.New(mockVerifier, tolerance);
            var now = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            Assert.True(auth.IsRequestAuthentic(MockHttpRequest.MakeWith(
                "GET", "/", new Dictionary<string, IEnumerable<string>>(), new Dictionary<string, IEnumerable<string>>()
            {
                { "SignedHeaders", new List<string>() { "foo" } },
                { "Authorization", new List<string>() { $"a:b:{now}:d" } }
            }, "")));
            Assert.Equal("a", mockVerifier.CalledWithAlgorithm);
            Assert.Equal("b", mockVerifier.CalledWithKey);
            Assert.NotNull(mockVerifier.CalledWithStringToSign);
            Assert.NotNull(mockVerifier.CalledWithSignature);
            Assert.Equal(1, mockVerifier.CallCount);
        }
    }
}

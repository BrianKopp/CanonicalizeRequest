using System;
using System.Collections.Generic;
using Xunit;

namespace CanonicalizeRequest.Tests
{
    public class RequestAuthenticationPartsTest
    {
        [Fact]
        public void FailsWithNullRequest()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                RequestAuthenticationParts.MakeFromRequest(null);
            });
            Assert.Equal("req", exception.ParamName);
        }
        [Fact]
        public void FailsWithoutSignedHeadersHeader()
        {
            var req = MockHttpRequest.MakeWith(new Dictionary<string, IEnumerable<string>>());
            var ex = Assert.Throws<ArgumentException>(() => RequestAuthenticationParts.MakeFromRequest(req));
            Assert.Equal("SignedHeaders header not present", ex.Message);
        }
        [Fact]
        public void FailsWithRepeatedSignedHeadersHeader()
        {
            var req = MockHttpRequest.MakeWith(new Dictionary<string, IEnumerable<string>>()
            {
                { "SignedHeaders", new List<string>() { "foo", "bar" } }
            });
            var ex = Assert.Throws<ArgumentException>(() => RequestAuthenticationParts.MakeFromRequest(req));
            Assert.Contains("expected one SignedHeaders header, got", ex.Message);
        }
        [Fact]
        public void FailsWithEmptySignedHeadersHeader()
        {
            var req = MockHttpRequest.MakeWith(new Dictionary<string, IEnumerable<string>>()
            {
                { "SignedHeaders", new List<string>() }
            });
            var ex = Assert.Throws<ArgumentException>(() => RequestAuthenticationParts.MakeFromRequest(req));
            Assert.Contains("expected one SignedHeaders header, got", ex.Message);
        }
        [Fact]
        public void FailsWithMissingAuthorizationHeader()
        {
            var req = MockHttpRequest.MakeWith(new Dictionary<string, IEnumerable<string>>()
            {
                { "SignedHeaders", new List<string>() { "foo" } }
            });
            var ex = Assert.Throws<ArgumentException>(() => RequestAuthenticationParts.MakeFromRequest(req));
            Assert.Equal("request missing Authorization header", ex.Message);
        }
        [Fact]
        public void FailsWithRepeatedAuthorizationHeader()
        {
            var req = MockHttpRequest.MakeWith(new Dictionary<string, IEnumerable<string>>()
            {
                { "SignedHeaders", new List<string>() { "foo" } },
                { "Authorization", new List<string>() { "foo", "bar" } }
            });
            var ex = Assert.Throws<ArgumentException>(() => RequestAuthenticationParts.MakeFromRequest(req));
            Assert.Contains("expected one Authorization header, got", ex.Message);
        }
        [Fact]
        public void FailsWithEmptyAuthorizationHeader()
        {
            var req = MockHttpRequest.MakeWith(new Dictionary<string, IEnumerable<string>>()
            {
                { "SignedHeaders", new List<string>() { "foo" } },
                { "Authorization", new List<string>() }
            });
            var ex = Assert.Throws<ArgumentException>(() => RequestAuthenticationParts.MakeFromRequest(req));
            Assert.Contains("expected one Authorization header, got", ex.Message);
        }
        [Fact]
        public void FailsIfAuthorizationNotFourPartsDelimitedByColon()
        {
            var req = MockHttpRequest.MakeWith(new Dictionary<string, IEnumerable<string>>()
            {
                { "SignedHeaders", new List<string>() { "foo" } },
                { "Authorization", new List<string>() { "bar" } }
            });
            var ex = Assert.Throws<ArgumentException>(() => RequestAuthenticationParts.MakeFromRequest(req));
            Assert.Equal("expected Authorization header to be 4 parts split by ':'", ex.Message);
        }
        [Fact]
        public void FailsIfTimestampNotLong()
        {
            var req = MockHttpRequest.MakeWith(new Dictionary<string, IEnumerable<string>>()
            {
                { "SignedHeaders", new List<string>() { "foo" } },
                { "Authorization", new List<string>() { "a:b:c:d" } }
            });
            var ex = Assert.Throws<FormatException>(() => RequestAuthenticationParts.MakeFromRequest(req));
            Assert.Equal("could not parse timestamp into long", ex.Message);
        }
        [Fact]
        public void SucceedsOtherwiseWithValues()
        {
            var req = MockHttpRequest.MakeWith(new Dictionary<string, IEnumerable<string>>()
            {
                { "SignedHeaders", new List<string>() { "foo" } },
                { "Authorization", new List<string>() { "a:b:1:d" } }
            });
            var parts = RequestAuthenticationParts.MakeFromRequest(req);
            Assert.Equal("foo", parts.SignedHeaders);
            Assert.Equal("d", parts.Signature);
            Assert.Equal("a", parts.SignatureAlgorithm);
            Assert.Equal("b", parts.SignatureKey);
            Assert.Equal(1, parts.SignatureTimestamp);
        }
    }
}

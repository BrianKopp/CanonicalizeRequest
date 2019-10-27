using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Microsoft.Extensions.Primitives;

namespace CanonicalizeRequest.Tests
{
    public class RequestCanonicalizationTest
    {
        [Fact]
        public void HttpMethodCanonicalizedCorrectly()
        {
            Assert.Equal("GET", RequestCanonicalization.CanonicalizeMethod("get"));
            Assert.Equal("GET", RequestCanonicalization.CanonicalizeMethod("get "));
            Assert.Equal("GET", RequestCanonicalization.CanonicalizeMethod(" get"));
            Assert.Equal("GET", RequestCanonicalization.CanonicalizeMethod("Get"));
        }
        [Fact]
        public void HttpPathCanonicalizedCorrectly()
        {
            Assert.Equal("/", RequestCanonicalization.CanonicalizePath("/"));
            Assert.Equal("/route", RequestCanonicalization.CanonicalizePath("/ROUTE"));
            Assert.Equal("/route/subroute", RequestCanonicalization.CanonicalizePath("/ROUTE/subroute  "));
        }
        [Fact]
        public void HttpQueryParametersCanonicalizedCorrectlyWhenEmpty()
        {
            Assert.Equal("", RequestCanonicalization.CanonicalizeQueryParameters(null));
            Assert.Equal("", RequestCanonicalization.CanonicalizeQueryParameters(new List<KeyValuePair<string, StringValues>>()));
        }
        [Fact]
        public void HttpQueryParametersCanonicalizedCorrectly()
        {
            var qps = new List<KeyValuePair<string, StringValues>>()
            {
                new KeyValuePair<string, StringValues>(null, new string[0]), // should be filtered out
                new KeyValuePair<string, StringValues>("B", new string[1] { "BB" }),
                new KeyValuePair<string, StringValues>("a", new string[1] { "aa" }),
                new KeyValuePair<string, StringValues>("b", new string[2] { "bb", "bbbb" }),
                new KeyValuePair<string, StringValues>(" ", new string[0]), // should be filtered out
            };
            var expected = "B=BB&a=aa&b=bb&b=bbbb"; // B is ordinally before a
            Assert.Equal(expected, RequestCanonicalization.CanonicalizeQueryParameters(qps));
        }
        [Fact]
        public void HttpHeadersCanonicalizedCorrectly()
        {
            var headers = new Dictionary<string, StringValues>()
            {
                { "B", new string[1] { "BB" } },
                { "a", new string[1] { "aa" } },
                { "b", new string[2] { "bb", " bb   bb  " } }
            };
            var expected = "a=aa\nb=BB,bb,bb bb"; // BB is ordinally before bb
            Assert.Equal(expected, RequestCanonicalization.CanonicalizeHeaders(headers));
        }
        [Fact]
        public void HttpSignedHeadersCanonicalizedCorrectly()
        {
            var signedHeaders = new List<string>()
            {
                "a",
                "Z"
            };
            Assert.Equal("a;z", RequestCanonicalization.CanonicalizeSignedHeaders(signedHeaders));
        }
        [Fact]
        public void HttpBodyCanonicalizedCorrectly()
        {
            var body = "foobar";
            Assert.Equal("3858F62230AC3C915F300C664312C63F", RequestCanonicalization.CanonicalizeRequestBody(body));
        }
    }
}

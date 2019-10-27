using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using Moq;

namespace CanonicalizeRequest.Tests
{
    public class MockHttpRequest
    {
        public static HttpRequest MakeWith(IDictionary<string, IEnumerable<string>> headers)
        {
            var mockReq = new Mock<HttpRequest>();
            mockReq.Setup(r => r.Headers).Returns(new HeaderDictionary(headers.ToDictionary(
                kvp => kvp.Key, kvp => new StringValues(kvp.Value.ToArray()))));
            return mockReq.Object;
        }
        public static HttpRequest MakeWith(string method, string path, IDictionary<string, IEnumerable<string>> queryValues,
            IDictionary<string, IEnumerable<string>> headers, string body)
        {
            var mockReq = new Mock<HttpRequest>();
            mockReq.Setup(r => r.Method).Returns(method);
            mockReq.Setup(r => r.Path).Returns(path);
            mockReq.Setup(r => r.Query).Returns(new QueryCollection(queryValues.ToDictionary(
                kvp => kvp.Key, kvp => new StringValues(kvp.Value.ToArray()))));
            mockReq.Setup(r => r.Headers).Returns(new HeaderDictionary(headers.ToDictionary(
                kvp => kvp.Key, kvp => new StringValues(kvp.Value.ToArray()))));
            var ms = new MemoryStream();
            var sr = new StreamWriter(ms, Encoding.UTF8, 1024, true);
            sr.Write(body);
            mockReq.Setup(r => r.Body).Returns(ms);
            return mockReq.Object;
        }
    }
}

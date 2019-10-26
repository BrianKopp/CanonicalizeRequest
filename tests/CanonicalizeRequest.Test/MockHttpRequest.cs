using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
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
    }
}

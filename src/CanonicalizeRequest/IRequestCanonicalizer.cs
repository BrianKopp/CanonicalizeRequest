using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace CanonicalizeRequest
{
    public interface IRequestCanonicalizer
    {
        string MakeCanonicalRepresentation(HttpRequest req);
        string MakeCanonicalRepresentation(string httpMethod,
            string httpPath,
            IEnumerable<KeyValuePair<string, StringValues>> queryParameters,
            IDictionary<string, StringValues> headers,
            string signedHeaders,
            string body);
        string MakeStringToSign(string algorithm, long requestTimestamp, string canonicalRequest);
    }
}

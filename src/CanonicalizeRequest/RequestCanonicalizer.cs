using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace CanonicalizeRequest
{
    public class RequestCanonicalizer : IRequestCanonicalizer
    {
        public string MakeCanonicalRepresentation(HttpRequest req)
        {
            return RequestCanonicalization.CanonicalRepresentation(req);
        }
        public string MakeCanonicalRepresentation(string httpMethod, string httpPath, IEnumerable<KeyValuePair<string, StringValues>> queryParameters, IDictionary<string, StringValues> headers, string signedHeaders, string body)
        {
            return RequestCanonicalization.CanonicalRepresentation(httpMethod,
                httpPath, queryParameters, headers, headers["SignedHeaders"], body);
        }
        public string MakeStringToSign(string algorithm, long requestTimestamp, string canonicalRequest)
        {
            return RequestCanonicalization.CreateStringToSign(algorithm, requestTimestamp, canonicalRequest);
        }
    }
}

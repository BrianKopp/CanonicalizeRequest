using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace CanonicalizeRequest
{
    public class RequestAuthenticationParts
    {
        public string SignedHeaders;
        public string SignatureAlgorithm;
        public string SignatureKey;
        public long SignatureTimestamp;
        public string Signature;
        public static RequestAuthenticationParts MakeFromRequest(HttpRequest req)
        {
            if (req == null)
            {
                throw new ArgumentNullException("req");
            }

            if (!req.Headers.TryGetValue("SignedHeaders", out StringValues signedHeadersValues))
            {
                throw new ArgumentException("SignedHeaders header not present");
            }

            if (signedHeadersValues.Count != 1)
            {
                throw new ArgumentException($"expected one SignedHeaders header, got {signedHeadersValues.Count}");
            }

            string signedHeaders = signedHeadersValues[0];
            if (!req.Headers.TryGetValue("Authorization", out StringValues authorizationValues))
            {
                throw new ArgumentException("request missing Authorization header");
            }

            if (authorizationValues.Count != 1)
            {
                throw new ArgumentException($"expected one Authorization header, got {authorizationValues.Count}");
            }
                
            var sigParts = authorizationValues[0].Split(':');
            if (sigParts.Length != 4)
            {
                throw new ArgumentException("expected Authorization header to be 4 parts split by ':'");
            }

            var algorithm = sigParts[0];
            var key = sigParts[1];
            long timestamp;
            try
            {
                timestamp = long.Parse(sigParts[2]);
            }
            catch (Exception ex)
            {
                throw new FormatException("could not parse timestamp into long", ex);
            }

            var signature = sigParts[3];

            return new RequestAuthenticationParts
            {
                SignedHeaders = signedHeaders,
                Signature = signature,
                SignatureAlgorithm = algorithm,
                SignatureKey = key,
                SignatureTimestamp = timestamp
            };
        }
    }
}

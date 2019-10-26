using System;
using System.Collections.Generic;
using System.Text;
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
            if (!req.Headers.TryGetValue("SignedHeaders", out StringValues signedHeadersValues))
            {
                throw new ArgumentException("SignedHeaders header not present");
            }

            if (signedHeadersValues.Count != 1)
            {
                throw new ArgumentException("unexpected repeating SignedHeaders header");
            }

            string signedHeaders = signedHeadersValues[0];

            if (req.Headers.TryGetValue("Authorization", out StringValues svs))
            {
                if (svs.Count != 1)
                {
                    throw new ArgumentException("unexpected repeating Authorization header");
                }
                
                var sigParts = svs[0].Split(':');
                if (sigParts.Length != 4)
                {
                    throw new ArgumentException("expected Authorization header to be 4 parts split by ':'");
                }

                var algorithm = sigParts[0];
                // TODO verify algorithm
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
            else
            {
                throw new ArgumentException("request missing Authorization header");
            }            
        }
    }
}

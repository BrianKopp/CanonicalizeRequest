using System;
using Microsoft.AspNetCore.Http;

namespace CanonicalizeRequest
{
    public class RequestAuthenticator : IRequestAuthenticator
    {
        private readonly long SecondsDriftAllowed;
        private readonly ICryptoVerifier Verifier;
        public RequestAuthenticator(ICryptoVerifier verifier, long secondsDriftAllowed)
        {
            Verifier = verifier;
            SecondsDriftAllowed = secondsDriftAllowed;
        }
        public bool IsRequestAuthentic(HttpRequest request)
        {
            try
            {
                var parts = RequestAuthenticationParts.MakeFromRequest(request);
                if (!IsTimestampValid(parts.SignatureTimestamp, GetCurrentTimestamp()))
                {
                    return false;
                }

                var canonicalRepresentation = RequestCanonicalizer.CanonicalRepresentation(request);
                var stringToSign = RequestCanonicalizer.CreateStringToSign(
                    parts.SignatureAlgorithm, parts.SignatureTimestamp, canonicalRepresentation);

                return Verifier.VerifyText(parts.SignatureAlgorithm, parts.SignatureKey,
                    stringToSign, parts.Signature);
            }
            catch
            {
                return false;
            }
        }
        private long GetCurrentTimestamp()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }
        public bool IsTimestampValid(long timestamp, long currentTimestamp)
        {
            return (timestamp <= (currentTimestamp + SecondsDriftAllowed))
                && (timestamp >= (currentTimestamp - SecondsDriftAllowed));
        }
    }
}

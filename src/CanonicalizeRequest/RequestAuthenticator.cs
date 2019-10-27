using System;
using Microsoft.AspNetCore.Http;

namespace CanonicalizeRequest
{
    public class RequestAuthenticator : IRequestAuthenticator
    {
        private readonly long SecondsDriftAllowed;
        private readonly ICryptoVerifier Verifier;
        private readonly IRequestCanonicalizer Canonicalizer;
        private readonly IRequestPartMaker PartMaker;
        public RequestAuthenticator(IRequestPartMaker maker, ICryptoVerifier verifier,
            IRequestCanonicalizer canonicalizer, long secondsDriftAllowed)
        {
            PartMaker = maker;
            Verifier = verifier;
            SecondsDriftAllowed = secondsDriftAllowed;
            Canonicalizer = canonicalizer;
        }
        public bool IsRequestAuthentic(HttpRequest request)
        {
            try
            {
                var parts = PartMaker.MakeFromRequest(request);
                if (!IsTimestampValid(parts.SignatureTimestamp, GetCurrentTimestamp()))
                {
                    return false;
                }

                var canonicalRepresentation = Canonicalizer.MakeCanonicalRepresentation(request);
                var stringToSign = Canonicalizer.MakeStringToSign(
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
        public static RequestAuthenticator New(ICryptoVerifier verifier, long secondsDriftAllowed)
        {
            return new RequestAuthenticator(new RequestPartMaker(),
                verifier, new RequestCanonicalizer(), secondsDriftAllowed);
        }
    }
}

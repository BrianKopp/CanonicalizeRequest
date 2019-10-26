using Microsoft.AspNetCore.Http;

namespace CanonicalizeRequest
{
    public class RequestAuthenticator : IRequestAuthenticator
    {
        private readonly long SecondsDriftAllowed;
        private readonly ICryptoVerifier Verifier;
        private readonly ITimestampProvider TimestampProvider;
        public RequestAuthenticator(ICryptoVerifier verifier, long secondsDriftAllowed, ITimestampProvider timestampProvider)
        {
            Verifier = verifier;
            SecondsDriftAllowed = secondsDriftAllowed;
            TimestampProvider = timestampProvider;
        }
        public bool IsRequestAuthentic(HttpRequest request)
        {
            try
            {
                var parts = RequestAuthenticationParts.MakeFromRequest(request);
                if (!IsTimestampValid(parts.SignatureTimestamp, TimestampProvider.Now()))
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
        public bool IsTimestampValid(long timestamp, long currentTimestamp)
        {
            return (timestamp <= (currentTimestamp + SecondsDriftAllowed))
                && (timestamp >= (currentTimestamp - SecondsDriftAllowed));
        }
    }
}

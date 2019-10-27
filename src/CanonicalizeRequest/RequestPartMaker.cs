using Microsoft.AspNetCore.Http;

namespace CanonicalizeRequest
{
    public class RequestPartMaker : IRequestPartMaker
    {
        public RequestAuthenticationParts MakeFromRequest(HttpRequest req)
        {
            return RequestAuthenticationParts.MakeFromRequest(req);
        }
    }
}

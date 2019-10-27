using Microsoft.AspNetCore.Http;

namespace CanonicalizeRequest
{
    public interface IRequestPartMaker
    {
        RequestAuthenticationParts MakeFromRequest(HttpRequest req);
    }
}

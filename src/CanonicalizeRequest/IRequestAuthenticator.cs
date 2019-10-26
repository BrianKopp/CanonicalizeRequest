using Microsoft.AspNetCore.Http;

namespace CanonicalizeRequest
{
    public interface IRequestAuthenticator
    {
        bool IsRequestAuthentic(HttpRequest request);
    }
}

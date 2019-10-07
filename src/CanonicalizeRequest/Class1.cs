using System;
using Microsoft.AspNetCore.Http;

namespace CanonicalizeRequest
{
    public static class RequestCanonicalizer
    {
        public static string CanonicalRepresentation(this HttpRequest req)
        {
            var representation = "";
            representation += req.Method.ToUpper() + "\n";
            representation += req.Path + "\n";
        }
    }
}

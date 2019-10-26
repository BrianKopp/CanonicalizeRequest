using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace CanonicalizeRequest
{
    public static class RequestCanonicalizer
    {
        public static string CanonicalRepresentation(this HttpRequest req)
        {
            using (var sr = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
            {
                return CreateCanonicalRepresentation(
                    req.Method,
                    req.Path,
                    req.Query,
                    req.Headers,
                    req.Headers["SignedHeaders"],
                    sr.ReadToEnd());
            }
        }
        public static string CreateCanonicalRepresentation(
            string httpMethod,
            string httpPath,
            IEnumerable<KeyValuePair<string, StringValues>> queryParameters,
            IDictionary<string, StringValues> headers,
            string signedHeaders,
            string body)
        {
            return CanonicalizeMethod(httpMethod) + "\n"
                + CanonicalizePath(httpPath) + "\n"
                + CanonicalizeQueryParameters(queryParameters) + "\n"
                + CanonicalizeHeaders(headers) + "\n"
                + CanonicalizeSignedHeaders(signedHeaders) + "\n"
                + CanonicalizeRequestBody(body);
        }
        public static string CreateStringToSign(
            string algorithm,
            long requestTimestamp,
            string canonicalRequest)
        {
            return algorithm + "\n"
                + requestTimestamp + "\n"
                + HashStringSha256(canonicalRequest);
        }
        public static string CanonicalizeMethod(string method)
        {
            return method.Trim().ToUpper();
        }
        public static string CanonicalizePath(string path)
        {
            return WebUtility.UrlEncode(path.Trim().ToLower());
        }
        public static string CanonicalizeQueryParameters(IEnumerable<KeyValuePair<string, StringValues>> queryParameters)
        {
            if (queryParameters == null || !queryParameters.Any())
            {
                return "";
            }

            var nonEmptySortedParams = queryParameters
                .Where(kvp => !string.IsNullOrEmpty(kvp.Key) && !string.IsNullOrWhiteSpace(kvp.Key))
                .OrderBy(kvp => kvp.Key, StringComparer.Ordinal);

            var queryStringRepresentations = new List<string>();
            foreach(var queryParamKvp in nonEmptySortedParams)
            {
                var queryParamName = queryParamKvp.Key;
                var values = queryParamKvp.Value;
                if (values.Any())
                {
                    var urlEncodedParamName = WebUtility.UrlEncode(queryParamName);
                    var sortedValues = values.OrderBy(v => v, StringComparer.Ordinal);
                    foreach (var sv in sortedValues)
                    {
                        queryStringRepresentations.Add($"{urlEncodedParamName}={WebUtility.UrlEncode(sv)}");
                    }
                }
            }

            return string.Join("&", queryStringRepresentations);
        }
        public static string CanonicalizeHeaders(IDictionary<string, StringValues> headers)
        {
            var headersAndLowercaseHeaders = headers.Keys.ToDictionary(k => k, k => k.ToLower());
            var originalHeadersSortedByLowercase = headersAndLowercaseHeaders
                .OrderBy(kvp => kvp.Value, StringComparer.Ordinal)
                .Select(kvp => kvp.Key);
            
            var headerRepresentations = new List<string>();
            var regex = new Regex("[ ]{2,}", RegexOptions.None);
            foreach(var originalHeader in originalHeadersSortedByLowercase)
            {
                var headerValues = headers[originalHeader].Select(v => regex.Replace(v.Trim(), " "));
                var headerValuesString = string.Join(",", headerValues);
                headerRepresentations.Add($"{headersAndLowercaseHeaders[originalHeader]}={headerValuesString}");
            }

            return string.Join("\n", headerRepresentations);
        }
        public static string CanonicalizeSignedHeaders(string signedHeaders)
        {
            return string.Join(";", signedHeaders.Split(';')
                .Select(s => s.ToLower())
                .OrderBy(s => s, StringComparer.Ordinal));
        }
        public static string CanonicalizeRequestBody(string body)
        {
            return HashStringSha256(body);
        }
        public static string HashStringSha256(string s)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(s));
                var sb = new StringBuilder();
                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}

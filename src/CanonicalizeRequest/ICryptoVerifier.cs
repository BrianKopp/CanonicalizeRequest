using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalizeRequest
{
    public interface ICryptoVerifier
    {
        bool VerifyText(string algorithm, string key, string strignToSign, string signature);
    }
}

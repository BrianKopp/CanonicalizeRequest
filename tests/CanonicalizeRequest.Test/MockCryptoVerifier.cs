using System;
using System.Collections.Generic;
using System.Text;

namespace CanonicalizeRequest.Tests
{
    public class MockCryptoVerifier : ICryptoVerifier
    {
        public string CalledWithAlgorithm;
        public string CalledWithKey;
        public string CalledWithStringToSign;
        public string CalledWithSignature;
        public int CallCount;
        public Func<bool> Response;
        public MockCryptoVerifier(Func<bool> response)
        {
            Response = response;
            CallCount = 0;
        }
        public bool VerifyText(string algorithm, string key, string stringToSign, string signature)
        {
            CallCount += 1;
            CalledWithAlgorithm = algorithm;
            CalledWithKey = key;
            CalledWithStringToSign = stringToSign;
            CalledWithSignature = signature;
            return Response();
        }
    }
}

using System;
using System.Security.Cryptography;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace BlockChainMachine.Core
{
    [Serializable]
    public struct Transaction
    {
        public string UserHash { get; set; }
        public byte[] Data { get; set; }
        public byte[] Signature { get; set; }
        // ReSharper disable once MemberCanBePrivate.Global
        public RSAParameters Key { get; set; }
        public bool Valid => Validate();

        private bool Validate()
        {
            try
            {
                var crypt = new RSACryptoServiceProvider();
                crypt.ImportParameters(Key);
                return crypt.VerifyData(Data, new SHA256CryptoServiceProvider(), Signature);
            }
            catch (CryptographicException)
            {
                return false;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Crypt.Json
{
    // A class to encrypt and decrypt byte arrays using aes, takes key and iv in its constructor
    public class AesCrypt : IDisposable
    {

        public void Dispose()
        {
            this.AesAlg.Dispose();
        }

        private readonly byte[] _key;
        private readonly byte[] _iv;
        private readonly Aes AesAlg;
        public AesCrypt(byte[] key, byte[] iv)
        {
            AesAlg = Aes.Create();
            if (key == null || !IsLegalKeySize(key.Length))
                throw new ArgumentException("Key must be 16, 24 or 32 bytes long.", nameof(key));
            if (iv == null || (iv.Length != 16))
                throw new ArgumentNullException(nameof(iv));

            _key = key;
            _iv = iv;

        }

        private bool IsLegalKeySize(int size)
        {
            foreach (var keySize in AesAlg.LegalKeySizes)
            {
                for (int i = keySize.MinSize; i < keySize.MaxSize; i += keySize.SkipSize)
                {
                    if (i == size * 8)
                        return true;
                }
            }

            return false;
        }

        // Encrypts the given byte array using aes
        public byte[] Encrypt(byte[] data)
        {
            // Implementation of encryption logic
            if (data == null || data.Length <= 0)
                throw new ArgumentNullException(nameof(data));

            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            AesAlg.Key = this._key;
            AesAlg.IV = this._iv;

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = AesAlg.CreateEncryptor(AesAlg.Key, AesAlg.IV);

            // Create the streams used for encryption.
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var ms = new MemoryStream(data))
                    {
                        ms.CopyTo(csEncrypt);
                    }
                }

                encrypted = msEncrypt.ToArray();
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }
        // Decrypts the given byte array using aes
        public byte[] Decrypt(byte[] data)
        {
            // Implementation of decryption logic
            // Check arguments.
            if (data == null || data.Length <= 0)
                throw new ArgumentNullException(nameof(data));

            // Declare the string used to hold
            // the decrypted text.
            byte[] result;

            // Create an Aes object
            // with the specified key and IV.
            AesAlg.Key = this._key;
            AesAlg.IV = this._iv;

            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = AesAlg.CreateDecryptor(AesAlg.Key, AesAlg.IV);

            // Create the streams used for decryption.
            using (MemoryStream msDecrypt = new MemoryStream(data))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    // read all bytes from csDecrypt and put them into result
                    using (MemoryStream ms = new MemoryStream())
                    {
                        csDecrypt.CopyTo(ms);
                        result = ms.ToArray();
                    }
                }
            }

            return result;
        }
    }
}

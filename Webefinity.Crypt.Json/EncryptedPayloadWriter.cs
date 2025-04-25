using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Crypt.Json
{
    public class EncryptedPayloadWriter
    {
        public static void WriteEncryptedPayload(string filePath, EncryptedPayload encryptedJson)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                WriteEncryptedPayload(fileStream, encryptedJson);
            }
        }

        private static byte[] Combine(byte[] first, byte[] second)
        {
            var result = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, result, 0, first.Length);
            Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
            return result;
        }

        public static void WriteEncryptedPayload(Stream outputStream, EncryptedPayload encryptedJson)
        {
            using (var streamWriter = new BinaryWriter(outputStream))
            {
                streamWriter.Write(new byte[] { 2, 9, 0, 1 });
                var hash = SHA256.HashData(Combine(encryptedJson.Iv, encryptedJson.Bytes));
                streamWriter.Write((UInt32)hash.Length);
                streamWriter.Write((UInt32)encryptedJson.Iv.Length);
                streamWriter.Write((UInt32)encryptedJson.Bytes.Length);
                streamWriter.Write(hash);
                streamWriter.Write(encryptedJson.Iv);
                streamWriter.Write(encryptedJson.Bytes);
            }
        }

        public static EncryptedPayload ReadEncryptedPayload(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return ReadEncryptedPayload(fileStream);
            }
        }

        public static EncryptedPayload ReadEncryptedPayload(Stream inputStream)
        {
            using (var streamReader = new BinaryReader(inputStream))
            {
                var version = streamReader.ReadBytes(4);
                if (version[0] != 2 || version[1] != 9 || version[2] != 0 || version[3] != 1)
                    throw new HeaderException("Invalid file format");

                var hashLength = streamReader.ReadUInt32();
                var ivLength = streamReader.ReadUInt32();
                var dataLength = streamReader.ReadUInt32();
                if (inputStream.Length != hashLength + ivLength + dataLength + 16)
                {
                    throw new FileLengthException("Invalid input stream length");
                }
                var hash = streamReader.ReadBytes((int)hashLength);
                var iv = streamReader.ReadBytes((int)ivLength);
                var data = streamReader.ReadBytes((int)dataLength);

                var hash2 = SHA256.HashData(Combine(iv, data));
                if (!hash.SequenceEqual(hash2))
                    throw new HashException("Invalid hash");

                return new EncryptedPayload(iv, data);
            }
        }

    }
}

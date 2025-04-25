namespace Webefinity.Crypt.Json;

public class EncryptedPayload
{
    public EncryptedPayload(byte[] iv, byte[] bytes)
    {
        Iv = iv;
        Bytes = bytes;
    }

    // Iv should be 16 bytes
    public byte[] Iv { get; set; } = [];

    // Encrypted json can be any length
    public byte[] Bytes { get; set; } = [];
}

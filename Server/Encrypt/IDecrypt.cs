namespace Server.Encrypt
{
    internal interface IDecrypt
    {
        byte[] Decrypt(byte[] data);
    }
}

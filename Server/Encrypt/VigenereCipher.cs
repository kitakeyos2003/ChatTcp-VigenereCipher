namespace Server.Encrypt
{
    internal class VigenereCipher : IDecrypt, IEncrypt
    {
        private byte[] key;
        public VigenereCipher(byte[] key)
        {
            this.key = key;
        }

        public byte[] Decrypt(byte[] data)
        {
            byte[] result = new byte[data.Length];
            int keyIndex = 0;
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)((data[i] - key[keyIndex]) % 256); 
                keyIndex = (keyIndex + 1) % key.Length;
            }
            return result;
        }

        public byte[] Encrypt(byte[] data)
        {
            byte[] result = new byte[data.Length];
            int keyIndex = 0;
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)((data[i] + key[keyIndex]) % 256);
                keyIndex = (keyIndex + 1) % key.Length;
            }
            return result;
        }
    }
}

using System;

namespace Koyashiro.UdonJwt.SHA2
{
    public static class SHA256
    {
        private const int MESSAGE_BLOCK_LENGTH = 64;
        public const int DIGEST_LENGTH = 32;

        public static byte[] ComputeHash(byte[] buffer)
        {
            var K = new uint[MESSAGE_BLOCK_LENGTH] {
                0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
                0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
                0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
                0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
                0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
                0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
                0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
                0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
            };

            var hBuf = new uint[] { 0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a, 0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19 };

            var paddedBuffer = Pad(buffer);
            var wb = Divide(paddedBuffer);

            foreach (var w in wb)
            {
                for (var i = 16; i < MESSAGE_BLOCK_LENGTH; i++)
                {
                    w[i] = SmallSigma1(w[i - 2]) + w[i - 7] + SmallSigma0(w[i - 15]) + w[i - 16];
                }

                var a = hBuf[0];
                var b = hBuf[1];
                var c = hBuf[2];
                var d = hBuf[3];
                var e = hBuf[4];
                var f = hBuf[5];
                var g = hBuf[6];
                var h = hBuf[7];

                for (var i = 0; i < MESSAGE_BLOCK_LENGTH; i++)
                {
                    var t1 = h + LargeSigma1(e) + Ch(e, f, g) + K[i] + w[i];
                    var t2 = LargeSigma0(a) + Maj(a, b, c);
                    h = g;
                    g = f;
                    f = e;
                    e = d + t1;
                    d = c;
                    c = b;
                    b = a;
                    a = t1 + t2;
                }

                hBuf[0] = a + hBuf[0];
                hBuf[1] = b + hBuf[1];
                hBuf[2] = c + hBuf[2];
                hBuf[3] = d + hBuf[3];
                hBuf[4] = e + hBuf[4];
                hBuf[5] = f + hBuf[5];
                hBuf[6] = g + hBuf[6];
                hBuf[7] = h + hBuf[7];
            }

            var digest = new byte[DIGEST_LENGTH];
            for (var i = 0; i < DIGEST_LENGTH / 4; i++)
            {
                digest[4 * i] = (byte)((hBuf[i] >> 24) & 0xff);
                digest[4 * i + 1] = (byte)((hBuf[i] >> 16) & 0xff);
                digest[4 * i + 2] = (byte)((hBuf[i] >> 8) & 0xff);
                digest[4 * i + 3] = (byte)(hBuf[i] & 0xff);
            }
            return digest;
        }

        private static byte[] Pad(byte[] input)
        {
            var inputLength = input.LongLength;
            var bufferLength = (((inputLength + 8L) / MESSAGE_BLOCK_LENGTH) + 1L) * MESSAGE_BLOCK_LENGTH;

            var buffer = new byte[bufferLength];
            Array.Copy(input, buffer, inputLength);
            buffer[inputLength] = 0x80;

            var bitsLength = inputLength * 8L;
            buffer[bufferLength - 8] = (byte)((bitsLength >> 56) & 0xff);
            buffer[bufferLength - 7] = (byte)((bitsLength >> 48) & 0xff);
            buffer[bufferLength - 6] = (byte)((bitsLength >> 40) & 0xff);
            buffer[bufferLength - 5] = (byte)((bitsLength >> 32) & 0xff);
            buffer[bufferLength - 4] = (byte)((bitsLength >> 24) & 0xff);
            buffer[bufferLength - 3] = (byte)((bitsLength >> 16) & 0xff);
            buffer[bufferLength - 2] = (byte)((bitsLength >> 8) & 0xff);
            buffer[bufferLength - 1] = (byte)(bitsLength & 0xff);

            return buffer;
        }

        private static uint[][] Divide(byte[] input)
        {
            var inputLength = input.LongLength;
            var mLength = inputLength / MESSAGE_BLOCK_LENGTH;
            var mu = new uint[mLength][];
            for (var i = 0; i < mLength; i++)
            {
                mu[i] = new uint[MESSAGE_BLOCK_LENGTH];
                var ix = i * MESSAGE_BLOCK_LENGTH;
                for (var j = 0; j < 16; j++)
                {
                    var ifs = ix + (4 * j);
                    mu[i][j] = ((uint)input[ifs] << 24) | ((uint)input[ifs + 1] << 16) | ((uint)input[ifs + 2] << 8) | ((uint)input[ifs + 3]);
                }
            }

            return mu;
        }

        private static uint Ch(uint x, uint y, uint z)
        {
            return (x & y) ^ (~x & z);
        }

        private static uint Maj(uint x, uint y, uint z)
        {
            return (x & y) ^ (x & z) ^ (y & z);
        }

        private static uint LargeSigma0(uint x)
        {
            return Rotr(x, 2) ^ Rotr(x, 13) ^ Rotr(x, 22);
        }

        private static uint LargeSigma1(uint x)
        {
            return Rotr(x, 6) ^ Rotr(x, 11) ^ Rotr(x, 25);
        }

        private static uint SmallSigma0(uint x)
        {
            return Rotr(x, 7) ^ Rotr(x, 18) ^ Shr(x, 3);
        }

        private static uint SmallSigma1(uint x)
        {
            return Rotr(x, 17) ^ Rotr(x, 19) ^ Shr(x, 10);
        }

        private static uint Rotr(uint x, int n)
        {
            return x << (32 - n) | x >> n;
        }

        private static uint Shr(uint x, int n)
        {
            return x >> n;
        }
    }
}

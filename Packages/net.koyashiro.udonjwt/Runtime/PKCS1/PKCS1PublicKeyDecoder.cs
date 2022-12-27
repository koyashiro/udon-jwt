using System;
using System.Numerics;
using Koyashiro.UdonJwt.Asn1;

namespace Koyashiro.UdonJwt.PKCS1
{
    public static class PKCS1PublicKeyDecoder
    {
        public static bool TryDecode(byte[] input, out BigInteger m, out int n)
        {
            return TryDecode(input, 0, out m, out n);
        }

        public static bool TryDecode(byte[] input, int startIndex, out BigInteger n, out int e)
        {
            var index = startIndex;

            // SEQUENCE
            if (!Asn1Decoder.TryDecode(input, index, out var kind, out var valueStartIndex, out var valueLength) || kind != Asn1TagKind.SequenceAndSequenceOf)
            {
                n = default;
                e = default;
                return false;
            }
            index = valueStartIndex;

            // INTEGER
            if (!Asn1Decoder.TryDecode(input, index, out kind, out valueStartIndex, out valueLength) || kind != Asn1TagKind.Integer)
            {
                n = default;
                e = default;
                return false;
            }
            var nBytes = new byte[valueLength];
            Array.Copy(input, valueStartIndex, nBytes, 0, valueLength);
            // bigendian to littleendian
            Array.Reverse(nBytes);
            n = new BigInteger(nBytes);
            index = valueStartIndex + valueLength;

            // INTEGER
            if (!Asn1Decoder.TryDecode(input, index, out kind, out valueStartIndex, out valueLength) || kind != Asn1TagKind.Integer)
            {
                n = default;
                e = default;
                return false;
            }
            e = 0;
            for (var i = valueStartIndex; i < valueStartIndex + valueLength; i++)
            {
                e = e << 8 | input[i];
            }
            index = valueStartIndex + valueLength;

            if (input.Length != index)
            {
                n = default;
                e = default;
                return false;
            }

            return true;
        }
    }
}

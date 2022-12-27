using System.Numerics;
using Koyashiro.UdonJwt.Asn1;
using Koyashiro.UdonJwt.PKCS1;

namespace Koyashiro.UdonJwt.PKCS8
{
    public static class PKCS8PublicKeyDecoder
    {
        public static bool TryDecode(byte[] input, out BigInteger n, out int e)
        {
            var index = 0;

            // SEQUENCE
            if (!Asn1Decoder.TryDecode(input, index, out var kind, out var valueStartIndex, out var valueLength) || kind != Asn1TagKind.SequenceAndSequenceOf)
            {
                n = default;
                e = default;
                return false;
            }
            index = valueStartIndex;

            // SEQUENCE
            if (!Asn1Decoder.TryDecode(input, index, out kind, out valueStartIndex, out valueLength) || kind != Asn1TagKind.SequenceAndSequenceOf)
            {
                n = default;
                e = default;
                return false;
            }
            index = valueStartIndex;

            // OBJECT IDENTIFIER
            if (!Asn1Decoder.TryDecode(input, index, out kind, out valueStartIndex, out valueLength) || kind != Asn1TagKind.ObjectIdentifier)
            {
                n = default;
                e = default;
                return false;
            }

            var RSA_ENCRYPTION = new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01 };
            if (valueLength != RSA_ENCRYPTION.Length)
            {
                n = default;
                e = default;
                return false;
            }
            for (var i = 0; i < RSA_ENCRYPTION.Length; i++)
            {
                if (input[valueStartIndex + i] != RSA_ENCRYPTION[i])
                {
                    n = default;
                    e = default;
                    return false;
                }
            }
            index = valueStartIndex + valueLength;

            // NULL
            if (!Asn1Decoder.TryDecode(input, index, out kind, out valueStartIndex, out valueLength) || kind != Asn1TagKind.Null)
            {
                n = default;
                e = default;
                return false;
            }
            index = valueStartIndex + valueLength;

            // NULL
            if (!Asn1Decoder.TryDecode(input, index, out kind, out valueStartIndex, out valueLength) || kind != Asn1TagKind.BitString)
            {
                n = default;
                e = default;
                return false;
            }
            index = valueStartIndex;

            return PKCS1PublicKeyDecoder.TryDecode(input, index, out n, out e);
        }
    }
}

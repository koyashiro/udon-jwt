using System;
using System.Linq;
using Koyashiro.UdonJwt.PKCS8;

namespace Koyashiro.UdonJwt
{
    public static class PublicKeyDecoder
    {
        public static bool TryDecode(string input, out byte[] n, out int e)
        {
            if (input == null)
            {
                n = default;
                e = default;
                return false;
            }

            var trimmedInput = input.Replace("\r\n", "\n").Replace('\r', '\n').Trim();
            var lines = trimmedInput.Split('\n');
            if (lines.Length < 3)
            {
                n = default;
                e = default;
                return false;
            }

            if (lines[0] != "-----BEGIN PUBLIC KEY-----")
            {
                n = default;
                e = default;
                return false;
            }

            if (lines[lines.Length - 1] != "-----END PUBLIC KEY-----")
            {
                n = default;
                e = default;
                return false;
            }

            var base64Lines = string.Join("\n", lines.Skip(1).Take(lines.Length - 2));
            var bytes = Convert.FromBase64String(base64Lines);
            return PKCS8PublicKeyDecoder.TryDecode(bytes, out n, out e);
        }
    }
}

namespace Koyashiro.UdonJwt.Asn1
{
    public static class Asn1Decoder
    {
        public static bool TryDecode(byte[] input, int startIndex, out Asn1TagKind kind, out int valueStartIndex, out int valueLength)
        {
            var index = startIndex;

            if (input == null)
            {
                kind = default;
                valueStartIndex = default;
                valueLength = default;
                return false;
            }

            if (input.Length <= startIndex)
            {
                kind = default;
                valueStartIndex = default;
                valueLength = default;
                return false;
            }

            kind = (Asn1TagKind)input[index++];

            if (input.Length <= startIndex)
            {
                kind = default;
                valueStartIndex = default;
                valueLength = default;
                return false;
            }

            valueLength = (int)input[index++];
            if (valueLength > 0x80)
            {
                var count = valueLength - 0x80;

                if (input.Length < startIndex + count)
                {
                    kind = default;
                    valueStartIndex = default;
                    valueLength = default;
                    return false;
                }

                valueLength = 0;
                for (var i = 0; i < count; i++)
                {
                    valueLength = valueLength << 8 | input[index++];
                }
            }

            if (input.Length < startIndex + valueLength)
            {
                kind = default;
                valueStartIndex = default;
                valueLength = default;
                return false;
            }

            while (input[index] == 0x00)
            {
                valueLength -= 1;
                index++;
            }

            valueStartIndex = index;
            index += valueLength;
            return true;
        }
    }
}

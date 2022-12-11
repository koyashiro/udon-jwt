namespace Koyashiro.UdonJwt.Numerics
{
    public static class UnsignedBigInteger
    {
        public static uint[] FromBytes(byte[] input)
        {
            var result = new uint[input.Length / 4];
            for (var i = 0; i < result.Length; i++)
            {
                var a = (uint)input[4 * i] << 24;
                var b = (uint)input[4 * i + 1] << 16;
                var c = (uint)input[4 * i + 2] << 8;
                var d = (uint)input[4 * i + 3];
                result[result.Length - 1 - i] = a | b | c | d;
            }
            return result;
        }

        public static byte[] ToBytes(uint[] input)
        {
            var result = new byte[input.Length * 4];
            for (var i = 0; i < input.Length; i++)
            {
                var x = input[input.Length - 1 - i];
                result[4 * i] = (byte)((x >> 24) & 0xff);
                result[4 * i + 1] = (byte)((x >> 16) & 0xff);
                result[4 * i + 2] = (byte)((x >> 8) & 0xff);
                result[4 * i + 3] = (byte)(x & 0xff);
            }
            return result;
        }

        public static uint[] Add(uint[] left, uint[] right)
        {
            var leftLength = left.Length;
            var rightLength = right.Length;

            var sum = new uint[leftLength + 1];

            var carry = 0UL;
            for (var i = 0; i < rightLength; i++)
            {
                var s = (ulong)left[i] + (ulong)right[i] + carry;
                sum[i] = (uint)(s << 32 >> 32);
                carry = s >> 32;
            }

            for (var i = rightLength; i < leftLength; i++)
            {
                var s = (ulong)left[i] + carry;
                sum[i] = (uint)(s << 32 >> 32);
                carry = s >> 32;
            }

            sum[leftLength] = (uint)carry;

            return sum;
        }

        public static uint[] Subtract(uint[] left, uint[] right)
        {
            var leftLength = left.Length;
            var rightLength = right.Length;

            var difference = new uint[left.Length];

            var carry = 1UL;
            for (var i = 0; i < rightLength; i++)
            {
                var s = (ulong)left[i] + (ulong)(~right[i]) + carry;
                difference[i] = (uint)(s << 32 >> 32);
                carry = s >> 32;
            }

            for (var i = rightLength; i < left.Length; i++)
            {
                var s = (ulong)left[i] + (ulong)(~0U) + carry;
                difference[i] = (uint)(s << 32 >> 32);
                carry = s >> 32;
            }

            return difference;
        }

        public static uint[] Multiply(uint[] left, uint[] right)
        {
            var buf = new uint[left.Length + right.Length];
            for (var i = 0; i < right.Length; i++)
            {
                var partialProd = new uint[left.Length + 1];
                var carry = 0UL;
                for (var j = 0; j < left.Length; j++)
                {
                    var p = (ulong)left[j] * (ulong)right[i] + carry;
                    partialProd[j] = (uint)(p << 32 >> 32);
                    carry = p >> 32;
                }
                partialProd[left.Length] = (uint)carry;

                carry = 0UL;
                for (var j = 0; j < partialProd.Length; j++)
                {
                    var s = (ulong)buf[i + j] + (ulong)partialProd[j] + carry;
                    buf[i + j] = (uint)(s << 32 >> 32);
                    carry = s >> 32;
                }
            }
            return buf;
        }

        public static bool Equals(uint[] left, uint[] right)
        {
            for (var i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool GreaterThanOrEqual(uint[] left, uint[] right)
        {
            var leftLength = left.Length;
            var rightLength = right.Length;

            if (leftLength == rightLength)
            {
                for (var i = left.Length - 1; i >= 0; i--)
                {
                    var l = left[i];
                    var r = right[i];
                    if (l > r)
                    {
                        return true;
                    }
                    else if (l < r)
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (leftLength > rightLength)
            {
                for (var i = leftLength - 1; i >= rightLength; i--)
                {
                    if (left[i] != 0)
                    {
                        return true;
                    }
                }
                for (var i = rightLength - 1; i >= 0; i--)
                {
                    var l = left[i];
                    var r = right[i];
                    if (l > r)
                    {
                        return true;
                    }
                    else if (l < r)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                for (var i = rightLength - 1; i >= leftLength; i--)
                {
                    if (right[i] != 0)
                    {
                        return true;
                    }
                }
                for (var i = leftLength - 1; i >= 0; i--)
                {
                    var l = left[i];
                    var r = right[i];
                    if (l > r)
                    {
                        return true;
                    }
                    else if (l < r)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public static string ToHexString(uint[] input)
        {
            int startIndex = 0;
            for (var i = input.Length - 1; i >= 1; i--)
            {
                if (input[i] != 0)
                {
                    startIndex = i;
                    break;
                }
            }
            var s = $"0x{input[startIndex]:x}";
            for (var i = startIndex - 1; i >= 0; i--)
            {
                s += input[i].ToString("x8");
            }
            return s;
        }
    }
}

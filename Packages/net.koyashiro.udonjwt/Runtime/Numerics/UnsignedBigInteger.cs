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

            var leftLong = GetConvertedArrayUintToUlong(left);
            var rightLong = GetConvertedArrayUintToUlong(right);

            var sum = new ulong[leftLength + 1];

            var carry = 0UL;
            for (var i = 0; i < rightLength; i++)
            {
                var s = leftLong[i] + rightLong[i] + carry;
                sum[i] = s & 0xffffffff;
                carry = s >> 32;
            }

            for (var i = rightLength; i < leftLength; i++)
            {
                var s = leftLong[i] + carry;
                sum[i] = s & 0xffffffff;
                carry = s >> 32;
            }

            sum[leftLength] = carry;
            var result = GetConvertedArrayUlongToUint(sum);
            return result;
        }

        public static uint[] Subtract(uint[] left, uint[] right)
        {
            var leftLength = left.Length;
            var rightLength = right.Length;

            var leftLong = GetConvertedArrayUintToUlong(left);
            var rightLong = GetConvertedArrayUintToUlong(right);

            var difference = new ulong[leftLength];

            var carry = 1UL;
            for (var i = 0; i < rightLength; i++)
            {
                var s = leftLong[i] + (ulong)(~(uint)rightLong[i]) + carry;
                difference[i] = s & 0xffffffff;
                carry = s >> 32;
            }

            for (var i = rightLength; i < leftLength; i++)
            {
                var s = leftLong[i] + (ulong)(~0U) + carry;
                difference[i] = s & 0xffffffff;
                carry = s >> 32;
            }

            var result = GetConvertedArrayUlongToUint(difference);
            return result;
        }

        public static uint[] Multiply(uint[] left, uint[] right)
        {
            var leftLength = left.Length;
            var rightLength = right.Length;

            var leftLong = GetConvertedArrayUintToUlong(left);
            var rightLong = GetConvertedArrayUintToUlong(right);
            var partialProd = new ulong[leftLength + 1];
            var buf = new ulong[leftLength + rightLength];

            var partialProdLength = partialProd.Length;

            for (var i = 0; i < rightLength; i++)
            {
                var carry = 0UL;
                for (var j = 0; j < leftLength; j++)
                {
                    var p = leftLong[j] * rightLong[i] + carry;
                    partialProd[j] = p & 0xffffffff;
                    carry = p >> 32;
                }
                partialProd[leftLength] = carry;

                carry = 0UL;
                for (var j = 0; j < partialProdLength; j++)
                {
                    var s = buf[i + j] + partialProd[j] + carry;
                    buf[i + j] = s & 0xffffffff;
                    carry = s >> 32;
                }
            }
            var result = GetConvertedArrayUlongToUint(buf);
            return result;
        }

        static ulong[] GetConvertedArrayUintToUlong(uint[] src)
        {
            var srcLength = src.Length;
            var ulongArray = new ulong[srcLength];
            for (int i = 0; i < srcLength; i++)
            {
                ulongArray[i] = (ulong)src[i];
            }
            return ulongArray;
        }

        static uint[] GetConvertedArrayUlongToUint(ulong[] src)
        {
            var srcLength = src.Length;
            var uintArray = new uint[srcLength];
            for (int i = 0; i < srcLength; i++)
            {
                uintArray[i] = (uint)(src[i] & 0xffffffff);
            }
            return uintArray;
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

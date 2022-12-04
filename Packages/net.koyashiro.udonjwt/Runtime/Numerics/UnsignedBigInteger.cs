using System;

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

        public static uint[] Add(uint[] left, uint right)
        {
            var sum = new uint[left.Length];
            var carry = (ulong)right;
            for (var i = 0; i < left.Length; i++)
            {
                var digit = (ulong)left[i] + carry;
                sum[i] = (uint)((digit) & 0xffffffff);
                carry = digit >> 32;
            }
            return sum;
        }

        public static uint[] Subtract(uint[] left, uint[] right)
        {
            var difference = new uint[left.Length];
            var carry = 1UL;
            for (var i = 0; i < left.Length; i++)
            {
                var digit = (ulong)left[i] + (ulong)(~right[i]) + carry;
                difference[i] = (uint)((digit) & 0xffffffff);
                carry = digit >> 32;
            }
            return difference;
        }

        public static uint[] Multiply(uint[] left, uint[] right)
        {
            var product = new uint[left.Length];
            for (var i = 0; i < left.Length; i++)
            {
                var partialProd = new uint[left.Length];
                var carry = 0UL;
                for (var j = 0; j < left.Length - i; j++)
                {
                    var p = (ulong)left[j] * (ulong)right[i] + carry;
                    partialProd[i + j] = (uint)(p << 32 >> 32);
                    carry = p >> 32;
                }
                carry = 0UL;
                for (var j = 0; j < left.Length; j++)
                {
                    var digit = (ulong)product[j] + (ulong)partialProd[j] + carry;
                    product[j] = (uint)(digit << 32 >> 32);
                    carry = digit >> 32;
                }
            }
            return product;
        }

        public static uint[] Inverse(uint[] value, out int fixedPointLength)
        {
            fixedPointLength = 2 * value.Length + 1;
            var length = 2 * fixedPointLength;

            var v = new uint[length];
            Array.Copy(value, v, value.Length);

            var two = new uint[length];
            two[0] = 2;
            UnsignedBigInteger.ShiftLeftAssign(two, fixedPointLength);

            var buf = new uint[length];
            buf[0] = 1;
            UnsignedBigInteger.ShiftLeftAssign(buf, fixedPointLength / 2);

            var prevBuf = new uint[length];

            while (!UnsignedBigInteger.Equals(buf, prevBuf))
            {
                Array.Copy(buf, prevBuf, length);
                buf = UnsignedBigInteger.Multiply(buf, UnsignedBigInteger.Subtract(two, UnsignedBigInteger.Multiply(v, buf)));
                UnsignedBigInteger.ShiftRightAssign(buf, fixedPointLength);
            }

            return buf;
        }

        public static uint[] RemainderWithReciprocal(uint[] dividend, uint[] divisor, uint[] reciprocalOfDivisor, int fixedPointLength)
        {
            var quotient = Multiply(dividend, reciprocalOfDivisor);
            ShiftRightAssign(quotient, fixedPointLength);
            var mulLow = Multiply(divisor, quotient);
            var mulHigh = Multiply(divisor, Add(quotient, 1));

            if (GreaterThan(mulHigh, dividend))
            {
                return Subtract(dividend, mulLow);
            }
            else
            {
                return Subtract(dividend, mulHigh);
            }
        }

        public static void ShiftRightAssign(uint[] value, int count)
        {
            Array.ConstrainedCopy(value, count, value, 0, value.Length - count);
            Array.Copy(new uint[count], 0, value, value.Length - count, count);
        }

        public static void ShiftLeftAssign(uint[] value, int count)
        {
            Array.Copy(value, 0, value, count, value.Length - count);
            Array.Copy(new uint[count], 0, value, 0, count);
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

        public static bool GreaterThan(uint[] left, uint[] right)
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
            return false;
        }

        public static bool GreaterThanOrEqual(uint[] left, uint[] right)
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

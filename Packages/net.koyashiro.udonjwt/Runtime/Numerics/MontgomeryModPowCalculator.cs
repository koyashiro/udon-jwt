using System;
using UnityEngine;
using UdonSharp;

namespace Koyashiro.UdonJwt.Numerics
{
    public class MontgomeryModPowCalculator : UdonSharpBehaviour
    {
        /// <summary>
        /// R
        /// </summary>
        [SerializeField]
        public uint[] _r;

        /// <summary>
        /// R2
        /// </summary>
        [SerializeField]
        public uint[] _r2;

        /// <summary>
        /// N
        /// </summary>
        [SerializeField]
        public uint[] _n;

        /// <summary>
        /// N'
        /// </summary>
        [SerializeField]
        public uint[] _nPrime;

        /// <summary>
        /// Callback
        /// </summary>
        [SerializeField]
        public MontgomeryModPowCalculatorCallback _callback;

        private int _e;
        private uint[] _base;
        private uint[] _buf;

        public uint[] R
        {
            get => _r;
            set => _r = value;
        }

        public uint[] R2
        {
            get => _r2;
            set => _r2 = value;
        }

        public uint[] N
        {
            get => _n;
            set => _n = value;
        }

        public uint[] NPrime
        {
            get => _nPrime;
            set => _nPrime = value;
        }

        public void ModPow(uint[] value, int exponent)
        {
            _e = exponent;
            _base = MontgomeryReduction(UnsignedBigInteger.Multiply(value, _r2));
            _buf = MontgomeryReduction(_r2);

            SendCustomEventDelayedFrames(nameof(_Loop), 0);
        }

        public void _Loop()
        {
            if (_e > 0)
            {
                if (_e % 2 != 0)
                {
                    _buf = MontgomeryReduction(UnsignedBigInteger.Multiply(_buf, _base));
                    Debug.Log(UnsignedBigInteger.ToHexString(_buf));
                }
                _base = MontgomeryReduction(UnsignedBigInteger.Multiply(_base, _base));
                _e >>= 1;

                SendCustomEventDelayedFrames(nameof(_Loop), 0);
            }
            else
            {
                SendCustomEventDelayedFrames(nameof(_End), 0);
            }
        }

        public void _End()
        {
            _callback.Result = MontgomeryReduction(_buf);
            _callback.SendCustomEventDelayedFrames(nameof(_callback.OnEnd), 0);
        }

        private uint[] MontgomeryReduction(uint[] t)
        {
            var baseLength = _n.Length;

            // T * N'
            var a = UnsignedBigInteger.Multiply(t, _nPrime);

            // (T * N') mod R
            var b = new uint[baseLength];
            Array.Copy(a, b, baseLength);

            // ((T * N') mod R) * N
            var c = UnsignedBigInteger.Multiply(b, _n);

            // T + (((T * N') mod R) * N)
            var d = UnsignedBigInteger.Add(c, t);

            // (T + (((T * N') mod R) * N)) / R
            var e = new uint[baseLength + 1];
            Array.Copy(d, baseLength, e, 0, baseLength + 1);

            uint[] f;
            if (UnsignedBigInteger.GreaterThanOrEqual(e, _n))
            {
                f = UnsignedBigInteger.Subtract(e, _n);
            }
            else
            {
                f = e;
            }

            var result = new uint[baseLength];
            Array.Copy(f, result, baseLength);

            return result;
        }
    }
}

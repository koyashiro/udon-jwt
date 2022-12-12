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

        private int _e;
        private int _totalStep;
        private uint[] _base;
        private uint[] _buf;
        private MontgomeryModPowCalculatorCallback _callback;

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

        public void ModPow(uint[] value, int exponent, MontgomeryModPowCalculatorCallback callback)
        {
            _e = exponent;
            _totalStep = 1;

            for (var e = exponent; e > 0; e >>= 1)
            {
                _totalStep += 1;
            }
            _base = MontgomeryReduction(UnsignedBigInteger.Multiply(value, _r2));
            _buf = MontgomeryReduction(_r2);
            _callback = callback;
            _callback.Progress = 0;

            SendCustomEventDelayedFrames(nameof(_Loop), 0);
        }

        public void _Loop()
        {
            if (_e > 0)
            {
                if (_e % 2 != 0)
                {
                    _buf = MontgomeryReduction(UnsignedBigInteger.Multiply(_buf, _base));
                }
                _base = MontgomeryReduction(UnsignedBigInteger.Multiply(_base, _base));
                _e >>= 1;
                _callback.Progress += 1f / (float)_totalStep;

                _callback.SendCustomEventDelayedFrames(nameof(_callback.OnProgress), 0);
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
            _callback.Progress = 1;
            _callback.SendCustomEventDelayedFrames(nameof(_callback.OnProgress), 0);
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

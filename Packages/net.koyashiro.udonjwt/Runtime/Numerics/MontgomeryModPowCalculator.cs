using System;
using UnityEngine;
using UdonSharp;

namespace Koyashiro.UdonJwt.Numerics
{
    public class MontgomeryModPowCalculator : UdonSharpBehaviour
    {
        [SerializeField]
        private uint[] _r;

        [SerializeField]
        private uint[] _r2;

        [SerializeField]
        private uint[] _n;

        [SerializeField]
        private uint[] _nPrime;

        private bool _isRunning;

        private int _e;

        private int _totalStep;

        private uint[] _base;

        private uint[] _buf;

        private MontgomeryModPowCalculatorCallback _callback;

        /// <summary>
        /// R
        /// </summary>
        public uint[] R
        {
            get => _r;
            set => _r = value;
        }

        /// <summary>
        /// R2
        /// </summary>
        public uint[] R2
        {
            get => _r2;
            set => _r2 = value;
        }

        /// <summary>
        /// N
        /// </summary>
        public uint[] N
        {
            get => _n;
            set => _n = value;
        }

        /// <summary>
        /// N'
        /// </summary>
        public uint[] NPrime
        {
            get => _nPrime;
            set => _nPrime = value;
        }

        public bool IsRunning => _isRunning;

        public void ModPow(uint[] value, int exponent, MontgomeryModPowCalculatorCallback callback)
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;
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
            _isRunning = false;
            _callback.Result = MontgomeryReduction(_buf);
            _callback.Progress = 1;
            _callback.SendCustomEventDelayedFrames(nameof(_callback.OnProgress), 0);
            _callback.SendCustomEventDelayedFrames(nameof(_callback.OnEnd), 0);
        }

        private uint[] MontgomeryReduction(uint[] t)
        {
            var baseLength = _n.Length;

            var tLow = new uint[baseLength];
            Array.Copy(t, tLow, baseLength);

            // T * N'
            var a = UnsignedBigInteger.Multiply(tLow, _nPrime);

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

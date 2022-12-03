using System;
using UnityEngine;
using UdonSharp;

namespace Koyashiro.UdonJwt
{
    [AddComponentMenu("")]
    public class ModPowCalculator : UdonSharpBehaviour
    {
        [SerializeField]
        private int _delayFrames = 1;

        [SerializeField]
        private ReciprocalCalculator _reciprocalCalculator;

        private uint[] _value;
        private int _exponent;
        private uint[] _modulus;
        private UdonSharpBehaviour _callbackThis;
        private string _callbackEventName;

        private int _fixedPointLength;
        private uint[] _reciprocalOfModulus;

        private uint[] _base;
        private uint[] _buf;
        private uint[] _m;
        private uint[] _output;

        public void Calculate(uint[] value, int exponent, uint[] modulus, UdonSharpBehaviour callbackThis, string callbackEventName)
        {
            Debug.Log($"{nameof(ModPowCalculator)}.{nameof(Calculate)}");

            _value = value;
            _exponent = exponent;
            _modulus = modulus;
            _callbackThis = callbackThis;
            _callbackEventName = callbackEventName;

            _reciprocalCalculator.Calculate(modulus, this, nameof(_ReciprocalCalculateEnd));
        }

        public void _ReciprocalCalculateEnd()
        {
            Debug.Log($"{nameof(ModPowCalculator)}.{nameof(_ReciprocalCalculateEnd)}");

            _fixedPointLength = _reciprocalCalculator.GetFixedPointLength();
            Debug.Log($"fixedPointLength: {_fixedPointLength}");
            _reciprocalOfModulus = _reciprocalCalculator.GetOutput();
            Debug.Log($"reciprocalOfModulus: {UnsignedBigInteger.ToHexString(_reciprocalOfModulus)}");

            _m = new uint[_reciprocalOfModulus.Length];
            Array.Copy(_modulus, _m, _modulus.Length);

            _base = new uint[_reciprocalOfModulus.Length];
            Array.Copy(_value, _base, _value.Length);
            _base = UnsignedBigInteger.RemainderWithReciprocal(_base, _m, _reciprocalOfModulus, _fixedPointLength);

            _buf = new uint[_reciprocalOfModulus.Length];
            _buf[0] = 1;

            SendCustomEventDelayedFrames(nameof(_CalculateLoop), _delayFrames);
        }

        public void _CalculateLoop()
        {
            Debug.Log($"{nameof(ModPowCalculator)}.{nameof(_CalculateLoop)}");

            if (_exponent == 0)
            {
                SendCustomEventDelayedFrames(nameof(_CalculateEnd), 0);
                return;
            }

            if (_exponent % 2 != 0)
            {
                _buf = UnsignedBigInteger.Multiply(_buf, _base);
                _buf = UnsignedBigInteger.RemainderWithReciprocal(_buf, _m, _reciprocalOfModulus, _fixedPointLength);
            }

            _base = UnsignedBigInteger.Multiply(_base, _base);
            _base = UnsignedBigInteger.RemainderWithReciprocal(_base, _m, _reciprocalOfModulus, _fixedPointLength);
            _exponent >>= 1;

            SendCustomEventDelayedFrames(nameof(_CalculateLoop), _delayFrames);
        }

        public void _CalculateEnd()
        {
            Debug.Log($"{nameof(ModPowCalculator)}.{nameof(_CalculateEnd)}");

            _output = new uint[_value.Length];
            Array.Copy(_buf, _output, _output.Length);
            _callbackThis.SendCustomEventDelayedFrames(_callbackEventName, _delayFrames);
        }

        public uint[] GetOutput()
        {
            return _output;
        }
    }
}

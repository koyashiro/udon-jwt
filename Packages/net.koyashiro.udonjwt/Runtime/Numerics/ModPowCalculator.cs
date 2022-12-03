using System;
using UnityEngine;
using UdonSharp;

namespace Koyashiro.UdonJwt.Numerics
{
    [AddComponentMenu("")]
    public class ModPowCalculator : UdonSharpBehaviour
    {
        private uint[] _value;
        private int _exponent;
        private uint[] _modulus;
        private uint[] _modulusInverse;
        private int _fixedPointLength;

        private UdonSharpBehaviour _callbackThis;
        private string _callbackEventName;

        private uint[] _base;
        private uint[] _buf;
        private uint[] _result;

        public uint[] Result => _result;

        public void Calculate(uint[] value, int exponent, uint[] modulus, uint[] modulusInverse, int fixedPointLength, UdonSharpBehaviour callbackThis, string callbackEventName)
        {
            _value = value;
            _exponent = exponent;
            _modulus = modulus;
            _modulusInverse = modulusInverse;
            _fixedPointLength = fixedPointLength;
            _callbackThis = callbackThis;
            _callbackEventName = callbackEventName;

            _base = new uint[modulusInverse.Length];
            Array.Copy(_value, _base, _value.Length);
            _base = UnsignedBigInteger.RemainderWithReciprocal(_base, _modulus, _modulusInverse, _fixedPointLength);

            _buf = new uint[modulusInverse.Length];
            _buf[0] = 1;

            SendCustomEventDelayedFrames(nameof(_CalculateLoop), 1);
        }

        public void _CalculateLoop()
        {
            if (_exponent == 0)
            {
                SendCustomEventDelayedFrames(nameof(_CalculateEnd), 0);
                return;
            }

            if (_exponent % 2 != 0)
            {
                _buf = UnsignedBigInteger.Multiply(_buf, _base);
                _buf = UnsignedBigInteger.RemainderWithReciprocal(_buf, _modulus, _modulusInverse, _fixedPointLength);
            }

            _base = UnsignedBigInteger.Multiply(_base, _base);
            _base = UnsignedBigInteger.RemainderWithReciprocal(_base, _modulus, _modulusInverse, _fixedPointLength);
            _exponent >>= 1;

            SendCustomEventDelayedFrames(nameof(_CalculateLoop), 1);
        }

        public void _CalculateEnd()
        {
            _result = new uint[_value.Length];
            Array.Copy(_buf, _result, _result.Length);
            _callbackThis.SendCustomEventDelayedFrames(_callbackEventName, 1);
        }
    }
}

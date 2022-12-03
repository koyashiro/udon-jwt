using System;
using UnityEngine;
using UdonSharp;

namespace Koyashiro.UdonJwt
{
    [AddComponentMenu("")]
    public class ReciprocalCalculator : UdonSharpBehaviour
    {
        [SerializeField]
        private int _delayFrames = 1;

        private uint[] _input;
        private UdonSharpBehaviour _callbackThis;
        private string _callbackEventName;

        private int _fixedPointLength;
        private int _length;
        private uint[] _inputExtended;
        private uint[] _two;
        private uint[] _buf;
        private uint[] _prevBuf;
        private uint[] _output;

        public void Calculate(uint[] input, UdonSharpBehaviour callbackThis, string callbackEventName)
        {
            Debug.Log($"{nameof(ReciprocalCalculator)}.{nameof(Calculate)}");

            _input = input;
            _callbackThis = callbackThis;
            _callbackEventName = callbackEventName;

            _fixedPointLength = 2 * input.Length + 1;
            _length = 4 * input.Length + 2;

            _inputExtended = new uint[_length];
            Array.Copy(input, _inputExtended, input.Length);

            _two = new uint[_length];
            _two[0] = 2;
            UnsignedBigInteger.ShiftLeftAssign(_two, _fixedPointLength);

            _buf = new uint[_length];
            _buf[0] = 1;
            UnsignedBigInteger.ShiftLeftAssign(_buf, _fixedPointLength / 2);

            _prevBuf = new uint[_length];

            SendCustomEventDelayedFrames(nameof(_CalculateLoop), _delayFrames);
        }

        public void _CalculateLoop()
        {
            Debug.Log($"{nameof(ReciprocalCalculator)}.{nameof(_CalculateLoop)}");

            if (UnsignedBigInteger.Equals(_buf, _prevBuf))
            {
                SendCustomEventDelayedFrames(nameof(_CalculateEnd), 10);
                return;
            }

            Array.Copy(_buf, _prevBuf, _length);
            _buf = UnsignedBigInteger.Multiply(_buf, UnsignedBigInteger.Subtract(_two, UnsignedBigInteger.Multiply(_inputExtended, _buf)));
            UnsignedBigInteger.ShiftRightAssign(_buf, _fixedPointLength);

            SendCustomEventDelayedFrames(nameof(_CalculateLoop), _delayFrames);
        }

        public void _CalculateEnd()
        {
            Debug.Log($"{nameof(ReciprocalCalculator)}.{nameof(_CalculateEnd)}");

            _output = _buf;
            _callbackThis.SendCustomEventDelayedFrames(_callbackEventName, _delayFrames);
        }

        public int GetFixedPointLength()
        {
            return _fixedPointLength;
        }

        public uint[] GetOutput()
        {
            return _output;
        }
    }
}

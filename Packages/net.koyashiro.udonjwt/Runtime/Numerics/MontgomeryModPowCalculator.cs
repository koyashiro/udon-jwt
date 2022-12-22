using System;
using UnityEngine;
using UdonSharp;

namespace Koyashiro.UdonJwt.Numerics
{
    public class MontgomeryModPowCalculator : UdonSharpBehaviour
    {
        [SerializeField, HideInInspector]
        private int _e;

        [SerializeField, HideInInspector]
        private uint[] _r;

        [SerializeField, HideInInspector]
        private uint[] _r2;

        [SerializeField, HideInInspector]
        private uint[] _n;

        [SerializeField, HideInInspector]
        private uint[] _nPrime;

        private bool _isRunning;

        private int _e;

        private int _totalStep;

        private uint[] _base;

        private uint[] _buf;

        private MontgomeryModPowCalculagorCallback _montgomeryModPowCalculagorCallback;
        private JwtDecorderCallback _jwtDecorderCallback;

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

        private void CofficientInitialize()
        {
            _r = new uint[] { 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000001 };
            _r2 = new uint[] { 0xe086a04e, 0x18a37f03, 0x9d6ddb73, 0x24ee0efe, 0xb756d0ae, 0xcf5c9205, 0x5e116a41, 0x7e3d5bd8, 0xabcea141, 0xc3df1e3a, 0x9c589752, 0xd91f7ff6, 0xbcca1448, 0x563c4e54, 0x5b03ddbb, 0xc4b40f30, 0x34af7eca, 0xfc496247, 0x86d4393c, 0x92b7a6b3, 0x913f557a, 0x20e0e08b, 0x196f7aca, 0x21007507, 0xeaba29b9, 0xf4c54a86, 0x83dec291, 0xc012cd00, 0xed8903ab, 0x4e62d201, 0xbf48d080, 0x51d5f5f8, 0x52c15122, 0xbde18472, 0xf53c22c6, 0x8ef6d571, 0xf90b13de, 0x637785b2, 0x8b66ea37, 0x9a5c79f3, 0x62ae3ad6, 0xcee77a03, 0xe17ccef0, 0xf90a7b76, 0x0a6704ab, 0x33dbaefc, 0x8a9b6b0e, 0x626a879a, 0xa71a2dd8, 0x52488361, 0x3505cc25, 0x24df5f7d, 0x32fa1bca, 0x4b4ccd9d, 0x185eec2f, 0x790b7f9f, 0xd8e36171, 0x4790905a, 0xbb68b409, 0x75a9e765, 0x0e8a18cb, 0xa6695eb7, 0xc787436b, 0x552d1975 };
            _n = new uint[] { 0xdd876619, 0xfcde8005, 0xa9f8105e, 0xd9cb60a5, 0x0a812a3d, 0x5a1f10fc, 0xdb3f99bd, 0xb0659477, 0x0c59abd4, 0x1369a293, 0x905296ff, 0x3a480932, 0xad95dca7, 0xdf46b759, 0x8780d054, 0xf0fb7151, 0x2504e92a, 0xb3c98650, 0x1b6638cb, 0xe75dd8a8, 0x0d4253c9, 0xb7c947d7, 0x49ea3c32, 0xece6f144, 0xe199c535, 0x21eef0f6, 0xc79ddd13, 0xee70c80e, 0x5976e7d3, 0xe05989bb, 0x2c78cd93, 0x0dcc723e, 0x8cdfc412, 0x1149e9b0, 0xa7bd4698, 0x9bad0d5f, 0xff4b3140, 0x1d0fffb3, 0x9d7212f2, 0x6b53197a, 0xd3bb153a, 0x9b55470f, 0x41b5b60f, 0xa261a6c8, 0x242e3481, 0x7d07c2f8, 0x41be51d8, 0x008c898f, 0x3eb96b5c, 0x688552c0, 0x5a14481b, 0x59fa5790, 0x5799deb1, 0x3fb8ee62, 0x5b2dbf47, 0x111daec1, 0xa038de38, 0x8c6c6c4b, 0x21cd6869, 0x9414cc81, 0x4a3190f1, 0xb282c9b5, 0x0d2ed294, 0xbcaaf97c };
            _nPrime = new uint[] { 0x90d169d7, 0x13c89bee, 0x8c2eec28, 0xd8ac3289, 0x334b7e51, 0x8fb7fe44, 0x21bae550, 0xd4042ff0, 0xba59aa27, 0xf643c578, 0x6e92b0fb, 0x2a19df93, 0xa975d66a, 0xfde5a9a6, 0x73a65179, 0xc6adab28, 0xd9796c7a, 0xa29fd54a, 0x0cba57d0, 0xbdedac74, 0x1986ad46, 0x065ce41d, 0x112a213b, 0x92e80321, 0x998b5905, 0x9437205a, 0x866a2c2e, 0xf8b5fa83, 0x2f3eac15, 0xaa2244ac, 0x84f79256, 0xd5ce28f0, 0x1278b09e, 0x45fbab9c, 0x51a2df07, 0x49a35fe5, 0x95eaed40, 0x807d7a62, 0x2055f077, 0x7963d5fc, 0xb71ca5b2, 0x9bf9d4fc, 0xeb401e58, 0xe7d31adc, 0x3493bc6c, 0x4fd2d8a6, 0x9fb45846, 0xc7283af4, 0xa5c0ec7e, 0xffc77af6, 0x41a010f8, 0x4723dcf9, 0xa9c06388, 0xcc767c13, 0x5dc5ae3b, 0x1bcd74c0, 0x0190b139, 0x64360845, 0x2f3be4c8, 0xcf1d5fc4, 0x69953306, 0x49f0f63e, 0xaba74251, 0x8d9e103d };
        }

        public bool IsRunning => _isRunning;

        public void ModPow(uint[] value, int exponent, JwtDecorderCallback jwtDecorderCallback, MontgomeryModPowCalculagorCallback montgomeryModPowCalculagorCallback)
        {
            CofficientInitialize();
            _montgomeryModPowCalculagorCallback = montgomeryModPowCalculagorCallback;
            _jwtDecorderCallback = jwtDecorderCallback;
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
            _jwtDecorderCallback.AuthenticationProgress = 0;

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
                _jwtDecorderCallback.AuthenticationProgress += 1f / (float)_totalStep;
                _jwtDecorderCallback.OnProgress();
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
            _jwtDecorderCallback.AuthenticationProgress = 1;
            _jwtDecorderCallback.OnProgress();
            _montgomeryModPowCalculagorCallback.Result = MontgomeryReduction(_buf);
            _montgomeryModPowCalculagorCallback.OnEnd();
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

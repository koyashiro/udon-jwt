using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using Koyashiro.UdonEncoding;
using Koyashiro.UdonJwt.Numerics;
using Koyashiro.UdonJwt.PKCS1;

namespace Koyashiro.UdonJwt
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class JwtRS256Decoder : UdonSharpBehaviour
    {
        [SerializeField, TextArea(10, 20)]
        private string _publicKey;
        public string PublicKey => _publicKey;

        [SerializeField, HideInInspector]
        private int _e;
        private int _eBuf;

        [SerializeField, HideInInspector]
        private uint[] _r2;

        [SerializeField, HideInInspector]
        private uint[] _n;

        [SerializeField, HideInInspector]
        private uint[] _nPrime;

        public bool IsBusy => _isBusy;
        private bool _isBusy;

        private JwtDecorderCallback _callback;

        private DataToken _headerJson;
        private DataToken _payloadJson;

        private string _tokenHashSource;
        private uint _totalStep;
        private const uint SIGNATURE_LENGTH = 256;

        public int E => _e;
        public uint[] N => _n;
        public bool HasPublicKey => _e != 0 && _r2 != null && _n != null && _nPrime != null;

        public void SetPublicKey(int e, uint[] r2, uint[] n, uint[] nPrime)
        {
            _e = e;
            _r2 = r2;
            _n = n;
            _nPrime = nPrime;
        }

        public void Decode(string token, JwtDecorderCallback callback)
        {
            if (_isBusy)
            {
                DecodeError(JwtDecodeErrorKind.Busy);
                return;
            }
            _isBusy = true;
            InitializeParameters(callback);

            if (token == null)
            {
                DecodeError(JwtDecodeErrorKind.InvalidToken);
                return;
            }

            var splitTokens = token.Split('.');
            if (splitTokens.Length != 3)
            {
                DecodeError(JwtDecodeErrorKind.InvalidToken);
                return;
            }

            var header = splitTokens[0];
            var payload = splitTokens[1];
            var signature = splitTokens[2];
            _tokenHashSource = token.Substring(0, header.Length + 1 + payload.Length);

            if (!TryParseHeaderBase64Url(header, out var headerJson))
            {
                DecodeError(JwtDecodeErrorKind.InvalidToken);
                return;
            }
            _headerJson = headerJson;

            if (!TryParsePayloadBase64Url(payload, out var payloadJson))
            {
                DecodeError(JwtDecodeErrorKind.InvalidToken);
                return;
            }
            _payloadJson = payloadJson;

            if (!TryFromBase64Url(signature, out var signatureBytes))
            {
                DecodeError(JwtDecodeErrorKind.InvalidToken);
                return;
            }

            if (signatureBytes.Length != SIGNATURE_LENGTH)
            {
                DecodeError(JwtDecodeErrorKind.InvalidSignature);
                return;
            }

            ModPow(UnsignedBigInteger.FromBytesBE(signatureBytes));
        }

        private void InitializeParameters(JwtDecorderCallback callback)
        {
            _eBuf = _e;
            _headerJson = default;
            _payloadJson = default;
            _tokenHashSource = default;
            _totalStep = default;
            _callback = callback;
            _callback.Result = default;
            _callback.ErrorKind = default;
            _callback.Header = default;
            _callback.Payload = default;
            _callback.Progress = default;
        }

        private static bool TryParseBase64Url(string base64Url, out DataToken value)
        {
            if (!TryFromBase64Url(base64Url, out var bytes))
            {
                value = default;
                return false;
            }

            if (!UdonUTF8.TryGetString(bytes, out var str))
            {
                value = default;
                return false;
            }

            return VRCJson.TryDeserializeFromJson(str, out value);
        }

        private static bool TryParseHeaderBase64Url(string headerBase64Url, out DataToken headerJson)
        {
            if (!TryParseBase64Url(headerBase64Url, out headerJson))
            {
                headerJson = default;
                return false;
            }

            if (headerJson.TokenType != TokenType.DataDictionary)
            {
                headerJson = default;
                return false;
            }

            if (!headerJson.DataDictionary.TryGetValue("alg", out var algorithmValue))
            {
                headerJson = default;
                return false;
            }

            if (algorithmValue.TokenType != TokenType.String)
            {
                headerJson = default;
                return false;
            }

            var algorithm = algorithmValue.String;
            if (algorithm != "RS256")
            {
                headerJson = default;
                return false;
            }

            return true;
        }
        private static bool TryParsePayloadBase64Url(string payloadBase64Url, out DataToken payloadJson)
        {
            if (!TryParseBase64Url(payloadBase64Url, out payloadJson))
            {
                payloadJson = default;
                return false;
            }

            if (payloadJson.TokenType != TokenType.DataDictionary)
            {
                return false;
            }

            return true;
        }

        #region Montgomery

        private uint[] _modPowBase;
        private uint[] _modPowBuf;

        private void ModPow(uint[] value)
        {
            _totalStep = 1;

            for (var e = _eBuf; e > 0; e >>= 1)
            {
                _totalStep += 1;
            }

            _modPowBase = MontgomeryReduction(UnsignedBigInteger.Multiply(value, _r2));
            _modPowBuf = MontgomeryReduction(_r2);

            SendCustomEventDelayedFrames(nameof(_ModPowLoop), 1);
        }

        public void _ModPowLoop()
        {
            if (_eBuf > 0)
            {
                if (_eBuf % 2 != 0)
                {
                    _modPowBuf = MontgomeryReduction(UnsignedBigInteger.Multiply(_modPowBuf, _modPowBase));
                }
                _modPowBase = MontgomeryReduction(UnsignedBigInteger.Multiply(_modPowBase, _modPowBase));
                _eBuf >>= 1;
                _callback.Progress += 1f / (float)_totalStep;
                _callback.OnProgress();
                SendCustomEventDelayedFrames(nameof(_ModPowLoop), 1);
            }
            else
            {
                SendCustomEventDelayedFrames(nameof(_ModPowEnd), 1);
            }
        }

        public void _ModPowEnd()
        {
            _modPowBuf = MontgomeryReduction(_modPowBuf);
            SendCustomEventDelayedFrames(nameof(_VerifyHash), 1);
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
        #endregion

        public void _VerifyHash()
        {
            var em = UnsignedBigInteger.ToBytes(_modPowBuf);
            var emPrime = PKCS1V15Encoder.Encode(UdonUTF8.GetBytes(_tokenHashSource));

            if (em.Length != emPrime.Length)
            {
                DecodeError(JwtDecodeErrorKind.InvalidSignature);
                return;
            }

            for (var i = 0; i < em.Length; i++)
            {
                if (em[i] != emPrime[i])
                {
                    DecodeError(JwtDecodeErrorKind.InvalidSignature);
                    return;
                }
            }

            // Expiration check
            if (_payloadJson.DataDictionary.TryGetValue("exp", out var expirationValue))
            {
                if (!expirationValue.IsNumber)
                {
                    DecodeError(JwtDecodeErrorKind.InvalidToken);
                    return;
                }

                var expiration = (long)expirationValue.Number;
                var nowUnixTime = GetNowUnixTime();
                if (expiration < nowUnixTime)
                {
                    DecodeError(JwtDecodeErrorKind.ExpiredToken);
                    return;
                }
            }

            // JWT decode is success
            _callback.Result = true;
            _callback.ErrorKind = JwtDecodeErrorKind.None;
            _callback.Header = _headerJson;
            _callback.Payload = _payloadJson;
            _callback.Progress = 1;
            _callback.OnProgress();
            _callback.OnEnd();
            _isBusy = false;
        }

        private static long GetNowUnixTime()
        {
            var serverTimeTicks = Networking.GetNetworkDateTime().Ticks;
            var dto = new DateTimeOffset(serverTimeTicks, new TimeSpan(00, 00, 00));
            return dto.ToUnixTimeSeconds();
        }

        private void DecodeError(JwtDecodeErrorKind errorKind)
        {
            _callback.Result = false;
            _callback.ErrorKind = errorKind;
            _callback.Header = default;
            _callback.Payload = default;
            _callback.Progress = 1;
            _callback.OnProgress();
            _callback.OnEnd();
            _isBusy = false;
        }

        private static bool TryFromBase64Url(string base64Url, out byte[] bytes)
        {
            foreach (var c in base64Url)
            {
                if ("-_0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".IndexOf(c) == -1)
                {
                    bytes = default;
                    return false;
                }
            }

            var base64 = base64Url.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 1:
                    bytes = default;
                    return false;
                case 2:
                    base64 += "==";
                    break;
                case 3:
                    base64 += "=";
                    break;
            }

            bytes = Convert.FromBase64String(base64);
            return true;
        }
    }
}

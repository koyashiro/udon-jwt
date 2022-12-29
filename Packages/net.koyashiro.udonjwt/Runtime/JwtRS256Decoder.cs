using System;
using UnityEngine;
using UdonSharp;
using Koyashiro.UdonJson;
using Koyashiro.UdonEncoding;
using Koyashiro.UdonJwt.Numerics;
using Koyashiro.UdonJwt.PKCS1;
using Koyashiro.UdonJwt.SHA2;
using VRC.SDKBase;

namespace Koyashiro.UdonJwt
{
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

        private UdonJsonValue _headerJson;
        private UdonJsonValue _payloadJson;

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

            // Get data
            var header = splitTokens[0];
            var payload = splitTokens[1];
            var signature = splitTokens[2];
            _tokenHashSource = token.Substring(0, header.Length + 1 + payload.Length);

            if (!GetCheckedHeaderJson(ToBase64(header)))
            {
                DecodeError(JwtDecodeErrorKind.InvalidToken);
                return;
            }

            if (!GetCheckedPayloadJson(ToBase64(payload)))
            {
                DecodeError(JwtDecodeErrorKind.InvalidToken);
                return;
            }

            var signatureBytes = Convert.FromBase64String(ToBase64(signature));

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

        private bool GetCheckedHeaderJson(string headerBase64)
        {
            var headerStr = UdonUTF8.GetString(Convert.FromBase64String(headerBase64));
            if (!UdonJsonDeserializer.TryDeserialize(headerStr, out _headerJson))
            {
                return false;
            }

            if (_headerJson.GetKind() != UdonJsonValueKind.Object)
            {
                return false;
            }

            if (!_headerJson.TryGetValue("alg", out var algorithmValue))
            {
                return false;
            }

            if (algorithmValue.GetKind() != UdonJsonValueKind.String)
            {
                return false;
            }

            var algorithm = algorithmValue.AsString();
            if (algorithm != "RS256")
            {
                return false;
            }

            return true;
        }

        private bool GetCheckedPayloadJson(string payloadBase64)
        {
            var payloadStr = UdonUTF8.GetString(Convert.FromBase64String(payloadBase64));
            if (!UdonJsonDeserializer.TryDeserialize(payloadStr, out _payloadJson))
            {
                return false;
            }

            if (_payloadJson.GetKind() != UdonJsonValueKind.Object)
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
            if (_payloadJson.TryGetValue("exp", out var expirationValue))
            {
                if (expirationValue.GetKind() != UdonJsonValueKind.Number)
                {
                    DecodeError(JwtDecodeErrorKind.InvalidToken);
                    return;
                }

                var expiration = (long)expirationValue.AsNumber();
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

        private long GetNowUnixTime()
        {
            var serverTimeTicks = Networking.GetNetworkDateTime().Ticks;
            var dto = new DateTimeOffset(serverTimeTicks, new TimeSpan(00, 00, 00));
            return dto.ToUnixTimeSeconds();
        }

        private void DecodeError(JwtDecodeErrorKind errorKind)
        {
            _callback.Result = false;
            _callback.ErrorKind = errorKind;
            _callback.Header = null;
            _callback.Payload = null;
            _callback.Progress = 1;
            _callback.OnProgress();
            _callback.OnEnd();
            _isBusy = false;
        }

        private static string ToBase64(string base64Url)
        {
            var base64 = base64Url.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 1:
                    base64 += "===";
                    break;
                case 2:
                    base64 += "==";
                    break;
                case 3:
                    base64 += "=";
                    break;
            }
            return base64;
        }
    }
}

using System;
using UnityEngine;
using UdonSharp;
using Koyashiro.UdonJson;
using Koyashiro.UdonEncoding;
using Koyashiro.UdonJwt.Numerics;
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

        [SerializeField, HideInInspector]
        private uint[] _r;

        [SerializeField, HideInInspector]
        private uint[] _r2;

        [SerializeField, HideInInspector]
        private uint[] _n;

        [SerializeField, HideInInspector]
        private uint[] _nPrime;

        private JwtDecorderCallback _callback;

        private bool _isRunning;

        private UdonJsonValue _headerJson;
        private UdonJsonValue _payloadJson;

        private byte[] _headerBytes;
        private byte[] _payloadBytes;

        private uint _totalStep;

        public void SetPublicKey(int e, uint[] r, uint[] r2, uint[] n, uint[] nPrime)
        {
            _e = e;
            _r = r;
            _r2 = r2;
            _n = n;
            _nPrime = nPrime;
        }

        public void Decode(string token, JwtDecorderCallback callback)
        {
            if (_isRunning)
            {
                DecodeError(JwtDecodeErrorKind.Busy);
                return;
            }
            _isRunning = true;

            _callback = callback;
            _callback.Progress = 0;

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
                        
            var headerBase64Url = splitTokens[0];
            var payloadBase64Url = splitTokens[1];
            var signatureBase64Url = splitTokens[2];

            var headerBase64 = ToBase64(headerBase64Url);
            var payloadBase64 = ToBase64(payloadBase64Url);
            var signatureBase64 = ToBase64(signatureBase64Url);

            // TODO: make bytes from Base64Url
            //_headerBytes = ;
            //_payloadBytes = ;

            if (!UdonJsonDeserializer.TryDeserialize(UdonUTF8.GetString(Convert.FromBase64String(headerBase64)), out _headerJson))
            {
                DecodeError(JwtDecodeErrorKind.InvalidToken);
                return;
            }

            if (_headerJson.GetKind() != UdonJsonValueKind.Object)
            {
                DecodeError(JwtDecodeErrorKind.InvalidToken);
                return;
            }

            // TODO: check header
            if (!UdonJsonDeserializer.TryDeserialize(UdonUTF8.GetString(Convert.FromBase64String(payloadBase64)), out _payloadJson))
            {
                DecodeError(JwtDecodeErrorKind.InvalidToken);
                return;
            }

            // TODO: check body
            var signatureBytes = Convert.FromBase64String(signatureBase64);
            ModPow(UnsignedBigInteger.FromBytes(signatureBytes));
        }

        #region Montgomery

        private uint[] _modPowBase;
        private uint[] _modPowBuf;

        private void ModPow(uint[] value)
        {
            _totalStep = 1;

            for (var e = _e; e > 0; e >>= 1)
            {
                _totalStep += 1;
            }

            _modPowBase = MontgomeryReduction(UnsignedBigInteger.Multiply(value, _r2));
            _modPowBuf = MontgomeryReduction(_r2);

            SendCustomEventDelayedFrames(nameof(_ModPowLoop), 1);
        }

        public void _ModPowLoop()
        {
            if (_e > 0)
            {
                if (_e % 2 != 0)
                {
                    _modPowBuf = MontgomeryReduction(UnsignedBigInteger.Multiply(_modPowBuf, _modPowBase));
                }
                _modPowBase = MontgomeryReduction(UnsignedBigInteger.Multiply(_modPowBase, _modPowBase));
                _e >>= 1;
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
            //get hash from header and payload
            var tokenBytes = new byte[_headerBytes.Length + _payloadBytes.Length +1];
            Array.Copy(_headerBytes, tokenBytes, _headerBytes.Length);
            tokenBytes[_headerBytes.Length] = (byte)'.';
            Array.Copy(_payloadBytes,0, tokenBytes, _headerBytes.Length + 1, _payloadBytes.Length);

            var hashedTokenBytes = SHA256.ComputeHash(tokenBytes);

            //TODO: Get hash from ModPow result.
            var modPowResultBytes = UnsignedBigInteger.ToBytes(_modPowBuf);

            for (var i = 0; i < hashedTokenBytes.Length; i++)
            {
                if (modPowResultBytes[i] != hashedTokenBytes[i])
                {
                    DecodeError(JwtDecodeErrorKind.InvalidSignature);
                    return;
                }
            }

            // expiration check
            // TODO: TryGetValue for UdonJson
            var expirationValue = _payloadJson.GetValue("exp");

            if (expirationValue != null)
            {
                if (expirationValue.GetKind() != UdonJsonValueKind.Number)
                {
                    DecodeError(JwtDecodeErrorKind.InvalidToken);
                    return;
                }
                var expiration =  (long)expirationValue.AsNumber();
                var nowUnixTime = GetNowUnixTime();
                if (expiration < nowUnixTime)
                {
                    DecodeError(JwtDecodeErrorKind.ExpiredToken);
                    return;
                }
            }

            //JWT decode is success
            _callback.Result = true;
            _callback.Header = _headerJson;
            _callback.Payload = _payloadJson;
            _callback.Progress = 1;
            _callback.OnProgress();
            _callback.OnEnd();
            _isRunning = false;
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
            _isRunning = false;
        }

        public static string ToBase64(string base64Url)
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

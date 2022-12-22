using System;
using UnityEngine;
using UdonSharp;
using Koyashiro.UdonJson;
using Koyashiro.UdonEncoding;
using Koyashiro.UdonJwt.Verifier;
using Koyashiro.UdonJwt.Numerics;

namespace Koyashiro.UdonJwt
{
    public class JwtDecoder : RS256VerifierCallback
    {
        [SerializeField]
        private JwtAlgorithmKind _algorithmKind = JwtAlgorithmKind.RS256;

        #region RS256
        [SerializeField]
        private RS256Verifier _rs256Verifier;

        [SerializeField, TextArea(10, 20)]
        private string _publicKey;

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
        #endregion

        private JwtDecorderCallback _jwtDecorderCallback;

        public JwtAlgorithmKind AlgorithmKind => _algorithmKind;

        public RS256Verifier RS256Verifier => _rs256Verifier;

        public string PublicKey => _publicKey;

        private ulong _expiration;

        public void SetPublicKey(int e, uint[] r, uint[] r2, uint[] n, uint[] nPrime)
        {
            _e = e;
            _r = r;
            _r2 = r2;
            _n = n;
            _nPrime = nPrime;
        }

        public void Initialize()
        {
            _rs256Verifier.Initialize(_e, _r, _r2, _n, _nPrime);
        }

        public void Decode(string token, JwtDecorderCallback jwtDecorderCallback)
        {
            _jwtDecorderCallback = jwtDecorderCallback;
            if (token == null)
            {
                _jwtDecorderCallback.SendCustomEventDelayedFrames("", 1);
                return;
            }

            var splitTokens = token.Split('.');
            if (splitTokens.Length != 3)
            {
                _jwtDecorderCallback.SendCustomEventDelayedFrames("", 1);
                return;
            }

            var headerBase64 = ToBase64(splitTokens[0]);
            var payloadBase64 = ToBase64(splitTokens[1]);
            var signatureBase64 = ToBase64(splitTokens[2]);

            if (!UdonJsonDeserializer.TryDeserialize(UdonUTF8.GetString(Convert.FromBase64String(headerBase64)), out var header))
            {
                _jwtDecorderCallback.SendCustomEventDelayedFrames("", 1);
                return;
            }

            if (header.GetKind() != UdonJsonValueKind.Object)
            {
                _jwtDecorderCallback.SendCustomEventDelayedFrames("", 1);
                return;
            }

            // TODO: check header
            if (!UdonJsonDeserializer.TryDeserialize(UdonUTF8.GetString(Convert.FromBase64String(payloadBase64)), out var payload))
            {
                _jwtDecorderCallback.SendCustomEventDelayedFrames("", 1);
                return;
            }

            // TODO: check body
            var expirationValue = payload.GetValue("exp"); //try get value?
            var _expiration = expirationValue.GetKind() == UdonJsonValueKind.Number ? expirationValue.AsNumber() : ulong.MaxValue;

            var signature = Convert.FromBase64String(signatureBase64);

            switch (_algorithmKind)
            {
                case JwtAlgorithmKind.RS256:
                    _rs256Verifier.Verify(headerBase64, payloadBase64, signature, _jwtDecorderCallback, this);
                    break;
            }
        }

        public override void OnEnd()
        {
            switch (RS256VerifierResult)
            {
                case RS256VerifierResult.OK:
                    _jwtDecorderCallback.Result = JwtAuthenticationResult.OK;
                    break;
                case RS256VerifierResult.ERROR_DISACCORD_HASH:
                    _jwtDecorderCallback.Result = JwtAuthenticationResult.ERROR_DISACCORD_HASH;
                    break;
                case RS256VerifierResult.ERROR_INCORRECT_DATA:
                    _jwtDecorderCallback.Result = JwtAuthenticationResult.ERROR_INCORRECT_STRUCTURE;
                    break;
                case RS256VerifierResult.ERROR_OTHER:
                    _jwtDecorderCallback.Result = JwtAuthenticationResult.ERROR_OTHER;
                    break;
            }
            //TODO: Check expire
            //_expiration
            _jwtDecorderCallback.OnEnd();
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

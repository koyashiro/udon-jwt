using System;
using UnityEngine;
using UdonSharp;
using Koyashiro.UdonJson;
using Koyashiro.UdonEncoding;
using Koyashiro.UdonJwt.Verifier;

namespace Koyashiro.UdonJwt
{
    public class JwtDecoder : UdonSharpBehaviour
    {
        [SerializeField]
        private JwtAlgorithmKind _algorithmKind;

        #region RS256
        [SerializeField]
        private RS256Verifier _rs256Verifier;
        [SerializeField, TextArea(10, 20)]
        private string _publicKey;
        #endregion

        private UdonSharpBehaviour _callbackThis;
        private string _callbackEventName;
        private bool _result;
        private UdonJsonValue _header;
        private UdonJsonValue _payload;

        public JwtAlgorithmKind AlgorithmKind => _algorithmKind;

        public RS256Verifier RS256Verifier => _rs256Verifier;

        public string PublicKey => _publicKey;

        public bool Result => _result;

        public UdonJsonValue Header => _header;

        public UdonJsonValue Payload => _payload;

        public void Decode(string token, UdonSharpBehaviour callbackThis, string callbackEventName)
        {
            if (token == null)
            {
                callbackThis.SendCustomEventDelayedFrames(callbackEventName, 1);
                return;
            }

            var splitTokens = token.Split('.');
            if (splitTokens.Length != 3)
            {
                callbackThis.SendCustomEventDelayedFrames(callbackEventName, 1);
                return;
            }

            var headerBase64 = ToBase64(splitTokens[0]);
            var payloadBase64 = ToBase64(splitTokens[1]);
            var signatureBase64 = ToBase64(splitTokens[2]);

            if (!UdonJsonDeserializer.TryDeserialize(UdonUTF8.GetString(Convert.FromBase64String(headerBase64)), out var header))
            {
                callbackThis.SendCustomEventDelayedFrames(callbackEventName, 1);
                return;
            }

            if (header.GetKind() != UdonJsonValueKind.Object)
            {
                callbackThis.SendCustomEventDelayedFrames(callbackEventName, 1);
                return;
            }

            // TODO: check header

            if (!UdonJsonDeserializer.TryDeserialize(UdonUTF8.GetString(Convert.FromBase64String(payloadBase64)), out var payload))
            {
                callbackThis.SendCustomEventDelayedFrames(callbackEventName, 1);
                return;
            }

            // TODO: check body

            var signature = Convert.FromBase64String(signatureBase64);

            _header = header;
            _payload = payload;
            _callbackThis = callbackThis;
            _callbackEventName = callbackEventName;

            switch (_algorithmKind)
            {
                case JwtAlgorithmKind.RS256:
                    _rs256Verifier.Verify(headerBase64, payloadBase64, signature, this, nameof(_Decode));
                    break;
            }
        }

        public void _Decode()
        {
            _result = _rs256Verifier.Result;
            _callbackThis.SendCustomEventDelayedFrames(_callbackEventName, 1);
            Destroy(gameObject);
        }

        public JwtDecoder Clone()
        {
            var decoder = Instantiate(gameObject).GetComponent<JwtDecoder>();
            decoder._algorithmKind = _algorithmKind;
            switch (_algorithmKind)
            {
                case JwtAlgorithmKind.RS256:
                    decoder._publicKey = _publicKey;
                    decoder._rs256Verifier.SetPublicKey(_rs256Verifier.E, _rs256Verifier.N, _rs256Verifier.NInverse, _rs256Verifier.FixedPointLength);
                    break;
            }
            return decoder;
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

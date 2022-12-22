using UnityEngine;
using UdonSharp;
using Koyashiro.UdonJwt.Numerics;
using Koyashiro.UdonEncoding;
using Koyashiro.UdonJwt.SHA2;

namespace Koyashiro.UdonJwt.Verifier
{
    [AddComponentMenu("")]
    public class RS256Verifier : MontgomeryModPowCalculagorCallback
    {
        [SerializeField]
        private MontgomeryModPowCalculator _montgomeryModPowCalculator;
        private RS256VerifierCallback _rS256VerifierCallback;
        private string _headerBase64;
        private string _payloadBase64;

        public void Initialize(int e, uint[] r, uint[] r2, uint[] n, uint[] nPrime)
        {
            _montgomeryModPowCalculator.Initialize(e, r, r2, n, nPrime);
        }

        public void Verify(string headerBase64, string payloadBase64, byte[] signature, JwtDecorderCallback jwtDecorderCallback, RS256VerifierCallback rS256VerifierCallback)
        {
            _rS256VerifierCallback = rS256VerifierCallback;
            _headerBase64 = headerBase64;
            _payloadBase64 = payloadBase64;
            _montgomeryModPowCalculator.ModPow(UnsignedBigInteger.FromBytes(signature), jwtDecorderCallback, this);
        }

        public override void OnEnd()
        {
            //get hash from header and payload
            var tokenBytes = UdonUTF8.GetBytes(_headerBase64 + "." + _payloadBase64);
            var hashedTokenBytes = SHA256.ComputeHash(tokenBytes);

            //TODO: Get hash from ModPow result.
            var modPowResultBytes = UnsignedBigInteger.ToBytes(Result);

            for (var i = 0; i < hashedTokenBytes.Length; i++)
            {
                if (modPowResultBytes[i] != hashedTokenBytes[i])
                {
                    _rS256VerifierCallback.RS256VerifierResult = RS256VerifierResult.ERROR_DISACCORD_HASH;
                    _rS256VerifierCallback.OnEnd();
                    return;
                }
            }

            _rS256VerifierCallback.RS256VerifierResult = RS256VerifierResult.OK;
            _rS256VerifierCallback.OnEnd();
        }
    }
}

#if UNITY_EDITOR
using System.Numerics;
using UnityEngine;
using UnityEditor;
using Koyashiro.UdonJwt.Numerics;

namespace Koyashiro.UdonJwt.Editor
{
    [CustomEditor(typeof(JwtRS256Decoder))]
    public class JwtDecoderInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var jwtDecoder = target as JwtRS256Decoder;

            if (GUILayout.Button("Set Public Key"))
            {
                if (!PublicKeyDecoder.TryDecode(jwtDecoder.PublicKey, out var n, out var e))
                {
                    Debug.LogError("[UdonJwt] Failed to parse public key");
                    return;
                }

                // 2 ^ 4096
                var r = BigInteger.Parse("2");
                r <<= 4096;

                var r2 = BigInteger.ModPow(r, 2, n);
                var nPrime = CalculateNPrime(n, r);

                jwtDecoder.SetPublicKey(
                    e,
                    UnsignedBigInteger.FromBytesLE(r2.ToByteArray()),
                    UnsignedBigInteger.FromBytesLE(n.ToByteArray()),
                    UnsignedBigInteger.FromBytesLE(nPrime.ToByteArray())
                );

                EditorUtility.SetDirty(jwtDecoder);
            }
        }

        private static BigInteger CalculateNPrime(BigInteger n, BigInteger r)
        {
            var nPrime = BigInteger.Parse("0");
            var t = BigInteger.Zero;
            var i = 1;

            while (r > 1)
            {
                if (!((t % 2) == 0))
                {
                    t += n;
                    nPrime += i;
                }
                t /= 2;
                r /= 2;
                i *= 2;
            }

            return nPrime;
        }
    }
}
#endif

#if UNITY_EDITOR
using System;
using System.Numerics;
using UnityEngine;
using UnityEditor;
using Koyashiro.UdonJwt.Numerics;

namespace Koyashiro.UdonJwt.Editor
{
    [CustomEditor(typeof(JwtRS256Decoder))]
    public class JwtDecoderInspector : UnityEditor.Editor
    {
        /// <summary>
        /// 2 ^ 2047
        /// </summary>
        private static BigInteger POW_2_2047 = BigInteger.One << 2047;

        /// <summary>
        /// 2 ^ 2048
        /// </summary>
        private static BigInteger POW_2_2048 = BigInteger.One << 2048;

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

                if (n <= POW_2_2047 || POW_2_2048 <= n)
                {
                    Debug.LogError($"[UdonJwt] Unsupported public key: n = {n}");
                    return;
                }

                // 2 ^ 2048
                var r = POW_2_2048;
                var r2 = BigInteger.ModPow(r, 2, n);
                var nPrime = CalculateNPrime(n, r);

                var r2Bytes = r2.ToByteArray();

                jwtDecoder.SetPublicKey(
                    e,
                    UnsignedBigInteger.FromBytesLE(r2.ToByteArray()),
                    UnsignedBigInteger.FromBytesLE(n.ToByteArray()),
                    UnsignedBigInteger.FromBytesLE(nPrime.ToByteArray())
                );

                EditorUtility.SetDirty(jwtDecoder);
            }

            if (jwtDecoder.HasPublicKey)
            {
                GUILayout.Space(16);

                var e = jwtDecoder.E;
                GUILayout.Label("e");
                GUILayout.TextField(e.ToString());

                var nBytesUnsinged = UnsignedBigInteger.ToBytes(jwtDecoder.N);
                var nBytesSigned = new byte[nBytesUnsinged.Length + 1];
                Array.Copy(nBytesUnsinged, 0, nBytesSigned, 1, nBytesUnsinged.Length);
                Array.Reverse(nBytesSigned);
                var n = new BigInteger(nBytesSigned);
                GUILayout.Label("n");
                GUILayout.TextArea(n.ToString());
                GUILayout.TextArea($"0x{n:x}");
            }
        }

        private static BigInteger CalculateNPrime(BigInteger n, BigInteger r)
        {
            var nPrime = BigInteger.Zero;
            var t = BigInteger.Zero;
            var i = BigInteger.One;

            while (r > 1)
            {
                if (t % 2 == 0)
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

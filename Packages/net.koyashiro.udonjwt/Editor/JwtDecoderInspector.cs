#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using Koyashiro.UdonJwt.Numerics;

namespace Koyashiro.UdonJwt.Editor
{
    [CustomEditor(typeof(JwtDecoder))]
    public class JwtDecoderInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var jwtDecoder = target as JwtDecoder;

            if (GUILayout.Button("Set Public Key"))
            {
                switch (jwtDecoder.AlgorithmKind)
                {
                    case JwtAlgorithmKind.RS256:
                        if (!PublicKeyDecoder.TryDecode(jwtDecoder.PublicKey, out var nBytes, out var e))
                        {
                            Debug.LogError("[UdonJwt] Failed to parse public key");
                            return;
                        }

                        var n = UnsignedBigInteger.FromBytes(nBytes);
                        var nInverse = UnsignedBigInteger.Inverse(n, out var fixedPointLength);
                        var nResized = new uint[nInverse.Length];
                        Array.Copy(n, nResized, n.Length);
                        jwtDecoder.SetPublicKey(e, nResized, nInverse, fixedPointLength);
                        EditorUtility.SetDirty(jwtDecoder);
                        EditorUtility.SetDirty(jwtDecoder.RS256Verifier);
                        break;
                }
            }

            if (jwtDecoder.E != 0 && jwtDecoder.N != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(nameof(jwtDecoder.E), $"0x{jwtDecoder.E:x8}");
                EditorGUILayout.TextField(nameof(jwtDecoder.N), UnsignedBigInteger.ToHexString(jwtDecoder.N));
                EditorGUILayout.TextField(nameof(jwtDecoder.NInverse), UnsignedBigInteger.ToHexString(jwtDecoder.NInverse));
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}
#endif

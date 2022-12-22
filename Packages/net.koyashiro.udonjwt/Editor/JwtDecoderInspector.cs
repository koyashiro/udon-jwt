#if UNITY_EDITOR
using System;
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
                if (!PublicKeyDecoder.TryDecode(jwtDecoder.PublicKey, out var nBytes, out var e))
                {
                    Debug.LogError("[UdonJwt] Failed to parse public key");
                    return;
                }

                var n = UnsignedBigInteger.FromBytes(nBytes);

                // TODO: get values from public key.
                //jwtDecoder.SetPublicKey(e, r, r2, n, nPrime);

                EditorUtility.SetDirty(jwtDecoder);
            }

            /*
            if (jwtDecoder.E != 0 && jwtDecoder.N != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(nameof(jwtDecoder.E), $"0x{jwtDecoder.E:x8}");
                EditorGUILayout.TextField(nameof(jwtDecoder.N), UnsignedBigInteger.ToHexString(jwtDecoder.N));
                EditorGUILayout.TextField(nameof(jwtDecoder.NInverse), UnsignedBigInteger.ToHexString(jwtDecoder.NInverse));
                EditorGUI.EndDisabledGroup();
            }
            */
        }
    }
}
#endif

using System;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDKBase.Editor.BuildPipeline;
using Koyashiro.UdonJwt.Numerics;

namespace Koyashiro.UdonJwt.VRCSDKCallback
{
    public class SetupPublicKeyCallback : IVRCSDKBuildRequestedCallback, IProcessSceneWithReport
    {
        public int callbackOrder => default;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            return SetupPublicKey();
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            SetupPublicKey();
        }

        private bool SetupPublicKey()
        {
            try
            {
                var jwtDecoders = Resources.FindObjectsOfTypeAll<JwtDecoder>().Where(d => d.hideFlags == HideFlags.None);
                foreach (var jwtDecoder in jwtDecoders)
                {
                    switch (jwtDecoder.AlgorithmKind)
                    {
                        case JwtAlgorithmKind.RS256:
                            if (!PublicKeyDecoder.TryDecode(jwtDecoder.PublicKey, out var nBytes, out var e))
                            {
                                Debug.LogError("[UdonJwt] Failed to parse public key");
                                return false;
                            }

                            var n = UnsignedBigInteger.FromBytes(nBytes);
                            var nInverse = UnsignedBigInteger.Inverse(n, out var fixedPointLength);
                            var nResized = new uint[nInverse.Length];
                            Array.Copy(n, nResized, n.Length);
                            jwtDecoder.RS256Verifier.SetPublicKey(e, nResized, nInverse, fixedPointLength);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }

            return true;
        }
    }
}

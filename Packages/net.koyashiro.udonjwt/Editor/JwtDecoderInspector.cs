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
        private static BigInteger POW_2_2047 = BigInteger.Parse("16158503035655503650357438344334975980222051334857742016065172713762327569433945446598600705761456731844358980460949009747059779575245460547544076193224141560315438683650498045875098875194826053398028819192033784138396109321309878080919047169238085235290822926018152521443787945770532904303776199561965192760957166694834171210342487393282284747428088017663161029038902829665513096354230157075129296432088558362971801859230928678799175576150822952201848806616643615613562842355410104862578550863465661734839271290328348967522998634176499319107762583194718667771801067716614802322659239302476074096777926805529798115328");

        /// <summary>
        /// 2 ^ 2048
        /// </summary>
        private static BigInteger POW_2_2048 = BigInteger.Parse("32317006071311007300714876688669951960444102669715484032130345427524655138867890893197201411522913463688717960921898019494119559150490921095088152386448283120630877367300996091750197750389652106796057638384067568276792218642619756161838094338476170470581645852036305042887575891541065808607552399123930385521914333389668342420684974786564569494856176035326322058077805659331026192708460314150258592864177116725943603718461857357598351152301645904403697613233287231227125684710820209725157101726931323469678542580656697935045997268352998638215525166389437335543602135433229604645318478604952148193555853611059596230656");

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
                    ToUnsignedBigInteger(r2),
                    ToUnsignedBigInteger(n),
                    ToUnsignedBigInteger(nPrime)
                );

                EditorUtility.SetDirty(jwtDecoder);
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

        private static uint[] ToUnsignedBigInteger(BigInteger value)
        {
            var bytesFromBigInteger = value.ToByteArray();
            // littleendian to bigendian
            Array.Reverse(bytesFromBigInteger);

            const int BYTES_LENGTH = 64;
            var bytes = new byte[BYTES_LENGTH];
            Array.Copy(bytesFromBigInteger, bytes, BYTES_LENGTH);

            return UnsignedBigInteger.FromBytes(bytes);
        }
    }
}
#endif

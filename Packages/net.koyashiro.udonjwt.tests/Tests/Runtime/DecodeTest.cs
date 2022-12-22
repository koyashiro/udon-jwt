using UnityEngine;
using UdonSharp;
using Koyashiro.UdonJson;
using Koyashiro.UdonTest;
using Koyashiro.UdonJwt.Numerics;

namespace Koyashiro.UdonJwt.Tests
{
    public class DecodeTest : JwtDecorderCallback
    {
        [SerializeField]
        private JwtRS256Decoder _decoder;

        public void Start()
        {
            var token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.c_TDh9O85HFOAska2cM9kN8TGwj2IJkvPtuEc1kBkZM9ZU-UiyypfusznKlPquwUUA00IKfhR1dZDMrG_cNLjPOZ7ocPZL8bWoT2I2gCb-HNG00dNfq44aF-wsomSNn2JL6myJmnIWbPgNP5WOCcJipfsxNloudLGHATLwMM5iGY70YBGF5u9D_Zy7sTTbBJzVAebCos1zgkUIsVxPOwP2zRptOROfKCWB0QCo576ymb41_MTO_kV59VQqwkgk1PLNDsl02BsaMe9CQrTx0_NUtZWox7sPe0rsnVDL2mm4w8vKKgaeXffdzNyEhgXFm6_UjKybXK_Be7u8QddJNN-g";
            _decoder.Decode(token, this);
        }

        public override void OnEnd()
        {
            Assert.True(Result);
        }
    }
}

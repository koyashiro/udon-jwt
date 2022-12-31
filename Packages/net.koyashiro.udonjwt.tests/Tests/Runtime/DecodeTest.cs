using UnityEngine;
using UdonSharp;
using Koyashiro.UdonTest;

namespace Koyashiro.UdonJwt.Tests
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DecodeTest : JwtDecorderCallback
    {
        [SerializeField]
        private JwtRS256Decoder _decoder;

        public void Start()
        {
            var token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiSm9obiBEb2UifQ.Zhe74qVZx0JvMRL7NA-KDWl2wt7T6equyPRZFAdn649Rr-W_DbaVfPPaxuJqt71_LsMgPukEJ9DZfsfEnjJunmruWA1Q1DDXjn66oq-GXQi_mU3rd2DE9Sg3Q-x5h2z8CXNxCj_xGfQwRD1oM0eYJlTfqeGFGQ735EZOzfLpCGCSZIFtFG_4BilHRNsbY-OqfO9Kis6IEvmfkTSa9kn0OBRTCFK8n9xdjFFcYh8KpXU__UqxfcoJBBxfm_RYigZzKxVMVJlEKqFsmNDoKuPjyMsaBhd4ArELSZRKgA9RHYETkLVNP6AP1qavuoV2mfea3SjzUvIkM-3PTuMY8a9EYQ";
            _decoder.Decode(token, this);
        }

        public override void OnProgress()
        {
            Debug.Log($"JWT decode progress: {(uint)(Progress * 100)}%");
        }

        public override void OnEnd()
        {
            Assert.True(Result);
            if (!Result)
            {
                var errorNames = new string[] { "None", "Busy", "InvalidToken", "InvalidSignature", "ExpiredToken", "Other" };
                Debug.Log("JWT decode error: " + errorNames[(int)ErrorKind]);
            }
        }
    }
}

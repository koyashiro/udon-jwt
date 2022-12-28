using UnityEngine;
using UdonSharp;
using Koyashiro.UdonJson;
using Koyashiro.UdonTest;
using Koyashiro.UdonJwt.Numerics;
using System;

namespace Koyashiro.UdonJwt.Tests
{
    public class DecodeTest : JwtDecorderCallback
    {
        [SerializeField]
        private JwtRS256Decoder _decoder;

        [SerializeField, TextArea(10, 20)]
        private string _token;

        public void Start()
        {
            _decoder.Decode(_token, this);
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

using UdonSharp;
using Koyashiro.UdonJson;
public enum JwtDecodeErrorKind
{
    None,
    Busy,
    InvalidToken,
    InvalidSignature,
    ExpiredToken,
    Other,
}

namespace Koyashiro.UdonJwt.Numerics
{
    public abstract class JwtDecorderCallback : UdonSharpBehaviour
    {
        public float Progress { get; set; }
        public bool Result { get; set; }

        public UdonJsonValue Header { get; set; }
        public UdonJsonValue Payload { get; set; }

        public JwtDecodeErrorKind ErrorKind { get; set; }
        virtual public void OnProgress() { }
        abstract public void OnEnd();
    }
}

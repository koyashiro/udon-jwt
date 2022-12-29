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
        private object _header;
        private object _payload;

        public float Progress { get; set; }
        public bool Result { get; set; }

        public UdonJsonValue Header
        {
            get => (UdonJsonValue)_header;
            set => _header = value;
        }
        public UdonJsonValue Payload
        {
            get => (UdonJsonValue)_payload;
            set => _payload = value;
        }

        public JwtDecodeErrorKind ErrorKind { get; set; }
        virtual public void OnProgress() { }
        abstract public void OnEnd();
    }
}

using UdonSharp;
using VRC.SDK3.Data;

namespace Koyashiro.UdonJwt
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public abstract class JwtDecorderCallback : UdonSharpBehaviour
    {
        public float Progress { get; set; }
        public bool Result { get; set; }

        public DataDictionary Header { get; set; }
        public DataDictionary Payload { get; set; }

        public JwtDecodeErrorKind ErrorKind { get; set; }
        virtual public void OnProgress() { }
        abstract public void OnEnd();
    }
}

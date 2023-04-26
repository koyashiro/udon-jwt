using UdonSharp;
using VRC.SDK3.Data;

namespace Koyashiro.UdonJwt
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public abstract class JwtDecorderCallback : UdonSharpBehaviour
    {
        public float Progress { get; set; }
        public bool Result { get; set; }

        public DataToken Header { get; set; }
        public DataToken Payload { get; set; }

        public JwtDecodeErrorKind ErrorKind { get; set; }
        virtual public void OnProgress() { }
        abstract public void OnEnd();
    }
}

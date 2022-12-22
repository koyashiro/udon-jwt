using UdonSharp;
public enum JwtAuthenticationResult
{
    OK,
    ERROR_INCORRECT_STRUCTURE,
    ERROR_DISACCORD_HASH,
    ERROR_TOKEN_EXPIRED,
    ERROR_OTHER,
}

namespace Koyashiro.UdonJwt.Numerics
{
    public abstract class JwtDecorderCallback : UdonSharpBehaviour
    {
        public float AuthenticationProgress { get; set; }
        public JwtAuthenticationResult AuthenticationResult { get; set; }
        virtual public void OnProgress() { }
        abstract public void OnEnd();
    }
}

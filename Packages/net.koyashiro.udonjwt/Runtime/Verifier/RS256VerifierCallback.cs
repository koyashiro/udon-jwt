using UdonSharp;
public enum RS256VerifierResult
{
    OK,
    ERROR_INCORRECT_DATA,
    ERROR_DISACCORD_HASH,
    ERROR_OTHER,
}

namespace Koyashiro.UdonJwt.Numerics
{
    public abstract class RS256VerifierCallback : UdonSharpBehaviour
    {
        public RS256VerifierResult RS256VerifierResult { get; set; }
        abstract public void OnEnd();
    }
}

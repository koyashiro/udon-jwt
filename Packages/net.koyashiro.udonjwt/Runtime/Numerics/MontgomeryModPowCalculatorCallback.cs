using UdonSharp;

namespace Koyashiro.UdonJwt.Numerics
{
    public abstract class MontgomeryModPowCalculatorCallback : UdonSharpBehaviour
    {
        public uint[] Result { get; set; }

        abstract public void OnEnd();
    }
}

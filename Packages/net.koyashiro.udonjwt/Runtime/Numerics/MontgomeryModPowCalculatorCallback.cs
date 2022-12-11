using UdonSharp;

namespace Koyashiro.UdonJwt.Numerics
{
    public abstract class MontgomeryModPowCalculatorCallback : UdonSharpBehaviour
    {
        public float Progress { get; set; }
        public uint[] Result { get; set; }

        virtual public void OnProgress() { }
        abstract public void OnEnd();
    }
}

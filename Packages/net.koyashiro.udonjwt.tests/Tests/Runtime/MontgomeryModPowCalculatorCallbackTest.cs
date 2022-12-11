using UnityEngine;
using Koyashiro.UdonJwt.Numerics;
using Koyashiro.UdonTest;

namespace Koyashiro.UdonJwt.Tests
{
    public class MontgomeryModPowCalculatorCallbackTest : MontgomeryModPowCalculatorCallback
    {
        public override void OnProgress()
        {
            Debug.Log($"Progress: {Progress}");
        }

        public override void OnEnd()
        {
            // 986236757547332986472011617696226561292849812918563355472727826767720188564083584387121625107510786855734801053524719833194566624465665316622563244215340671405971599343902468620306327831715457360719532421388780770165778156818229863337344187575566725786793391480600129482653072861971002459947277805295727097226389568776499707662505334062639449916265137796823793276300221537201727072401742985542559596685092673521228140822200236743113743661549252453726123450722876929538747702356573783116197523966334991563351853851212597377279504828784682446028818761068109940631363816408458588267723732811613487140951927354435094
            var expected = new uint[] { 0xa7ba2616, 0x4637aa4e, 0x336d7637, 0x9121bafe, 0x66910b81, 0x83881a5b, 0x8502820a, 0x1ab1a2bb, 0x05000420, 0x03040201, 0x86480165, 0x0d060960, 0x00303130, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0x0001ffff };
            var actual = Result;
            Assert.Equal(expected, actual);
        }
    }
}

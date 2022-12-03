namespace Koyashiro.UdonJwt.Asn1
{
    public enum Asn1TagKind
    {
        Integer = 0x02,
        BitString = 0x03,
        OctetString = 0x04,
        Null = 0x05,
        ObjectIdentifier = 0x06,
        SequenceAndSequenceOf = 0x30 // (0x10 + 0x20)
    }
}

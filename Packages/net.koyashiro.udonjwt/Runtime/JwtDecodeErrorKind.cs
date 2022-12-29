namespace Koyashiro.UdonJwt
{
    public enum JwtDecodeErrorKind
    {
        None,
        Busy,
        InvalidToken,
        InvalidSignature,
        ExpiredToken,
        Other,
    }
}

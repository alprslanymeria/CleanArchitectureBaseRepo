namespace Base.Domain.Options;

public class TokenOption
{
    public const string Key = "TokenOptions";

    /// <summary>
    /// WHO CAN USE THE TOKEN
    /// </summary>
    public List<string> Audience { get; set; } = null!;

    /// <summary>
    /// WHO CREATED THE TOKEN
    /// </summary>
    public string Issuer { get; set; } = null!;

    /// <summary>
    /// EXPIRATION TIME OF THE ACCESS TOKEN (IN MINUTES)
    /// </summary>
    public int AccessTokenExpiration { get; set; }

    /// <summary>
    /// SECURITY KEY USED TO SIGN THE TOKEN
    /// </summary>
    public string SecurityKey { get; set; } = null!;
}
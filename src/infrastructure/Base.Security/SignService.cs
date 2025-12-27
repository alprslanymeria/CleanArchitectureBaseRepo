using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Base.Security;

public static class SignService
{
    /// <summary>
    /// SYMMETRIC KEY FOR SIGN TOKEN
    /// </summary>
    public static SecurityKey GetSymmetricKey(string securityKey)
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
    }
}
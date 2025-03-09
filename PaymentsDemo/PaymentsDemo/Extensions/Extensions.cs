using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PaymentsDemo.Extensions;

public static class Extensions
{
    public static SymmetricSecurityKey ToSecurityKey(this string str) => new (Encoding.UTF8.GetBytes(str));
}
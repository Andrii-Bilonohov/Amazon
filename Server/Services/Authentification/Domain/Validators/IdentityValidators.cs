using System.Text.RegularExpressions;

namespace Domain.Validators;

public static partial class IdentityValidators
{
    [GeneratedRegex(@"^\+?\d{1,3}?[- .]?\(?\d{1,4}?\)?[- .]?\d{1,4}[- .]?\d{1,9}$", RegexOptions.IgnoreCase)]
    private static partial Regex PhoneRegexGenerated();
    
    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegexGenerated();
    
    public static bool IsValidPhone(string phone) => 
        !string.IsNullOrWhiteSpace(phone) && PhoneRegexGenerated().IsMatch(phone);

    public static bool IsValidEmail(string email) => 
        !string.IsNullOrWhiteSpace(email) && EmailRegexGenerated().IsMatch(email);
    
    public static bool IsValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
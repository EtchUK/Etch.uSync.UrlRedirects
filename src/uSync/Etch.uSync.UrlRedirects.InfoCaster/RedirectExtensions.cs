using Umbraco.Extensions;
using UrlTracker.Core.Models;

namespace Etch.uSync.UrlRedirects;

public static class RedirectExtensions
{
    public static Guid GetKey(this Redirect redirect)
    {
        return (redirect.SourceUrl ?? redirect.SourceRegex ?? redirect.Id.ToString()).ToGuid();
    }
}

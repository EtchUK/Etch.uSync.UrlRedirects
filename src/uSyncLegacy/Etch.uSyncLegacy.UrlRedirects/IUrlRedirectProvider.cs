using System.Collections.Generic;

namespace Etch.uSyncLegacy.UrlRedirects
{
    public interface IUrlRedirectProvider
    {
        IEnumerable<UrlRedirect> GetAll();
    }
}

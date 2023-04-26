using System.Collections.Generic;

namespace Etch.uSyncLegacy.UrlRedirects
{
    public class DummyUrlRedirectProvider : IUrlRedirectProvider
    {
        public IEnumerable<UrlRedirect> GetAll()
        {
            throw new System.NotImplementedException("Be sure to set a URL redirect provider on the UrlRedirectContext");
        }
    }
}

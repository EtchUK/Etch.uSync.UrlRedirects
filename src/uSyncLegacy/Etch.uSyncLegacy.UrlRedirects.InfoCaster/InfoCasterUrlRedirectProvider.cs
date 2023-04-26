using InfoCaster.Umbraco.UrlTracker.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Etch.uSyncLegacy.UrlRedirects.InfoCaster
{
    public class InfoCasterUrlRedirectProvider : IUrlRedirectProvider
    {
        public IEnumerable<UrlRedirect> GetAll()
        {
            return UrlTrackerRepository.GetUrlTrackerEntries().Select(x => new UrlRedirect
            {
                Id = (x.OldUrl ?? x.OldRegex).ToGuid(),
                ForceRedirect = x.ForceRedirect,
                Notes = x.Notes,
                ForwardQueryString = x.RedirectPassThroughQueryString,
                Created = x.Inserted,
                InboundUrl = x.OldUrl,
                InboundRegex = x.OldRegex,
                DestinationNodeId = x.RedirectNodeId,
                DestinationUrl = x.RedirectUrl,
                RootNodeId = x.RedirectRootNodeId,
            });
        }
    }
}

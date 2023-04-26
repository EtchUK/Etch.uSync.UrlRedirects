using Jumoo.uSync.BackOffice;
using Jumoo.uSync.BackOffice.Handlers;
using Jumoo.uSync.BackOffice.Helpers;
using Jumoo.uSync.Core;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Etch.uSyncLegacy.UrlRedirects
{
    public class UrlRedirectHandler : uSyncBaseHandler<UrlRedirect>, ISyncHandler
    {
        private IContentService contentService;
        public UrlRedirectHandler()
        {
            contentService = ApplicationContext.Current.Services.ContentService;
        }

        public int Priority => 200;

        public string Name => "uSync: UrlRedirect handler";

        public string SyncFolder => "UrlRedirect";

        public IEnumerable<uSyncAction> ExportAll(string folder)
        {
            return UrlRedirectContext.Current.UrlRedirectProvider.GetAll()
                .Select(item => ExportItem(item, folder))
                .ToList();
        }

        private uSyncAction ExportItem(UrlRedirect item, string rootFolder)
        {
            try
            {
                PopulateDestinationKeyAndUrl(item);

                var attempt = ((UrlRedirectSerializer)uSyncCoreContext.Instance.Serailizers[UrlRedirectSerializer.SerializerName]).Serialize(item);

                string filename = string.Empty;
                if (attempt.Success)
                {
                    filename = uSyncIOHelper.SavePath(rootFolder, SyncFolder, string.Empty, item.Id.ToString());
                    uSyncIOHelper.SaveNode(attempt.Item, filename);
                }

                return uSyncActionHelper<XElement>.SetAction(attempt, filename);
            }
            catch (Exception ex)
            {
                return uSyncAction.Fail(item.Name, typeof(UrlRedirect), ChangeType.Export, ex);
            }
        }

        public override Jumoo.uSync.Core.SyncAttempt<UrlRedirect> Import(string filePath, bool force = false)
        {
            throw new System.NotImplementedException();
        }

        public void RegisterEvents()
        {
            // UrlRedirectContext.Current.UrlRedirectProvider = 
        }

        public override uSyncAction ReportItem(string file)
        {
            throw new System.NotImplementedException();
        }

        private void PopulateDestinationKeyAndUrl(UrlRedirect redirect)
        {
            if (!redirect.DestinationNodeKey.HasValue && redirect.DestinationNodeId.HasValue)
            {
                var content = contentService.GetById(redirect.DestinationNodeId.Value);
                if (content != null)
                {
                    redirect.DestinationNodeKey = content.Key;
                }
            }

            if (string.IsNullOrEmpty(redirect.DestinationUrl) && redirect.DestinationNodeId.HasValue)
            {
                var publishedContent = UmbracoContext.Current.ContentCache.GetById(redirect.DestinationNodeId.Value);
                if (publishedContent != null)
                {
                    redirect.DestinationUrl = publishedContent.Url;
                }
            }

            if (!redirect.RootNodeKey.HasValue)
            {
                var content = contentService.GetById(redirect.RootNodeId);
                if (content != null)
                {
                    redirect.RootNodeKey = content.Key;
                }
            }
        }
    }
}

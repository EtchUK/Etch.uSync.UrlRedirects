using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using uSync.BackOffice;
using uSync.BackOffice.Configuration;
using uSync.BackOffice.Services;
using uSync.BackOffice.SyncHandlers;

namespace Etch.uSync.UrlRedirects;
/// <summary>
///  Handler to mange Domain settings for uSync
/// </summary>
[SyncHandler("UrlRedirectHandler", "UrlRedirects", "UrlRedirects", uSyncConstants.Priorites.USYNC_RESERVED_UPPER + 10, Icon = "icon-home usync-addon-icon", EntityType = "urlredirect")]
public class UrlRedirectHandler : SyncHandlerBase<UrlRedirect, UrlRedirectService>, ISyncHandler,
    INotificationHandler<SavedNotification<UrlRedirect>>,
    INotificationHandler<DeletedNotification<UrlRedirect>>
{
    public UrlRedirectHandler(ILogger<SyncHandlerBase<UrlRedirect, UrlRedirectService>> logger, IEntityService entityService, AppCaches appCaches, IShortStringHelper shortStringHelper, SyncFileService syncFileService, uSyncEventService mutexService, uSyncConfigService uSyncConfig, global::uSync.Core.ISyncItemFactory syncItemFactory)
        : base(logger, entityService, appCaches, shortStringHelper, syncFileService, mutexService, uSyncConfig, syncItemFactory)
    {
    }

    protected override string GetItemName(UrlRedirect item)
    {
        return item.Name;
    }

    /// <inheritdoc/>
    public override IEnumerable<uSyncAction> ExportAll(string folder, HandlerSettings config, SyncUpdateCallback callback)
    {
        throw new NotImplementedException();
    }
}

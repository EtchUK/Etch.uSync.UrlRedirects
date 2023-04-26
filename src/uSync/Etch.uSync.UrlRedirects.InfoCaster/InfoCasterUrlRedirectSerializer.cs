using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using UrlTracker.Core;
using uSync.Core;
using uSync.Core.Models;
using uSync.Core.Serialization;

namespace Etch.uSync.UrlRedirects;

[SyncSerializer("08EB033D-132E-4C38-9C9E-9C2345788C61", "InfoCaster Url Redirect Serializer", "UrlRedirect")]
public class InfoCasterUrlRedirectSerializer : SyncSerializerBase<UrlRedirect>, ISyncSerializer<UrlRedirect>
{
    private readonly IRedirectService redirectService;
    private readonly IUmbracoContextFactory umbracoContextFactory;

    public InfoCasterUrlRedirectSerializer(IEntityService entityService, ILogger<SyncSerializerBase<UrlRedirect>> logger, IRedirectService redirectService, IUmbracoContextFactory umbracoContextFactory)
        : base(entityService, logger)
    {
        this.redirectService = redirectService;
        this.umbracoContextFactory = umbracoContextFactory;
    }

    public override void DeleteItem(UrlRedirect item)
    {
        var redirect = Task.Run(() => redirectService.GetAsync(item.Id)).GetAwaiter().GetResult();
        if (redirect == null)
        {
            return;
        }

        Task.Run(() => redirectService.DeleteAsync(redirect)).GetAwaiter().GetResult();
    }

    public override UrlRedirect FindItem(int id)
    {
        var item = Task.Run(() => redirectService.GetAsync(id)).GetAwaiter().GetResult();
        return MapFromRedirect(item);
    }

    public override UrlRedirect FindItem(Guid key)
    {
        var redirects = Task.Run(() => redirectService.GetAsync()).GetAwaiter().GetResult();
        var redirect = redirects
            .FirstOrDefault(r => r.GetKey() == key);
        return MapFromRedirect(redirect);
    }

    public override UrlRedirect FindItem(string alias)
    {
        var redirects = Task.Run(() => redirectService.GetAsync()).GetAwaiter().GetResult();
        var redirect = redirects
            .Select(MapFromRedirect)
            .FirstOrDefault(r => r?.Name == alias);
        return redirect;
    }

    public override string ItemAlias(UrlRedirect item)
    {
        return item.Name;
    }

    public override void SaveItem(UrlRedirect item)
    {
        var redirect = MapToRedirect(item);
        if (item.HasIdentity)
        {
            Task.Run(() => redirectService.UpdateAsync(redirect)).GetAwaiter().GetResult();
        }
        else
        {
            Task.Run(() => redirectService.AddAsync(redirect)).GetAwaiter().GetResult();
        }
    }

    protected override SyncAttempt<UrlRedirect> DeserializeCore(XElement node, SyncSerializerOptions options)
    {
        var item = new UrlRedirect
        {
            InboundUrl = node.Element("InboundUrl")?.Value,
            InboundRegex = node.Element("InboundRegex")?.Value,
            DestinationNodeId = int.TryParse(node.Element("DestinationNodeId")?.Value, out var n) ? n : default,
            DestinationNodeKey = Guid.TryParse(node.Element("DestinationNodeKey")?.Value, out var k) ? k : default(Guid?),
            DestinationUrl = node.Element("DestinationUrl")?.Value,
            RootNodeId = int.Parse(node.Element("RootNodeId")?.Value ?? "0"),
            RootNodeKey = Guid.TryParse(node.Element("RootNodeKey")?.Value, out var g) ? g : Guid.Empty, 
            ForceRedirect = bool.Parse(node.Element("ForceRedirect")?.Value ?? "false"),
            CreateDate = DateTime.TryParse(node.Element("Created")?.Value, out var d) ? d : default,
            Key = node.AttributeValue<Guid>("Key"),
        };

        return SyncAttempt<UrlRedirect>.Succeed(item.Name, item, typeof(UrlRedirect), ChangeType.Import);    
    }

    protected override SyncAttempt<XElement> SerializeCore(UrlRedirect item, SyncSerializerOptions options)
    {
        var node = new XElement("UrlRedirect");

        node.SetAttributeValue("Key", item.Id.ToString());
        node.SetAttributeValue("Alias", item.Name);

        node.Add(new XElement("InboundUrl", item.InboundUrl));
        node.Add(new XElement("InboundRegex", item.InboundRegex));
        node.Add(new XElement("DestinationNodeId", item.DestinationNodeId));
        node.Add(new XElement("DestinationNodeKey", item.DestinationNodeKey));
        node.Add(new XElement("DestinationUrl", item.DestinationUrl));
        node.Add(new XElement("RootNodeId", item.RootNodeId));
        node.Add(new XElement("RootNodeKey", item.RootNodeKey));
        node.Add(new XElement("ForceRedirect", item.ForceRedirect));
        node.Add(new XElement("Created", item.CreateDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffff'Z'")));

        return SyncAttempt<XElement>.SucceedIf(
            node != null, item.InboundUrl ?? item.InboundRegex, node, typeof(UrlRedirect), ChangeType.Export);
    }

    private UrlRedirect? MapFromRedirect(UrlTracker.Core.Models.Redirect? redirect)
    {
        if (redirect == null)
        {
            return null;
        }

        if ((redirect.TargetNode == null || redirect.TargetRootNode == null) && string.IsNullOrEmpty(redirect.TargetUrl))
        {
            throw new ArgumentException($"{nameof(redirect.TargetNode)} or {nameof(redirect.TargetUrl)} must not be null");
        }

        return new UrlRedirect()
        {
            Id = redirect.Id ?? default,
            Key = redirect.GetKey(),
            InboundUrl = redirect.SourceUrl,
            InboundRegex = redirect.SourceRegex,
            DestinationNodeId = redirect.TargetNode?.Id,
            DestinationNodeKey = redirect.TargetNode?.Key,
            DestinationUrl = redirect.TargetUrl,
            RootNodeId = redirect.TargetRootNode?.Id ?? default,
            RootNodeKey = redirect.TargetRootNode?.Key,
            ForceRedirect = redirect.Force,
            CreateDate = redirect.Inserted,
            Notes = redirect.Notes,
        };
    }

    private UrlTracker.Core.Models.Redirect MapToRedirect(UrlRedirect redirect)
    {
        using var context = umbracoContextFactory.EnsureUmbracoContext();
        var content = redirect.DestinationNodeKey == null ? null : context.UmbracoContext.Content?.GetById(redirect.DestinationNodeKey.Value);
        var root = redirect.RootNodeKey == null ? null : context.UmbracoContext.Content?.GetById(redirect.RootNodeKey.Value);
        return new UrlTracker.Core.Models.Redirect()
        {
            Id = redirect.Id,
            SourceUrl = redirect.InboundUrl,
            SourceRegex = redirect.InboundRegex,
            TargetNode = content,
            TargetUrl = content == null ? redirect.DestinationUrl : null, // only set Target URL if we don't have a valid content item
            TargetRootNode = root,
            Force = redirect.ForceRedirect,
            Inserted = redirect.CreateDate,
            Notes = redirect.Notes,
            TargetStatusCode = System.Net.HttpStatusCode.Moved,
        };
    }
}

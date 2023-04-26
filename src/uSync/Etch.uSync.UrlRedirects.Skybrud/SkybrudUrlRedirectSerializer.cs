using Microsoft.Extensions.Logging;
using Skybrud.Umbraco.Redirects.Models;
using Skybrud.Umbraco.Redirects.Services;
using System.Xml.Linq;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using uSync.Core;
using uSync.Core.Models;
using uSync.Core.Serialization;

namespace Etch.uSync.UrlRedirects;

[SyncSerializer("747F444F-A1CC-4F57-8188-D690C77BB1AC", "Skybrud Url Redirect Serializer", "UrlRedirect")]
public class SkybrudUrlRedirectSerializer : SyncSerializerBase<UrlRedirect>, ISyncSerializer<UrlRedirect>
{
    private readonly IUmbracoContextFactory umbracoContextFactory;
    private readonly IRedirectsService redirectsService;

    public SkybrudUrlRedirectSerializer(IEntityService entityService, ILogger<SyncSerializerBase<UrlRedirect>> logger, IUmbracoContextFactory umbracoContextFactory, IRedirectsService redirectsService)
        : base(entityService, logger)
    {
        this.umbracoContextFactory = umbracoContextFactory;
        this.redirectsService = redirectsService;
    }

    public override void DeleteItem(UrlRedirect item)
    {
        var redirect = redirectsService.GetRedirectById(item.Id);
        if (redirect == null)
        {
            return;
        }

        redirectsService.DeleteRedirect(redirect);
    }

    public override UrlRedirect FindItem(int id)
    {
        var item = redirectsService.GetRedirectById(id);
        return MapFromRedirect(item);
    }

    public override UrlRedirect FindItem(Guid key)
    {
        var redirect = redirectsService.GetRedirectByKey(key);
        return MapFromRedirect(redirect);
    }

    public override UrlRedirect FindItem(string alias)
    {
        var redirects = redirectsService.GetAllRedirects();
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
        if (redirect.Url.Trim('/') == redirect.Destination.Url.Trim('/'))
        {
            throw new ArgumentException("Circular redirect detected");
        }

        if (item.HasIdentity)
        {
            redirectsService.SaveRedirect(redirect);
        }
        else
        {
            redirectsService.AddRedirect(new Skybrud.Umbraco.Redirects.Models.Options.AddRedirectOptions
            {
                Type = redirect.Type,
                Destination = new RedirectDestination
                {
                    Type = redirect.Destination.Type,
                    Url = redirect.Destination.Url,
                    Fragment = redirect.Destination.Fragment,
                    Id = redirect.Destination.Id,
                    Key = redirect.Destination.Key,
                    Name = redirect.Destination.Name,
                    Query = redirect.Destination.Query,
                },
                RootNodeKey = redirect.RootKey,
                ForwardQueryString = redirect.ForwardQueryString,
                IsPermanent = redirect.IsPermanent,
                OriginalUrl = redirect.Url,
            });
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
            ForwardQueryString = bool.Parse(node.Element("ForwardQueryString")?.Value ?? "true"),
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
        node.Add(new XElement("ForwardQueryString", item.ForwardQueryString));
        node.Add(new XElement("Created", item.CreateDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffff'Z'")));

        return SyncAttempt<XElement>.SucceedIf(
            node != null, item.InboundUrl ?? item.InboundRegex, node, typeof(UrlRedirect), ChangeType.Export);
    }

    private UrlRedirect? MapFromRedirect(IRedirect? redirect)
    {
        if (redirect == null)
        {
            return null;
        }

        return new UrlRedirect()
        {
            Id = redirect.Id,
            Key = redirect.Key,
            InboundUrl = redirect.Url,
            DestinationNodeId = redirect.Destination.Id,
            DestinationNodeKey = redirect.Destination.Key,
            DestinationUrl = redirect.Destination.Url,
            RootNodeKey = redirect.RootKey,
            CreateDate = redirect.CreateDate.DateTimeOffset.DateTime,
            UpdateDate = redirect.UpdateDate.DateTimeOffset.DateTime,
            ForwardQueryString = redirect.ForwardQueryString,
        };
    }

    private IRedirect MapToRedirect(UrlRedirect redirect)
    {
        using var context = umbracoContextFactory.EnsureUmbracoContext();
        var content = redirect.DestinationNodeKey == null ? null : context.UmbracoContext.Content?.GetById(redirect.DestinationNodeKey.Value);
        var root = redirectsService.GetDomains()
            .Select(r => context.UmbracoContext.Content?.GetById(r.RootNodeId))
            .WhereNotNull()
            .FirstOrDefault(r => r.Key == redirect.RootNodeKey);
        var destinationUrl = content == null ? redirect.DestinationUrl ?? "/" : content.Url();
        return new Redirect()
        {
            Url = $"/{redirect.InboundUrl?.TrimStart("/")}",
            RootKey = root?.Key ?? Guid.Empty,
            CreateDate = redirect.CreateDate,
            UpdateDate = redirect.UpdateDate,
            Type = RedirectType.Permanent,
            IsPermanent = true,
            Destination = new RedirectDestination
            {
                Id = content?.Id ?? default,
                Key = redirect.DestinationNodeKey ?? Guid.Empty,
                Url = destinationUrl,
                Type = redirect.DestinationNodeKey.HasValue ? RedirectDestinationType.Content : RedirectDestinationType.Url,
            },
            ForwardQueryString = redirect.ForwardQueryString,
        };
    }
}

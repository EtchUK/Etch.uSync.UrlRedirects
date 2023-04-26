using Umbraco.Cms.Core.Models.Entities;

namespace Etch.uSync.UrlRedirects;

public class UrlRedirect : EntityBase
{
    public string? InboundUrl { get; set; }

    public string? InboundRegex { get; set; }

    public int RootNodeId { get; set; }

    public Guid? RootNodeKey { get; set; }

    public int? DestinationNodeId { get; set; }

    public Guid? DestinationNodeKey { get; set; }

    public string? DestinationUrl { get; set; }

    public bool ForceRedirect { get; set; }

    public string? Notes { get; set; }

    public bool ForwardQueryString { get; set; }

    public string Name => InboundUrl ?? InboundRegex ?? Key.ToString();
}
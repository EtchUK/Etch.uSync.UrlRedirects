using Jumoo.uSync.Core;
using System;
using System.Xml.Linq;
using System.Collections.Generic;
using Jumoo.uSync.Core.Helpers;
using Jumoo.uSync.Core.Interfaces;

namespace Etch.uSyncLegacy.UrlRedirects
{
    public class UrlRedirectSerializer : ISyncSerializer<UrlRedirect>, ISyncChangeDetail
    {
        public const string SerializerName = "UrlRedirect";

        public string SerializerType => SerializerName;

        public int Priority => 200;

        public IEnumerable<uSyncChange> GetChanges(XElement node)
        {
            throw new NotImplementedException();
        }

        public SyncAttempt<XElement> Serialize(UrlRedirect item)
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
            node.Add(new XElement("Created", item.Created.ToString("yyyy-MM-ddTHH:mm:ss.fffffff'Z'")));

            return SyncAttempt<XElement>.SucceedIf(
                node != null, item.InboundUrl ?? item.InboundRegex, node, typeof(UrlRedirect), ChangeType.Export);
        }

        public SyncAttempt<UrlRedirect> DeSerialize(XElement node, bool forceUpdate)
        {
            throw new NotImplementedException();
        }

        public bool IsUpdate(XElement node)
        {
            throw new NotImplementedException();
        }
    }
}

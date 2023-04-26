namespace Etch.uSyncLegacy.UrlRedirects
{
    public class UrlRedirectContext
    {
        public static UrlRedirectContext Current { get; private set; } = new UrlRedirectContext();

        public IUrlRedirectProvider UrlRedirectProvider { get; set; } = new DummyUrlRedirectProvider();
    }
}

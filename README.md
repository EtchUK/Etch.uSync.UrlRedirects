# Etch.uSync.UrlRedirects

Addons for uSync that facilitate migration of Url Redirects from Umbraco 7 to 10+.

At the time of writing, there is support for:

- exporting URL redirects from the Umbraco 7 InfoCaster UrlTracker plugin
- importing URL redirects to the Skybrud.Umbraco.Redirects plugin

## How to use

1. Install Etch.uSyncLegacy.UrlRedirects and Etch.uSyncLegacy.UrlRedirects.InfoCaster into your Umbraco 7 project
2. Set the UrlRedirectProvider by adding this line to an ApplicationEventHandler startup class (such as [this one](https://github.com/jbreuer/Hybrid-Framework-for-Umbraco-v7-Best-Practises/blob/master/Umbraco.Extensions/Events/UmbracoEvents.cs#L44))
   `UrlRedirectContext.Current.UrlRedirectProvider = new InfoCasterUrlRedirectProvider();`
3. Run a full uSync Export
4. Copy the `uSync/data/UrlRedirect` folder to your Umbraco 10 project, to the following location: `uSync/v9/UrlRedirects` - note the pluralization of the `UrlRedirect` folder
5. Install Etch.uSync.UrlRedirects and Etch.uSync.UrlRedirects.Skybrud into your Umbraco 10 project
6. Add the following to your appsettings.json - this adds a new Handler Group for importing Url Redirects by themselves:

```
  "uSync": {
    "Sets": {
      "Default": {
        "Handlers": {
          "UrlRedirectHandler": {
            "Group": "Url Redirects"
          }
        }
      }
    }
  }
```

7. Run your Umbraco 10 site and click "Import" on the Url Redirects group in uSync.

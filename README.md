# SEO-friendly URL module

## Description

This module is used in all ParTech project to enable SEO-friendly URL's for Sitecore items.  
For example: The URL of an item with path "/About Us/what WE do" will be generated as "/about-us/what-we-do"  

The module consists of a LinkProvider and an ItemResolver.  
The LinkProvider will make sure that a friendly URL is returned by Sitecore.Links.LinkManager.GetItemUrl().  
The ItemResolver handles requests and resolves the friendly URL's to the correct item.  

No changes are made to Sitecore, it only requires the config file and DLL to be installed and you're good to go!


## Usage
- Forcing of friendly URL is only applied for GET requests
- Configure the LinkProvider in the config file (in /App_Config/Include) using the following attributes (all are optional):
	- applyForSites: Apply the linkprovider ONLY to for sites that are listed here (comma separated, use the names that are used in <site name=""> attributes).
	- ignoreForSites: Don't apply the linkprovider to the sites that are listed here.
	- forceFriendlyUrl: If true, all requests to items that are made without the friendly URL are 301-redirected to their friendly URL.
	- trailingSlash: If true, a trailing slash is always added to the friendly URL. If false, the trailing slash is always removed.
- Although "ignoreForSites" is optional, it's highly recommended that, if you're not using the *applyForSites* setting, you leave at least the default value (shell,login,admin) in there to prevent the Sitecore admin from breaking.


## References
Blog: http://www.partechit.nl/nl/blog/2013/05/sitecore-seo-friendly-url-module  
GitHub: https://github.com/ParTech/Seo-Friendly-Url


## Installation
The Sitecore package *[\Release\ParTech.Modules.SeoUrl-1.0.10.zip](https://github.com/ParTech/Seo-Friendly-Url/raw/master/Release/ParTech.Modules.SeoUrl-1.0.10.zip)* contains:
- Binary (release build).
- Configuration include file.

Use the Sitecore Installation Wizard to install the package.
After installation, the module will be immediately activated.


## Release notes
### 1.0.0
- Initial release.

### 1.0.1
- Fixed a bug that caused item resolvement to fail if useDisplayName was set to false for the linkprovider and the requested item had a displayname that's different from the item name.

### 1.0.2
- Added ignore for trailing slashes as suggested by scottmulligan@github

### 1.0.3
- Fixed a bug in which FindChild() was still being called even though the parent node couldn't be resolved. This could result in an incorrect item being resolved.

### 1.0.4
- Added configuration attribute for trailing slash behaviour.

### 1.0.5
- Fixed a bug in which in rare cases, no scheme is present in the base result of the LinkProvider which caused the Uri constructor to fail.

### 1.0.6
- Changed default value for trailing slash configuration to false, disabling trailing slashes in the URL.

### 1.0.7
- Added handling of the LinkProvider's ignore/apply sites setting to the ItemResolver.
- Changed default value for languageEmbedding to "asNeeded".
- Fixed a bug introduced since Sitecore versions 6.6 Update-7, 7.1 Update-1 and 7.2 that caused an infinite redirect loop when languageEmbedding="always" was combined with forceFriendlyUrl="true".
- Fixed a bug that caused invalid Sitecore Client links to be generated by the LinkProvider resulting in parts of the Sitecore Client returning 404 pages. All GetItemUrl() requests to ignored items (i.e. part of the sites in the ignoreForSites list) now use the UrlOptions from the default Sitecore LinkProvider or a hard-coded set of UrlOptions if the default provider has been removed.
- Fixed handling of inaccessible items. It uses a SecurityDisabler to allow unaccessible items to be located and applies security afterwards so Sitecore uses its own security handler to return an error page.

### 1.0.8
- Reverted a bug fix from 1.0.7 (concerning the possible infinite redirect loop) and implemented a new fix.
- Changed the logic for forcing friendly URL's to use the original URL from Sitecore's UrlRewriter module as request URL if it's available. This fixes the infinite loop bug that could occur in 1.0.6 (see last item on this release note list)
- Reverted the change in which default UrlOptions were used for all LinkProvider.GetItemUrl calls for items that belong to ignored sites. This behavior is no longer required. (see last item on this release note list)
- Added custom HTTP response header "X-SFUM-Redirect: true" when the ItemResolver redirects to force a friendly URL.
- Changed ItemResolver.ResolveItem method accessibility to be public static so you can call it from your own solution if needed. This was actually changed in 1.0.7, but we forgot to mention it.
- We will publish a blog post later that explains the behavior of "Languages.AlwaysStripLanguage" and how it must be used when it's combined with the "languageEmbedding" setting of the LinkProvider, but it basically comes down to this:  
**Never set *Languages.AlwaysStripLanguage="false"* when you are using *languageEmbedding="always"* (or *asNeeded*) with *languageLocation="filePath"* in your LinkProvider configuration**.

### 1.0.9
- Changed the logic for forcing friendly URL's again. See this blog post for more details: http://www.partechit.nl/blog/2014/03/seo-friendly-url-resolver-issue-striplanguage-and-alwaysstriplanguage

### 1.0.10
- Fixed a bug with double trailing slash on root item requests.

### 1.0.11
- Changed handling of non-alphanumeric characters. When they are replaced by hyphens, the double occurrences of hyphens are removed.

### 1.0.12
- Changed handling of friendly URL enforcement for wildcard items. They are now ignored.

### 1.0.13
- Added a way to disable *force friendly URL* for specific requests from another *HttpRequestBegin* processor. This can be done by setting an HttpContext item to true in your own ItemResolver.   
Example: `args.Context.Items[ParTech.Modules.SeoUrl.Pipelines.ItemResolver.DisableForceFriendlyUrlKey] = true;`


## Author
This solution was brought to you and is supported by Ruud van Falier, ParTech IT

Twitter: @BrruuD / @ParTechIT   
E-mail: ruud@partechit.nl   
Web: http://www.partechit.nl
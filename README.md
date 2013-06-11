SEO-friendly URL module
==========================

Description
-----------
This module is used in all ParTech project to enable SEO-friendly URL's for Sitecore items.  
For example: The URL of an item with path "/About Us/what WE do" will be generated as "/about-us/what-we-do"  

The module consists of a LinkProvider and an ItemResolver.  
The LinkProvider will make sure that a friendly URL is returned by Sitecore.Links.LinkManager.GetItemUrl().  
The ItemResolver handles requests and resolves the friendly URL's to the correct item.  

No changes are made to Sitecore, it only requires the config file and DLL to be installed and you're good to go!


Usage:
------
- Forcing of friendly URL is only applied for GET requests
- Configure the LinkProvider in the config file (in /App_Config/Include) using the following attributes (all are optional):
	- applyForSites: Apply the linkprovider ONLY to for sites that are listed here (comma separated, use the names that are used in <site name=""> attributes).
	- ignoreForSites: Don't apply the linkprovider to the sites that are listed here.
	- forceFriendlyUrl: If true, all requests to items that are made without the friendly URL are 301-redirected to their friendly URL.
- Although "ignoreForSites" is optional, it's highly recommended you leave at least the default value (shell,login,admin) in there to prevent the Sitecore admin from breaking.


References
------------
Blog: http://www.partechit.nl/nl/blog/2013/05/sitecore-seo-friendly-url-module  
GitHub: https://github.com/ParTech/Seo-Friendly-Url


Installation
------------
The Sitecore package *\Release\ParTech.Modules.SeoUrl-1.0.0.zip* contains:
- Binary (release build).
- Configuration include file.

Use the Sitecore Installation Wizard to install the package.
After installation, the module will be immediately activated.


Release notes
-------------
*1.0.0*
- Initial release.

*1.0.1*
- Fixed a bug that caused item resolvement to fail if useDisplayName was set to false for the linkprovider and the requested item had a displayname that's different from the item name.

Author
------
This solution was brought to you and supported by Ruud van Falier, ParTech IT

Twitter: @BrruuD / @ParTechIT   
E-mail: ruud@partechit.nl   
Web: http://www.partechit.nl
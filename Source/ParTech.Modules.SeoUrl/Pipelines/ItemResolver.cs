using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Data.Items;
using Sitecore.Links;
using Sitecore.Collections;
using Sitecore.Configuration;
using ParTechProviders = ParTech.Modules.SeoUrl.Providers;

namespace ParTech.Modules.SeoUrl.Pipelines
{
    public class ItemResolver : HttpRequestProcessor
    {
        public override void Process(HttpRequestArgs args)
        {
            // If there was a file found on disk for the current request, don't resolve an item
            if (Sitecore.Context.Page != null && !string.IsNullOrWhiteSpace(Sitecore.Context.Page.FilePath))
            {
                return;
            }

            // Only process if we are not using the Core database (which means we are requesting parts of Sitecore admin)
            if (Sitecore.Context.Database == null || Sitecore.Context.Database.Name.Equals("core", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            // Only continue if Sitecore has not found an item yet
            if (args != null && !string.IsNullOrEmpty(args.Url.ItemPath) && Sitecore.Context.Item == null)
            {
                string path = Sitecore.MainUtil.DecodeName(args.Url.ItemPath);
                
                // Resolve the item based on the requested path
                Sitecore.Context.Item = ResolveItem(path);
            }

            // If the item was not requested using its SEO-friendly URL, 301 redirect to force friendly URL
            if (Sitecore.Context.Item != null && Sitecore.Context.PageMode.IsNormal)
            {
                var provider = LinkManager.Provider as ParTechProviders.LinkProvider;
                if (provider != null && provider.ForceFriendlyUrl)
                {
                    ForceFriendlyUrl();
                }
            }
        }

        /// <summary>
        /// Resolve the item with specified path by traversing the Sitecore tree
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private Item ResolveItem(string path)
        {
            bool resolveComplete = false;

            // Only continue if the requested item belongs to the current site
            if (string.IsNullOrEmpty(Sitecore.Context.Site.RootPath) || !path.StartsWith(Sitecore.Context.Site.RootPath, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;                
            }
            
            // Strip website's rootpath from item path
            path = path.Remove(0, Sitecore.Context.Site.RootPath.Length);

            // Start searching from the site root
            string resolvedPath = Sitecore.Context.Site.RootPath;
            string[] itemNames = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < itemNames.Length; i++)
            {
                string itemName = itemNames[i];

                if (!string.IsNullOrWhiteSpace(itemName))
                {
                    Item child = FindChild(resolvedPath, ParTechProviders.LinkProvider.Normalize(itemName));

                    if (child != null)
                    {
                        resolvedPath = child.Paths.FullPath;
                        resolveComplete = i == itemNames.Length - 1;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // Only return an item if we completely resolved the requested path
            if (resolveComplete)
            {
                return Sitecore.Context.Database.GetItem(resolvedPath);
            }

            return null;
        }

        /// <summary>
        /// Search the children of parentPath for one that matched the normalized item name
        /// </summary>
        /// <param name="parentPath"></param>
        /// <param name="normalizedItemName"></param>
        /// <returns></returns>
        private Item FindChild(string parentPath, string normalizedItemName)
        {
            Item result = null;

            if (!string.IsNullOrWhiteSpace(parentPath))
            {
                ChildList children = Sitecore.Context.Database.GetItem(parentPath).Children;

                foreach (Item child in children)
                {
                    if (ParTechProviders.LinkProvider.Normalize(child.Name).Equals(normalizedItemName, StringComparison.InvariantCultureIgnoreCase)
                        || ParTechProviders.LinkProvider.Normalize(child.DisplayName).Equals(normalizedItemName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        result = child;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Force items to be requested using their SEO-friendly URL (by 301 redirecting )
        /// </summary>
        private void ForceFriendlyUrl()
        {
            // Only apply for GET requests
            if (HttpContext.Current.Request.HttpMethod.Equals("get", StringComparison.InvariantCultureIgnoreCase))
            {
                string requestedPath = ParTechProviders.LinkProvider.ToRelativeUrl(HttpContext.Current.Request.Url.AbsolutePath);
                string friendlyPath = ParTechProviders.LinkProvider.ToRelativeUrl(LinkManager.GetItemUrl(Sitecore.Context.Item));
                
                if (requestedPath != friendlyPath)
                {
                    // Redirect to the SEO-friendly URL
                    string friendlyUrl = string.Concat(LinkManager.GetItemUrl(Sitecore.Context.Item), HttpContext.Current.Request.Url.Query);

                    Redirect301(friendlyUrl);
                }
            }
        }

        /// <summary>
        /// Redirect to a URL using a 301 Moved Permanently header
        /// </summary>
        /// <param name="url"></param>
        private void Redirect301(string url)
        {
            var context = HttpContext.Current;

            context.Response.StatusCode = 301;
            context.Response.Status = "301 Moved Permanently";
            context.Response.CacheControl = "no-cache";
            context.Response.AddHeader("Location", url);
            context.Response.AddHeader("Pragma", "no-cache");
            context.Response.Expires = -1;

            context.Response.End();
        }
    }
}
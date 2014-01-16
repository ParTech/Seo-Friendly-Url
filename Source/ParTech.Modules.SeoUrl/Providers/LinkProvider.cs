using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Web;
using System.Collections.Specialized;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Links;
using Sitecore.Sites;
using Sitecore.Globalization;
using Sitecore.Web;

namespace ParTech.Modules.SeoUrl.Providers
{
    public class LinkProvider : Sitecore.Links.LinkProvider
    {
        public string[] ApplyForSites { get; set; }

        public string[] IgnoreForSites { get; set; }

        public bool ForceFriendlyUrl { get; set; }

        public bool TrailingSlash { get; set; }

        /// <summary>
        /// Initialize the LinkProvider
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            // Load forceFriendlyUrl attribute value
            ForceFriendlyUrl = MainUtil.GetBool(config["forceFriendlyUrl"], false);

            // Load trailingSlash attribute value
            TrailingSlash = MainUtil.GetBool(config["trailingSlash"], false);

            // Load applyForSites attribute value
            string attr = StringUtil.GetString(config["applyForSites"], string.Empty);

            if (!string.IsNullOrEmpty(attr))
            {
                ApplyForSites = StringUtil.GetString(config["applyForSites"], string.Empty)
                    .Replace(" ", string.Empty)
                    .ToLower()
                    .Split(',');
            }

            // Load ignoreForSites attribute value
            attr = StringUtil.GetString(config["ignoreForSites"], string.Empty);

            if (!string.IsNullOrEmpty(attr))
            {
                IgnoreForSites = StringUtil.GetString(config["ignoreForSites"], string.Empty)
                    .Replace(" ", string.Empty)
                    .ToLower()
                    .Split(',');
            }
        }

        /// <summary>
        /// Get a SEO friendly URL for the specified item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override string GetItemUrl(Item item, UrlOptions options)
        {
            // Ignore custom linkprovider for sites listed in ignoreForSites attribute.
            // Only apply custom linkprovider for specified sites listed in applyForSites attribute (if any are specified)
            // Only continue if we have a database so we're able to resolve the item
            if ((IgnoreForSites != null && IgnoreForSites.Contains(options.Site.Name.ToLower()))
                || (ApplyForSites != null && !ApplyForSites.Contains(options.Site.Name.ToLower()))
                || Sitecore.Context.Database == null)
            {
                return base.GetItemUrl(item, options);
            }

            // Retrieve the full URL including domain using the SiteResolving option
            string url = base.GetItemUrl(item, new UrlOptions
            {
                AlwaysIncludeServerUrl = true,
                SiteResolving = true,
                AddAspxExtension = options.AddAspxExtension,
                EncodeNames = options.EncodeNames,
                LanguageEmbedding = options.LanguageEmbedding,
                LanguageLocation = options.LanguageLocation,
                Language = options.Language,
                LowercaseUrls = options.LowercaseUrls,
                UseDisplayName = options.UseDisplayName
            });

            var uri = new Uri(url);            
            string path = Normalize(uri.GetComponents(UriComponents.Path, UriFormat.Unescaped));

            string trailingSlash = TrailingSlash
                ? "/"
                : string.Empty;

            // Only include scheme and domain if the item is from another site than the current site
            Item root = Sitecore.Context.Database.GetItem(options.Site.RootPath);

            if (root == null || !root.Paths.FullPath.Equals(options.Site.RootPath, StringComparison.InvariantCultureIgnoreCase) || options.AlwaysIncludeServerUrl)
            {
                string domain = uri.GetComponents(UriComponents.Host, UriFormat.Unescaped);
                string scheme = string.Concat(GetRequestScheme(), "://");

                return string.Concat(scheme, domain, "/", path, trailingSlash);
            }

            // Return the relative URL
            return string.Concat("/", path, trailingSlash);
        }

        /// <summary>
        /// Replace all non-alphanumeric characters with hyphens, ignoring slashes
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string Normalize(string path)
        {
            if (path == null)
            {
                return path;
            }

            string replaced = string.Empty;
            path.ToList().ForEach(x => replaced += char.IsLetterOrDigit(x) || x == '/' ? char.ToLower(x) : '-');

            return replaced;
        }

        /// <summary>
        /// Converts an absolute url to a relative url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string ToRelativeUrl(string url)
        {
            return Regex.Replace(url, @"^https?://[^/]+", string.Empty);
        }

        /// <summary>
        /// Find out wheter http or https is used during the current request
        /// </summary>
        /// <returns>String "http" or "https"</returns>
        private string GetRequestScheme()
        {
            if (HttpContext.Current != null && HttpContext.Current.Request.IsSecureConnection)
            {
                return "https";
            }

            return "http";
        }
    }
}
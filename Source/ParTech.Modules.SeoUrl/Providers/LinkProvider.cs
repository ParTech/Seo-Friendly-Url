namespace ParTech.Modules.SeoUrl.Providers
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using Sitecore;
    using Sitecore.Data.Items;
    using Sitecore.Links;

    /// <summary>
    /// Sitecore LinkProvider that generates SEO-friendly URL's for items.
    /// </summary>
    public class LinkProvider : Sitecore.Links.LinkProvider
    {
        #region Public properties

        /// <summary>
        /// Gets or sets the apply for sites.
        /// </summary>
        public string[] ApplyForSites { get; set; }

        /// <summary>
        /// Gets or sets the ignore for sites.
        /// </summary>
        public string[] IgnoreForSites { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to force requests to use the SEO-friendly URL.
        /// </summary>
        public bool ForceFriendlyUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a trailing slash must be applied to the URL.
        /// </summary>
        public bool TrailingSlash { get; set; }

        #endregion

        #region Public static methods

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

        #endregion

        #region Public methods

        /// <summary>
        /// Initialize the LinkProvider
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            // Load forceFriendlyUrl attribute value
            this.ForceFriendlyUrl = MainUtil.GetBool(config["forceFriendlyUrl"], false);

            // Load trailingSlash attribute value
            this.TrailingSlash = MainUtil.GetBool(config["trailingSlash"], false);

            // Load applyForSites attribute value
            string attr = StringUtil.GetString((object)config["applyForSites"], string.Empty);

            if (!string.IsNullOrEmpty(attr))
            {
                this.ApplyForSites = StringUtil.GetString((object)config["applyForSites"], string.Empty)
                    .Replace(" ", string.Empty)
                    .ToLower()
                    .Split(',');
            }

            // Load ignoreForSites attribute value
            attr = StringUtil.GetString((object)config["ignoreForSites"], string.Empty);

            if (!string.IsNullOrEmpty(attr))
            {
                this.IgnoreForSites = StringUtil.GetString((object)config["ignoreForSites"], string.Empty)
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
            if ((this.IgnoreForSites != null && this.IgnoreForSites.Contains(options.Site.Name.ToLower()))
                || (this.ApplyForSites != null && !this.ApplyForSites.Contains(options.Site.Name.ToLower()))
                || Context.Database == null)
            {
                // Return base GetItemUrl result if the SEO-friendly LinkProvider is ignored.
                return base.GetItemUrl(item, options);
            }

            // Retrieve the full URL including domain using the SiteResolving option
            string url = base.GetItemUrl(item, new UrlOptions
            {
                AlwaysIncludeServerUrl = true,
                AddAspxExtension = options.AddAspxExtension,
                EncodeNames = options.EncodeNames,
                Language = options.Language,
                LanguageEmbedding = options.LanguageEmbedding,
                LanguageLocation = options.LanguageLocation,
                LowercaseUrls = options.LowercaseUrls,
                SiteResolving = true,
                UseDisplayName = options.UseDisplayName
            });

            // Uri constructor does not support empty protocol.
            // In some rare cases, the base LinkProvider can return URL's without specific protocol.
            if (url.StartsWith("://"))
            {
                url = string.Concat("http", url);
            }

            var uri = new Uri(url);
            string path = Normalize(uri.GetComponents(UriComponents.Path, UriFormat.Unescaped));

            //only add a slash if there is a path
            string trailingSlash = this.TrailingSlash && !(string.IsNullOrWhiteSpace(path))
               ? "/"
               : string.Empty;


            // Only include scheme and domain if the item is from another site than the current site
            Item root = Context.Database.GetItem(options.Site.RootPath);

            if (root == null || !root.Paths.FullPath.Equals(options.Site.RootPath, StringComparison.InvariantCultureIgnoreCase) || options.AlwaysIncludeServerUrl)
            {
                string domain = uri.GetComponents(UriComponents.Host, UriFormat.Unescaped);
                string scheme = string.Concat(this.GetRequestScheme(), "://");

                return string.Concat(scheme, domain, "/", path, trailingSlash);
            }

            // Return the relative URL
            return string.Concat("/", path, trailingSlash);
        }

        #endregion

        /// <summary>
        /// Find out wheter http or https is used during the current request
        /// </summary>
        /// <returns>String "http" or "https"</returns>
        private string GetRequestScheme()
        {
            return HttpContext.Current != null && HttpContext.Current.Request.IsSecureConnection
                ? "https"
                : "http";
        }
    }
}
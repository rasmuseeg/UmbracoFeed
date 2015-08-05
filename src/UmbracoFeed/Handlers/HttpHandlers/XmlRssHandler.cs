using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using umbraco.cms.businesslogic.web;
using umbraco;
using System.Xml.Linq;
using System.Globalization;
using umbraco.BusinessLogic;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using System.Web.Configuration;

namespace UmbracoFeed.Handlers.HttpHandlers
{
    // TODO: browserconfig.xml: https://msdn.microsoft.com/en-us/library/dn320426(v=vs.85).aspx
    // TODO: <link rel="alternate" type="application/rss+xml" href="http://www.xul.fr/rss.xml" title="Your title">
    // TODO: atom.xml: https://en.wikipedia.org/wiki/Atom_(standard)#Example_of_an_Atom_1.0_feed
    // TODO: <link href="atom.xml" type="application/atom+xml" rel="alternate" title="Sitewide ATOM Feed" />
    /// <summary>
    /// Add and httphandler for rss.xml
    /// Remember to include in head tag: <link rel="alternate" type="application/rss+xml" href="http://www.xul.fr/rss.xml" title="Your title">
    /// </summary>
    public class XmlRssHandler : IHttpHandler
    {
        private UmbracoContext umbContext;
        private ApplicationContext appContext;
        private ServiceContext services;
        private UmbracoHelper helper;

        public XmlRssHandler ()
        {
            umbContext = UmbracoContext.Current;
            appContext = ApplicationContext.Current;
            services = appContext.Services;
            helper = new UmbracoHelper( umbContext );
        }

        public void ProcessRequest (HttpContext context)
        {
            HttpRequest Request = context.Request;
            HttpResponse Response = context.Response;

            // Determine the current domain
            var domain = GetUmbracoDomains().FirstOrDefault();

            // Current site root
            var rootNode = services.ContentService.GetById( domain.RootNodeId );

            // Get the current language
            string language = rootNode.Language;

            // Get the current Umbraco version
            string version = WebConfigurationManager.AppSettings["umbracoConfigurationStatus"] as string + "";

            List<IContent> items = services.ContentService
              .GetDescendants( rootNode )
              .Where( p => p.Published )
              .OrderByDescending( p => p.UpdateDate )
              .ToList();

            DateTime pubDate = items.FirstOrDefault().CreateDate;
            DateTime lastModified = items.FirstOrDefault().UpdateDate;

            XDocument doc = new XDocument( 
                new XDeclaration( "1.0", "utf-8", "yes" ),
                new XProcessingInstruction("xml-stylesheet", "type='text/xsl' href='hello.xsl'")
            );
            XElement rss = new XElement( "rss", new XAttribute( "version", "2.0" ) );

            var typedRootNode = helper.TypedContent( rootNode.Id );

            XElement channel = new XElement( "channel",
                new XElement( "title", new XCData(helper.StripHtml( heltypedRootNode.Name ).ToString()) ),
                new XElement( "link", typedRootNode.NiceUrlWithDomain() ),
                new XElement( "description", new XCData( typedRootNode.Name ) ),
                new XElement( "language", language ),
                new XElement( "pubdate", UtcDate( pubDate ) ),
                new XElement( "lastbuilddate", UtcDate( lastModified ) ),
                new XElement( "generator", "Umbraco " + version ),
                new XElement( "image",
                  new XElement( "url", "" ),
                  new XElement( "title", "" ),
                  new XElement( "link", "" )
                )
              );

            foreach( var node in items )
            {
                channel.Add(
                  new XElement( "item",
                    new XElement( "title", new XCData(helper.StripHtml( node.Name ).ToString()) ),
                    new XElement( "link", helper.NiceUrlWithDomain( node.Id ) ),
                    new XElement( "description", new XCData( node.Name ) ),
                    new XElement( "author", "" ),
                    new XElement( "guid", 
                        new XAttribute("isPermaLink", false), 
                        helper.NiceUrlWithDomain( node.Id ) 
                    ),
                    new XElement( "category", ""),
                    new XElement( "pubdate", UtcDate( node.UpdateDate ) ),
                    new XElement( "comments", "" ), // url to comments
                    new XElement( "enclosure", 
                        new XAttribute("url", ""), // Actual url to image
                        new XAttribute("length", ""), // Size in bytes
                        new XAttribute( "type", "image/jpeg" ) // Standard meme type
                    )
                  )
                );
            }

            rss.Add( channel );
            doc.Add( rss );

            // Set headers
            Response.AddHeader( "Content-Type", "text/xml;charset=utf-8;" );
            Response.Write( doc.ToString() );
        }

        public bool IsReusable
        {
            get { return true; }
        }

        private static IEnumerable<umbraco.cms.businesslogic.web.Domain> GetUmbracoDomains ()
        {
            var sqlHelper = Application.SqlHelper;
            var result = new List<umbraco.cms.businesslogic.web.Domain>();
            using( var dr = sqlHelper.ExecuteReader( "select id, domainName from umbracoDomains" ) )
            {
                while( dr.Read() )
                {
                    var domainId = dr.GetInt( "id" );

                    result.Add( new umbraco.cms.businesslogic.web.Domain( domainId ) );
                }
            }
            return result;
        }

        public string UtcDate (DateTime date)
        {
            return date.ToString( "ddd, dd MMM yyyy HH:mm:ss" ).ToUpper() + "GMT";
        }
    }
}

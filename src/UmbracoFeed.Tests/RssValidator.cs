using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UmbracoFeed.Tests
{
    [TestClass]
    public class RssValidator
    {
        
        

        /// <summary>
        /// Make sure the rss link doesn't contains æøå
        /// </summary>
        [TestMethod]
        public void VerifyChars ()
        {
            
        }

        /// <summary>
        /// Title should not contain HTML
        /// </summary>
        [TestMethod]
        public void TitleElementsDoesNotContainHtml()
        {
            
        }

        /// <summary>
        /// Make sure description data is wrapped in <![CDATA[&shy;]]>
        /// </summary>
        [TestMethod]
        public void DescriiptionIsEnclosed()
        {
            
        }
    }
}

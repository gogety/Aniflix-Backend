using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SeleniumBrowserStdLib;

namespace Aniflix_WebAPI.Logic
{
    public class BrowserHelper
    {
        // For now, work with one browser instance ?
        //private static BrowserManager _browser;

        public static HtmlDocument ExecuteWebRequestHTTPWithJsDoc(string url, string xPathFilter = "", string xPathWaitFor = "", string attribute = "", int timeout = 10)
        {
            HtmlDocument doc = new HtmlDocument();
            String browserOutput = ExecuteWebRequestHTTPWithJs(url, xPathFilter, xPathWaitFor, attribute, timeout);
            doc.LoadHtml(browserOutput);
            return doc;
        }

        public static string ExecuteWebRequestHTTPWithJs(string url, string xPathFilter = "", string xPathWaitFor = "", string attribute = "", int timeout = 10)
        {
            String browserOutput = string.Empty;
            browserOutput = BrowserManager.Browse(url, xPathFilter,xPathWaitFor, attribute, timeout);
            return browserOutput;
        }

       
    }
}

using System;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SeleniumBrowserStdLib
{
    public class BrowserManager : IDisposable
    {
        private static ChromeDriver _chromeDriver;
        private const bool DEBUG = false;
        public BrowserManager(bool useAdBlock)
        {
            //Create chrome driver
            ChromeOptions chromeOptions = new ChromeOptions();

            if (useAdBlock) chromeOptions.AddArgument("user-data-dir=C:\\Selenium\\BrowserProfile");

            chromeOptions.AddArgument("disable-gpu");

            ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService("C:\\Users\\Sam\\Documents\\Visual Studio 2017\\Projects\\Aniflix\\SeleniumBrowserStdLib\\bin\\Debug\\netstandard2.0");
            chromeDriverService.SuppressInitialDiagnosticInformation = true;
            chromeDriverService.HideCommandPromptWindow = true;
            _chromeDriver = new ChromeDriver(chromeDriverService, chromeOptions);

        }

        private static void Initialize(bool useAdBlock)
        {
            //Create chrome driver
            ChromeOptions chromeOptions = new ChromeOptions();

            if (useAdBlock) chromeOptions.AddArgument("user-data-dir=C:\\Selenium\\BrowserProfile");

            chromeOptions.AddArgument("disable-gpu");
            //Do not wait for page to load completely, as we handle the wait ourselves
            chromeOptions.PageLoadStrategy = PageLoadStrategy.None;
            ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService("C:\\Users\\Sam\\Documents\\Visual Studio 2017\\Projects\\Aniflix\\SeleniumBrowserStdLib\\bin\\Debug\\netstandard2.0");
            chromeDriverService.SuppressInitialDiagnosticInformation = true;
            chromeDriverService.HideCommandPromptWindow = true;
            _chromeDriver = new ChromeDriver(chromeDriverService, chromeOptions);
        }


        public static String Browse(string url, string xPathFilter = "", string xPathWaitFor = "", string attribute = "", int timeout = 10)
        {
            if (_chromeDriver == null)
                Initialize(true);

            Options options;
            bool isValid = true;
            String response = String.Empty;
            if (String.IsNullOrEmpty(xPathWaitFor) && !String.IsNullOrEmpty(xPathFilter))
            {
                xPathWaitFor = xPathFilter;
            }

            //Parse options from arguments
            if (DEBUG)
            {
                options = new Options { Attribute = "src", Timeout = 30, URL = "http://m.anilinkz.to/overlord-ii-episode-1?src=7", XPathFilter = "//*[@class='spart']/iframe", XPathWaitFor = "//*[@class='spart']/iframe" };
                isValid = true;
            }
            else
            {
                options = new Options { Attribute = attribute, Timeout = timeout, URL = url, XPathFilter = xPathFilter, XPathWaitFor = xPathWaitFor };
            }

            if (!isValid)
            {
                response = "ERROR:Invalid arguments";
                return response;
            }


            //    using (ChromeDriver chromeDriver = new ChromeDriver(chromeDriverService, chromeOptions))

            //  {
            _chromeDriver.Navigate().GoToUrl(options.URL);
            IWebElement element = null;

            //if a waitfor is set, wait until the element is found
            //The wait method does not work if the browser is still loading
            if (!String.IsNullOrEmpty(options.XPathWaitFor))
            {
                WebDriverWait wait = new WebDriverWait(_chromeDriver, new System.TimeSpan(0, 0, options.Timeout));
                try
                {
                    //see for more details on xpath https://stackoverflow.com/questions/2009268/how-to-write-an-xpath-query-to-match-two-attributes
                    //The wait method does not work while the browser is still loading
                    element = wait.Until(ExpectedConditions.ElementExists(By.XPath(options.XPathWaitFor)));
                    if (element == null)
                        throw new NoSuchElementException();

                }
                catch (NoSuchElementException)
                {
                    response = "ERROR:WaitFor did not return an element.";
                    return response;
                }
                catch (WebDriverException ex)
                {
                    response = "ERROR:" + ex.Message;
                    return response;
                }
            }

            //if a specific filter is set, fetch the specified node
            if (!String.IsNullOrEmpty(options.XPathFilter) && !options.XPathFilter.Equals(options.XPathWaitFor))
            {
                element = _chromeDriver.FindElementByXPath(options.XPathFilter);
                if (element == null)
                {
                    response = "ERROR:Filter did not return an element.";
                    return response;
                }
            }

            //only look for a specific attribute if there is a filter
            if (!String.IsNullOrEmpty(options.XPathFilter))
            {
                //if a specific attribute is selected, get its content
                if (!String.IsNullOrEmpty(options.Attribute))
                {
                    string attributeValue = element.GetAttribute(options.Attribute);
                    if (String.IsNullOrEmpty(attributeValue))
                    {
                        response = "ERROR:Attribute not found.";
                        return response;
                    }
                    else
                    {
                        response = attributeValue;
                        return response;
                    }
                }
                //otherwise, return element filtered on
                else
                {
                    response = element.GetAttribute("outerHTML");
                    return response;
                }
            }
                   
            // if nothing is returned by now, return the whole DOM
           // response = _chromeDriver.FindElement(By.TagName("html")).GetAttribute("outerHTML");
           // response = _chromeDriver.FindElementByTagName("html").GetAttribute("outerHTML");
            response = _chromeDriver.PageSource;
            //   }
            return response;
        }

        public void Dispose()
        {
            _chromeDriver.Close();
            _chromeDriver.Dispose();
        }

        class Options
        {
            //[Option("url", HelpText = "Url to browse", Required = true)]
            public string URL { get; set; }

            //[Option("timeout", DefaultValue = 1, HelpText = "Timeout for request, in seconds")]
            public int Timeout { get; set; }

            // [Option("xpathfilter", DefaultValue = "", HelpText = "XPath Filter")]
            public string XPathFilter { get; set; }

            public string XPathWaitFor { get; set; }

            // [Option("attribute", DefaultValue = "", HelpText = "Attribute to return")]
            public string Attribute { get; set; }

            // [Option("useadblock", DefaultValue = false, HelpText = "Use AdBlock in the request (may slow the request by one second or so)")]
            public bool UseAdBlock { get; set; }
        }


    }
}

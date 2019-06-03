using System;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Remote;
using System.Threading;

namespace Aniflix_WebAPI.Logic
{
    public class BrowserManager : IDisposable
    {
        //private static IWebDriver driver;
        private const bool DEBUG = false;
        public BrowserManager(bool useAdBlock)
        {
            ////Create chrome driver
            //ChromeOptions chromeOptions = new ChromeOptions();

            //if (useAdBlock) chromeOptions.AddArgument("user-data-dir=C:\\Selenium\\BrowserProfile");
            //if (useAdBlock) chromeOptions.AddArgument("user-data-dir=C:\\Selenium\\BrowserProfile");


            //chromeOptions.AddArgument("disable-gpu");

            //ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService("C:\\Users\\Sam\\Documents\\Visual Studio 2017\\Projects\\Aniflix\\SeleniumBrowserStdLib\\bin\\Debug\\netstandard2.0");
            //chromeDriverService.SuppressInitialDiagnosticInformation = true;
            //chromeDriverService.HideCommandPromptWindow = true;
            //_chromeDriver = new ChromeDriver(chromeDriverService, chromeOptions);

            Initialize(useAdBlock);

        }

        private static IWebDriver Initialize(bool useAdBlock)
        {
            //Create chrome driver
            var chromeOptions = new ChromeOptions();

            //if (useAdBlock) chromeOptions.AddArgument("user-data-dir=C:\\SAM\\Selenium\\BrowserProfile");
            if (useAdBlock)  chromeOptions.AddArgument("user-data-dir=/app/bin/Debug/netcorapp2.0/BrowserProfile");


            chromeOptions.AddArgument("headless");
            chromeOptions.AddArgument("disable-gpu");
            //Do not wait for page to load completely, as we handle the wait ourselves
            chromeOptions.PageLoadStrategy = PageLoadStrategy.None;
            //ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService("C:\\SAM\\Selenium");
            //ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService("C:\\app\\bin\\Debug\\netcoreapp2.0");

            //chromeDriverService.SuppressInitialDiagnosticInformation = true;
            //chromeDriverService.HideCommandPromptWindow = true;
            //_chromeDriver = new ChromeDriver(chromeDriverService, chromeOptions);
            //_chromeDriver = new RemoteWebDriver(new Uri("http://selenium-chrome-standalone:4444/wd/hub"), chromeOptions);
            //_chromeDriver = new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub"), chromeOptions);
            //return new RemoteWebDriver(new Uri("http://selenium-chrome-standalone:4444/wd/hub"), chromeOptions);
            return new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub"), chromeOptions);

        }


        public static String Browse(string url, string xPathFilter = "", string xPathWaitFor = "", string attribute = "", int timeout = 10)
        {
            //if (_chromeDriver == null)
            //    Initialize(true);
            IWebDriver driver = Initialize(false);

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
            driver.Navigate().GoToUrl(options.URL);
            IWebElement element = null;

            //try catch for custom exceptions
            try
            {
                //if a waitfor is set, wait until the element is found
                //The wait method does not work if the browser is still loading
                if (!String.IsNullOrEmpty(options.XPathWaitFor))
                {
                    WebDriverWait wait = new WebDriverWait(driver, new System.TimeSpan(0, 0, options.Timeout));
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
                        throw new BrowserException("ERROR:WaitFor did not return an element.");
                    }
                    catch (WebDriverException ex)
                    {
                        throw new BrowserException("ERROR: " + ex.Message);
                    }
                }

                //if a specific filter is set, fetch the specified node
                if (!String.IsNullOrEmpty(options.XPathFilter) && !options.XPathFilter.Equals(options.XPathWaitFor))
                {
                    element = driver.FindElement(By.XPath(options.XPathFilter));
                    if (element == null)
                    {
                        throw new BrowserException("ERROR:Filter did not return an element.");
                    }
                }

                //only look for a specific attribute if there is a filter
                if (!String.IsNullOrEmpty(options.XPathFilter))
                {
                    //if a specific attribute is selected, get its content
                    if (!String.IsNullOrEmpty(options.Attribute))
                    {
                        string attributeValue = string.Empty;

                        //Stale elements can be found when the dom is changed between the moment the element is found and processed
                         attributeValue= GetElementEvenIfStale(element, options, driver).GetAttribute(options.Attribute);

                        if (String.IsNullOrEmpty(attributeValue))
                        {
                            throw new BrowserException("ERROR:Attribute not found.");
                        }
                        else
                        {
                            response = attributeValue;
                        }
                    }
                    //otherwise, return element filtered on
                    else
                    {
                        response = GetElementEvenIfStale(element, options, driver).GetAttribute("outerHTML");
                    }
                }
            }
            catch(BrowserException ex)
            {
                response = ex.Message;
            }
            finally
            {
                if (response.Length == 0)
                {
                    // if nothing is returned by now, return the whole DOM
                    response = driver.PageSource;
                }
                driver.Quit();
                //reset DOM to prevent new searches from detecting old elements
                //_chromeDriver.Navigate().GoToUrl("about:blank");
            }
            return response;
        }

        private static IWebElement GetElementEvenIfStale (IWebElement element,Options options, IWebDriver driver)
        {
            Thread.Sleep(1000);
            //assuming the element is not null - what to do if it's the case though ?
            try
            {
                if (element.Enabled)
                {
                    return element;
                }
                else
                {
                    throw new StaleElementReferenceException();
                }
            }
            catch (StaleElementReferenceException ex)
            {
                if (!string.IsNullOrEmpty(options.XPathFilter))
                {
                    return driver.FindElement(By.XPath(options.XPathFilter));
                }
                if (!string.IsNullOrEmpty(options.XPathWaitFor))
                {
                    return driver.FindElement(By.XPath(options.XPathWaitFor));
                }
                return null;
            }
                        
        }

        public void Dispose()
        {
            //driver.Close();
            //driver.Dispose();
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

        class BrowserException : Exception
        {
            public BrowserException(string message) : base(message) { }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aniflix_WebAPI.Logic
{
    public class SourceHelper
    {
        // TODO : replace strings with source IDs
        //private static IList<string> = new List<string>(){"sMango"};
        private static IDictionary<string, string[]> implementedSources = new Dictionary<string, string[]>()
        {
            {"sMango", new string[]{"//*[@id='mgvideo_html5_api']" , "src" } },
           // {"rVid", new string[]{"//source[@label='720p']", "src"}},
           // {"Vev", new string[]{"//video", "src"}},
            {"Tiwi", new string[]{"//video", "src"}},
            {"4up", new string[]{"//video", "src"}},
            {"Pbb", new string[]{"//video", "src"}},
            {"vZoo", new string[]{"//video", "src"}},
            {"Byzo", new string[]{"//video", "src"}},
            {"v44", new string[]{"//video", "src"}},
            {"p44", new string[]{"//video", "src"}},
            {"easyV", new string[]{"//video", "src"}},
            {"tVid", new string[]{"//video", "src"}}
        };

        public static bool IsImplemented(string sourceName)
        {
            return implementedSources.ContainsKey(sourceName);
        }

        // TODO : Some sort of factory ? review best pattern for this ...
        public static string FetchLinkFromSource(string sourceName, string url)
        {
            try
            {
                switch (IsImplemented(sourceName))
                {
                    case true:
                        string[] options = implementedSources[sourceName];
                        return BrowserHelper.ExecuteWebRequestHTTPWithJs(url, xPathFilter: options[0], attribute: options[1], timeout: 10); ;
                    default:
                        throw new InvalidOperationException("Invalid source");
                }
            }
            catch(Exception ex)
            {
                return ("ERROR: " +ex.Message);
            }
           
        }

    }
}

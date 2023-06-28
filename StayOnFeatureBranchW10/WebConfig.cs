using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace StayOnFeatureBranchW10
{
    class WebConfig
    {
        private const string wikiPage = "https://de.wikipedia.org/wiki/Microsoft_Windows_10";

        private const string tableHeader = "Windows-10-Versionsinformationen:";
        private const string tableRow = "<th scope=\"row\">";
        private const string tableEnd = "</table>";

        public string ReadWikiContent()
        {
            WebClient client = new WebClient();

            string wikiContent = client.DownloadString(wikiPage);

            int headerIndex = wikiContent.IndexOf(tableHeader);
            wikiContent = wikiContent.Substring(headerIndex);

            int tableEndIndex = wikiContent.IndexOf(tableEnd);
            wikiContent = wikiContent.Substring(0, tableEndIndex);

            wikiContent = wikiContent.Replace("\\", "");
            string[] delimiter = {tableRow};
            string[] contentArray = wikiContent.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

            return contentArray[3];
        }
    }
}

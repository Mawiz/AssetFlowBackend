using System.Net;
using System.Text;

namespace AssetFlow.Common.Helper
{
    public static class HtmlGenerator
    {
        public static string GetEmailBody(string templatePath, Dictionary<string, string> content)
        {
            string emailContent = null;

            using (var client = new WebClient())
            {
                var clientContent = client.DownloadData(templatePath);
                emailContent = Encoding.Default.GetString(clientContent).ToString();
            }

            StringBuilder template = new StringBuilder();
            template.Append(emailContent);

            foreach (KeyValuePair<string, string> pair in content)
            {
                template.Replace(pair.Key, pair.Value);
            }

            return template.ToString();
        }
    }
}

using System.Text;
using CSVision.Models;

namespace CSVision.Utilities
{
    public static class HtmlUtilities
    {
        public static string GenerateHtmlResponse(ModelResult content)
        {
            var htmlBuilder = new StringBuilder();

            htmlBuilder.AppendLine("<h3>Model prediction graph</h3>");
            // TODO:  Base64 image embedding

            htmlBuilder.AppendLine("<h3>Model evaluation</h3>");

            foreach (var metric in content.Metrics)
            {
                htmlBuilder.AppendLine(
                    $@"
                    <div class='metric'>
                        <b>{metric.Key}</b> 
                        <p>{metric.Value:F4}</p>
                    </div>"
                );
            }

            return htmlBuilder.ToString();
        }
    }
}

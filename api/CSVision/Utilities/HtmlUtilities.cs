using System.Text;
using CSVision.Models;
namespace CSVision.Utilities
{
    public static class HtmlUtilities
    {
        public static string GenerateHtmlResponse(ModelResult content, byte[] graphImage)
        {
            var htmlBuilder = new StringBuilder();

            htmlBuilder.AppendLine("<h3>Model prediction graph</h3>");
            string base64Image = Convert.ToBase64String(graphImage);

            return $@"
            <html>
                <head>
                    <title>Prediction Results</title>
                </head>
                <body>
                    <h1>Model: {content.ModelName}</h1>
                    <h2>Metrics</h2>
                    <ul>
                        {string.Join("", content.Metrics.Select(m => $"<li>{m.Key}: {m.Value:F4}</li>"))}
                    </ul>
                    <h2>Graph</h2>
                    <img src='data:image/png;base64,{base64Image}' alt='Prediction Graph' />
                </body>
            </html>";

        }
    }
}

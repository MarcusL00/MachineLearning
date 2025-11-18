using System.Text;
using CSVision.Models;
namespace CSVision.Utilities
{
    public static class HtmlUtilities
    {
        public static string GenerateHtmlResponse(ModelResult content, byte[] graphImage)
        {
            string base64Image = Convert.ToBase64String(graphImage);

            return $@"
            <html>
                <body>
                    <h2>Model: {content.ModelName}</h2>
                    <h3>Graph</h3>
                    <img src='data:image/png;base64,{base64Image}' alt='Prediction Graph' />
                    <h3>Metrics</h3>
                    <ul>
                        {string.Join("", content.Metrics.Select(m => $"<li>{m.Key}: {m.Value:F4}</li>"))}
                    </ul>
                </body>
            </html>";

        }
    }
}

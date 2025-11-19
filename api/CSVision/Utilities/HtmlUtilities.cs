using System.Text;
using CSVision.Models;
namespace CSVision.Utilities
{
    public static class HtmlUtilities
    {
        public static string GenerateHtmlResponse(ModelResult content, byte[] graphImage, byte[]? confusionMatrixImage)
        {
            string base64Image = Convert.ToBase64String(graphImage);
            string base64ConfusionMatrixImage = Convert.ToBase64String(confusionMatrixImage ?? Array.Empty<byte>());

            return $@"
            <html>
                <body>
                    <h2>Model: {content.ModelName}</h2>
                    <h3>Graph</h3>
                    <img src='data:image/png;base64,{base64Image}' alt='Prediction Graph' />
                    <img src='data:image/png;base64,{base64ConfusionMatrixImage}' alt='Confusion Matrix' />
                    <h3>Metrics</h3>
                    <ul>
                        {string.Join("", content.Metrics.Select(m => $"<li>{m.Key}: {m.Value:F4}</li>"))}
                    </ul>
                </body>
            </html>";

        }
    }
}

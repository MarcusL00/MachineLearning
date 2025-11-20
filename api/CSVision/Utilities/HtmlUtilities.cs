using System.Text;
using CSVision.Models;

namespace CSVision.Utilities
{
    public static class HtmlUtilities
    {
        /// <summary>
        /// Generates an HTML response string for a successful model training, including the model metrics and images.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="graphImage"></param>
        /// <param name="confusionMatrixImage"></param>
        /// <returns></returns>
        public static string GenerateSuccessfulHtmlResponse(
            ModelResult content,
            byte[] graphImage,
            byte[]? confusionMatrixImage
        )
        {
            StringBuilder stringBuilder = new StringBuilder();

            string base64Image = Convert.ToBase64String(graphImage);
            string base64ConfusionMatrixImage = Convert.ToBase64String(
                confusionMatrixImage ?? Array.Empty<byte>()
            );

            stringBuilder.AppendLine(
                $@"
            <html>
                <body>
                    <h2>Model: {content.ModelName}</h2>
                    <h3>Graph</h3>
                    <img src='data:image/png;base64, {base64Image}' alt='Prediction Graph' />"
            );

            if (confusionMatrixImage != null)
            {
                stringBuilder.AppendLine(
                    $@"
                    <h3>Confusion Matrix</h3>
                    <img src='data:image/png;base64, {base64ConfusionMatrixImage}' alt='Confusion Matrix' />"
                );
            }
            stringBuilder.AppendLine(
                @"
                    <h3>Metrics</h3>"
            );
            foreach (var metric in content.Metrics)
            {
                stringBuilder.AppendLine(
                    $@"
                    <p>{metric.Key}: {metric.Value:F4}</p>"
                );
            }

            stringBuilder.AppendLine(
                @"
                </body>
            </html>"
            );

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Generates an HTML response string for an error message.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static string GenerateErrorHtmlResponse(string errorMessage)
        {
            return $@"
            <html>
                <body>
                    <h2>Error</h2>
                    <p>{errorMessage}</p>
                </body>
            </html>";
        }
    }
}

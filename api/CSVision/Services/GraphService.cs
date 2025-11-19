using ScottPlot;
using CSVision.Interfaces;
using CSVision.Models;

namespace CSVision.Services
{
    public class GraphService : IGraphService
    {
        public byte[] GenerateGraph(ModelResult result)
        {
            // ModelName values from the concrete models currently include the word "Model" (e.g. "Logistic Regression Model").
            // Be tolerant when matching so variations don't prevent graph generation.
            var name = result?.ModelName ?? string.Empty;
            var actuals = result?.Actuals ?? Array.Empty<double>();
            var preds = result?.Predictions ?? Array.Empty<double>();

            result.Coefficients = 0.05;

            if (name.Contains("Linear Regression", StringComparison.OrdinalIgnoreCase))
                return GenerateLinearRegressionGraph(actuals, preds);

            if (name.Contains("Logistic Regression", StringComparison.OrdinalIgnoreCase))
                return GenerateLogisticRegressionGraph(actuals, preds, result?.FeatureName ?? string.Empty);

            // Fallback: decide by data shape (classification vs regression)
            if (IsClassificationData(actuals))
                return GenerateConfusionMatrixGraph(actuals, preds);

            return GenerateLinearRegressionGraph(actuals, preds);
        }
        private bool IsClassificationData(double[] values)
        {
            // Check if all values are integers (or very close to integers)
            return values.All(v => Math.Abs(v - Math.Round(v)) < 0.0001);
        }

        private byte[] GenerateLinearRegressionGraph(double[] actualValues, double[] predictedValues)
        {
            Plot myPlot = new();
            var sp = myPlot.Add.Scatter(actualValues, predictedValues);
            sp.LineWidth = 0;
            sp.MarkerSize = 10;

            // Perform regression
            var reg = new ScottPlot.Statistics.LinearRegression(actualValues, predictedValues);

            // Plot regression line
            var pt1 = new Coordinates(actualValues.Min(), reg.GetValue(actualValues.Min()));
            var pt2 = new Coordinates(actualValues.Max(), reg.GetValue(actualValues.Max()));
            var line = myPlot.Add.Line(pt1, pt2);
            line.LineWidth = 2;
            line.LinePattern = LinePattern.Dashed;

            // Show formula with R²
            myPlot.Title(reg.FormulaWithRSquared);
            return myPlot.GetImageBytes(600, 400, format: ImageFormat.Png);
        }
        public byte[] GenerateLogisticRegressionGraph(
            double[] actualValues,
            double[] predictedValues,
            string featureName)
        {
            Plot myPlot = new();

            // Clamp predictions to avoid NaN in logit transform
            double Clamp(double p) => Math.Min(0.999999, Math.Max(0.000001, p));
            double[] logits = predictedValues.Select(p => Math.Log(Clamp(p) / (1 - Clamp(p)))).ToArray();

            // Use index as independent variable
            double[] indices = Enumerable.Range(0, predictedValues.Length).Select(i => (double)i).ToArray();

            // Fit linear regression on logits vs. index
            var reg = new ScottPlot.Statistics.LinearRegression(indices, logits);
            double coefficients = reg.Slope;
            double intercept = reg.Offset;

            // Sort by index for smooth plotting
            var sorted = indices.Select((x, i) => new { X = x, Y = predictedValues[i], Actual = actualValues.ElementAtOrDefault(i) })
                .OrderBy(p => p.X)
                .ToArray();

            double[] sortedX = sorted.Select(p => p.X).ToArray();
            double[] sortedY = sorted.Select(p => p.Y).ToArray();
            double[] sortedActual = sorted.Select(p => p.Actual).ToArray();

            // Generate smooth sigmoid curve
            double minX = sortedX.Min();
            double maxX = sortedX.Max();
            double[] smoothX = Enumerable.Range(0, 100)
                .Select(i => minX + i * (maxX - minX) / 99.0)
                .ToArray();

            double[] smoothY = smoothX.Select(x =>
                1.0 / (1.0 + Math.Exp(-(intercept + coefficients * x)))
            ).ToArray();

            var curve = myPlot.Add.Scatter(smoothX, smoothY);
            curve.LineWidth = 2;
            curve.MarkerSize = 0;
            curve.Color = ScottPlot.Color.FromHex("#1f77b4");
            curve.LegendText = $"Sigmoid Curve ({featureName})";

            // Overlay actual binary labels
            var actualPoints = myPlot.Add.Scatter(sortedX, sortedActual);
            actualPoints.LineWidth = 0;
            actualPoints.MarkerSize = 6;
            actualPoints.MarkerShape = MarkerShape.FilledCircle;
            actualPoints.Color = ScottPlot.Color.FromHex("#ff7f0e");
            actualPoints.LegendText = "Actual Labels";

            // Threshold line at 0.5
            var threshold = myPlot.Add.HorizontalLine(0.5);
            threshold.LineWidth = 2;
            threshold.LinePattern = LinePattern.Dashed;
            threshold.Color = ScottPlot.Color.FromHex("#2ca02c");

            // Final plot styling
            myPlot.Title("Logistic Regression Sigmoid");
            myPlot.XLabel(featureName);
            myPlot.YLabel("Predicted Probability");
            myPlot.ShowLegend();
            myPlot.Axes.SetLimitsY(0, 1);

            return myPlot.GetImageBytes(600, 400, format: ImageFormat.Png);
        }



        public byte[] GenerateConfusionMatrixGraph(double[] actualValues, double[] predictedValues)
        {
            var plt = new Plot();

            // Build confusion matrix
            int numClasses = (int)Math.Max(actualValues.Max(), predictedValues.Max()) + 1;
            double[,] confusionMatrix = new double[numClasses, numClasses];

            for (int i = 0; i < actualValues.Length; i++)
            {
                int actual = (int)Math.Round(actualValues[i]);
                int predicted = (int)Math.Round(predictedValues[i]);
                if (actual >= 0 && actual < numClasses && predicted >= 0 && predicted < numClasses)
                {
                    confusionMatrix[actual, predicted]++;
                }
            }

            // Create heatmap
            var heatmap = plt.Add.Heatmap(confusionMatrix);
            heatmap.Colormap = new ScottPlot.Colormaps.Viridis();

            plt.Title($"Classification Confusion Matrix ({numClasses} classes)");
            plt.XLabel("Predicted Class");
            plt.YLabel("Actual Class");

            // Add accuracy text
            int correct = 0;
            for (int i = 0; i < numClasses; i++)
                correct += (int)confusionMatrix[i, i];
            double accuracy = (double)correct / actualValues.Length * 100;

            var text = plt.Add.Text($"Accuracy: {accuracy:F2}%", 0, numClasses + 0.5);
            text.LabelFontSize = 14;
            text.LabelBold = true;

            return plt.GetImageBytes(600, 400, format: ImageFormat.Png);
        }
    }
}
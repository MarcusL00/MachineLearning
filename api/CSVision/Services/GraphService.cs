using ScottPlot;
using CSVision.Interfaces;
using CSVision.Models;

namespace CSVision.Services
{
    public class GraphService : IGraphService
    {
        public byte[] GeneratePredictionGraph(ModelResult result)
        {
            if (result.Actuals.Length == 0 || result.Predictions.Length == 0)
            {
                return CreateEmptyGraph();
            }

            // Detect if this is classification (discrete integer values) or regression (continuous)
            bool isClassification = IsClassificationData(result.Actuals) && IsClassificationData(result.Predictions);

            if (isClassification)
            {
                return GenerateConfusionMatrixGraph(result.Actuals, result.Predictions);
            }
            else
            {
                return GenerateRegressionGraph(result.Actuals, result.Predictions);
            }
        }

        private bool IsClassificationData(double[] values)
        {
            // Check if all values are integers (or very close to integers)
            return values.All(v => Math.Abs(v - Math.Round(v)) < 0.0001);
        }

        private byte[] GenerateRegressionGraph(double[] actualValues, double[] predictedValues)
        {
            Plot myPlot = new();
            var sp = myPlot.Add.Scatter(actualValues, predictedValues);
            sp.LineWidth = 0;
            sp.MarkerSize = 10;

            // Perform regression
            var reg = new ScottPlot.Statistics.LinearRegression(actualValues, predictedValues);

            // Plot regression line
            var pt1 = new Coordinates(actualValues.First(), reg.GetValue(actualValues.First()));
            var pt2 = new Coordinates(actualValues.Last(), reg.GetValue(actualValues.Last()));
            var line = myPlot.Add.Line(pt1, pt2);
            line.LineWidth = 2;
            line.LinePattern = LinePattern.Dashed;

            // Show formula with R²
            myPlot.Title(reg.FormulaWithRSquared);
            return myPlot.GetImageBytes(600, 400, format: ImageFormat.Png);
        }

        private byte[] GenerateConfusionMatrixGraph(double[] actualValues, double[] predictedValues)
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

        private byte[] CreateEmptyGraph()
        {
            var plt = new Plot();
            plt.Add.Text("No data available", 0.5, 0.5);
            plt.Title("No Predictions");
            return plt.GetImageBytes(600, 400, format: ImageFormat.Png);
        }
    }
}
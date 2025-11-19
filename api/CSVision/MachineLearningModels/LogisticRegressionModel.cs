using CSVision.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using ScottPlot;

namespace CSVision.MachineLearningModels
{
    public sealed class LogisticRegressionModel : AbstractMachineLearningModel
    {
        internal override string ModelName => "Logistic Regression Model";

        internal LogisticRegressionModel(string[] features, string target, int seed)
            : base(features, target, seed) { }

        protected override IEstimator<ITransformer> BuildTrainer(MLContext mlContext)
        {
            return mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                labelColumnName: "Label",
                featureColumnName: "Features"
            );
        }

        protected override (Dictionary<string, double>, ConfusionMatrix? confusionMatrix) EvaluateModel(
            MLContext mlContext,
            IDataView predictions
        )
        {
            var metrics = mlContext.BinaryClassification.Evaluate(predictions, "Label");

            var results = new Dictionary<string, double>
            {
                { "Accuracy", metrics.Accuracy },
                { "F1Score", metrics.F1Score },
                { "PositivePrecision", metrics.PositivePrecision },
                { "PositiveRecall", metrics.PositiveRecall },
                { "NegativePrecision", metrics.NegativePrecision },
                { "NegativeRecall", metrics.NegativeRecall },
                { "LogLoss", metrics.LogLoss },
                { "LogLossReduction", metrics.LogLossReduction },
            };

            // Only add AUC if it’s valid, to avoid an ArgumentOutOfRangeException
            if (!double.IsNaN(metrics.AreaUnderRocCurve) && metrics.AreaUnderRocCurve > 0)
            {
                results.Add("AUC", metrics.AreaUnderRocCurve);
            }

            if (
                !double.IsNaN(metrics.AreaUnderPrecisionRecallCurve)
                && metrics.AreaUnderPrecisionRecallCurve > 0
            )
            {
                results.Add("PRC", metrics.AreaUnderPrecisionRecallCurve);
            }

            return (results, metrics.ConfusionMatrix);
        }
        protected override IEstimator<ITransformer> BuildLabelConversion(MLContext mlContext)
        {
            return mlContext.Transforms.Conversion.ConvertType("Label", Target, DataKind.Boolean);
        }
        public override ModelResult TrainModel(IFormFile file) => TrainWithTemplate(file);

        public override byte[] GeneratePredictionGraph(double[] actualValues, double[] predictedValues)
        {
            Plot myPlot = new();

            // Use indices as X values (Logistic model plotting expects a continuous feature,
            // but we may not have one available here), then sort by X for a smooth curve.
            var xVals = Enumerable.Range(0, predictedValues.Length).Select(i => (double)i).ToArray();

            var sorted = xVals.Select((x, i) => new { X = x, Y = predictedValues[i], Actual = actualValues.ElementAtOrDefault(i) })
                .OrderBy(p => p.X)
                .ToArray();

            double[] sortedX = sorted.Select(p => p.X).ToArray();
            double[] sortedY = sorted.Select(p => p.Y).ToArray();
            double[] sortedActual = sorted.Select(p => p.Actual).ToArray();

            var curve = myPlot.Add.Scatter(sortedX, sortedY);
            curve.LineWidth = 2;
            curve.MarkerSize = 0;
            curve.Color = ScottPlot.Color.FromHex("#1f77b4");
            curve.LegendText = "Sigmoid Curve";

            var actualPoints = myPlot.Add.Scatter(sortedX, sortedActual);
            actualPoints.LineWidth = 0;
            actualPoints.MarkerSize = 6;
            actualPoints.MarkerShape = MarkerShape.FilledCircle;
            actualPoints.Color = ScottPlot.Color.FromHex("#ff7f0e");
            actualPoints.LegendText = "Actual Labels";

            var threshold = myPlot.Add.HorizontalLine(0.5);
            threshold.LineWidth = 2;
            threshold.LinePattern = LinePattern.Dashed;
            threshold.Color = ScottPlot.Color.FromHex("#2ca02c");

            myPlot.Title("Logistic Regression Sigmoid");
            myPlot.XLabel("Index");
            myPlot.YLabel("Predicted Probability");
            myPlot.ShowLegend();
            myPlot.Axes.SetLimitsY(0, 1);

            return myPlot.GetImageBytes(600, 400, format: ImageFormat.Png);
        }
    }
}

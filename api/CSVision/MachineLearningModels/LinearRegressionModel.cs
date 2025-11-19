using CSVision.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using ScottPlot;

namespace CSVision.MachineLearningModels
{
    public sealed class LinearRegressionModel : AbstractMachineLearningModel
    {
        internal override string ModelName => "Linear Regression Model";

        internal LinearRegressionModel(string[] features, string target, int seed)
            : base(features, target, seed) { }

        protected override IEstimator<ITransformer> BuildTrainer(MLContext mlContext)
        {
            return mlContext.Regression.Trainers.Sdca(
                labelColumnName: "Label",
                featureColumnName: "Features"
            );
        }

        protected override (Dictionary<string, double>, ConfusionMatrix? confusionMatrix) EvaluateModel(
            MLContext mlContext,
            IDataView predictions
        )
        {
            var metrics = mlContext.Regression.Evaluate(
                predictions,
                labelColumnName: "Label",
                scoreColumnName: "Score"
            );

            return (new Dictionary<string, double>
            {
                { "RSquared", metrics.RSquared },
                { "MeanAbsoluteError", metrics.MeanAbsoluteError },
                { "MeanSquaredError", metrics.MeanSquaredError },
                { "RootMeanSquaredError", metrics.RootMeanSquaredError },
            }, null);
        }

        protected override IEstimator<ITransformer> BuildLabelConversion(MLContext mlContext)
        {
            return mlContext.Transforms.Conversion.ConvertType("Label", Target, DataKind.Single);
        }

        public override ModelResult TrainModel(IFormFile file) => TrainWithTemplate(file);

        public override byte[] GeneratePredictionGraph(double[] actualValues, double[] predictedValues)
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
    }
}

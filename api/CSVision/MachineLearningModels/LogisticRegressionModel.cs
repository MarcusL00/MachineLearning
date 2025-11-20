using CSVision.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

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

        protected override (
            Dictionary<string, double>,
            ConfusionMatrix? confusionMatrix
        ) EvaluateModel(MLContext mlContext, IDataView predictions)
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
    }
}

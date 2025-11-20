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

        /// <summary>
        /// Builds the trainer for logistic regression using the SDCA algorithm.
        /// </summary>
        /// <param name="mlContext"></param>
        /// <returns></returns>
        protected override IEstimator<ITransformer> BuildTrainer(MLContext mlContext)
        {
            return mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                labelColumnName: "Label",
                featureColumnName: "Features"
            );
        }

        /// <summary>
        /// Evaluates the model and returns binary classification metrics.
        /// </summary>
        /// <param name="mlContext"></param>
        /// <param name="predictions"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Builds the label conversion for binary classification (to boolean).
        /// </summary>
        /// <param name="mlContext"></param>
        /// <returns></returns>
        protected override IEstimator<ITransformer> BuildLabelConversion(MLContext mlContext)
        {
            return mlContext.Transforms.Conversion.ConvertType("Label", Target, DataKind.Boolean);
        }

        /// <summary>
        /// Trains the logistic regression model using the provided CSV file.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public override ModelResult TrainModel(IFormFile file) => TrainWithTemplate(file);
    }
}

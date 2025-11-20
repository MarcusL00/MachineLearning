using CSVision.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace CSVision.MachineLearningModels
{
    public sealed class LinearRegressionModel : AbstractMachineLearningModel
    {
        internal override string ModelName => "Linear Regression Model";

        internal LinearRegressionModel(string[] features, string target, int seed)
            : base(features, target, seed) { }

        /// <summary>
        /// Builds the trainer for linear regression using SDCA algorithm.
        /// </summary>
        /// <param name="mlContext"></param>
        /// <returns></returns>
        protected override IEstimator<ITransformer> BuildTrainer(MLContext mlContext)
        {
            return mlContext.Regression.Trainers.Sdca(
                labelColumnName: "Label",
                featureColumnName: "Features"
            );
        }
        /// <summary>
        /// Evaluates the model and returns regression metrics.
        /// </summary>
        /// <param name="mlContext"></param>
        /// <param name="predictions"></param>
        /// <returns></returns>
        protected override (
            Dictionary<string, double>,
            ConfusionMatrix? confusionMatrix
        ) EvaluateModel(MLContext mlContext, IDataView predictions)
        {
            var metrics = mlContext.Regression.Evaluate(
                predictions,
                labelColumnName: "Label",
                scoreColumnName: "Score"
            );

            return (
                new Dictionary<string, double>
                {
                    { "RSquared", metrics.RSquared },
                    { "MeanAbsoluteError", metrics.MeanAbsoluteError },
                    { "MeanSquaredError", metrics.MeanSquaredError },
                    { "RootMeanSquaredError", metrics.RootMeanSquaredError },
                },
                null
            );
        }

        /// <summary>
        /// Builds the label conversion for regression (to float).
        /// </summary>
        /// <param name="mlContext"></param>
        /// <returns></returns>
        protected override IEstimator<ITransformer> BuildLabelConversion(MLContext mlContext)
        {
            return mlContext.Transforms.Conversion.ConvertType("Label", Target, DataKind.Single);
        }

        /// <summary>
        /// Trains the linear regression model using the provided CSV file.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public override ModelResult TrainModel(IFormFile file) => TrainWithTemplate(file);
    }
}

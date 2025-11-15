using CSVision.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace CSVision.MachineLearningModels
{
    public sealed class DecisionTreeModel : AbstractMachineLearningModel
    {
        internal override string ModelName => "Decision Tree Model";

        internal DecisionTreeModel(string[] features, string target, int seed)
            : base(features, target, seed) { }

        protected override IEstimator<ITransformer> BuildTrainer(MLContext mlContext)
        {
            return mlContext.MulticlassClassification.Trainers.OneVersusAll(
                mlContext.BinaryClassification.Trainers.FastTree(),
                labelColumnName: "Label"
            );
        }

        protected override Dictionary<string, double> EvaluateModel(
            MLContext mlContext,
            IDataView predictions
        )
        {
            var metrics = mlContext.MulticlassClassification.Evaluate(predictions, "Label");
            return new Dictionary<string, double>
            {
                { "MicroAccuracy", metrics.MicroAccuracy },
                { "MacroAccuracy", metrics.MacroAccuracy },
                { "LogLoss", metrics.LogLoss },
            };
        }

        protected override IEstimator<ITransformer> BuildLabelConversion(MLContext mlContext)
        {
            // Convert labels to Key type for multiclass
            return mlContext.Transforms.Conversion.MapValueToKey("Label", Target);
        }

        public override ModelResult TrainModel(IFormFile file) => TrainWithTemplate(file);
    }
}

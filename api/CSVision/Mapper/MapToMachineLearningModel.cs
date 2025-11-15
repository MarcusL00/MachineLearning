using CSVision.MachineLearningModels;

namespace CSVision.Mapper
{
    internal static class MapToMachineLearningModel
    {
        internal static AbstractMachineLearningModel MapToModel(
            string modelType,
            string[] features,
            string target
        )
        {
            return modelType switch
            {
                "LinearRegression" => new LinearRegressionModel(features, target),
                "LogisticRegression" => new LogisticRegressionModel(features, target),
                "DecisionTree" => new DecisionTreeModel(features, target),
                _ => throw new ArgumentException("Invalid model type", nameof(modelType)),
            };
        }
    }
}

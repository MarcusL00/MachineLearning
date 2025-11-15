using CSVision.MachineLearningModels;

namespace CSVision.Mapper
{
    internal static class MapToMachineLearningModel
    {
        internal static AbstractMachineLearningModel MapToModel(
            string modelType,
            string[] features,
            string target,
            int seed
        )
        {
            return modelType switch
            {
                "LinearRegression" => new LinearRegressionModel(features, target, seed),
                "LogisticRegression" => new LogisticRegressionModel(features, target, seed),
                "DecisionTree" => new DecisionTreeModel(features, target, seed),
                _ => throw new ArgumentException("Invalid model type", nameof(modelType)),
            };
        }
    }
}

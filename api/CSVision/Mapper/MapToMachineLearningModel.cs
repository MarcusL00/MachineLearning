using CSVision.MachineLearningModels;

namespace CSVision.Mapper
{
    internal static class MapToMachineLearningModel
    {
        /// <summary>
        /// Maps the provided model type to its corresponding machine learning model instance.
        /// </summary>
        /// <param name="modelType"></param>
        /// <param name="features"></param>
        /// <param name="target"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
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
                _ => throw new ArgumentException("Invalid model type", nameof(modelType)),
            };
        }
    }
}

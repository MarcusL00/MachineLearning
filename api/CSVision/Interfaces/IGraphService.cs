using CSVision.Models;

namespace CSVision.Interfaces
{
    public interface IGraphService
    {
        byte[] GenerateGraph(ModelResult result);
        byte[] GenerateConfusionMatrixGraph(ModelResult result);
    }
}
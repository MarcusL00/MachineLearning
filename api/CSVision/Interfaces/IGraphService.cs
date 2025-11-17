using CSVision.Models;

namespace CSVision.Interfaces
{
    public interface IGraphService
    {
        byte[] GeneratePredictionGraph(ModelResult result);
    }
}
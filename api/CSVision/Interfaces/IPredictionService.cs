using CSVision.DTOs;
using CSVision.Models;

namespace CSVision.Interfaces
{
    public interface IPredictionService
    {
        Task<ModelResult> GeneratePredictionsAsync(PredictionsRequestDto requestDto);
    }
}

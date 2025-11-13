using CSVision.DTOs;

namespace CSVision.Interfaces
{
    public interface IPredictionService
    {
        Task GeneratePredictionsAsync(PredictionsRequestDto requestDto);
    }
}

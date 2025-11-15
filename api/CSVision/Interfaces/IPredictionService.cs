using CSVision.DTOs;
using CSVision.Models;

namespace CSVision.Interfaces
{
    public interface IPredictionService
    {
        ModelResult GeneratePredictionsAsync(PredictionsRequestDto requestDto);
    }
}

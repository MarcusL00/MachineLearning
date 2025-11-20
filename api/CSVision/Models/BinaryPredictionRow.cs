namespace CSVision.Models
{
    /// <summary>
    /// Represents a row of binary prediction results.
    /// </summary>
    public class BinaryPredictionRow
    {
        public bool Label { get; set; }
        public float Score { get; set; }
        public bool PredictedLabel { get; set; }
    }
}

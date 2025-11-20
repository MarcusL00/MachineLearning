namespace CSVision.Models
{
    /// <summary>
    /// Represents a row of numeric prediction results.
    /// </summary>
    public class NumericPredictionRow
    {
        public float Label { get; set; }
        public float Score { get; set; }
    }
}

namespace GPSDataRenderer.Models
{
    public class GpsDataStream
    {
        public Dictionary<string, int> ColumnMap { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public IAsyncEnumerable<List<double[]>> Batches { get; set; } = default!;

    }
}

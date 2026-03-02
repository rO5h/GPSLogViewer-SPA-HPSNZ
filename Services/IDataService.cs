using GPSDataRenderer.Models;

namespace GPSDataRenderer.Services
{
    public interface IDataService
    { 
        Task<GpsDataStream> GetGpsStreamAsync(string filePath, int batchSize = 100,CancellationToken cancellationToken = default);
    }
}

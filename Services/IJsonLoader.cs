namespace GPSDataRenderer.Services
{
    public interface IJsonLoader
    {
        Task<Stream> GetStreamAsync(string path,CancellationToken cancellationToken =default);
    }
}

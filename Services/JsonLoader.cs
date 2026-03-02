namespace GPSDataRenderer.Services
{
    public class JsonLoader : IJsonLoader
    {
        private readonly HttpClient _http;

        public JsonLoader(HttpClient http)
        {
            _http = http;
        }

        public async Task<Stream> GetStreamAsync(string path,CancellationToken cancellationToken = default)
        {          
            return await _http.GetStreamAsync(path,cancellationToken);
        }


        //decided not to JsonSerializer.deserialize because of big file size and large number of entries, preventing crash
        /*  public async Task<T> LoadJsonStreamAsync<T>(string path)
        {
            try
            {
                using var stream = await _http.GetStreamAsync(path);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(stream, options);
                Console.WriteLine("succes");
                if (result == null)
                {
                    throw new InvalidOperationException("Failed to deserialize JSON: Null Object reference");
                }

                return result;

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to deserialize JSON:{ex.Message}");
            }
        }
        */
    }
}


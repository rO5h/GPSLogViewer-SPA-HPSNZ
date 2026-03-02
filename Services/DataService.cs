using System.Text.Json;
using GPSDataRenderer.Models;
using System.Text;
using System.Runtime.CompilerServices;


namespace GPSDataRenderer.Services
{

    public class DataService : IDataService
    {
        private readonly IJsonLoader _jsonLoader;

        private const int DefaultBufferSize = 64 * 1024;

        public DataService(IJsonLoader jsonLoader)
        {
            _jsonLoader = jsonLoader;
        }

        public async Task<GpsDataStream> GetGpsStreamAsync(string filePath, int batchSize = 100, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (batchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be greater than zero.");

            var rawStream = await _jsonLoader.GetStreamAsync(filePath, cancellationToken);

            // BufferedStream significantly improves sequential read performance in WASM.
            var stream = new BufferedStream(rawStream, DefaultBufferSize);

            var columnMap = await ParseMetadataAsync(stream, cancellationToken);

            var allRows = new List<List<double[]>>();

            return new GpsDataStream
            {
                ColumnMap = columnMap,
                Batches = StreamRowsAsync(stream, batchSize, cancellationToken)
            };
        }

        //Find column metadata
        //Used Utf8JsonReader for columns since the array is small.
        private async Task<Dictionary<string, int>> ParseMetadataAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            if (!stream.CanSeek)
                throw new InvalidOperationException("Stream must be seekable.");

            var mapping = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var readerOptions = new JsonReaderOptions
            {
                AllowTrailingCommas = true
            };

            byte[] buffer = new byte[DefaultBufferSize];
            var jsonState = new JsonReaderState(readerOptions);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                if (bytesRead == 0)
                    break;

                var reader = new Utf8JsonReader(buffer.AsSpan(0, bytesRead), isFinalBlock: false, jsonState);

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals("columns"))
                    {
                        reader.Read(); // StartArray

                        int index = 0;

                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        {
                            if (reader.TokenType == JsonTokenType.String)
                            {
                                var columnName = reader.GetString();
                                if (!string.IsNullOrEmpty(columnName))
                                {
                                    mapping[columnName] = index++;
                                }

                            }
                        }

                        stream.Position = 0;
                        return mapping;
                    }
                }

                jsonState = reader.CurrentState;
            }

            throw new InvalidOperationException("Could not locate 'columns' metadata.");
        }


        //Custom async streaming parser to stream "values"
        private async IAsyncEnumerable<List<double[]>> StreamRowsAsync(Stream stream, int batchSize, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (!MoveToValuesArray(stream))
            {
                yield break;
            }               

            using var reader = new StreamReader(stream, leaveOpen: true);

            var currentBatch = new List<double[]>(batchSize);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            while (!reader.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int next = reader.Read();
                if (next == -1) break;

                char c = (char)next;

                if (c == ']')
                    break;

                if (c == '[')
                {
                    var rowJson = ReadUntilClosingBracket(reader);

                    try
                    {
                        var row = JsonSerializer.Deserialize<double[]>(rowJson, options);
                        if (row != null)
                        {
                            currentBatch.Add(row);
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error Deserializing double array:{ex.Message}");
                    }
                }


                if (currentBatch.Count >= batchSize)
                {
                    yield return currentBatch;
                    currentBatch = new List<double[]>(batchSize);

                    await Task.Delay(10);
                }
            }

            if (currentBatch.Count > 0)
                yield return currentBatch;
        }


        #region[Helper]
        private string ReadUntilClosingBracket(StreamReader reader)
        {
            var sb = new StringBuilder("[");
            int openBrackets = 1;

            while (openBrackets > 0)
            {
                int c = reader.Read();
                if (c == -1) break;

                char ch = (char)c;
                sb.Append(ch);

                if (ch == '[') openBrackets++;
                else if (ch == ']') openBrackets--;
            }

            return sb.ToString();
        }

        private bool MoveToValuesArray(Stream stream)
        {
            ReadOnlySpan<byte> target = "\"values\":"u8;
            int matchIndex = 0;

            while (true)
            {
                int b = stream.ReadByte();
                if (b == -1) return false;

                if ((byte)b == target[matchIndex])
                {
                    matchIndex++;
                    if (matchIndex == target.Length) break;
                }
                else
                {
                    matchIndex = 0;
                }
            }

            // Finding opening '[' of the values array
            while (true)
            {
                int b = stream.ReadByte();
                if (b == -1) return false;
                if ((char)b == '[') return true;
            }
        }

        #endregion
    }
}



    




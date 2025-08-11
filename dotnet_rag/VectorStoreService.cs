using Qdrant.Client;
using Qdrant.Client.Grpc;

public class VectorStoreService
{
    private readonly QdrantClient _client;
    private readonly string _collectionName;

    public VectorStoreService(string endpoint, string apiKey, string collectionName)
    {
        _client = new QdrantClient(new Uri(endpoint));
        _collectionName = collectionName;
    }

    public async Task InitializeAsync(int vectorSize = 768)
    {
        try
        {
            await _client.GetCollectionInfoAsync(_collectionName);
        }
        catch
        {
            await _client.CreateCollectionAsync(_collectionName, new VectorParams
            {
                Size = (ulong)vectorSize,
                Distance = Distance.Cosine
            });
        }
    }

    public async Task UpsertAsync(string id, ReadOnlyMemory<float> embedding, Dictionary<string, object> metadata)
    {
        var point = new PointStruct
        {
            Id = new PointId { Uuid = id },
            Vectors = embedding.ToArray(),
            Payload = { }
        };

        foreach (var kvp in metadata)
        {
            point.Payload[kvp.Key] = kvp.Value switch
            {
                string s => s,
                int i => i,
                bool b => b,
                _ => kvp.Value.ToString() ?? string.Empty
            };
        }

        await _client.UpsertAsync(_collectionName, new[] { point });
    }

    public async Task<List<ScoredPoint>> SearchAsync(ReadOnlyMemory<float> queryEmbedding, int limit = 3)
    {
        var searchResult = await _client.SearchAsync(_collectionName, queryEmbedding.ToArray(), limit: (ulong)limit);
        return searchResult.ToList();
    }
}
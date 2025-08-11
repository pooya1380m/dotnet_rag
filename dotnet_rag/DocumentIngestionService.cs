using Microsoft.Extensions.AI;

public class DocumentIngestionService
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingService;
    private readonly VectorStoreService _vectorStoreService;

    public DocumentIngestionService(IEmbeddingGenerator<string, Embedding<float>> embeddingService, VectorStoreService vectorStoreService)
    {
        _embeddingService = embeddingService;
        _vectorStoreService = vectorStoreService;
    }

    public async Task IngestDocumentAsync(string documentPath, string documentId)
    {
        var content = await File.ReadAllTextAsync(documentPath);
        var chunks = ChunkText(content, 300, 60);

        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var embeddingResult = await _embeddingService.GenerateAsync(chunk);
            var embedding = embeddingResult.Vector;
            
            await _vectorStoreService.UpsertAsync(
                id: Guid.NewGuid().ToString(),
                embedding: embedding,
                metadata: new Dictionary<string, object>
                {
                    ["document_id"] = documentId,
                    ["chunk_index"] = i,
                    ["text"] = chunk,
                    ["document_path"] = documentPath
                }
            );
        }
    }

    private List<string> ChunkText(string text, int chunkSize, int overlap)
    {
        var chunks = new List<string>();
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 0; i < words.Length; i += chunkSize - overlap)
        {
            var chunkWords = words.Skip(i).Take(chunkSize).ToArray();
            var chunk = string.Join(" ", chunkWords);
            chunks.Add(chunk);
            
            if (i + chunkSize >= words.Length)
                break;
        }
        
        return chunks;
    }
}
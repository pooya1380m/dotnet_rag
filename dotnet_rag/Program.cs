using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;

class Program
{
    static async Task Main(string[] args)
    {
        // Create kernel builder
        var builder = Kernel.CreateBuilder();

        // Define embedding model paths (replace with actual paths)
        var embeddModelPath = "./model.onnx";
        var embedVocab = "./vocab.txt";

        // Configure AI services
        builder.AddBertOnnxEmbeddingGenerator(embeddModelPath, embedVocab);
        builder.AddOpenAIChatCompletion("qwen2.5-coder-0.5b-instruct-generic-gpu", new Uri("http://localhost:55797/v1"),
            apiKey: "", serviceId: "qwen2.5-0.5b");

        // Build kernel
        var kernel = builder.Build();

        // Initialize services
        var chatService = kernel.GetRequiredService<IChatCompletionService>(serviceKey: "qwen2.5-0.5b");
        var embeddingService = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

        var vectorStoreService = new VectorStoreService(
            "http://localhost:6334",
            "",
            "demodocs");

        await vectorStoreService.InitializeAsync();

        var documentIngestionService = new DocumentIngestionService(embeddingService, vectorStoreService);
        var ragQueryService = new RagQueryService(chatService);

        // Ingest document
        var filePath = "./foundry-local-architecture.md";
        var fileID = "3";
        await documentIngestionService.IngestDocumentAsync(filePath, fileID);

        while (true)
        {
            // Execute RAG query
            var question = Console.ReadLine();
            var answer = await ragQueryService.QueryAsync(question);

            // Display result
            Console.WriteLine($"Question: {question}");
            Console.WriteLine($"Answer: {answer}");
        }
    }
}
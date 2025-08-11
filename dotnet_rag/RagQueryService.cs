using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;

public class RagQueryService
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingService;
    private readonly IChatCompletionService _chatService;
    private readonly VectorStoreService _vectorStoreService;

    public RagQueryService(
        IEmbeddingGenerator<string, Embedding<float>> embeddingService,
        IChatCompletionService chatService,
        VectorStoreService vectorStoreService)
    {
        _embeddingService = embeddingService;
        _chatService = chatService;
        _vectorStoreService = vectorStoreService;
    }

    public async Task<string> QueryAsync(string question)
    {
        // return question; // For now, just return the question as a placeholder
           var queryEmbeddingResult = await _embeddingService.GenerateAsync(question);
//         Console.WriteLine(question);
            var queryEmbedding = queryEmbeddingResult.Vector;
            var searchResults = await _vectorStoreService.SearchAsync(queryEmbedding, limit: 5);

            string str_context = "";
            foreach (var result in searchResults)
            {
                if (result.Payload.TryGetValue("text", out var text))
                {
                    str_context += text.ToString();
                }
            }
            var prompt = $@"According to the question {question},, optimize and simplify the content. {str_context}";


            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage("You are a helpful assistant that answers questions based on the provided context.");
            chatHistory.AddUserMessage(prompt);

            var fullMessage = string.Empty;

            await foreach (var chatUpdate in _chatService.GetStreamingChatMessageContentsAsync(chatHistory, cancellationToken: default))
            {                     
                if (chatUpdate.Content is { Length: > 0 })
                {
                    fullMessage += chatUpdate.Content;
                }
            }
            return fullMessage ?? "I couldn't generate a response.";
    }
}
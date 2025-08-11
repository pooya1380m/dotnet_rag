using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;

public class RagQueryService
{
    private readonly IChatCompletionService _chatService;

    public RagQueryService(
        IChatCompletionService chatService
    )
    {
        _chatService = chatService;
    }

    public async Task<string> QueryAsync(string question)
    {

        var prompt = $@"According to the question {question}";


        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(
            "You are a helpful assistant that answers questions based on the provided context.");
        chatHistory.AddUserMessage(prompt);

        var fullMessage = string.Empty;

        await foreach (var chatUpdate in _chatService.GetStreamingChatMessageContentsAsync(chatHistory,
                           cancellationToken: default))
        {
            if (chatUpdate.Content is { Length: > 0 })
            {
                fullMessage += chatUpdate.Content;
            }
        }

        return fullMessage ?? "I couldn't generate a response.";
    }
}
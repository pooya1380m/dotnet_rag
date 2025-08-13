# Foundry Local RAG Implementation

This project demonstrates a Retrieval-Augmented Generation (RAG) system built with Foundry Local, Semantic Kernel, ONNX embeddings, and Qdrant vector database. The system ingests documents, generates embeddings, stores them in a vector database, and supports querying with context-aware responses using a local AI model.

## Project Structure

- **FoundryLocalRAG.csproj**: The .NET project file defining dependencies and build configuration.
- **Program.cs**: The main entry point that sets up the Semantic Kernel, services, and demonstrates document ingestion and querying.
- **VectorStoreService.cs**: A service for interacting with the Qdrant vector database, handling collection initialization, vector upsertion, and similarity search.
- **RagQueryService.cs**: Implements the RAG functionality, combining embedding generation, vector search, and chat completion for query responses.
- **DocumentIngestionService.cs**: Handles document ingestion by reading files, chunking text, generating embeddings, and storing them in Qdrant.
- **foundry-local-architecture.md**: A sample markdown document for ingestion, containing information about Foundry Local.

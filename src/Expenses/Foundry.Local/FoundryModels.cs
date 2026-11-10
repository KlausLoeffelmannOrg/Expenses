namespace Foundry.Local;

public sealed record FoundryInstallation(bool IsInstalled, string? Version, bool IsSupported);

public sealed record FoundryModel(
    string Id, string Alias, string Type, string Device, double FileSizeMb,
    string License, bool Cached, bool Loaded)
{
    public bool SupportsImages => Type.Equals("Multimodal", StringComparison.OrdinalIgnoreCase);
    public bool SupportsChat => SupportsImages || Type.Equals("Chat", StringComparison.OrdinalIgnoreCase);
    public string Purpose => Type.ToLowerInvariant() switch
    {
        "multimodal" => "Image descriptions, receipt OCR, translation and chat",
        "chat" => "Translation and text (no image input)",
        "speech" => "Speech transcription (not used by this library)",
        "embedding" => "Text embeddings (not used by this library)",
        _ => Type
    };
}

public interface IFoundryRuntime
{
    Task<FoundryInstallation> DetectAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FoundryModel>> GetModelsAsync(CancellationToken cancellationToken = default);
    Task StartServerAsync(CancellationToken cancellationToken = default);
    Task<Uri> GetEndpointAsync(CancellationToken cancellationToken = default);
    Task DownloadAsync(string modelId, CancellationToken cancellationToken = default);
    Task LoadAsync(string modelId, CancellationToken cancellationToken = default);
    Task UnloadAsync(string modelId, CancellationToken cancellationToken = default);
    Task RemoveFromCacheAsync(string modelId, CancellationToken cancellationToken = default);
}

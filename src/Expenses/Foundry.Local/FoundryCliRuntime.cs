using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Foundry.Local;

public sealed class FoundryCliRuntime(string executable = "foundry") : IFoundryRuntime
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<FoundryInstallation> DetectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            string value = (await RunAsync(["--version"], TimeSpan.FromSeconds(15), cancellationToken).ConfigureAwait(false)).Trim();
            bool supported = Version.TryParse(value.Split('-')[0], out var version) && version >= new Version(0, 13);
            return new(true, value, supported);
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode is 2 or 3)
        {
            return new(false, null, false);
        }
    }

    public async Task<IReadOnlyList<FoundryModel>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        var catalog = await ReadAsync<ModelList>(["model", "list", "--variants"], cancellationToken).ConfigureAwait(false);
        var cache = await ReadAsync<ModelList>(["cache", "list", "--variants"], cancellationToken).ConfigureAwait(false);
        var cached = cache.Models.ToDictionary(m => m.Id, StringComparer.OrdinalIgnoreCase);
        return catalog.Models.Concat(cache.Models).DistinctBy(m => m.Id, StringComparer.OrdinalIgnoreCase)
            .Select(m => new FoundryModel(m.Id, m.Alias, m.Type, m.Device, m.FileSizeMb, m.License,
                m.Cached || cached.ContainsKey(m.Id), cached.GetValueOrDefault(m.Id)?.Loaded ?? false))
            .OrderByDescending(m => m.Loaded).ThenByDescending(m => m.Cached).ThenBy(m => m.Alias).ThenBy(m => m.Device).ToList();
    }

    public Task StartServerAsync(CancellationToken cancellationToken = default) =>
        RunCommandAsync(["server", "start"], TimeSpan.FromMinutes(2), cancellationToken);

    public async Task<Uri> GetEndpointAsync(CancellationToken cancellationToken = default)
    {
        var status = await ReadAsync<ServerStatus>(["server", "status"], cancellationToken).ConfigureAwait(false);
        if (!status.Running) throw new InvalidOperationException("Foundry Local is stopped. Start its server in Model management.");
        var endpoint = status.WebUrls.Select(u => Uri.TryCreate(u, UriKind.Absolute, out var uri) ? uri : null)
            .FirstOrDefault(u => u is not null && u.IsLoopback && u.Scheme is "http" or "https");
        return endpoint ?? throw new InvalidDataException("Foundry Local did not report a loopback HTTP endpoint.");
    }

    public Task DownloadAsync(string modelId, CancellationToken cancellationToken = default) =>
        ModelCommandAsync(["model", "download"], modelId, TimeSpan.FromHours(2), cancellationToken);
    public Task LoadAsync(string modelId, CancellationToken cancellationToken = default) =>
        ModelCommandAsync(["model", "load"], modelId, TimeSpan.FromMinutes(10), cancellationToken);
    public Task UnloadAsync(string modelId, CancellationToken cancellationToken = default) =>
        ModelCommandAsync(["model", "unload"], modelId, TimeSpan.FromMinutes(2), cancellationToken);
    public Task RemoveFromCacheAsync(string modelId, CancellationToken cancellationToken = default) =>
        ModelCommandAsync(["cache", "remove", "--force"], modelId, TimeSpan.FromMinutes(2), cancellationToken);

    private Task ModelCommandAsync(string[] command, string modelId, TimeSpan timeout, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);
        if (modelId.StartsWith('-')) throw new ArgumentException("Expected a model identifier, not a command option.", nameof(modelId));
        return RunCommandAsync([.. command, modelId], timeout, cancellationToken);
    }

    private async Task RunCommandAsync(string[] arguments, TimeSpan timeout, CancellationToken cancellationToken) =>
        _ = await RunAsync([.. arguments, "--output", "json"], timeout, cancellationToken).ConfigureAwait(false);

    private async Task<T> ReadAsync<T>(string[] arguments, CancellationToken cancellationToken) =>
        JsonSerializer.Deserialize<T>(await RunAsync([.. arguments, "--output", "json"], TimeSpan.FromMinutes(2),
            cancellationToken).ConfigureAwait(false), JsonOptions)
        ?? throw new InvalidDataException("Foundry Local returned an empty JSON response.");

    private async Task<string> RunAsync(string[] arguments, TimeSpan timeout, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var limit = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        limit.CancelAfter(timeout);
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(executable)
            {
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true
            }
        };
        foreach (string argument in arguments) process.StartInfo.ArgumentList.Add(argument);
        process.Start();
        Task<string> output = process.StandardOutput.ReadToEndAsync();
        Task<string> errors = process.StandardError.ReadToEndAsync();
        try
        {
            await process.WaitForExitAsync(limit.Token).ConfigureAwait(false);
            string stdout = await output.ConfigureAwait(false);
            string stderr = await errors.ConfigureAwait(false);
            if (process.ExitCode != 0)
                throw new InvalidOperationException($"Foundry Local command failed ({process.ExitCode}): {stderr.Trim()} {stdout.Trim()}".Trim());
            return stdout;
        }
        catch (OperationCanceledException) when (limit.IsCancellationRequested)
        {
            // Only our CLI process is owned here, never the shared Foundry daemon.
            if (!process.HasExited) process.Kill();
            await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
            await Task.WhenAll(output, errors).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            throw new TimeoutException($"Foundry Local did not finish within {timeout}.");
        }
    }

    private sealed class ModelList
    {
        [JsonPropertyName("variants")]
        public required List<CliModel> Models { get; init; }
    }

    private sealed class CliModel
    {
        [JsonPropertyName("variantId")]
        public required string Id { get; init; }
        public required string Alias { get; init; }
        public required string Type { get; init; }
        public required string Device { get; init; }
        public double FileSizeMb { get; init; }
        public string License { get; init; } = "";
        public bool Cached { get; init; }
        public bool Loaded { get; init; }
    }

    private sealed class ServerStatus
    {
        public required bool Running { get; init; }
        public string[] WebUrls { get; init; } = [];
    }
}

namespace Foundry.Local;

public sealed class ImageInput
{
    public const int MaximumBytes = 20 * 1024 * 1024;
    private readonly byte[] _bytes;
    public string MediaType { get; }

    public ImageInput(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length is 0 or > MaximumBytes)
            throw new ArgumentException("Use an image between 1 byte and 20 MiB.", nameof(bytes));
        MediaType = bytes.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) ? "image/png"
            : bytes.StartsWith(new byte[] { 255, 216, 255 }) ? "image/jpeg"
            : throw new ArgumentException("Use a PNG or JPEG image (PDF and TIFF are not image-model inputs).", nameof(bytes));
        _bytes = bytes.ToArray();
    }

    public static async Task<ImageInput> FromFileAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        if (stream.Length is 0 or > MaximumBytes) throw new ArgumentException("Use an image between 1 byte and 20 MiB.", nameof(path));
        byte[] bytes = new byte[checked((int)stream.Length)];
        await stream.ReadExactlyAsync(bytes, cancellationToken).ConfigureAwait(false);
        return new ImageInput(bytes);
    }

    public Stream OpenRead() => new MemoryStream(_bytes, writable: false);
    internal string Base64 => Convert.ToBase64String(_bytes);
}

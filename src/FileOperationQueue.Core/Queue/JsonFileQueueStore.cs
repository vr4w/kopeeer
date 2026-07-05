using System.Text.Json;

namespace FileOperationQueue.Core.Queue;

public sealed class JsonFileQueueStore(string filePath) : IQueueStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public async Task<QueueSnapshot> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            return new QueueSnapshot { Jobs = Array.Empty<FileOperationJob>() };
        }

        await using var stream = File.OpenRead(filePath);
        var snapshot = await JsonSerializer.DeserializeAsync<QueueSnapshot>(
            stream,
            SerializerOptions,
            cancellationToken);

        return snapshot ?? new QueueSnapshot { Jobs = Array.Empty<FileOperationJob>() };
    }

    public async Task SaveAsync(QueueSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, snapshot, SerializerOptions, cancellationToken);
    }
}


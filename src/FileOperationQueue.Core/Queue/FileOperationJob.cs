namespace FileOperationQueue.Core.Queue;

public sealed record FileOperationJob
{
    public required Guid Id { get; init; }

    public required FileOperationKind Kind { get; init; }

    public required IReadOnlyList<string> Sources { get; init; }

    public required string DestinationDirectory { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required FileOperationJobStatus Status { get; init; }

    public DateTimeOffset? StartedAt { get; init; }

    public DateTimeOffset? FinishedAt { get; init; }

    public string? ErrorMessage { get; init; }

    public static FileOperationJob Create(
        FileOperationKind kind,
        IEnumerable<string> sources,
        string destinationDirectory,
        TimeProvider? timeProvider = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationDirectory);

        var sourceList = sources
            .Select(source => source.Trim())
            .Where(source => source.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (sourceList.Length == 0)
        {
            throw new ArgumentException("At least one source path is required.", nameof(sources));
        }

        return new FileOperationJob
        {
            Id = Guid.NewGuid(),
            Kind = kind,
            Sources = sourceList,
            DestinationDirectory = destinationDirectory.Trim(),
            CreatedAt = (timeProvider ?? TimeProvider.System).GetUtcNow(),
            Status = FileOperationJobStatus.Queued
        };
    }
}


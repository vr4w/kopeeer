using FileOperationQueue.Core.Queue;

namespace FileOperationQueue.App.Commands;

public sealed record QueueCommandRequest(FileOperationKind Kind, IReadOnlyList<string> Sources)
{
    public static QueueCommandRequest? TryParse(IReadOnlyList<string> args)
    {
        if (args.Count < 2)
        {
            return null;
        }

        var kind = args[0] switch
        {
            "--queue-copy" => FileOperationKind.Copy,
            "--queue-move" => FileOperationKind.Move,
            _ => (FileOperationKind?)null
        };

        if (kind is null)
        {
            return null;
        }

        var sources = args
            .Skip(1)
            .Select(source => source.Trim())
            .Where(source => source.Length > 0)
            .ToArray();

        return sources.Length == 0 ? null : new QueueCommandRequest(kind.Value, sources);
    }
}


using Kopeeer.Core;

namespace Kopeeer.App;

public sealed record StartupQueueRequest(FileOperationType OperationType, string SourcePath)
{
    public static StartupQueueRequest? TryParse(IReadOnlyList<string> args)
    {
        if (args.Count < 2)
        {
            return null;
        }

        var operationType = args[0] switch
        {
            "--queue-copy" => FileOperationType.Copy,
            "--queue-move" => FileOperationType.Move,
            _ => (FileOperationType?)null
        };

        if (operationType is null)
        {
            return null;
        }

        var sourcePath = args[1].Trim();
        return sourcePath.Length == 0 ? null : new StartupQueueRequest(operationType.Value, sourcePath);
    }
}


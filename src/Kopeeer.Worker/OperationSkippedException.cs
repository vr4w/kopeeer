namespace Kopeeer.Worker;

public sealed class OperationSkippedException(string message) : Exception(message);

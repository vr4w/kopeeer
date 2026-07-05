using System.IO.Pipes;
using System.Text.Json;

namespace Kopeeer.App;

public sealed class SingleInstanceCoordinator : IDisposable
{
    public const string MutexName = "Kopeeer.App.SingleInstance";
    private const string PipeName = "Kopeeer.App.Enqueue";

    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Func<StartupQueueRequest, Task> _handleRequest;
    private readonly Task _serverTask;

    private SingleInstanceCoordinator(Func<StartupQueueRequest, Task> handleRequest)
    {
        _handleRequest = handleRequest;
        _serverTask = Task.Run(ServerLoopAsync);
    }

    public static SingleInstanceCoordinator Start(Func<StartupQueueRequest, Task> handleRequest) =>
        new(handleRequest);

    public static bool TrySendToRunningInstance(StartupQueueRequest request)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(2000);
            JsonSerializer.Serialize(client, request);
            client.Flush();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();

        try
        {
            _serverTask.Wait(500);
        }
        catch
        {
            // Shutdown should not block app exit.
        }

        _cancellationTokenSource.Dispose();
    }

    private async Task ServerLoopAsync()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            await using var server = new NamedPipeServerStream(
                PipeName,
                PipeDirection.In,
                maxNumberOfServerInstances: 1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            try
            {
                await server.WaitForConnectionAsync(_cancellationTokenSource.Token);
                var request = await JsonSerializer.DeserializeAsync<StartupQueueRequest>(
                    server,
                    cancellationToken: _cancellationTokenSource.Token);

                if (request is not null)
                {
                    await _handleRequest(request);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch
            {
                // A broken enqueue request should not take down the running queue app.
            }
        }
    }
}

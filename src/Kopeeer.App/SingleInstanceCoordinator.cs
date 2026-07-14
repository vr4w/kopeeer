using System.IO.Pipes;
using System.Text.Json;

namespace Kopeeer.App;

public sealed class SingleInstanceCoordinator : IDisposable
{
    public const string MutexName = "Kopeeer.App.SingleInstance";
    private const string PipeName = "Kopeeer.App.Enqueue";
    private const int ConnectAttempts = 20;
    private const int ConnectTimeoutMilliseconds = 250;

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
        for (var attempt = 1; attempt <= ConnectAttempts; attempt++)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                client.Connect(ConnectTimeoutMilliseconds);
                JsonSerializer.Serialize(client, request);
                client.Flush();
                AppDiagnostics.Write($"IPC send succeeded attempt={attempt}");
                return true;
            }
            catch (TimeoutException)
            {
                AppDiagnostics.Write($"IPC send timed out attempt={attempt}");
            }
            catch (IOException exception)
            {
                AppDiagnostics.Write($"IPC send failed attempt={attempt} error=\"{exception.Message}\"");
            }
            catch (UnauthorizedAccessException exception)
            {
                AppDiagnostics.Write($"IPC send denied attempt={attempt} error=\"{exception.Message}\"");
                return false;
            }

            Thread.Sleep(100);
        }

        AppDiagnostics.Write("IPC send failed after retries");
        return false;
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
                    AppDiagnostics.Write($"IPC request received operation={request.OperationType} sources={request.SourcePaths.Length} target=\"{request.TargetFolder}\" pickTarget={request.PickTarget}");
                    await _handleRequest(request);
                    AppDiagnostics.Write("IPC request accepted");
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception exception)
            {
                AppDiagnostics.Write($"IPC request failed error=\"{exception.Message}\"");
            }
        }
    }
}

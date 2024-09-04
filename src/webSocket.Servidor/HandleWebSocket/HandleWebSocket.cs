using System.Net.WebSockets;
using System.Text;

namespace webSocket.Servidor.HandleWebSocket;

internal static class HandleWebSocket
{
    internal static async Task HandleWebSocketAsync(ClientInfo clientInfo, object _lock, List<ClientInfo> _clients)
    {
        var buffer = new byte[1024 * 4];
        var segment = new ArraySegment<byte>(buffer);

        try
        {
            var result = await clientInfo.WebSocket.ReceiveAsync(segment, CancellationToken.None);
            var initialMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

            if (initialMessage.StartsWith("NAME:"))
            {
                clientInfo.Nome = initialMessage.Substring(5); 
            }

            while (clientInfo.WebSocket.State == WebSocketState.Open)
            {
                result = await clientInfo.WebSocket.ReceiveAsync(segment, CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    lock (_lock)
                    {
                        _clients.Remove(clientInfo);
                    }
                    await clientInfo.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Fechamento normal", CancellationToken.None);
                    Console.WriteLine($"Conexão WebSocket fechada pelo cliente: {clientInfo.Nome} ({clientInfo.Id})");
                }
                else
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"{clientInfo.Nome}  ({clientInfo.Id}): {message}");

                    List<Task> sendTasks = new List<Task>();
                    lock (_lock)
                    {
                        foreach (var client in _clients)
                        {
                            if (client != clientInfo && client.WebSocket.State == WebSocketState.Open)
                            {
                                var responseMessage = $"{clientInfo.Nome}: {message}";
                                var responseBuffer = Encoding.UTF8.GetBytes(responseMessage);
                                var responseSegment = new ArraySegment<byte>(responseBuffer);

                                sendTasks.Add(client.WebSocket.SendAsync(responseSegment, WebSocketMessageType.Text, true, CancellationToken.None));
                            }
                        }
                    }

                    await Task.WhenAll(sendTasks);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exceção durante a comunicação WebSocket: {ex.Message}");
        }
        finally
        {
            clientInfo.WebSocket.Dispose();
            Console.WriteLine($"WebSocket disposeado: {clientInfo.Nome} ({clientInfo.Id})");
        }
    }
}

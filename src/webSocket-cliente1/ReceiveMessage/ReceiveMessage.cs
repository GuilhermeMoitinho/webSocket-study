using System.Net.WebSockets;
using System.Text;

namespace webSocket_cliente2.ReceiveMessage;

internal static class ReceiveMessage
{
    internal static async Task ReceiveMessagesAsync(ClientWebSocket client)
    {
        var buffer = new byte[1024 * 4];
        var segment = new ArraySegment<byte>(buffer);

        while (client.State == WebSocketState.Open)
        {
            var result = await client.ReceiveAsync(segment, CancellationToken.None);
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Console.WriteLine($"Recebido: {message}");
        }
    }
}

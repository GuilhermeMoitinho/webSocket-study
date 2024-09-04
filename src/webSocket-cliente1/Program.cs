using System.Net.WebSockets;
using System.Text;
using webSocket_cliente2.ReceiveMessage;

class Program
{
    static async Task Main(string[] args)
    {
        var uri = new Uri("ws://localhost:8080/");
        using (var client = new ClientWebSocket())
        {
            await client.ConnectAsync(uri, CancellationToken.None);
            Console.WriteLine("Conectado ao servidor WebSocket.");

            // Enviar o nome do cliente
            Console.Write("Digite seu nome: ");
            var name = Console.ReadLine();
            var nameBuffer = Encoding.UTF8.GetBytes($"NAME:{name}");
            var nameSegment = new ArraySegment<byte>(nameBuffer);
            await client.SendAsync(nameSegment, WebSocketMessageType.Text, true, CancellationToken.None);

            var receiveTask = ReceiveMessage.ReceiveMessagesAsync(client);

            while (true)
            {
                Console.Write($"{name}> ");
                var sendMessage = Console.ReadLine();
                if (sendMessage == "exit")
                    break;

                var sendBuffer = Encoding.UTF8.GetBytes(sendMessage);
                var sendSegment = new ArraySegment<byte>(sendBuffer);

                await client.SendAsync(sendSegment, WebSocketMessageType.Text, true, CancellationToken.None);
            }

            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Fechamento normal", CancellationToken.None);
            Console.WriteLine("Conexão WebSocket fechada.");
        }
    }
}

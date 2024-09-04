using System.Net;
using System.Net.WebSockets;
using webSocket.Servidor.HandleWebSocket;

class Program
{
    private static readonly IList<ClientInfo> _clients = new List<ClientInfo>();
    private static readonly object _lock = new object();

    static async Task Main(string[] args)
    {
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();
        Console.WriteLine("Servidor WebSocket iniciado.");

        while (true)
        {
            var context = await listener.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                var webSocketContext = await context.AcceptWebSocketAsync(null);
                var webSocket = webSocketContext.WebSocket;

                var clientInfo = new ClientInfo
                {
                    WebSocket = webSocket,
                    Id = Guid.NewGuid(),
                    Nome = "Desconhecido" 
                };

                lock (_lock)
                {
                    _clients.Add(clientInfo);
                }

                _ = HandleWebSocket.HandleWebSocketAsync(clientInfo, _lock, _clients);
            }
            else
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
    }
}

class ClientInfo
{
    public WebSocket WebSocket { get; set; }
    public string Nome { get; set; }
    public Guid Id { get; set; }
}

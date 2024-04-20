using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace ChessServer;

public class Program
{
    public static async Task Main(string[] args)
    {
        Server server = new();
        await server.Start(5555);
    }
}

class Server
{
    ConcurrentDictionary<string, GameRoom> _gameRooms = new();

    const string MessageRegex = "#&#";
    
    public async Task Start(int port)
    {
        TcpListener tcpListener = new TcpListener(IPAddress.Any, port);
        Console.WriteLine("Server was started...");
        tcpListener.Start();

        while (true)
        {
            var tcpClient = await tcpListener.AcceptTcpClientAsync();
            Console.WriteLine($"Connected User: {tcpClient.Client.RemoteEndPoint}");
            Task.Run(() => ProcessPlayer(new Player(tcpClient)));
        }
    }

    async void ProcessPlayer(Player player)
    {
        string roomName = null;
        string nickName = null;
        try
        {
            while (player.TcpClient.Connected)
            {
                var message = await player.StreamReader.ReadLineAsync();
                
                if (string.IsNullOrEmpty(message))
                {
                    continue;
                }
                
                if (message != null && message.StartsWith("show_rooms"))
                {
                    string roomNames = string.Join(MessageRegex, _gameRooms.Keys);
                    await player.StreamWriter.WriteLineAsync(roomNames);
                    await player.StreamWriter.FlushAsync();
                }

                //Чтобы не прерывалось подключение, а лишь сбрасывались данные
                if (message != null && message.StartsWith("exit")) 
                {
                    //вида exit
                    if (roomName != null && nickName != null)
                    {
                        if (_gameRooms.TryGetValue(roomName, out var gameRoom))
                        {
                            gameRoom.RemovePlayer(nickName, out var isViewer);
                            if (!isViewer)
                            {
                                //TODO убрать комнаты и всем зрителям и игроку выдать сообщение, что такой-то игрок закончил игру
                            }
                        }   
                    }

                    roomName = null;
                    nickName = null;
                    player.IsViewer = false;
                }

                if (message != null && message.StartsWith("create_room"))
                {
                    var command = message.Split(MessageRegex, StringSplitOptions.RemoveEmptyEntries);
                    // Format: create_room#&#room_name#&#player_name
                    roomName = command[1];
                    nickName = command[2];
                    if (_gameRooms.TryAdd(roomName, new GameRoom(roomName)))
                    {
                        player.Nickname = nickName;
                        _gameRooms[roomName].AddPlayer(player);
                    }
                    else
                    {
                        roomName = null;
                        nickName = null;
                        await player.StreamWriter.WriteLineAsync("Room with this name already exists.");
                        await player.StreamWriter.FlushAsync();
                    }
                }

                if (message != null && message.StartsWith("connect_room"))
                {
                    var command = message.Split(MessageRegex, StringSplitOptions.RemoveEmptyEntries); 
                    // Format: connect_room#&#room_name#&#nick_name
                    roomName = command[1];
                    nickName = command[2];
                    if (_gameRooms.TryGetValue(roomName, out var gameRoom))
                    {
                        player.Nickname = nickName;
                        if (gameRoom.Count > 2)
                        {
                            player.IsViewer = true;
                        }
                        gameRoom.AddPlayer(player);
                        
                        if (gameRoom.Count == 2)
                        {
                            gameRoom.Start();
                        }
                    }
                    else
                    {
                        roomName = null;
                        nickName = null;
                        await player.StreamWriter.WriteLineAsync("Room with this name does not exist.");
                        await player.StreamWriter.FlushAsync();
                    }
                }

                //make_move#&#a1b2
                if (message != null && message.StartsWith("make_move"))
                {
                    if (nickName != null && roomName != null && !player.IsViewer)
                    {
                        _gameRooms.TryGetValue(roomName, out var room);
                        //room.MakeMove;
                    } 
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while processing player: {ex.Message}");
        }
        finally
        {
            if (roomName != null && nickName != null)
            {
                if (_gameRooms.TryGetValue(roomName, out var gameRoom))
                {
                    gameRoom.RemovePlayer(nickName, out var isViewer);
                    if (!isViewer)
                    {
                        // TODO: Отправить сообщение всем зрителям и игроку о завершении игры.
                    }
                }
            }
            player.Dispose();
        }
    }

}

public class Player : IDisposable
{
    public string Nickname { get; set; }
    public TcpClient TcpClient { get; set; }
    public bool IsViewer { get; set; }

    public StreamReader StreamReader { get; set; }
    public StreamWriter StreamWriter { get; set; }

    public string GetPlayerIp() => (TcpClient.Client.RemoteEndPoint as IPEndPoint)!.Address.ToString();
    
    public Player(TcpClient tcpClient)
    {
        TcpClient = tcpClient;
        StreamReader = new StreamReader(tcpClient.GetStream());
        StreamWriter = new StreamWriter(tcpClient.GetStream());
    }

    public void Dispose()
    {
        TcpClient?.Dispose();
        StreamReader?.Dispose();
        StreamWriter?.Dispose();
    }
}

public class GameRoom {
    public string Name { get; }
    List<Player> Players { get; } = [];

    public int Count => Players.Count;

    public GameRoom(string name) 
    {
        Name = name;
    }

    public void RemovePlayer(string nickName, out bool isViewer)
    {
        var player = Players.FirstOrDefault(x => x.Nickname == nickName); 
        if (player != null)
        {
            Players.Remove(player);
            isViewer = player.IsViewer;
        }
        else
        {
            isViewer = false;
        }
    }
    
    public Player FirstPlayer { get; set; }
    public Player SecondPlayer { get; set; }

    public void Start()
    {
        FirstPlayer = Players[0];
        SecondPlayer = Players[1];
        //TODO
    }

    public void AddPlayer(Player player) 
    {
        Players.Add(player);
    }
}
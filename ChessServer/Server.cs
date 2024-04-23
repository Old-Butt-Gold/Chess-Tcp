using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ChessLogic.CoordinateClasses;

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
    ConcurrentDictionary<string, ChessRoom> _gameRooms = new();

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
            Task.Run(() => ProcessPlayer(new ChessPlayer(tcpClient)));
        }
    }

    async void ProcessPlayer(ChessPlayer chessPlayer)
    {
        string? roomName = null;
        try
        {
            while (chessPlayer.TcpClient.Connected)
            {
                var message = await chessPlayer.StreamReader.ReadLineAsync();
                
                if (string.IsNullOrEmpty(message))
                {
                    continue;
                }
                
                if (message.StartsWith("show_rooms"))
                {
                    StringBuilder roomNames = new();
                    foreach (var item in _gameRooms.Values)
                    {
                        roomNames.Append(item.Name + ":" + item.Count + MessageRegex);
                    }
                    await chessPlayer.StreamWriter.WriteLineAsync(roomNames);
                }

                if (message.StartsWith("create_room"))
                {
                    var command = message.Split(MessageRegex, StringSplitOptions.RemoveEmptyEntries);
                    // Format: create_room#&#room_name
                    roomName = command[1];
                    if (_gameRooms.TryAdd(roomName, new ChessRoom(roomName)))
                    {
                        var room = _gameRooms[roomName];
                        room.WhitePlayer = chessPlayer;
                        room.AddPlayer(chessPlayer);
                    }
                    else
                    {
                        roomName = null;
                        await chessPlayer.StreamWriter.WriteLineAsync("Room with this name already exists.");
                    }
                }

                if (message.StartsWith("connect_room"))
                {
                    var command = message.Split(MessageRegex, StringSplitOptions.RemoveEmptyEntries); 
                    //connect_room#&#room_name
                    roomName = command[1];
                    if (_gameRooms.TryGetValue(roomName, out var gameRoom))
                    {
                        if (gameRoom.Count < 2)
                        {
                            gameRoom.BlackPlayer = chessPlayer;
                            gameRoom.AddPlayer(chessPlayer);
                            gameRoom.Start();
                        }
                        else
                        {
                            roomName = null;
                            await chessPlayer.StreamWriter.WriteLineAsync("Room with this name is full.");    
                        }
                    }
                    else
                    {
                        roomName = null;
                        await chessPlayer.StreamWriter.WriteLineAsync("Room with this name does not exist.");
                    }
                }

                //make_move#&#1a:2b:White
                if (message.StartsWith("make_move"))
                {
                    if (roomName != null)
                    {
                        _gameRooms.TryGetValue(roomName, out var room);
                        
                        var str = message.Split(MessageRegex, StringSplitOptions.RemoveEmptyEntries);
                        var move = str[1];
                        var moveComponents = move.Split(":", StringSplitOptions.RemoveEmptyEntries);
                        Player currentPlayer = Enum.Parse<Player>(moveComponents[2]);
                        
                        var startPosition = moveComponents[0];
                        var endPosition = moveComponents[1];

                        if (currentPlayer == Player.White)
                        {
                            await room!.BlackPlayer!.StreamWriter.WriteLineAsync($"{startPosition}:{endPosition}");
                        }
                        else
                        {
                            await room!.WhitePlayer!.StreamWriter.WriteLineAsync($"{startPosition}:{endPosition}");
                        }
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
            if (roomName != null)
            {
                if (_gameRooms.TryGetValue(roomName, out var gameRoom))
                {
                    gameRoom.RemovePlayer(chessPlayer);
                }
            }
            chessPlayer.Dispose();
        }
    }

}

public class ChessPlayer : IDisposable, IEquatable<ChessPlayer>
{
    string Id { get; } = Guid.NewGuid().ToString();

    public TcpClient TcpClient { get; set; }

    public StreamReader StreamReader { get; set; }
    public StreamWriter StreamWriter { get; set; }

    public string GetPlayerIp() => (TcpClient.Client.RemoteEndPoint as IPEndPoint)!.Address.ToString();
    
    public ChessPlayer(TcpClient tcpClient)
    {
        TcpClient = tcpClient;
        StreamReader = new StreamReader(tcpClient.GetStream(), Encoding.UTF8);
        StreamWriter = new StreamWriter(tcpClient.GetStream(), Encoding.UTF8)
        {
            AutoFlush = true,
        };
    }

    public void Dispose()
    {
        TcpClient?.Dispose();
        StreamReader?.Dispose();
        StreamWriter?.Dispose();
    }

    public bool Equals(ChessPlayer? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ChessPlayer)obj);
    }

    public override int GetHashCode() => Id.GetHashCode();
}

public class ChessRoom {
    public string Name { get; }
    List<ChessPlayer> Players { get; } = [];

    public int Count => Players.Count;

    public ChessRoom(string name) 
    {
        Name = name;
    }

    public ChessPlayer? WhitePlayer { get; set; }
    public ChessPlayer? BlackPlayer { get; set; }

    public void Start()
    {
        //TODO выдать обоим сообщение о начале игры
    }

    public void AddPlayer(ChessPlayer chessPlayer) => Players.Add(chessPlayer);

    public void RemovePlayer(ChessPlayer chessPlayer)
    {
        Players.Remove(chessPlayer);
        if (chessPlayer.Equals(WhitePlayer))
        {
            WhitePlayer = null;
        }

        if (chessPlayer.Equals(BlackPlayer))
        {
            BlackPlayer = null;
        }
    }
}
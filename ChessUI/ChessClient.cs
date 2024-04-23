using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using ChessLogic.CoordinateClasses;
using ChessLogic.Moves;

namespace ChessUI;

public class ChessClient : IDisposable
{
    TcpClient _tcpClient = new(AddressFamily.InterNetwork);
    StreamReader _reader;
    StreamWriter _writer;
    
    public const string MessageRegex = "#&#";
    
    public async Task ConnectAsync(IPAddress ipAddress, int port)
    {
        if (!IsConnected)
        {
            await _tcpClient.ConnectAsync(ipAddress, port);
            var stream = _tcpClient.GetStream();
            _reader = new StreamReader(stream, Encoding.UTF8);
            _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        }
        else
        {
            MessageBox.Show($"You are already connected to the server: {_tcpClient.Client.RemoteEndPoint}");
        }
    }

    public async Task DisconnectAsync()
    {
        if (IsConnected)
        {
            _reader?.Dispose();
            _writer?.Dispose();
            _tcpClient.Close();
            _tcpClient?.Dispose();
            _tcpClient = new(AddressFamily.InterNetwork);
        }
        else
        {
            MessageBox.Show("You are not connected to the server.");
        }
    }

    public bool IsConnected => _tcpClient.Connected;

    public async Task SendMessageAsync(string message) => await _writer.WriteLineAsync(message);

    public async Task<string?> ReceiveMessageAsync() => await _reader.ReadLineAsync();
    
    public async Task SendMoveAsync(Move move, Player player) 
        => await _writer.WriteLineAsync($"make_move{MessageRegex}{move.FromPos}:{move.ToPos}:{player}");

    public async Task<Move> ReceiveMoveAsync()
    {
        //Инверсируется ход для врага
        
        var stringMove = await _reader.ReadLineAsync();

        var move = stringMove!.Split(":", StringSplitOptions.RemoveEmptyEntries);

        var startRow = 7 - int.Parse(move[0][0].ToString());
        var startColumn = 7 - (move[0][1] - 'a');
        var endRow = 7 - int.Parse(move[1][0].ToString());
        var endColumn = 7 - (move[1][1] - 'a');

        var startPosition = new Position(startRow, startColumn);
        var endPosition = new Position(endRow, endColumn);

        return new NormalMove(startPosition, endPosition);
    }
    
    

    public void Dispose()
    {
        _reader?.Dispose();
        _writer?.Dispose();
        _tcpClient?.Close();
    }
}
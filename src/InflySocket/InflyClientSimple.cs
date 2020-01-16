using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InflySocket
{
    public class InflyClientSimple
    {
        private bool _running;
        private Socket _socket;
        public char Separator = '\n';
        public delegate void OnConnectedHandler();
        public delegate void OnReceiveMessageHandler(string msg);
        public delegate void OnClosedHandler();

        public event OnConnectedHandler OnConnectedEvent;
        public event OnReceiveMessageHandler OnReceiveMessageEvent;
        public event OnClosedHandler OnCloseEvent;

        public bool IsConnected
        {
            get
            {
                if (_socket == null) return false;
                return _socket.Connected;
            }
        }

        public void Connect(string ip, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress iPAddress = IPAddress.Parse(ip);
            IPEndPoint point = new IPEndPoint(iPAddress, port);
            _running = true;

            Task.Run(() =>
            {
                while (_running)
                {
                    try
                    {
                        if (!_socket.Connected)
                        {
                            _socket.Connect(point);
                            OnConnected();

                            Thread.Sleep(500);
                        }
                        else
                        {
                            ProcessLinesAsync(_socket).ConfigureAwait(false);
                        }
                    }
                    catch (SocketException exp)
                    {

                    }
                }
            });
        }

        protected virtual void OnClosed()
        {
            OnCloseEvent?.Invoke();
        }

        protected virtual void OnConnected()
        {
            OnConnectedEvent?.Invoke();
        }

        protected virtual void OnReceiveMessage(string message)
        {
            OnReceiveMessageEvent?.Invoke(message);
        }

        public void Send(byte[] buf)
        {
            if (_socket.Connected)
                _socket.Send(buf);
        }

        public void Send(string msg)
        {
            var sendMsg = Encoding.UTF8.GetBytes(msg);
            Send(sendMsg);
        }

        #region Pipelines

        private async Task ProcessLinesAsync(Socket socket)
        {
            var pipe = new Pipe();
            Task writing = FillPipeAsync(socket, pipe.Writer);
            Task reading = ReadPipeAsync(pipe.Reader);

            await Task.WhenAll(reading, writing);

            OnClosed();
        }

        //写入循环
        private async Task FillPipeAsync(Socket socket, PipeWriter writer)
        {
            const int minimumBufferSize = 512;

            while (_running)
            {
                //从PipeWriter至少分配512字节
                Memory<byte> memory = writer.GetMemory(minimumBufferSize);
                try
                {
                    //将内存空间变成ArraySegment，提供给socket使用
                    if (!MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> arraySegment))
                    {
                        throw new InvalidOperationException("Buffer backed by array was expected");
                    }

                    //接受数据
                    int bytesRead = await socket.ReceiveAsync(arraySegment, SocketFlags.None);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    //一次接受完毕，数据已经在pipe中，告诉pipe已经给它写了多少数据。
                    writer.Advance(bytesRead);
                }
                catch
                {
                    break;
                }
                // 提示reader可以进行读取数据，reader可以继续执行readAsync()方法
                FlushResult result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }
            // 告诉pipe已完成
            await writer.CompleteAsync();
        }

        //读取流
        private async Task ReadPipeAsync(PipeReader reader)
        {
            while (_running)
            {
                //等待writer写数据
                ReadResult result = await reader.ReadAsync();

                //获得内存区域
                ReadOnlySequence<byte> buffer = result.Buffer;
                SequencePosition? position = null;

                do
                {
                    // 在缓冲数据中查找找一个行末尾
                    position = buffer.PositionOf((byte)Separator);

                    if (position != null)
                    {
                        // 处理这一行
                        ProcessLine(buffer.Slice(0, position.Value).ToArray());

                        // 跳过这一行
                        buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
                    }
                }
                while (position != null);
                //数据处理完毕，告诉pipe还剩下多少数据没有处理（数据包不完整的数据，找不到head）
                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }
            //将PipeReader标记为完成
            await reader.CompleteAsync();
        }

        private void ProcessLine(byte[] data)
        {
            string msg = Encoding.UTF8.GetString(data);
            OnReceiveMessage(msg);
        }
        #endregion
    }
}

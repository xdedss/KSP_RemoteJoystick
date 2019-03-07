using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace KSP_RemoteJoystick
{
    //参考：https://blog.csdn.net/qq_33022911/article/details/82432778
    class SocketServer
    {
        public bool listening = false;
        public bool hasClient { get { return listening && clients.Any((s) => (s != null && s.Connected)); } }
        private List<Socket> clients = new List<Socket>();

        public byte[] dataToSend = new byte[0];
        public byte[] dataReceived = new byte[0];
        public byte[] initialData = new byte[0];

        private object locker = new object();

        private string _ip = string.Empty;
        private int _port = 0;
        private Socket _socket = null;
        private Thread _listenThread;
        private byte[] buffer = new byte[1024 * 1024 * 2];

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ip">监听的IP</param>
        /// <param name="port">监听的端口</param>
        public SocketServer(string ip, int port)
        {
            this._ip = ip;
            this._port = port;
        }
        public SocketServer(int port)
        {
            this._ip = "0.0.0.0";
            this._port = port;
        }

        public void Close()
        {
            if (!listening)
            {
                return;
            }
            _listenThread.Abort();
            _socket.Close();
            _socket = null;
            listening = false;
        }

        public void StartListen()
        {
            if (listening)
            {
                return;
            }
            try
            {
                if(_socket != null)
                {
                    _socket.Close();
                }
                clients = new List<Socket>();
                //1.0 实例化套接字(IP4寻找协议,流式协议,TCP协议)
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //2.0 创建IP对象
                IPAddress address = IPAddress.Parse(_ip);
                //3.0 创建网络端口,包括ip和端口
                IPEndPoint endPoint = new IPEndPoint(address, _port);
                //4.0 绑定套接字
                _socket.Bind(endPoint);
                //5.0 设置最大连接数
                _socket.Listen(int.MaxValue);
                Debug.Log(string.Format("监听{0}消息成功", _socket.LocalEndPoint.ToString()));
                //6.0 开始监听
                _listenThread = new Thread(ListenClientConnect);
                _listenThread.Start();
                listening = true;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        public void Update()
        {
            if (listening)
            {
                lock (locker)
                {
                    foreach (var clientSocket in clients)
                    {
                        if (clientSocket != null)
                        {
                            try
                            {
                                //获取从客户端发来的数据
                                if (clientSocket.Available > 0)
                                {
                                    int length = clientSocket.Receive(buffer);
                                    //var msg = Encoding.ASCII.GetString(buffer, 0, length);//no other char
                                    var bytes = new byte[length];
                                    Array.ConstrainedCopy(buffer, 0, bytes, 0, length);
                                    //ConnectionInitializer.Log(msg);
                                    Debug.Log(string.Format("from{0},length{1},message:{2}", clientSocket.RemoteEndPoint.ToString(), bytes.Length, bytes.ToString()));
                                    dataReceived = bytes;


                                    //如果收到了就发送回信，客户端控制频率
                                    if (dataToSend != null && dataToSend.Length > 0)
                                    {
                                        clientSocket.Send(dataToSend);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError(ex.Message);
                                try
                                {
                                    //clientSocket.Shutdown(SocketShutdown.Both);
                                    //clientSocket.Close();
                                }
                                catch (Exception e) { }
                                //break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 监听客户端连接
        /// </summary>
        private void ListenClientConnect()
        {
            try
            {
                while (true)
                {
                    //Socket创建的新连接
                    Socket clientSocket = _socket.Accept();//阻塞直到连接
                    Console.WriteLine("连接到新客户端" + clientSocket.RemoteEndPoint.ToString());
                    clientSocket.Send(initialData);
                    lock (locker)
                    {
                        clients.Add(clientSocket);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}

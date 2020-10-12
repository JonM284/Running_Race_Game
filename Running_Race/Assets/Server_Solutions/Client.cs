using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int data_Buffer_Size = 4096;

    public string IP = "127.0.0.1";
    public int port = 25932;
    public int my_ID = 0;
    public TCP tcp;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }else if (instance != this)
        {
            Debug.Log("Another instance exists");
            Destroy(this);
        }
    }

    private void Start()
    {
        tcp = new TCP();
    }

    public void Connect_To_Server()
    {
        tcp.Connect();
    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private byte[] receive_Buffer;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = data_Buffer_Size,
                SendBufferSize = data_Buffer_Size
            };
            receive_Buffer = new byte[data_Buffer_Size];
            socket.BeginConnect(instance.IP, instance.port, Connect_Callback, socket);
        }

        private void Connect_Callback(IAsyncResult _result)
        {
            socket.EndConnect(_result);

            if (!socket.Connected)
            {
                return;
            }

            stream = socket.GetStream();
            stream.BeginRead(receive_Buffer, 0, data_Buffer_Size, Receive_Callback, null);
        }

        private void Receive_Callback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receive_Buffer, _data, _byteLength);

                stream.BeginRead(receive_Buffer, 0, data_Buffer_Size, Receive_Callback, null);
            }
            catch
            {

            }
        }
    }
}

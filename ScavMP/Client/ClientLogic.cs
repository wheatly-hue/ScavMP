using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using BepInEx.Logging;
using LiteEntitySystem;
using LiteNetLib;
using LiteNetLib.Utils;
using ScavMP.Shared;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ScavMP.Client;

public class ClientLogic : MonoBehaviour, ILiteNetEventListener
{
    private LiteNetManager _netManager;
    private NetDataWriter _writer;
    private NetPacketProcessor _packetProcessor;

    private string _userName;
    private LiteNetPeer _server;
    private ClientEntityManager _entityManager;
    private int _ping;
    private float _secondTimer;
    private BaseExpie _ourPlayer;
    private Action<DisconnectInfo> _onDisconnected;

    public static ClientLogic Instance { get; private set; }
    public ClientEntityManager EntityManager => _entityManager;
    public NetStatistics NetStats => _netManager?.Statistics;
    public int Ping => _ping;

    private static Logger _logger;

    static ClientLogic()
    {
        _logger = new Logger(new ManualLogSource("ScavMP.Client"));
        LiteEntitySystem.Logger.LoggerImpl = _logger;
    }

    private void Awake()
    {
        LiteEntitySystem.EntityManager.RegisterFieldType<Vector2>(Vector2.Lerp);

        Instance = this;
        _userName = Environment.MachineName + " " + Random.Range(0, 100000);
        _writer = new NetDataWriter();

        _packetProcessor = new NetPacketProcessor();
        _netManager = new LiteNetManager(this) { AutoRecycle = true };
        _netManager.Start();
    }

    void Update()
    {
        _netManager.PollEvents();
        _entityManager?.Update();
    }

    public void SetName(string userName)
    {
        _userName = userName;
    }

    private void SendPacket<T>(T packet, DeliveryMethod deliveryMethod)
        where T : class, new()
    {
        if (_server == null)
            return;
        _writer.Reset();
        _writer.Put((byte)PacketType.Serialized);
        _packetProcessor.Write(_writer, packet);
        _server.Send(_writer, deliveryMethod);
    }

    void ILiteNetEventListener.OnPeerConnected(LiteNetPeer peer)
    {
        Debug.Log("[C] Connected to server: " + peer);
        _server = peer;
    }

    void ILiteNetEventListener.OnPeerDisconnected(LiteNetPeer peer, DisconnectInfo disconnectInfo)
    {
        _server = null;
        _entityManager = null;
        Debug.Log("[C] Disconnected from server: " + disconnectInfo.Reason);
        if (_onDisconnected != null)
        {
            _onDisconnected(disconnectInfo);
            _onDisconnected = null;
        }
    }

    void ILiteNetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Debug.Log("[C] NetworkError: " + socketError);
    }

    void ILiteNetEventListener.OnNetworkReceive(
        LiteNetPeer peer,
        NetPacketReader reader,
        DeliveryMethod deliveryMethod
    )
    {
        byte packetType = reader.PeekByte();
        var pt = (PacketType)packetType;
        switch (pt)
        {
            case PacketType.EntitySystem:
                _entityManager.Deserialize(reader.GetRemainingBytesSpan());
                break;

            case PacketType.Serialized:
                reader.GetByte();
                _packetProcessor.ReadAllPackets(reader);
                break;

            default:
                Debug.Log("Unhandled packet: " + pt);
                break;
        }
    }

    void ILiteNetEventListener.OnNetworkReceiveUnconnected(
        IPEndPoint remoteEndPoint,
        NetPacketReader reader,
        UnconnectedMessageType messageType
    ) { }

    void ILiteNetEventListener.OnNetworkLatencyUpdate(LiteNetPeer peer, int latency)
    {
        _ping = latency;
    }

    void ILiteNetEventListener.OnConnectionRequest(LiteConnectionRequest request)
    {
        request.Reject();
    }

    public void Connect(string ip, int port, Action<DisconnectInfo> onDisconnected)
    {
        _onDisconnected = onDisconnected;
        _netManager.Connect(ip, port, "ScavGame");
    }
}

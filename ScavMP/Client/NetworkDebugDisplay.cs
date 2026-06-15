using UnityEngine;
using UnityEngine.UI;

namespace ScavMP.Client;

public class NetworkDebugDisplay : MonoBehaviour
{
    [SerializeField]
    private Text _debugText;

    private float _secondTimer;
    private int _bytesIn,
        _bytesOut,
        _packetsIn,
        _packetsOut;

    void Update()
    {
        var stats = ClientLogic.Instance.NetStats;
        if (stats == null)
        {
            _debugText.text = "Disconnected";
            return;
        }

        // обновляем раз в секунду
        _secondTimer += Time.deltaTime;
        if (_secondTimer >= 1f)
        {
            _secondTimer -= 1f;
            _bytesIn = (int)stats.BytesReceived;
            _bytesOut = (int)stats.BytesSent;
            _packetsIn = (int)stats.PacketsReceived;
            _packetsOut = (int)stats.PacketsSent;
            stats.Reset();
        }

        var em = ClientLogic.Instance.EntityManager;
        if (em == null)
            return;

        _debugText.text =
            $"C_ServerTick: {em.ServerTick}\n"
            + $"C_Tick: {em.Tick}\n"
            + $"C_LPRCS: {em.LastProcessedTick}\n"
            + $"C_StoredCommands: {em.StoredCommands}\n"
            + $"C_Entities: {em.EntitiesCount}\n"
            + $"C_ServerInputBuffer: {em.ServerInputBuffer}\n"
            + $"C_LerpBufferCount: {em.LerpBufferCount}\n"
            + $"C_LerpBufferTime: {em.LerpBufferTimeLength}\n"
            + $"Jitter: {em.NetworkJitter}\n"
            + $"Ping: {ClientLogic.Instance.Ping}\n"
            + $"IN: {_bytesIn / 1000f:F1} KB/s ({_packetsIn})\n"
            + $"OUT: {_bytesOut / 1000f:F1} KB/s ({_packetsOut})";
    }
}

using UnityEngine;
using WebSocketSharp;

public class TestWS : MonoBehaviour
{
    WebSocket ws;

    void Start()
    {
        // Replace localhost with your PC's LAN IP if testing on another device
        ws = new WebSocket("ws://localhost:9090/ws"); 

        ws.OnOpen += (s, e) => Debug.Log("Connected!");
        ws.OnClose += (s, e) => Debug.Log("Closed: " + e.Reason);
        ws.OnError += (s, e) => Debug.Log("Error: " + e.Message);

        Debug.Log("Connecting...");
        ws.Connect();
    }
}

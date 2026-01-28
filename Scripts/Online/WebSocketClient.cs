using UnityEngine;
using WebSocketSharp;
using System;
using System.Collections;

public class WebSocketClient : MonoBehaviour
{
    private WebSocket ws;
    public bool isConnected = false;

    [Header("References")]
    public GameManager gameManager;
    public UIManager uiManager;

    [HideInInspector]
    public char mySymbol;

    private GameStateMessage pendingState;
    private bool hasNewState = false;

  
    private Action pendingUIAction;

    void Update()
    {
      
        if (hasNewState)
        {
            gameManager.UpdateGameState(pendingState);
            hasNewState = false;
        }

    
        if (pendingUIAction != null)
        {
            pendingUIAction.Invoke();
            pendingUIAction = null;
        }
    }

    public void ConnectAndJoin()
    {
        if (ws != null && ws.IsAlive) return;

        ws = new WebSocket("ws://localhost:9090/ws");

        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("Connected to server");
            isConnected = true;

         
            SendJoinMessage();
        };

        ws.OnMessage += (sender, e) =>
        {
            try
            {
                Debug.Log("SERVER → CLIENT: " + e.Data);

                BaseMessage baseMsg = JsonUtility.FromJson<BaseMessage>(e.Data);
                if (baseMsg == null || string.IsNullOrEmpty(baseMsg.type)) return;

                switch (baseMsg.type)
                {
                    case "join":
                        {
                            GameStateMessage joinMsg = JsonUtility.FromJson<GameStateMessage>(e.Data);
                            mySymbol = joinMsg.playerId[0];
                            Debug.Log("Joined as " + mySymbol);
                            break;
                        }

                    case "waiting":
                        Debug.Log("Waiting for opponent...");

                        pendingUIAction = () =>
                            uiManager.ShowWaiting(mySymbol);
                        break;




                    

                    case "matched":
                        Debug.Log("Match found!");
                        char opponentSymbol = (mySymbol == 'X') ? 'O' : 'X';

                        pendingUIAction = () =>
                        {
                            uiManager.ShowMatchedAndStart(mySymbol, opponentSymbol, 2f);
                        };
                        break;



                    case "state":
                        pendingState = JsonUtility.FromJson<GameStateMessage>(e.Data);
                        hasNewState = true;
                        break;

                    default:
                        Debug.LogWarning("Unknown message type: " + baseMsg.type);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception in OnMessage: " + ex.Message + "\nData: " + e.Data);
            }
        };

        ws.OnClose += (sender, e) =>
        {
            Debug.Log("Connection closed");
            isConnected = false;
        };

        ws.OnError += (sender, e) =>
        {
            Debug.LogError("WebSocket Error: " + e.Message);
        };

        ws.ConnectAsync(); 
    }

    void SendJoinMessage()
    {
        if (!isConnected) return;

        JoinMessage msg = new JoinMessage { type = "join" };
        ws.Send(JsonUtility.ToJson(msg));
    }

    public void SendMove(int cellIndex)
    {
        if (!isConnected) return;

        MoveMessage msg = new MoveMessage
        {
            type = "move",
            cell = cellIndex
        };

        ws.Send(JsonUtility.ToJson(msg));
    }

    void OnApplicationQuit()
    {
        if (ws != null && ws.IsAlive)
            ws.Close();
    }

    [Serializable]
    private class BaseMessage
    {
        public string type;
    }

    public void CancelMatchmaking()
    {
        if (ws != null && ws.IsAlive)
        {
            
            MoveMessage cancelMsg = new MoveMessage { type = "cancel" };
            ws.Send(JsonUtility.ToJson(cancelMsg));

            ws.Close();
            isConnected = false;
            Debug.Log("Disconnected from server due to matchmaking cancel.");
        }
    }
public void Disconnect()
{
    if (ws != null)
    {
        if (ws.IsAlive)
            ws.Close();

        ws = null;
        isConnected = false;
        Debug.Log("WebSocket disconnected");
    }
}



}

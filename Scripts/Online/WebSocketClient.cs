using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;
using System;
using System.Collections;

public class WebSocketClient : MonoBehaviour
{
    public GameUIManager gameuiManager;
    public BoardManager boardManager;
    public static WebSocketClient Instance;

    private WebSocket ws;
    public bool isConnected = false;

    [HideInInspector]
    public int mySymbol;
    [HideInInspector]
    public string myPlayerId;

    private Action pendingUIAction;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (pendingUIAction != null)
        {
            pendingUIAction.Invoke();
            pendingUIAction = null;
        }
    }

 
    public void ConnectToServer()
    {
        if (ws != null && ws.IsAlive) return;

        ws = new WebSocket("ws://localhost:9090/ws");

        ws.OnOpen += (_, _) =>
        {
            Debug.Log("Connected to server!");
            isConnected = true;
        };

        ws.OnMessage += (_, e) =>
        {
            try
            {
                Debug.Log("SERVER → CLIENT: " + e.Data);

                BaseMessage baseMsg = JsonUtility.FromJson<BaseMessage>(e.Data);
                if (baseMsg == null || string.IsNullOrEmpty(baseMsg.type)) return;

                switch (baseMsg.type)
                {
                    case "waiting":
                        Debug.Log("Waiting for opponent...");
                        pendingUIAction = () =>
                        {
                            var menu = FindObjectOfType<MenuUIManager>();
                            if (menu != null) menu.ShowFindingMatch();
                        };
                        break;

                    case "game_start":
                        {
                            GameStartMessage startMsg = JsonUtility.FromJson<GameStartMessage>(e.Data);
                            if (startMsg == null) break;

                            mySymbol = startMsg.symbol;
                            myPlayerId = startMsg.playerId ?? "";
                            string turnPlayerId = startMsg.turn ?? "";

                            Debug.Log($"Game started! Room full. My symbol: {mySymbol}, my turn: {(turnPlayerId == myPlayerId)}");

                            pendingUIAction = () =>
                            {
                                var menu = FindObjectOfType<MenuUIManager>();
                                if (menu != null) menu.ShowMatchFound(mySymbol);
                                StartCoroutine(WaitThenLoadGameScene(myPlayerId, turnPlayerId));
                            };
                            break;
                        }

                    case "move_response":
                        {
                            MoveResponseMessage moveMsg = JsonUtility.FromJson<MoveResponseMessage>(e.Data);
                            if (moveMsg != null && moveMsg.valid)
                            {
                                int cell = moveMsg.cell;
                                int symbol = moveMsg.symbol;
                                string turn = moveMsg.turn != null ? moveMsg.turn : "";
                                string winner = moveMsg.winner != null ? moveMsg.winner : "";

                                pendingUIAction = () =>
                                {
                                    if (boardManager != null)
                                    {
                                        boardManager.ApplyMoveFromServer(cell, symbol, turn, winner);
                                    }
                                };
                            }
                            break;
                        }

                    case "opponent_disconnected":
                        {
                            // Parse to get winner info
                            MoveResponseMessage disconnectMsg = JsonUtility.FromJson<MoveResponseMessage>(e.Data);
                            Debug.Log($"Opponent disconnected. Winner: {disconnectMsg.winner}");
                            
                            pendingUIAction = () =>
                            {
                                if (boardManager != null)
                                {
                                    boardManager.HandleOpponentDisconnect();
                                }
                            };
                            break;
                        }

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

        ws.OnClose += (_, e) =>
        {
            Debug.Log("Connection closed");
            isConnected = false;
        };

        ws.OnError += (_, e) =>
        {
            Debug.LogError("WebSocket Error: " + e.Message);
        };

        ws.ConnectAsync();
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
        Debug.Log("Sent move: " + cellIndex);
    }

    /// <summary>
    /// After match found: pause so player sees "Match Found!" and symbols, then load game board.
    /// </summary>
    private IEnumerator WaitThenLoadGameScene(string myId, string turnId)
    {
        yield return new WaitForSeconds(2.5f);

        if (SceneManager.GetActiveScene().name != "GameScene")
        {
            SceneManager.LoadScene("GameScene");
            yield return new WaitForSeconds(0.5f);
        }

        boardManager = FindObjectOfType<BoardManager>();
        gameuiManager = FindObjectOfType<GameUIManager>();

        if (boardManager != null)
            boardManager.SetOnlineGameStart(myId, turnId);
        else
            Debug.LogError("BoardManager not found in GameScene!");

        if (gameuiManager != null)
            gameuiManager.ShowGameBoard();
    }

    public void DisconnectFromServer()
    {
        if (ws != null && ws.IsAlive)
        {
            Debug.Log("Disconnecting from server...");
            ws.Close();
            isConnected = false;
            
            // Reset state
            mySymbol = 0;
            myPlayerId = null;
            
            // Clear any pending actions
            pendingUIAction = null;
        }
    }

    void OnApplicationQuit()
    {
        DisconnectFromServer();
    }

    [Serializable]
    private class BaseMessage
    {
        public string type;
    }

public void OnClickCancelButton()
{
    Debug.Log("Cancel matchmaking");

    isConnected = false;
    pendingUIAction = null;
    myPlayerId = "";
    mySymbol = 0;

    if (ws != null)
    {
        if (ws.IsAlive)
        {
            ws.Close();
        }
        ws = null;
    }

    
        SceneManager.LoadScene("MenuScene");
    
}


}
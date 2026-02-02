using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class BoardManager : MonoBehaviour
{
    [Header("Board")]
        public Button[] cells;
    

    public int[] board;

    private bool isGameRunning = false;

    [Header("UI Manager")]
    public GameUIManager gameuiManager;

    [Header("Bot")]
    public BotPlayer botPlayer;


    private int currentPlayer = 1;   // 1 = O, -1 = X
    private bool isMatchmakingActive = false;

    [Header("Online turn tracking")]
    
    public string myPlayerId;
    
    
    public string currentTurnPlayerId;

    public enum GameMode
    {
        PassAndPlay,
        PlayWithComputer,
        PlayOnline,
    }

    public GameMode gameMode;

    // -------------------- INIT --------------------

    void Start()
    {
        gameMode = GameSettings.SelectedMode;
        
      
        if (board == null || board.Length != 9)
        {
            board = new int[9];
        }
        
        isGameRunning = true;

        Debug.Log($"BoardManager Start: Game Mode = {gameMode}");
       
        if (gameMode == GameMode.PlayOnline)
        {
            Debug.Log("Online mode detected. Waiting for server to start game...");
        }
    }
    void StartGame()
    {
        isGameRunning = true;
    }

    // -------------------- CELL CLICK --------------------

   
    public void OnCellClicked(int index)
    {
        if (!isGameRunning) return;
        if (board[index] != 0) return;

        Debug.Log($"On cell clicked {currentPlayer}");

        if (gameMode == GameMode.PlayOnline)
        {
            if (currentTurnPlayerId != myPlayerId)
            {
                Debug.Log("Not your turn!");
                return;
            }
            if (WebSocketClient.Instance != null)
            {
                WebSocketClient.Instance.SendMove(index);
            }
            return;
        }

        if (gameMode == GameMode.PassAndPlay)
        {
            MakeMove(index, currentPlayer);

            if (CheckWinForSymbol(currentPlayer, index, 3))
            {
                EndGameWithHighlight(currentPlayer == 1 ? "O" : "X", index, currentPlayer);
                return;
            }

            if (IsBoardFull())
            {
                EndDraw();
                return;
            }

            currentPlayer *= -1; // switch O ↔ X
        }
        else if (gameMode == GameMode.PlayWithComputer)
        {
            MakeMove(index, 1);

            if (CheckWinForSymbol(1, index, 3))
            {
                EndGameWithHighlight("O", index, 1);
                return;
            }

            if (IsBoardFull())
            {
                EndDraw();
                return;
            }
            DisableAllButtons();

            Invoke(nameof(PlayBotMove), 0.5f);
        }
    }

    // -------------------- BOT --------------------

    void PlayBotMove()
    {
        if (!isGameRunning) return;

        int botIndex = botPlayer.GetBotMove(this);
        if (botIndex < 0 || board[botIndex] != 0) return;

        MakeMove(botIndex, -1); // X

        if (CheckWinForSymbol(-1, botIndex, 3))
        {
            EndGameWithHighlight("X", botIndex, -1);
            return;
        }

        if (IsBoardFull())
        {
            EndDraw();
            return;
        }

        EnableEmptyButtons();

        currentPlayer = 1; // back to O
    }


     void DisableAllButtons()
    {
        for (int i = 0; i < cells.Length; i++)
            cells[i].interactable = false;
    }

    void EnableEmptyButtons()
    {
        for (int i = 0; i < cells.Length; i++)
            if (board[i] == 0)
                cells[i].interactable = true;
    }

    // -------------------- MOVE --------------------

    void MakeMove(int index, int player)
    {
        if (index < 0 || index >= board.Length)
        {
            Debug.LogError($"Invalid cell index: {index}");
            return;
        }
        if (cells == null || index >= cells.Length)
        {
            Debug.LogError($"Cells array issue: cells null={cells == null}, index={index}");
            return;
        }

        Debug.Log($"MakeMove: index={index}, player={player}, symbol={(player == 1 ? "O" : "X")}");
        board[index] = player;

        if (gameuiManager == null)
        {
            Debug.LogError("GameUIManager is NULL! Cannot place move on UI.");
            gameuiManager = FindObjectOfType<GameUIManager>();
            if (gameuiManager == null)
            {
                Debug.LogError("Still cannot find GameUIManager in scene!");
                return;
            }
        }

        gameuiManager.PlaceMove(cells[index], player);
    }

    // -------------------- WIN CHECK --------------------

    
    public bool CheckWinForSymbol(int player, int index, int size)
{
    int row = index / size;
    int col = index % size;

    bool onMainDiagonal = (row == col);
    bool onAntiDiagonal = (row + col == size - 1);


   return CheckRow(player, row, size)
    || CheckColumn(player, col, size)
    || (onMainDiagonal && CheckMainDiagonal(player, size))
    || (onAntiDiagonal && CheckAntiDiagonal(player, size));

}
bool CheckRow(int player, int row, int size)
{
    for (int col = 0; col < size; col++)
    {
        int idx = row * size + col;
        if (board[idx] != player)
            return false;
    }
    return true;
}

bool CheckColumn(int player, int col, int size)
{
    for (int row = 0; row < size; row++)
    {
        int idx = row * size + col;
        if (board[idx] != player)
            return false;
    }
    return true;
}

bool CheckMainDiagonal(int player, int size)
{
    for (int i = 0; i < size; i++)
    {
        int idx = i * size + i;
        if (board[idx] != player)
            return false;
    }
    return true;
}

bool CheckAntiDiagonal(int player, int size)
{
    for (int i = 0; i < size; i++)
    {
        int idx = i * size + (size - 1 - i);
        if (board[idx] != player)
            return false;
    }
    return true;
}

    /// <summary>
    /// Returns the 3 cell indices that form the winning line, or null if no win.
    /// </summary>
    public int[] GetWinningLine(int player, int index, int size)
    {
        int row = index / size;
        int col = index % size;
        int[] line = new int[size];

        if (CheckRow(player, row, size))
        {
            for (int c = 0; c < size; c++)
                line[c] = row * size + c;
            return line;
        }
        if (CheckColumn(player, col, size))
        {
            for (int r = 0; r < size; r++)
                line[r] = r * size + col;
            return line;
        }
        if (row == col && CheckMainDiagonal(player, size))
        {
            for (int i = 0; i < size; i++)
                line[i] = i * size + i;
            return line;
        }
        if (row + col == size - 1 && CheckAntiDiagonal(player, size))
        {
            for (int i = 0; i < size; i++)
                line[i] = i * size + (size - 1 - i);
            return line;
        }
        return null;
    }

    bool IsBoardFull()
    {
        for (int i = 0; i < board.Length; i++)
            if (board[i] == 0) return false;

        return true;
    }

    // -------------------- END GAME --------------------

    void EndGame(string winner)
    {
        isGameRunning = false;
        Debug.Log("Winner: " + winner);
        gameuiManager.ShowWinner(winner);
    }

    /// <summary>
    /// On win: allow 3rd symbol to show, highlight winning line, delay, then show winner panel.
    /// </summary>
    void EndGameWithHighlight(string winner, int lastMoveIndex, int winningSymbol)
    {
        isGameRunning = false;
        int[] line = GetWinningLine(winningSymbol, lastMoveIndex, 3);
        StartCoroutine(HighlightThenShowWinner(winner, line));
    }

    IEnumerator HighlightThenShowWinner(string winner, int[] winningLine)
    {
        float delay = (gameuiManager != null && gameuiManager.winningHighlightDelay > 0) ? gameuiManager.winningHighlightDelay : 1.5f;
        Color highlightColor = (gameuiManager != null) ? gameuiManager.winningLineColor : new Color(0.3f, 1f, 0.3f);

        if (winningLine != null && winningLine.Length > 0 && cells != null && gameuiManager != null)
        {
            gameuiManager.HighlightWinningCells(cells, winningLine, highlightColor);
            yield return new WaitForSeconds(delay);
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }

        if (gameuiManager != null)
            gameuiManager.ShowWinnerAfterHighlight(winner);
    }

    void EndDraw()
    {
        isGameRunning = false;
        Debug.Log("Draw");
        if (gameuiManager != null)
            gameuiManager.ShowWinnerAfterHighlight("Draw");
    }

    void ClearBoardUI()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            board[i] = 0;
            gameuiManager.ClearCell(cells[i]);
        }
    }

   
    public void SetOnlineGameStart(string myId, string turnPlayerId)
    {
        myPlayerId = myId;
        currentTurnPlayerId = turnPlayerId;
        board = new int[9];
        
        Debug.Log($"SetOnlineGameStart: myId={myId}, turnId={turnPlayerId}, myTurn={(currentTurnPlayerId == myPlayerId)}");
        
      
        if (gameuiManager == null)
        {
            gameuiManager = FindObjectOfType<GameUIManager>();
            Debug.Log($"Found GameUIManager: {(gameuiManager != null)}");
        }
        
        ClearBoardUI();
        UpdateCellsInteractable();
        
        Debug.Log($"Online game ready. My turn: {(currentTurnPlayerId == myPlayerId)}");
    }

   
    public void ApplyMoveFromServer(int cell, int symbol, string turnPlayerId, string winner)
    {
        if (board == null || board.Length != 9)
        {
            Debug.LogError("Board is null or wrong size!");
            return;
        }
        
        if (cell < 0 || cell >= board.Length)
        {
            Debug.LogError($"Invalid cell from server: {cell}");
            return;
        }
        
        if (board[cell] != 0)
        {
            Debug.LogWarning($"Cell {cell} already occupied with {board[cell]}. Skipping move.");
            return;
        }

        Debug.Log($">>> ApplyMoveFromServer: cell={cell}, symbol={symbol} ({(symbol == 1 ? "O" : "X")}), turn={turnPlayerId}, winner={winner}");

        MakeMove(cell, symbol);
        currentTurnPlayerId = turnPlayerId;
        UpdateCellsInteractable();

        if (!string.IsNullOrEmpty(winner))
        {
            if (winner == "Draw")
            {
                EndDraw();
            }
            else
            {
                string winnerSymbol;
                if (WebSocketClient.Instance != null)
                {
                    if (winner == myPlayerId)
                        winnerSymbol = (WebSocketClient.Instance.mySymbol == 1) ? "O" : "X";
                    else
                        winnerSymbol = (WebSocketClient.Instance.mySymbol == 1) ? "X" : "O";
                }
                else
                    winnerSymbol = "Winner";
                // Highlight winning line, delay, then show winner panel
                EndGameWithHighlight(winnerSymbol, cell, symbol);
            }
        }
    }

    void UpdateCellsInteractable()
    {
        if (cells == null || gameMode != GameMode.PlayOnline) return;
        bool myTurn = (currentTurnPlayerId == myPlayerId);
        Debug.Log($"UpdateCellsInteractable: myTurn={myTurn}, myId={myPlayerId}, turnId={currentTurnPlayerId}");
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].interactable = (board[i] == 0) && myTurn;
        }
    }

    public void HandleOpponentDisconnect()
    {
        isGameRunning = false;
        
    
        string mySymbol = "Winner";
        if (WebSocketClient.Instance != null && WebSocketClient.Instance.mySymbol != 0)
        {
            mySymbol = (WebSocketClient.Instance.mySymbol == 1) ? "O" : "X";
        }
        
        Debug.Log($"Opponent disconnected. You ({mySymbol}) win!");
        
   
        if (gameuiManager != null)
        {
            gameuiManager.ShowWinnerWithMessage($"{mySymbol} (You)", "Opponent Left!");
        }
    }

    public void RestartGame()
    {
        ClearBoardUI();
        gameuiManager.ShowGameBoard();
        Start();
    }
}

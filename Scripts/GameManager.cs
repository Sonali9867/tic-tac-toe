using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Board")]
    public Button[] cells;        // 9 buttons
    public char[] board;          // local board copy

    private bool isGameRunning = false;

    [Header("Timer")]
    public TextMeshProUGUI timerText;
    private float elapsedTime = 0f;

    [Header("UI Manager")]
    public UIManager uiManager;

    [Header("Bot")]
    public BotPlayer botPlayer;

    [Header("Online")]
    public WebSocketClient webSocketClient;

    public char currentPlayer = 'O';   // SERVER decides in online mode

    public enum GameMode
    {
        PassAndPlay,
        PlayWithComputer,
        PlayOnline,
    }

    public GameMode gameMode;

    // -------------------- GAME MODE SELECTION --------------------

    public void SetPassAndPlayMode()
    {
        gameMode = GameMode.PassAndPlay;
        uiManager.ShowGameBoard();
        StartGame();
    }

    public void SetPlayWithComputerMode()
    {
        gameMode = GameMode.PlayWithComputer;
        uiManager.ShowGameBoard();
        StartGame();
    }

    public void PlayOnline()
    {
        gameMode = GameMode.PlayOnline;
        uiManager.ShowMatchMakingPanel();
        board = new char[9];
        isGameRunning = false;
        ClearBoardUI();
        webSocketClient.ConnectAndJoin();
    }

    // -------------------- GAME START --------------------

    public void StartGame()
    {
        board = new char[9];
        currentPlayer = 'O';
        isGameRunning = true;
        elapsedTime = 0f;

        ClearBoardUI();
        UpdateTimerUI();
    }

    void ClearBoardUI()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            board[i] = '\0';
            cells[i].interactable = true;
            uiManager.ClearCell(cells[i]);
        }
    }

    // -------------------- UPDATE --------------------

    void Update()
    {
        if (!isGameRunning) return;

        elapsedTime += Time.deltaTime;
        UpdateTimerUI();
    }

    // -------------------- ONLINE START DELAY --------------------

    public IEnumerator ShowBoardAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        uiManager.ShowGameBoard();  // show board
        isGameRunning = true;        // enable clicks
        elapsedTime = 0f;
        UpdateTimerUI();
    }

    // -------------------- CELL CLICK --------------------

    public void OnCellClicked(int index)
    {
        if (!isGameRunning) return;
        if (board[index] != '\0') return;

        if (gameMode == GameMode.PlayOnline)
        {
            webSocketClient.SendMove(index);
            return;
        }

        if (gameMode == GameMode.PassAndPlay)
        {
            MakeMove(index, currentPlayer);
            if (CheckWinForSymbol(currentPlayer)) { EndGame(currentPlayer.ToString()); return; }
            if (IsBoardFull()) { EndDraw(); return; }
            SwitchPlayer();
        }
        else if (gameMode == GameMode.PlayWithComputer)
        {
            MakeMove(index, 'O');
            if (CheckWinForSymbol('O')) { EndGame("O"); return; }
            if (IsBoardFull()) { EndDraw(); return; }
            DisableAllButtons();
            Invoke(nameof(PlayBotMove), 0.4f);
        }
    }

    // -------------------- BOT --------------------

    void PlayBotMove()
    {
        if (!isGameRunning) return;

        int botIndex = botPlayer.GetBotMove(this);
        if (botIndex < 0 || board[botIndex] != '\0') return;

        MakeMove(botIndex, 'X');

        if (CheckWinForSymbol('X')) { EndGame("X"); return; }
        if (IsBoardFull()) { EndDraw(); return; }

        currentPlayer = 'O';
        EnableEmptyButtons();
    }

    void DisableAllButtons()
    {
        for (int i = 0; i < cells.Length; i++)
            cells[i].interactable = false;
    }

    void EnableEmptyButtons()
    {
        for (int i = 0; i < cells.Length; i++)
            if (board[i] == '\0')
                cells[i].interactable = true;
    }

    // -------------------- MOVE --------------------

    void MakeMove(int index, char player)
    {
        if (index < 0 || index >= board.Length) return;
        if (cells == null || index >= cells.Length) return;

        board[index] = player;
        uiManager.PlaceMove(cells[index], player);
        cells[index].interactable = false;
    }

    void SwitchPlayer()
    {
        currentPlayer = (currentPlayer == 'O') ? 'X' : 'O';
    }

    // -------------------- SERVER STATE UPDATE --------------------

    public void UpdateGameState(GameStateMessage state)
    {
        if (board == null || board.Length != 9) board = new char[9];
        if (state.board == null) return;

        int len = Mathf.Min(state.board.Length, board.Length);
        for (int i = 0; i < len; i++)
        {
            if (!string.IsNullOrEmpty(state.board[i]))
            {
                char incoming = state.board[i][0];
                if (board[i] == '\0') MakeMove(i, incoming);
            }
        }

        currentPlayer = state.turn[0];

        if (!string.IsNullOrEmpty(state.winner))
        {
            if (state.winner == "Draw") EndDraw();
            else EndGame(state.winner);
        }
    }

    // -------------------- WIN CHECK --------------------

    public bool CheckWinForSymbol(char symbol)
    {
        int[,] wins =
        {
            {0,1,2},{3,4,5},{6,7,8},
            {0,3,6},{1,4,7},{2,5,8},
            {0,4,8},{2,4,6}
        };

        for (int i = 0; i < wins.GetLength(0); i++)
            if (board[wins[i, 0]] == symbol &&
                board[wins[i, 1]] == symbol &&
                board[wins[i, 2]] == symbol)
                return true;

        return false;
    }

    bool IsBoardFull()
    {
        for (int i = 0; i < board.Length; i++)
            if (board[i] == '\0') return false;
        return true;
    }

    // -------------------- END GAME --------------------

    void EndGame(string winner)
    {
        isGameRunning = false;
        uiManager.ShowWinner(winner, elapsedTime);
    }

    void EndDraw()
    {
        isGameRunning = false;
        uiManager.ShowWinner("Draw", elapsedTime);
    }

    // -------------------- TIMER --------------------

    void UpdateTimerUI()
    {
        int min = Mathf.FloorToInt(elapsedTime / 60);
        int sec = Mathf.FloorToInt(elapsedTime % 60);
        timerText.text = $"Time {min:00}:{sec:00}";
    }

    // -------------------- RESET / RESTART --------------------

    public void RestartGame()
    {
        
        StartGame();
        uiManager.ShowGameBoard();
    }

    public void ResetGameState()
    {
        uiManager.ShowModeSelect();
        StartGame();
    }
}

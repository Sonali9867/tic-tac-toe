using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class GameUIManager : MonoBehaviour
{

    public GameObject gameBoard;

    [Header("Winner Panel")]
    public GameObject WinnerPanel;
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI timeText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip cellClickSound;

    [Header("Timer")]
    public TextMeshProUGUI timerText;
    private float elapsedTime = 0f;

    public TextMeshProUGUI turnText;
    public TextMeshProUGUI messageText;

    void Start()
    {
        ShowGameBoard();

    }
    void Update()
{
    elapsedTime += Time.deltaTime;
    UpdateTimerUI();
}


     public void ShowGameBoard()
    {
       
    elapsedTime = 0f;  
    UpdateTimerUI();

    gameBoard.SetActive(true);
    WinnerPanel.SetActive(false);
    }

    public void PlaceMove(Button button, int player)
    {
        Debug.Log($"PlaceMove called: Button={button.name}, player={player}");

        if (cellClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(cellClickSound);
        }

        // Determine symbol: 1 = O, -1 = X
        string symbol = "";
        string nextTurn = "";
        if (player == 1)
        {
            symbol = "O";
            nextTurn = "X's Turn";
        }
        else if (player == -1)
        {
            symbol = "X";
            nextTurn = "O's Turn";
        }

        // Update turn text
        if (turnText != null)
        {
            turnText.text = nextTurn;
        }

       
        TextMeshProUGUI tmp = button.GetComponent<TextMeshProUGUI>();
        if (tmp == null)
        {
            tmp = button.GetComponentInChildren<TextMeshProUGUI>();
        }
 
       if (tmp != null)
        {
            tmp.text = symbol;
            
        }
        

        button.interactable = false;
    }


    void UpdateTimerUI()
    {
        int min = Mathf.FloorToInt(elapsedTime / 60);
        int sec = Mathf.FloorToInt(elapsedTime % 60);
        timerText.text = $"Time {min:00}:{sec:00}";
    }


  public void ShowWinner(string winner)
    {
       
        gameBoard.SetActive(false);
        WinnerPanel.SetActive(true);
        if (winner == "Draw")
        {
            messageText.text = "OOps!"; 
            winnerText.text = "It's a Draw!";
        }
        else
        {
            messageText.text = "Congratulations!";
            winnerText.text = "Winner is " + winner;

        }

        int minutes = Mathf.FloorToInt(elapsedTime/ 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);

        timeText.text = "Total Time : " +
                        minutes.ToString("00") + ":" +
                        seconds.ToString("00") + " seconds";
    }

 
    public void ShowWinnerWithMessage(string winner, string customMessage)
    {
        gameBoard.SetActive(false);
        WinnerPanel.SetActive(true);
        
        messageText.text = customMessage;
        winnerText.text = "Winner is " + winner;

        int minutes = Mathf.FloorToInt(elapsedTime/ 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);

        timeText.text = "Total Time : " +
                        minutes.ToString("00") + ":" +
                        seconds.ToString("00") + " seconds";
    }

public void ClearCell(Button button)
{
    if (button == null) return;

    TextMeshProUGUI tmp = button.GetComponentInChildren<TextMeshProUGUI>();
    if (tmp != null)
    {
        tmp.text = "";
    }

    button.interactable = true;
}

    [Header("Win highlight")]
    public Color winningLineColor = new Color(0.3f, 1f, 0.3f);
    public float winningHighlightDelay = 1.5f;

    /// <summary>
    /// Highlights the winning line of cells with a color (e.g. green).
    /// </summary>
    public void HighlightWinningCells(Button[] cells, int[] winningIndices, Color highlightColor)
    {
        if (cells == null || winningIndices == null) return;
        foreach (int idx in winningIndices)
        {
            if (idx >= 0 && idx < cells.Length && cells[idx] != null)
            {
                Image img = cells[idx].GetComponent<Image>();
                if (img != null)
                    img.color = highlightColor;
                TextMeshProUGUI tmp = cells[idx].GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                    tmp.color = Color.white;
            }
        }
    }

    /// <summary>
    /// Shows winner panel after highlighting the winning line (called by BoardManager coroutine).
    /// </summary>
    public void ShowWinnerAfterHighlight(string winner)
    {
        gameBoard.SetActive(false);
        WinnerPanel.SetActive(true);
        if (winner == "Draw")
        {
            messageText.text = "OOps!";
            winnerText.text = "It's a Draw!";
        }
        else
        {
            messageText.text = "Congratulations!";
            winnerText.text = "Winner is " + winner;
        }
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        timeText.text = "Total Time : " + minutes.ToString("00") + ":" + seconds.ToString("00") + " seconds";
    }


public void OnBackPressed()
{
    // Check if we're in online mode
    BoardManager boardManager = FindObjectOfType<BoardManager>();
    if (boardManager != null && boardManager.gameMode == BoardManager.GameMode.PlayOnline)
    {
        ExitOnlineGame();
    }
    else
    {

        NavigationManager.Instance.GoBack();
    }
}


public void ExitOnlineGame()
{
    Debug.Log("Exiting online game - disconnecting from server");
    
   
    if (WebSocketClient.Instance != null)
    {
        WebSocketClient.Instance.DisconnectFromServer();
    }
 
    UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
}

public void QuitGame()
{
   
    if (WebSocketClient.Instance != null)
    {
        WebSocketClient.Instance.DisconnectFromServer();
    }
    
    Application.Quit();
}

public void OnClickCancel()
    {
       SceneManager.LoadScene("MenuScene");
    }

    
}

using UnityEngine;

using UnityEngine.UI;  // ← THIS IS REQUIRED FOR Button

using TMPro;

using System.Collections;


public class UIManager : MonoBehaviour

{
    [Header("References")]
public GameManager gameManager;  // drag your GameManager in inspector


    public GameObject mainMenu;
    public GameObject modeSelect;
    public GameObject gameBoard;
    public GameObject matchMakingPanel;

    public WebSocketClient webSocketClient;


    [Header("Matchmaking Images")]
    public Image playerSymbolImage;
    public Image opponentSymbolImage;

    [Header("Sprites")]
    public Sprite xSprite;
    public Sprite oSprite;
    public Sprite questionSprite;



    public GameObject xPrefab;
    public GameObject oPrefab;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip cellClickSound;


    [Header("Winner Panel")]
    public GameObject WinnerPanel;
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI timeText;

    public TextMeshProUGUI messageText;
     public TextMeshProUGUI matchMakingText; 

    // public GameManager gameManager; // assign this in inspector
    

    void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        modeSelect.SetActive(false);
        gameBoard.SetActive(false);
        WinnerPanel.SetActive(false);
    }

    public void ShowModeSelect()
    {
        mainMenu.SetActive(false);
        modeSelect.SetActive(true);
        gameBoard.SetActive(false);
        WinnerPanel.SetActive(false);
    }

    public void ShowMatchMakingPanel()
    {
        mainMenu.SetActive(false);
        modeSelect.SetActive(false);
        gameBoard.SetActive(false);
        matchMakingPanel.SetActive(true);
        WinnerPanel.SetActive(false);

    // if (matchMakingText != null)
    //     matchMakingText.text = message;

    }

    // public void OnCancelMatchMaking()
    // {
    //     ShowMainMenu();
    //     webSocketClient.CancelMatchmaking();
        
    // }

    public void ShowGameBoard()
    {
        mainMenu.SetActive(false);
        modeSelect.SetActive(false);
        matchMakingPanel.SetActive(false);
        gameBoard.SetActive(true);
        WinnerPanel.SetActive(false);
    }



    // Called when player clicks Pass & Play button


    public void ShowWinner(string winner, float time)
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

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        timeText.text = "Total Time : " +
                        minutes.ToString("00") + ":" +
                        seconds.ToString("00") + " seconds";
    }

    public void PlaceMove(Button button, char player)
    {
        Debug.Log("Button " + button);
        // Play click sound
        if (cellClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(cellClickSound);
        }

        
        GameObject prefab = player == 'X' ? xPrefab : oPrefab;

        // Instantiate prefab on top of the button
        GameObject moveImage = Instantiate(prefab, button.transform);

        // Center it and keep normal scale
        RectTransform rt = moveImage.GetComponent<RectTransform>();
        rt.localPosition = Vector3.zero;
        rt.localScale = Vector3.one;     

        float scale = 0.5f; 
        rt.sizeDelta = button.GetComponent<RectTransform>().sizeDelta * scale;


        // Disable button
        button.interactable = false;
    }



public void ShowWaiting(char mySymbol)
    {
        matchMakingPanel.SetActive(true);

        // Text
        matchMakingText.text = "Finding opponent...";

        // Player symbol
        playerSymbolImage.sprite = (mySymbol == 'X') ? xSprite : oSprite;
        playerSymbolImage.enabled = true;

        // Opponent unknown
        opponentSymbolImage.sprite = questionSprite;
        opponentSymbolImage.enabled = true;
    }

    // Matched state
    public void ShowMatched(char mySymbol, char opponentSymbol)
    {
        matchMakingPanel.SetActive(true);

       matchMakingText.text = "Match Found!";

        playerSymbolImage.sprite = (mySymbol == 'X') ? xSprite : oSprite;
        opponentSymbolImage.sprite = (opponentSymbol == 'X') ? xSprite : oSprite;
    }

public void ShowMatchedAndStart(char mySymbol, char opponentSymbol, float delay = 2f)
{
    ShowMatched(mySymbol, opponentSymbol);
    StartCoroutine(gameManager.ShowBoardAfterDelay(delay)); // Start board after delay
}



IEnumerator StartGameAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    ShowGameBoard();
}




    public void ClearCell(Button btn)
    {
        
        // If you use images instead, clear image here
    }






    public void ExitGame()
    {
        Application.Quit();
    }
}

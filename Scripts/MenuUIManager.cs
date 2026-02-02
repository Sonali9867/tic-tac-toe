using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuUIManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject modeSelect;
    public GameObject matchmaking;

    [Header("Matchmaking - Player vs Opponent")]
    public Image playerSymbolImage;
    public Image opponentSymbolImage;
    public Image playerSymbolChildImage;
    public Image opponentSymbolChildImage;
    public TextMeshProUGUI matchmakingStatusText;

    [Header("Symbol Sprites (assign in Inspector)")]
    public Sprite oSprite;
    public Sprite xSprite;
    public Sprite questionSprite;

    void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        modeSelect.SetActive(false);
        matchmaking.SetActive(false);
    }

    public void ShowModeSelect()
    {
        NavigationManager.Instance.PushState(ShowMainMenu);
        mainMenu.SetActive(false);
        modeSelect.SetActive(true);
        matchmaking.SetActive(false);
    }

    public void ShowMatchmaking()
    {
        mainMenu.SetActive(false);
        modeSelect.SetActive(false);
        matchmaking.SetActive(true);
        ShowFindingMatch();
    }

    /// <summary>
    /// While waiting for opponent: show ? for both you and opponent.
    /// </summary>
    public void ShowFindingMatch()
    {
        if (matchmakingStatusText != null)
            matchmakingStatusText.text = "Finding opponent...";
        SetChildSprite(playerSymbolChildImage, questionSprite);
        SetChildSprite(opponentSymbolChildImage, questionSprite);
    }

    /// <summary>
    /// Match found: show your symbol (O or X) and opponent's symbol.
    /// </summary>
    public void ShowMatchFound(int mySymbol)
    {
        if (matchmakingStatusText != null)
            matchmakingStatusText.text = "Match Found!";
        SetChildSprite(playerSymbolChildImage, (mySymbol == 1) ? oSprite : xSprite);
        SetChildSprite(opponentSymbolChildImage, (mySymbol == 1) ? xSprite : oSprite);
    }

    /// <summary>
    /// Assigns sprite to child Image and ensures it is visible (enabled, white color).
    /// </summary>
    private void SetChildSprite(Image childImage, Sprite sprite)
    {
        if (childImage == null) return;
        childImage.sprite = sprite;
        childImage.enabled = true;
        childImage.color = Color.white;
    }

    public void OnClickStart()
    {
        ShowModeSelect();
    }

    public void PlayPassAndPlay()
    {
        NavigationManager.Instance.PushState(() =>
        SceneManager.LoadScene("MenuScene")
    );
        GameSettings.SelectedMode = BoardManager.GameMode.PassAndPlay;
        SceneManager.LoadScene("GameScene");
    }

    public void PlayWithComputer()
    {
        NavigationManager.Instance.PushState(() =>
        SceneManager.LoadScene("MenuScene")
    );
        GameSettings.SelectedMode = BoardManager.GameMode.PlayWithComputer;
        SceneManager.LoadScene("GameScene");
    }

    public void PlayOnline()
    {
        GameSettings.SelectedMode = BoardManager.GameMode.PlayOnline;
        WebSocketClient.Instance.ConnectToServer();
        ShowMatchmaking();

    }

    public void OnBackPressed()
    {
        NavigationManager.Instance.GoBack();
    }


    public void QuitGame()
    {
        Application.Quit();
    }

    public void OnClickCancelButton()
{
    if (WebSocketClient.Instance != null)
    {
        WebSocketClient.Instance.OnClickCancelButton();
    }
}

}

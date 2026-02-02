[System.Serializable]
public class GameStateMessage
{
    public string type;
    public string[] board;
    public string turn;
    public string playerId; 
    public string winner;
}

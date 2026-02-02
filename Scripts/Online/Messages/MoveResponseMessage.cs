[System.Serializable]
public class MoveResponseMessage
{
    public string type;
    public int cell;
    public bool valid;
    public string turn;
    public string winner;
    public string playerId;
    public int symbol;
}

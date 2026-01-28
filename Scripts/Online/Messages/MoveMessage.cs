[System.Serializable]
public class MoveMessage
{
    public string type;
    public int cell;   // ⚠️ NOT cellIndex
}

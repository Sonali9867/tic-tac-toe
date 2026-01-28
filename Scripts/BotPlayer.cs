using UnityEngine;

public class BotPlayer : MonoBehaviour
{
    public int GetBotMove(GameManager gm)
    {
        char[] board = gm.board;

        // 1️⃣ Try to WIN
        for (int i = 0; i < 9; i++)
        {
            if (board[i] == '\0')
            {
                board[i] = 'X';
                if (gm.CheckWinForSymbol('X'))
                {
                    board[i] = '\0';
                    return i;
                }
                board[i] = '\0';
            }
        }

        // 2️⃣ Block PLAYER
        for (int i = 0; i < 9; i++)
        {
            if (board[i] == '\0')
            {
                board[i] = 'O';
                if (gm.CheckWinForSymbol('O'))
                {
                    board[i] = '\0';
                    return i;
                }
                board[i] = '\0';
            }
        }

        // 3️⃣ Center
        if (board[4] == '\0') return 4;

        // 4️⃣ Random
        return GetRandomMove(board);
    }

    int GetRandomMove(char[] board)
    {
        int index;
        do
        {
            index = Random.Range(0, 9);
        }
        while (board[index] != '\0');

        return index;
    }
}

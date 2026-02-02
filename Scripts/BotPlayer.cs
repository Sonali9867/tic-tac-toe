using UnityEngine;

public class BotPlayer : MonoBehaviour
{
    public int GetBotMove(BoardManager boardManager)
    {
        int[] board = boardManager.board;

        // 1️Try to WIN (X = -1)
        for (int i = 0; i < 9; i++)
        {
            if (board[i] == 0)
            {
                board[i] = -1;
                if (boardManager.CheckWinForSymbol(-1, i, 3))
                {
                    board[i] = 0;
                    return i;
                }
                board[i] = 0;
            }
        }

        // 2️ Block PLAYER (O = 1)
        for (int i = 0; i < 9; i++)
        {
            if (board[i] == 0)
            {
                board[i] = 1;
                if (boardManager.CheckWinForSymbol(1, i, 3))
                {
                    board[i] = 0;
                    return i;
                }
                board[i] = 0;
            }
        }

        // 3️ Center
        if (board[4] == 0)
            return 4;

        // 4️ Random
        return GetRandomMove(board);
    }

    int GetRandomMove(int[] board)
    {
        int index;
        do
        {
            index = Random.Range(0, 9);
        }
        while (board[index] != 0);

        return index;
    }
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 这是生成数独的类
/// </summary>
public class Generator
{
    public enum DifficultyLevel
    {
        EASY,
        MEDIUM,
        DIFFICULT
    }

    private const int GRID_SIZE = 9; //数独网格大小（9x9）
    private const int SUBGRID_SIZE = 3; //子网格大小（3x3）
    private const int BOARD_SIZE = 9; //棋盘大小（9）
    private const int MIN_SQUARES_REMOVED = 10; //最小需要移除的方块数
    private const int MAX_SQUARED_REMOVED = 50; //最大需要移除的方块数

    /// <summary>
    /// 生成数独
    /// </summary>
    /// <param name="difficultyLevel"></param>
    /// <returns></returns>
    public static int[,] GeneratePuzzle(int squaresToRemove)
    {
        var grid = new int[GRID_SIZE, GRID_SIZE];

        InitializeGrid(grid);
        RemoveSquares(grid, squaresToRemove);

        return grid;
    }

    /// <summary>
    /// 移除随机方块
    /// </summary>
    /// <param name="grid">传入的数独网格</param>
    /// <param name="difficultyLevel">难易程度</param>
    private static void RemoveSquares(int[,] grid, int squaresToRemove)
    {
        while (squaresToRemove > 0)
        {
            int randRow = Random.Range(0, BOARD_SIZE);
            int randCol = Random.Range(0, BOARD_SIZE);

            if (grid[randRow, randCol] != 0)
            {
                int temp = grid[randRow, randCol];
                grid[randRow, randCol] = 0;

                if (Solver.HasUniqueSolution(grid))
                {
                    squaresToRemove--;
                }
                else
                {
                    grid[randRow, randCol] = temp;
                }
            }
        }
    }

    /// <summary>
    /// 初始化数独
    /// </summary>
    /// <param name="grid"></param>
    public static void InitializeGrid(int[,] grid)
    {
        List<int> numbers = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        Shuffle(numbers);
        for (int i = 0; i < GRID_SIZE; i++)
        {
            grid[0, i] = numbers[i];
        }
        FillGrid(1, 0, grid);
    }

    /// <summary>
    /// 填充数独
    /// </summary>
    /// <param name="r"></param>
    /// <param name="c"></param>
    /// <param name="grid"></param>
    /// <returns></returns>
    private static bool FillGrid(int r, int c, int[,] grid)
    {
        if (r == GRID_SIZE)
        {
            return true;
        }
        if (c == GRID_SIZE)
        {
            return FillGrid(r + 1, 0, grid);
        }

        List<int> numbers = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        Shuffle(numbers);
        foreach (var num in numbers)
        {
            if (IsValid(num, r, c, grid))
            {
                grid[r, c] = num;

                if (FillGrid(r, c + 1, grid))
                {
                    return true;
                }
            }
        }

        grid[r, c] = 0;
        return false;
    }

    /// <summary>
    /// 判断是否合法
    /// </summary>
    /// <param name="val"></param>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <param name="board"></param>
    /// <returns></returns>
    private static bool IsValid(int val, int row, int col, int[,] board)
    {

        for (int i = 0; i < BOARD_SIZE; i++)
        {
            if (board[row, i] == val)
            {
                return false;
            }
        }

        for (int i = 0; i < BOARD_SIZE; i++)
        {
            if (board[i, col] == val)
            {
                return false;
            }
        }

        int subGridRow = row / SUBGRID_SIZE * SUBGRID_SIZE;
        int subGridCol = col / SUBGRID_SIZE * SUBGRID_SIZE;
        for (int r = subGridRow; r < subGridRow + SUBGRID_SIZE; r++)
        {
            for (int c = subGridCol; c < subGridCol + SUBGRID_SIZE; c++)
            {
                if (board[r, c] == val)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 随机打乱列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    private static void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n);
            T temp = list[k];
            list[k] = list[n];
            list[n] = temp;
        }
    }
}

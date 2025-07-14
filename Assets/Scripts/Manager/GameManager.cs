using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("基本设置")]
    [SerializeField] private Vector3 _startPos; //起始位置
    [SerializeField] private float _offsetX, _offsetY; //每个格子的偏移量
    [SerializeField] private SubGrid _subGridPrefab; //子网格预制体
    [SerializeField] private TMP_Text _currentSsquaresToRemoveText; //移除的格子数量显示
    [Range(1, 64)] private static int _squaresToRemove = 20; //当前移除的格子数量
    [SerializeField] private Slider _needSquaresToRemoveSlider; //需要移除的格子数量滑动条
    [SerializeField] private TMP_Text _needSquaresToRemoveText; //移除的格子数量显示
    [SerializeField] private bool _isSaved; //是否保存

    [Header("回溯算法设置")]
    [SerializeField] private Button _solveButton; //回溯求解/暂停按钮
    [SerializeField] private Button _clearButton; //清空未锁定数字按钮
    [Range(0.01f, 3f)] private static float _stepDelay = 0.5f; //每一步的延迟时间
    [SerializeField] private TMP_Text _solveButtonText; //按钮文本
    [SerializeField] private Slider _solutionSpeedSlider; //求解速度滑动条
    [SerializeField] private TMP_Text _solutionSpeedText; //求解速度文本

    [Header("需要禁用的按钮")]
    [SerializeField] private Button[] _disabledButtons; //需要禁用的按钮
    [SerializeField] private Color _disabledColor; //禁用颜色

    [Header("声音设置")]
    [SerializeField] private Button _soundButton; //音效按钮
    [SerializeField] private GameObject _disableSound;
    [SerializeField] private GameObject _enableSound;
    [SerializeField] private Button _musicButton; //音乐按钮
    [SerializeField] private GameObject _disableMusic;
    [SerializeField] private GameObject _enableMusic;

    private bool _hasGameFinished; //游戏是否结束
    private Cell[,] _cells; //格子数组
    public Cell selectedCell; //选中的格子
    public Cell lastSelectedCell; //上一次选中的格子
    private StringBuilder stringBuilder = new StringBuilder(20); // 预分配内存
    private bool _isSolving = false; //是否正在求解
    private bool _isPaused = false; //是否暂停求解

    private const int GRID_SIZE = 9; //数独网格大小（9x9）
    private const int SUBGRID_SIZE = 3; //子网格大小（3x3）

    private void Awake()
    {
        _hasGameFinished = false;
        _cells = new Cell[GRID_SIZE, GRID_SIZE];
        selectedCell = null;
        _solveButtonText = _solveButton.GetComponentInChildren<TMP_Text>();

        //初始化滑动条，显示当前移除的格子数量
        _needSquaresToRemoveSlider.value = _squaresToRemove;
        _needSquaresToRemoveSlider.onValueChanged.AddListener(SetSquaresToRemove);

        //初始化求解速度滑动条，显示当前求解速度
        _solutionSpeedSlider.value = _stepDelay;
        _solutionSpeedSlider.onValueChanged.AddListener(SetSoulutionSpeed);

        string formatted = string.Format("{0:F2}", _stepDelay);
        _solutionSpeedText.text = "Current Solutin Speed: " + formatted + "s";

        //初始化当前移除的格子数量显示
        UpdateCurrentRemoveSquareText(_squaresToRemove);
        _currentSsquaresToRemoveText.text = "Current Remove Square(s): " + _squaresToRemove.ToString();

        SpawnCells();

        //初始化音效按钮
        _disableSound.SetActive(!SoundsManager.Instance.isSoundOn);
        _enableSound.SetActive(SoundsManager.Instance.isSoundOn);

        _soundButton.onClick.AddListener(() =>
        {
            SoundsManager.Instance.isSoundOn = !SoundsManager.Instance.isSoundOn;
            _disableSound.SetActive(!SoundsManager.Instance.isSoundOn);
            _enableSound.SetActive(SoundsManager.Instance.isSoundOn);
        });

        //初始化音乐按钮
        _disableMusic.SetActive(!MusicManager.Instance.isMusicOn);
        _enableMusic.SetActive(MusicManager.Instance.isMusicOn);

        _musicButton.onClick.AddListener(() =>
        {
            MusicManager.Instance.SiwtchMusicState();
            _disableMusic.SetActive(!MusicManager.Instance.isMusicOn);
            _enableMusic.SetActive(MusicManager.Instance.isMusicOn);
        });
    }

    private void OnDisable()
    {
        _needSquaresToRemoveSlider.onValueChanged.RemoveListener(SetSquaresToRemove);
        _solutionSpeedSlider.onValueChanged.RemoveListener(SetSoulutionSpeed);
        StopAllCoroutines();
    }

    private void Update()
    {
        if (_hasGameFinished || _isSolving || !Input.GetMouseButton(0)) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);//获取鼠标在屏幕上的位置，转换为世界坐标
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);//转换为2D坐标
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero); //射线检测
        Cell tempCell; //临时格子

        //尝试获取射线击中的格子
        if (hit
            && hit.collider.gameObject.TryGetComponent(out tempCell)
            && tempCell != selectedCell
            && !tempCell.IsLocked
            )
        {
            ResetGrid();
            selectedCell = tempCell;
            HighLight();
        }
    }

    public void SetSquaresToRemove(float value)
    {
        UpdateCurrentRemoveSquareText(value);
        _squaresToRemove = (int)value;
    }

    private void UpdateCurrentRemoveSquareText(float value)
    {
        stringBuilder.Clear();
        stringBuilder.Append("Current Remove Square(s): ");
        stringBuilder.Append((int)value);
        _needSquaresToRemoveText.SetText(stringBuilder); //使用 StringBuilder 减少 GC
    }

    public void SetSoulutionSpeed(float value)
    {
        UpdateSolutinSpeedText(value);
        _stepDelay = value;
    }

    private void UpdateSolutinSpeedText(float value)
    {
        stringBuilder.Clear();
        stringBuilder.Append("Current Solutin Speed: ");

        string formatted = string.Format("{0:F2}", value);
        stringBuilder.Append(formatted);
        stringBuilder.Append("s");
        _solutionSpeedText.SetText(stringBuilder); //使用 StringBuilder 减少 GC
    }

    /// <summary>
    /// 生成格子
    /// </summary>
    private void SpawnCells()
    {
        int[,] puzzleGrid = new int[GRID_SIZE, GRID_SIZE];
        int squaresToRemove = _isSaved ? PlayerPrefs.GetInt("SquaresToRemove", 0) : _squaresToRemove;

        if (_isSaved)
        {
            squaresToRemove = PlayerPrefs.GetInt("SquaresToRemove", 0);
            LoadGameData(puzzleGrid, out squaresToRemove); //当开启了保存功能时，从PlayerPrefs中获取当前关卡
        }
        else
        {
            squaresToRemove = _squaresToRemove; //当关闭了保存功能时，使用静态变量临时存储
            Create(puzzleGrid, squaresToRemove);
        }

        _needSquaresToRemoveText.text = "Current Remove Square(s): " + squaresToRemove.ToString();

        //生成格子网格
        for (int i = 0; i < GRID_SIZE; i++)
        {
            Vector3 spawnPos = _startPos + i % 3 * _offsetX * Vector3.right + i / 3 * _offsetY * Vector3.up;
            SubGrid subGrid = Instantiate(_subGridPrefab, spawnPos, Quaternion.identity);
            List<Cell> subgridCells = subGrid.cells;
            int startRow = (i / 3) * 3;
            int startCol = (i % 3) * 3;
            for (int j = 0; j < GRID_SIZE; j++)
            {
                subgridCells[j].Row = startRow + j / 3;
                subgridCells[j].Col = startCol + j % 3;
                int cellValue = puzzleGrid[subgridCells[j].Row, subgridCells[j].Col];
                subgridCells[j].Init(cellValue);
                _cells[subgridCells[j].Row, subgridCells[j].Col] = subgridCells[j];
            }
        }
    }

    /// <summary>
    /// 创建游戏
    /// </summary>
    /// <param name="grid">格子数组</param>
    /// <param name="level">等级</param>
    private void Create(int[,] grid, int squaresToRemove)
    {
        int[,] tempGrid = Generator.GeneratePuzzle(squaresToRemove);
        string arrayString = "";
        for (int i = 0; i < GRID_SIZE; i++)
        {
            for (int j = 0; j < GRID_SIZE; j++)
            {
                arrayString += tempGrid[i, j].ToString() + ",";
                grid[i, j] = tempGrid[i, j];
            }
        }

        arrayString = arrayString.TrimEnd(','); //以字符串的形式存储格子数组

        //当开启了保存功能时，存储游戏数据，否则，使用静态变量临时存储
        if (_isSaved) SaveGameData(squaresToRemove, arrayString);
        else
        {
            SetSquaresToRemove(squaresToRemove);
        }
    }

    /// <summary>
    /// 存储游戏数据
    /// </summary>
    /// <param name="_squaresToRemove">等级</param>
    /// <param name="arrayString">格子数组</param>
    private void SaveGameData(int _squaresToRemove, string arrayString)
    {
        PlayerPrefs.SetInt("SquaresToRemove", _squaresToRemove); //存储等级
        PlayerPrefs.SetString("Grid", arrayString); //存储格子数组
    }

    /// <summary>
    /// 加载游戏数据
    /// </summary>
    /// <param name="grid">格子数组</param>
    /// <param name="_squaresToRemove">移除的格子数量</param>
    private void LoadGameData(int[,] grid, out int _squaresToRemove)
    {
        string arrayString = PlayerPrefs.GetString("Grid");
        _squaresToRemove = PlayerPrefs.GetInt("SquaresToRemove");

        string[] arrayValue = arrayString.Split(',');
        int index = 0;
        for (int i = 0; i < GRID_SIZE; i++)
        {
            for (int j = 0; j < GRID_SIZE; j++)
            {
                grid[i, j] = int.Parse(arrayValue[index]);
                index++;
            }
        }

    }

    /// <summary>
    /// 重置格子
    /// </summary>
    private void ResetGrid()
    {
        for (int i = 0; i < GRID_SIZE; i++)
        {
            for (int j = 0; j < GRID_SIZE; j++)
            {
                _cells[i, j].Reset();
            }
        }
    }

    /// <summary>
    /// 更新格子的值，通过外部绑定按钮来调用
    /// </summary>
    /// <param name="value"></param>
    public void UpdateCellValue(int value)
    {
        if (_hasGameFinished || selectedCell == null) return;
        selectedCell.UpdateValue(value);
        HighLight();
        CheckWin();
    }

    /// <summary>
    /// 检查是否胜利
    /// </summary>
    private void CheckWin()
    {
        for (int i = 0; i < GRID_SIZE; i++)
        {
            for (int j = 0; j < GRID_SIZE; j++)
            {
                if (_cells[i, j].IsIncorrect || _cells[i, j].Value == 0) return;
            }
        }

        _hasGameFinished = true;

        for (int i = 0; i < GRID_SIZE; i++)
        {
            for (int j = 0; j < GRID_SIZE; j++)
            {
                _cells[i, j].UpdateWin();
            }
        }

    }

    /// <summary>
    /// 高亮显示
    /// </summary>
    private void HighLight()
    {
        //更新所有单元格的状态
        for (int i = 0; i < GRID_SIZE; i++)
        {
            for (int j = 0; j < GRID_SIZE; j++)
            {
                _cells[i, j].IsIncorrect = !IsValid(_cells[i, j], _cells);
            }
        }

        int currentRow = selectedCell.Row;
        int currentCol = selectedCell.Col;
        int subGridRow = currentRow - currentRow % SUBGRID_SIZE;
        int subGridCol = currentCol - currentCol % SUBGRID_SIZE;

        for (int i = 0; i < GRID_SIZE; i++)
        {
            _cells[i, currentCol].HighLight();
            _cells[currentRow, i].HighLight();
            _cells[subGridRow + i % 3, subGridCol + i / 3].HighLight();
        }

        _cells[currentRow, currentCol].Select(this);
    }

    /// <summary>
    /// 判断是否有效
    /// </summary>
    /// <param name="cell">格子</param>
    /// <param name="cells">格子数组</param>
    /// <returns></returns>
    private bool IsValid(Cell cell, Cell[,] cells)
    {
        int row = cell.Row;
        int col = cell.Col;
        int value = cell.Value;
        cell.Value = 0;

        if (value == 0) return true;

        for (int i = 0; i < GRID_SIZE; i++)
        {
            if (cells[row, i].Value == value || cells[i, col].Value == value)
            {
                cell.Value = value;
                return false;
            }
        }

        int subGridRow = row - row % SUBGRID_SIZE;
        int subGridCol = col - col % SUBGRID_SIZE;

        for (int r = subGridRow; r < subGridRow + SUBGRID_SIZE; r++)
        {
            for (int c = subGridCol; c < subGridCol + SUBGRID_SIZE; c++)
            {
                if (cells[r, c].Value == value)
                {
                    cell.Value = value;
                    return false;
                }
            }
        }

        cell.Value = value;
        return true;
    }

    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    #region 回溯算法 求解

    /// <summary>
    /// 暂停/继续求解
    /// </summary>
    public void Solve()
    {
        if (!_isSolving && !_isPaused) //如果没有正在求解且没有暂停，则是第一次按下求解按钮，开始求解
        {
            StartVisualSolve();
        }
        else
        {
            _isPaused = !_isPaused;
            _solveButtonText.text = _isPaused ? "Resume" : "Pause";
        }
    }

    private void DisabledButton(Button button)
    {
        button.interactable = false;
        button.GetComponent<Image>().color = _disabledColor;

        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();

        if (text != null)
            text.color = new Color(text.color.r, text.color.g, text.color.b, _disabledColor.a);
    }

    /// <summary>
    /// 开始可视化求解
    /// </summary>
    public void StartVisualSolve()
    {
        ClearUnlockedCells();
        if (_isSolving) return;

        _isSolving = true;
        _isPaused = false;
        _solveButtonText.text = "Pause";
        _solveButton.interactable = true;
        _clearButton.onClick.RemoveAllListeners();

        foreach (var button in _disabledButtons)
        {
            DisabledButton(button);
        }

        //创建一个副本用于回溯求解
        int[,] puzzleGrid = new int[GRID_SIZE, GRID_SIZE];
        for (int i = 0; i < GRID_SIZE; i++)
        {
            for (int j = 0; j < GRID_SIZE; j++)
            {
                puzzleGrid[i, j] = _cells[i, j].Value;
            }
        }

        StartCoroutine(VisualBacktrackSolve(puzzleGrid, 0, 0));
    }

    /// <summary>
    /// 可视化回溯求解协程
    /// </summary>
    /// <param name="grid">网格数组</param>
    /// <param name="row">当前行</param>
    /// <param name="col">当前列</param>
    /// <returns></returns>
    private IEnumerator VisualBacktrackSolve(int[,] grid, int row, int col)
    {
        if (row == GRID_SIZE)
        {
            //求解完成
            FinishSolving();
            yield break;
        }

        //检查暂停状态
        while (_isPaused)
        {
            Debug.Log("暂停");
            yield return null;
        }

        if (col == GRID_SIZE)
        {
            yield return VisualBacktrackSolve(grid, row + 1, 0);
            yield break;
        }

        //如果单元格已锁定，跳过
        if (_cells[row, col].IsLocked)
        {
            yield return VisualBacktrackSolve(grid, row, col + 1);
            yield break;
        }

        //尝试1-9的数字
        for (int num = 1; num <= 9; num++)
        {
            //检查暂停状态
            while (_isPaused)
            {
                Debug.Log("暂停");
                yield return null;
            }

            if (IsValidForBacktrack(grid, row, col, num))
            {
                //更新UI
                _cells[row, col].UpdateValue(num);
                ResetGrid();
                selectedCell = _cells[row, col];
                HighLight();

                //等待一段时间以便观察
                yield return new WaitForSeconds(_stepDelay);

                grid[row, col] = num;

                //递归尝试下一个单元格
                yield return VisualBacktrackSolve(grid, row, col + 1);


                Debug.Log("Backtrack: " + row + ", " + col + ", " + num);
                //如果已经解决，提前返回
                if (_hasGameFinished) yield break;

                //检查暂停状态
                while (_isPaused)
                {
                    Debug.Log("暂停");
                    yield return null;
                }

                //回溯
                if (!_cells[row, col].IsLocked)
                {
                    _cells[row, col].UpdateValue(0);
                    ResetGrid();
                    selectedCell = _cells[row, col];
                    HighLight();
                    yield return new WaitForSeconds(_stepDelay);
                }
                grid[row, col] = 0;
            }
        }
    }

    /// <summary>
    /// 用于回溯算法的验证方法
    /// </summary>
    private bool IsValidForBacktrack(int[,] grid, int row, int col, int num)
    {
        //检查行
        for (int i = 0; i < GRID_SIZE; i++)
        {
            if (grid[row, i] == num) return false;
        }

        //检查列
        for (int i = 0; i < GRID_SIZE; i++)
        {
            if (grid[i, col] == num) return false;
        }

        //检查3x3子网格
        int subGridRow = row - row % SUBGRID_SIZE;
        int subGridCol = col - col % SUBGRID_SIZE;
        for (int i = subGridRow; i < subGridRow + SUBGRID_SIZE; i++)
        {
            for (int j = subGridCol; j < subGridCol + SUBGRID_SIZE; j++)
            {
                if (grid[i, j] == num) return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 完成求解后的清理工作
    /// </summary>
    private void FinishSolving()
    {
        _isSolving = false;
        _isPaused = false;
        _solveButtonText.text = "Finished";
        DisabledButton(_solveButton);

        StopAllCoroutines();
        CheckWin();
    }

    #endregion

    /// <summary>
    /// 清空所有未锁定的单元格
    /// </summary>
    public void ClearUnlockedCells()
    {
        if (_isSolving || _hasGameFinished) return;

        for (int i = 0; i < GRID_SIZE; i++)
        {
            for (int j = 0; j < GRID_SIZE; j++)
            {
                if (!_cells[i, j].IsLocked)
                {
                    _cells[i, j].UpdateValue(0);
                    _cells[i, j].IsIncorrect = false;
                }
            }
        }

        Debug.Log("清空所有未锁定的单元格");
        ResetGrid();
        selectedCell = null;
    }
}

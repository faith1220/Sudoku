using TMPro;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [HideInInspector] public int Value; //数字
    [HideInInspector] public int Row; //行
    [HideInInspector] public int Col; //列
    [HideInInspector] public bool IsLocked; //是否锁定
    [HideInInspector] public bool IsIncorrect; //是否错误

    [Header("资源设置")]
    [SerializeField] private SpriteRenderer _bgSrpite; //背景图片
    [SerializeField] private TMP_Text _valueText; //显示数字的文本框

    [Header("初始状态")]
    [SerializeField] private Sprite _startLockedCellSrpite; //初始网格图片
    [SerializeField] private Color _startLockedCellColor;
    [SerializeField] private Color _startLockedTextColor;
    [SerializeField] private Sprite _startUnLockedCellSrpite; //初始网格图片
    [SerializeField] private Color _startUnlockedCellColor;
    [SerializeField] private Color _startUnlockedTextColor;

    [Space]
    [Header("高亮状态")]
    [SerializeField] private Sprite _highlightLockedCellSrpite; //高亮网格图片
    [SerializeField] private Color _highlightLockedCellColor;
    [SerializeField] private Color _highlightLockedTextColor;
    [SerializeField] private Sprite _highlightUnlockedCellSrpite; //高亮网格图片
    [SerializeField] private Color _highlightUnlockedCellColor;
    [SerializeField] private Color _highlightUnlockedTextColor;
    [SerializeField] private Color _highlightWrongCellColor;
    [SerializeField] private Color _highlightWrongTextColor;
    [Space]
    [Header("选中状态")]
    [SerializeField] private Sprite _selectedCellSrpite; //选中网格图片
    [SerializeField] private Color _selectedCellColor;
    [SerializeField] private Color _selectedTextColor;
    [SerializeField] private Color _selectedWrongCellColor;
    [SerializeField] private Color _selectedWrongTextColor;
    [Space]
    [Header("重置状态")]
    [SerializeField] private Color _resetCellColor;
    [SerializeField] private Color _resetTextColor;
    [SerializeField] private Color _resetWrongCellColor;
    [SerializeField] private Color _resetWrongTextColor;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="value">数字</param>
    public void Init(int value)
    {
        IsIncorrect = false;
        Value = value;

        if (value == 0)
        {
            IsLocked = false;
            _bgSrpite.sprite = _startUnLockedCellSrpite;
            _bgSrpite.color = _startUnlockedCellColor;
            _valueText.color = _startUnlockedTextColor;
            _valueText.text = "";
        }
        else
        {
            IsLocked = true;
            _bgSrpite.sprite = _startLockedCellSrpite;
            _bgSrpite.color = _startLockedCellColor;
            _valueText.color = _startLockedTextColor;
            _valueText.text = Value.ToString();
        }

    }

    /// <summary>
    /// 高亮
    /// </summary>
    public void HighLight()
    {
        if (IsLocked)
        {
            _bgSrpite.sprite = _highlightLockedCellSrpite;
            _bgSrpite.color = _highlightLockedCellColor;
            _valueText.color = _highlightLockedTextColor;
        }
        else
        {
            _bgSrpite.sprite = _highlightUnlockedCellSrpite;
            if (IsIncorrect)
            {
                _bgSrpite.color = _highlightWrongCellColor;
                _valueText.color = _highlightWrongTextColor;
            }
            else
            {
                _bgSrpite.color = _highlightUnlockedCellColor;
                _valueText.color = _highlightUnlockedTextColor;
            }
        }

    }

    /// <summary>
    /// 选中
    /// </summary>
    public void Select(GameManager game)
    {
        if (game.selectedCell != game.lastSelectedCell)
            SoundsManager.Instance?.PlayCellAudio();
        game.lastSelectedCell = game.selectedCell;

        _bgSrpite.sprite = _selectedCellSrpite;
        if (IsIncorrect)
        {
            _bgSrpite.color = _selectedWrongCellColor;
            _valueText.color = _selectedWrongTextColor;
        }
        else
        {
            _bgSrpite.color = _selectedCellColor;
            _valueText.color = _selectedTextColor;
        }

    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        if (IsLocked)
        {
            _bgSrpite.sprite = _startLockedCellSrpite;
            _bgSrpite.sprite = _startLockedCellSrpite;
            _bgSrpite.color = _startLockedCellColor;
            _valueText.color = _startLockedTextColor;
        }
        else
        {
            _bgSrpite.sprite = _startUnLockedCellSrpite;
            if (IsIncorrect)
            {
                _bgSrpite.color = _resetWrongCellColor;
                _valueText.color = _resetWrongTextColor;
            }
            else
            {
                _bgSrpite.color = _resetCellColor;
                _valueText.color = _resetTextColor;
            }
        }
    }

    /// <summary>
    /// 更新数字
    /// </summary>
    /// <param name="value"></param>
    public void UpdateValue(int value)
    {
        Value = value;
        _valueText.text = Value == 0 ? "" : Value.ToString();
    }

    /// <summary>
    /// 胜利时更新背景颜色
    /// </summary>
    public void UpdateWin()
    {
        _bgSrpite.sprite = _startLockedCellSrpite;
        _bgSrpite.color = _startLockedCellColor;
        _valueText.color = _startLockedTextColor;
    }
}

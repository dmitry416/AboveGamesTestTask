using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Checker : MonoBehaviour
{
    [SerializeField] private Image _checker;
    [SerializeField] private TextMeshProUGUI _mainText;
    [SerializeField] private TextMeshProUGUI _subText;

    public void SetChecker(Sprite icon, Color color, Color subColor)
    {
        _mainText.color = color;
        if (_subText)
            _subText.color = subColor;
        _checker.sprite = icon;
    }
}

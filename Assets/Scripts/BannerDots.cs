using UnityEngine;
using UnityEngine.UI;

public class BannerDots : MonoBehaviour
{
    [SerializeField] private Sprite _activeDotSprite;
    [SerializeField] private Sprite _inactiveDotSprite;
    [SerializeField] private Image _dotPref;

    private Image[] _dots;
    private int _currentActive;

    public void InitializeDots(int count, int activeId = 0)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            count * _activeDotSprite.textureRect.width + (count - 1) * 14
        );
        _currentActive = activeId;
        _dots = new Image[count];

        for (int i = 0; i < count; ++i)
            _dots[i] = Instantiate(_dotPref, transform);

        _dots[_currentActive].sprite = _activeDotSprite;
    }

    public void UpdateActiveDot(int activeId)
    {
        _dots[_currentActive].sprite = _inactiveDotSprite;
        _currentActive = activeId;
        _dots[_currentActive].sprite = _activeDotSprite;
    }
}

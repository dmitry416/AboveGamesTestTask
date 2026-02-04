using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ButtonTransformPair
{
    public CarouselButton button;
    public Transform transform;
}

public enum CarouselButton
{
    All, Odd, Even
}

public class CarouselButtonController : MonoBehaviour
{
    [SerializeField] private Color _selectedTextColor;
    [SerializeField] private Transform _underlinePref;
    [SerializeField] private List<ButtonTransformPair> _buttonPairs = new List<ButtonTransformPair>();
    [SerializeField] private CarouselController _carouselController;

    private Dictionary<CarouselButton, Transform> _buttons;
    private Transform _underline;

    private void Awake()
    {
        _buttons = new Dictionary<CarouselButton, Transform>();
        foreach (var pair in _buttonPairs)
            _buttons[pair.button] = pair.transform;
    }

    private void Start()
    {
        MakeButtonActive(CarouselButton.All);
    }

    private void MakeButtonActive(CarouselButton button)
    {
        if (!_underline)
            _underline = Instantiate(_underlinePref, _buttons[CarouselButton.All]);

        foreach (var item in _buttons)
        {
            TextMeshProUGUI text = item.Value.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.color = (item.Key == button) ? _selectedTextColor : Color.black;
            }
        }

        _underline.transform.SetParent(_buttons[button], false);

        RectTransform underlineRect = _underline.GetComponent<RectTransform>();
        underlineRect.anchoredPosition = new Vector2(0, 0);
        underlineRect.anchorMin = new Vector2(0, 0);
        underlineRect.anchorMax = new Vector2(1, 0);
        underlineRect.pivot = new Vector2(0.5f, 0);
        underlineRect.sizeDelta = new Vector2(0, 2);
    }

    public void CarouselButtonClick(int buttonID)
    {
        CarouselButton button = (CarouselButton)buttonID;
        MakeButtonActive(button);

        if (_carouselController != null)
        {
            _carouselController.SetFilter(button);
        }
    }
}
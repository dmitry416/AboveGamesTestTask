using UnityEngine;
using UnityEngine.UI;

public class Banner : MonoBehaviour
{
    [SerializeField] private Image _bannerImage;
    [SerializeField] private Button _bannerButton;

    public void Setup(Sprite sprite, Button.ButtonClickedEvent clickEvent)
    {
        _bannerImage.sprite = sprite;
        _bannerButton.onClick = clickEvent;
    }
}


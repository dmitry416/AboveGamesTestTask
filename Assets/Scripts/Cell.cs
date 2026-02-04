using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Cell : MonoBehaviour
{
    [HideInInspector] public AudioSource clickSound;
    [HideInInspector] public GameObject premiumPanel;
    [HideInInspector] public GameObject fullPanel;

    [SerializeField] private Image _image;
    [SerializeField] private GameObject _premium;

    private Tween _loadingTween;
    private bool _isLoading = false;
    private Color _originalColor = Color.white;

    private void Start()
    {
        if (_image != null)
        {
            _originalColor = _image.color;
        }
        StartLoadingAnimation();
    }

    public void SetImage(Sprite sprite)
    {
        if (_image != null)
        {
            _image.sprite = sprite;

            if (sprite != null)
            {
                StopLoadingAnimation();
                _image.color = _originalColor;
            }
            else
            {
                _image.color = new Color(0.8f, 0.8f, 0.8f, 1f);
                StartLoadingAnimation();
            }
        }
    }

    public void SetPremium(bool isPremium)
    {
        if (_premium != null)
            _premium.SetActive(isPremium);
    }

    public void SetLoading(bool isLoading)
    {
        if (_isLoading == isLoading)
            return;

        _isLoading = isLoading;

        if (isLoading)
        {
            StartLoadingAnimation();
        }
        else
        {
            StopLoadingAnimation();
            if (_image != null)
                _image.color = _originalColor;
        }
    }

    private void StartLoadingAnimation()
    {
        if (_image == null)
            return;

        if (_loadingTween != null && _loadingTween.IsActive())
            _loadingTween.Kill();

        _loadingTween = _image.DOColor(new Color(0.6f, 0.6f, 0.6f, 1f), 0.8f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopLoadingAnimation()
    {
        if (_loadingTween != null && _loadingTween.IsActive())
        {
            _loadingTween.Kill();
            _loadingTween = null;
        }
    }

    public bool IsLoaded()
    {
        return _image != null && _image.sprite != null;
    }

    private void OnEnable()
    {
        if (_isLoading)
        {
            StartLoadingAnimation();
        }
    }

    private void OnDisable()
    {
        StopLoadingAnimation();
    }

    private void OnDestroy()
    {
        StopLoadingAnimation();
    }

    public void OpenFull()
    {
        if (_image.sprite == null)
            return;
        clickSound.Play();
        if (_premium != null && _premium.activeSelf)
        {
            premiumPanel.SetActive(true);
        }
        fullPanel.GetComponent<Image>().sprite = _image.sprite;
        fullPanel.transform.parent.gameObject.SetActive(true);
    }
}
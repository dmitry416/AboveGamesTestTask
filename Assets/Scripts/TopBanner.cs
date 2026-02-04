using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

[System.Serializable]
public class BannerData
{
    public Sprite image;
    public Button.ButtonClickedEvent onClick;
}

public class TopBanner : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private BannerDots _bannerDots;
    [SerializeField] private Banner _bannerPrefab;
    [SerializeField] private List<BannerData> _banners = new List<BannerData>();
    [SerializeField] private float _autoScrollTime = 5f;
    [SerializeField][Range(0f, 1f)] private float _swipeThreshold = 0.4f;
    [SerializeField] private RectTransform _content;
    [SerializeField] private float _snapDuration = 0.3f;
    [SerializeField] private float _bannerSpacing = 10f;

    private List<Banner> _activeBanners = new List<Banner>();
    private int _currentIndex = 0;
    private float _bannerWidth;
    private Vector2 _dragStartPosition;
    private Vector2 _contentStartPosition;
    private bool _isDragging = false;
    private float _timeSinceLastScroll = 0;
    private Tween _currentTween;

    private const int LEFT_INDEX = 0;
    private const int CENTER_INDEX = 1;
    private const int RIGHT_INDEX = 2;

    private void Start()
    {
        InitializeBanners();
        CalculateBannerWidth();
        UpdateBannersPosition();
        UpdateBannersData();
        if (_bannerDots != null)
            _bannerDots.InitializeDots(_banners.Count);
    }

    private void InitializeBanners()
    {
        if (_banners.Count == 0)
            return;

        foreach (Transform child in _content)
            Destroy(child.gameObject);

        _activeBanners.Clear();

        for (int i = 0; i < 3; i++)
        {
            Banner banner = Instantiate(_bannerPrefab, _content);
            _activeBanners.Add(banner);
        }
    }

    private void CalculateBannerWidth()
    {
        if (_bannerPrefab == null)
            return;

        RectTransform prefabRect = _bannerPrefab.GetComponent<RectTransform>();
        Canvas canvas = GetComponentInParent<Canvas>();
        float canvasWidth = canvas.GetComponent<RectTransform>().rect.width;

        prefabRect.sizeDelta = new Vector2(canvasWidth, prefabRect.sizeDelta.y);
        _bannerWidth = canvasWidth + _bannerSpacing;
    }

    private void UpdateBannersPosition()
    {
        if (_activeBanners.Count != 3)
            return;

        float startX = -_bannerWidth;

        for (int i = 0; i < 3; i++)
        {
            RectTransform bannerRect = _activeBanners[i].GetComponent<RectTransform>();
            bannerRect.anchoredPosition = new Vector2(startX + i * _bannerWidth, 0);
            bannerRect.sizeDelta = new Vector2(_bannerWidth - _bannerSpacing, bannerRect.sizeDelta.y);
        }
    }

    private void UpdateBannersData()
    {
        if (_banners.Count == 0 || _activeBanners.Count != 3)
            return;

        int leftIndex = (_currentIndex - 1 + _banners.Count) % _banners.Count;
        int centerIndex = _currentIndex;
        int rightIndex = (_currentIndex + 1) % _banners.Count;

        _activeBanners[LEFT_INDEX].Setup(_banners[leftIndex].image, _banners[leftIndex].onClick);
        _activeBanners[CENTER_INDEX].Setup(_banners[centerIndex].image, _banners[centerIndex].onClick);
        _activeBanners[RIGHT_INDEX].Setup(_banners[rightIndex].image, _banners[rightIndex].onClick);
    }

    private void Update()
    {
        if (_banners.Count <= 1 || _isDragging)
            return;

        _timeSinceLastScroll += Time.deltaTime;
        if (_timeSinceLastScroll >= _autoScrollTime)
        {
            ScrollToIndex(_currentIndex + 1, true);
            _timeSinceLastScroll = 0f;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        _dragStartPosition = eventData.position;
        _contentStartPosition = _content.anchoredPosition;
        _timeSinceLastScroll = 0;

        if (_currentTween != null && _currentTween.IsActive())
            _currentTween.Kill();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging)
            return;

        float dragDelta = eventData.position.x - _dragStartPosition.x;
        _content.anchoredPosition = new Vector2(_contentStartPosition.x + dragDelta, _contentStartPosition.y);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging)
            return;

        _isDragging = false;
        _timeSinceLastScroll = 0f;

        float dragDelta = eventData.position.x - _dragStartPosition.x;
        float normalizedDelta = dragDelta / Screen.width;

        if (Mathf.Abs(normalizedDelta) > _swipeThreshold)
        {
            if (normalizedDelta > 0)
                ScrollToIndex(_currentIndex - 1, true);
            else
                ScrollToIndex(_currentIndex + 1, true);
        }
        else
        {
            SnapToCurrent();
        }
    }

    private void ScrollToIndex(int targetIndex, bool smooth)
    {
        int direction = targetIndex > _currentIndex ? 1 : -1;
        int newIndex = (targetIndex + _banners.Count) % _banners.Count;

        if (smooth)
        {
            if (direction > 0)
                AnimateScrollRight(newIndex);
            else
                AnimateScrollLeft(newIndex);
        }
        else
        {
            if (direction > 0)
                ShiftRight();
            else
                ShiftLeft();
            _currentIndex = newIndex;
            _content.anchoredPosition *= Vector2.up;
            UpdateBannersData();
        }

        if (_bannerDots != null)
            _bannerDots.UpdateActiveDot(newIndex);
    }

    private void AnimateScrollRight(int newIndex)
    {
        if (_currentTween != null && _currentTween.IsActive())
            _currentTween.Kill();

        _currentTween = _content.DOAnchorPosX(-_bannerWidth, _snapDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => {
                _currentIndex = newIndex;
                ShiftRight();
                _content.anchoredPosition *= Vector2.up;
                UpdateBannersData();
            });
    }

    private void AnimateScrollLeft(int newIndex)
    {
        if (_currentTween != null && _currentTween.IsActive())
            _currentTween.Kill();

        _currentTween = _content.DOAnchorPosX(_bannerWidth, _snapDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => {
                _currentIndex = newIndex;
                ShiftLeft();
                _content.anchoredPosition *= Vector2.up;
                UpdateBannersData();
            });
    }

    private void SnapToCurrent()
    {
        if (_currentTween != null && _currentTween.IsActive())
            _currentTween.Kill();

        _currentTween = _content.DOAnchorPosX(0, _snapDuration)
            .SetEase(Ease.OutCubic);
    }

    private void ShiftRight()
    {
        if (_activeBanners.Count != 3)
            return;

        Banner leftBanner = _activeBanners[LEFT_INDEX];
        _activeBanners.RemoveAt(LEFT_INDEX);
        _activeBanners.Add(leftBanner);

        UpdateBannersPosition();
    }

    private void ShiftLeft()
    {
        if (_activeBanners.Count != 3)
            return;

        Banner rightBanner = _activeBanners[RIGHT_INDEX];
        _activeBanners.RemoveAt(RIGHT_INDEX);
        _activeBanners.Insert(0, rightBanner);

        UpdateBannersPosition();
    }
}
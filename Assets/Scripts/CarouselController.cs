using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class CarouselController : MonoBehaviour
{
    [SerializeField] private AudioSource _clickSound;
    [SerializeField] private GameObject _premiumPanel;
    [SerializeField] private GameObject _fullPanel;
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private RectTransform _content;
    [SerializeField] private Cell _cellPrefab;
    [SerializeField] private GridLayoutGroup _gridLayout;
    [SerializeField] private int _preloadCount = 5;
    [SerializeField] private float _scrollThreshold = 0.8f;
    [SerializeField] private int _batchSize = 10;

    private const string BASE_URL = "https://data.ikppbb.com/test-task-unity-data/pics/";
    private const int TOTAL_IMAGES = 66;

    private List<Cell> _cells = new List<Cell>();
    private List<int> _loadingImages = new List<int>();
    private int _maxConcurrentDownloads = 3;
    private Dictionary<int, Sprite> _loadedSprites = new Dictionary<int, Sprite>();
    private int _currentImageCount = 0;
    private bool _isLoadingMore = false;
    private CarouselButton _currentFilter = CarouselButton.All;

    private void Start()
    {
        if (_scrollRect == null)
            _scrollRect = GetComponent<ScrollRect>();
        if (_content == null)
            _content = _scrollRect.content;
        if (_gridLayout == null)
            _gridLayout = _content.GetComponent<GridLayoutGroup>();

        LoadInitialBatch();
        StartCoroutine(LoadImagesCoroutine());
    }

    private void OnEnable()
    {
        if (_scrollRect != null)
            _scrollRect.onValueChanged.AddListener(OnScroll);
    }

    private void OnDisable()
    {
        if (_scrollRect != null)
            _scrollRect.onValueChanged.RemoveListener(OnScroll);
    }

    public void SetFilter(CarouselButton filter)
    {
        _currentFilter = filter;
        UpdateAllCellsVisibility();
    }

    private bool ShouldShowImage(int imageNumber)
    {
        switch (_currentFilter)
        {
            case CarouselButton.All:
                return true;
            case CarouselButton.Odd:
                return imageNumber % 2 == 1;
            case CarouselButton.Even:
                return imageNumber % 2 == 0;
            default:
                return true;
        }
    }

    private bool IsPremiumImage(int imageNumber)
    {
        switch (_currentFilter)
        {
            case CarouselButton.All:
                return imageNumber >= 7 && imageNumber % 8 == 7 || imageNumber >= 8 && imageNumber % 8 == 0;
            case CarouselButton.Odd:
                return imageNumber >= 7 && imageNumber % 8 == 7;
            case CarouselButton.Even:
                return imageNumber >= 8 && imageNumber % 8 == 0;
            default:
                return false;
        }
    }

    private void UpdateAllCellsVisibility()
    {
        for (int i = 0; i < _cells.Count; i++)
        {
            int imageNumber = i + 1;
            bool shouldShow = ShouldShowImage(imageNumber);
            bool isPremium = IsPremiumImage(imageNumber);

            _cells[i].gameObject.SetActive(shouldShow);

            if (shouldShow && _cells[i].gameObject.activeSelf)
            {
                _cells[i].SetPremium(isPremium);
            }
        }

        UpdateContentSize();
    }

    private void UpdateContentSize()
    {
        int columns = _gridLayout.constraintCount;
        float cellHeight = _gridLayout.cellSize.y + _gridLayout.spacing.y;

        int visibleCells = 0;
        for (int i = 1; i <= _currentImageCount; i++)
        {
            if (ShouldShowImage(i))
                visibleCells++;
        }

        int visibleRows = Mathf.CeilToInt((float)visibleCells / columns);
        Vector2 contentSize = _content.sizeDelta;
        contentSize.y = visibleRows * cellHeight + 200;
        _content.sizeDelta = contentSize;
    }

    private IEnumerator LoadImagesCoroutine()
    {
        while (true)
        {
            if (_loadingImages.Count < _maxConcurrentDownloads)
            {
                var visibleImages = GetVisibleImageIndices();

                foreach (int imageNumber in visibleImages)
                {
                    if (_loadingImages.Count >= _maxConcurrentDownloads)
                        break;

                    if (imageNumber > 0 && imageNumber <= _currentImageCount &&
                        !_loadedSprites.ContainsKey(imageNumber) &&
                        !_loadingImages.Contains(imageNumber))
                    {
                        _loadingImages.Add(imageNumber);
                        StartCoroutine(LoadImageAsync(imageNumber));
                    }
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator LoadImageAsync(int imageNumber)
    {
        if (_loadedSprites.ContainsKey(imageNumber))
        {
            _loadingImages.Remove(imageNumber);
            yield break;
        }

        string url = $"{BASE_URL}{imageNumber}.jpg";

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );

                _loadedSprites[imageNumber] = sprite;
                UpdateCell(imageNumber);
            }
        }

        _loadingImages.Remove(imageNumber);
    }

    private void LoadInitialBatch()
    {
        int initialCount = Mathf.Min(_batchSize, TOTAL_IMAGES);
        _currentImageCount = initialCount;
        CreateAllCells();
    }

    private void CreateAllCells()
    {
        foreach (Transform child in _content)
        {
            Destroy(child.gameObject);
        }
        _cells.Clear();

        int columns = _gridLayout.constraintCount;

        for (int i = 1; i <= _currentImageCount; i++)
        {
            Cell cell = Instantiate(_cellPrefab, _content);

            bool shouldShow = ShouldShowImage(i);
            bool isPremium = IsPremiumImage(i);

            cell.gameObject.SetActive(shouldShow);

            if (_loadedSprites.ContainsKey(i))
            {
                cell.SetImage(_loadedSprites[i]);
                cell.SetLoading(false);
            }
            else
            {
                cell.SetImage(null);
                cell.SetLoading(true);
            }

            cell.SetPremium(isPremium);
            cell.clickSound = _clickSound;
            cell.premiumPanel = _premiumPanel;
            cell.fullPanel = _fullPanel;
            _cells.Add(cell);
        }

        UpdateContentSize();
    }

    private void UpdateCell(int imageNumber)
    {
        if (imageNumber > 0 && imageNumber <= _cells.Count)
        {
            int cellIndex = imageNumber - 1;
            if (cellIndex < _cells.Count)
            {
                var cell = _cells[cellIndex];
                bool shouldShow = ShouldShowImage(imageNumber);
                bool isPremium = IsPremiumImage(imageNumber);

                cell.gameObject.SetActive(shouldShow);

                if (shouldShow && _loadedSprites.ContainsKey(imageNumber))
                {
                    cell.SetImage(_loadedSprites[imageNumber]);
                    cell.SetLoading(false);
                    cell.clickSound = _clickSound;
                    cell.premiumPanel = _premiumPanel;
                    cell.fullPanel = _fullPanel;
                    cell.SetPremium(isPremium);
                }
            }
        }
    }

    private List<int> GetVisibleImageIndices()
    {
        var visibleIndices = new List<int>();

        if (_scrollRect == null || _scrollRect.viewport == null)
            return visibleIndices;

        int columns = _gridLayout.constraintCount;
        float cellHeight = _gridLayout.cellSize.y + _gridLayout.spacing.y;

        float scrollPercentage = Mathf.Clamp01(1 - _scrollRect.verticalNormalizedPosition);
        float viewportHeight = _scrollRect.viewport.rect.height;
        float contentHeight = _content.rect.height;

        float visibleStart = Mathf.Max(0, (contentHeight - viewportHeight) * scrollPercentage);
        float visibleEnd = visibleStart + viewportHeight;

        int startRow = Mathf.FloorToInt(visibleStart / cellHeight) - _preloadCount;
        int endRow = Mathf.CeilToInt(visibleEnd / cellHeight) + _preloadCount;

        startRow = Mathf.Max(0, startRow);
        endRow = Mathf.Min(Mathf.CeilToInt((float)_currentImageCount / columns) - 1, endRow);

        for (int row = startRow; row <= endRow; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int index = row * columns + col + 1;
                if (index <= _currentImageCount && ShouldShowImage(index))
                {
                    visibleIndices.Add(index);
                }
            }
        }

        return visibleIndices;
    }

    private void OnScroll(Vector2 scrollPosition)
    {
        float currentPosition = 1 - scrollPosition.y;

        if (currentPosition > _scrollThreshold &&
            _currentImageCount < TOTAL_IMAGES &&
            !_isLoadingMore &&
            _loadingImages.Count < _maxConcurrentDownloads)
        {
            LoadMoreImages();
        }
    }

    private void LoadMoreImages()
    {
        _isLoadingMore = true;

        int start = _currentImageCount + 1;
        int end = Mathf.Min(_currentImageCount + _batchSize, TOTAL_IMAGES);

        int oldCount = _currentImageCount;
        _currentImageCount = end;

        StartCoroutine(AddNewCellsCoroutine(oldCount + 1, end));
    }

    private IEnumerator AddNewCellsCoroutine(int startIndex, int endIndex)
    {
        for (int i = startIndex; i <= endIndex; i++)
        {
            Cell cell = Instantiate(_cellPrefab, _content);

            bool shouldShow = ShouldShowImage(i);
            bool isPremium = IsPremiumImage(i);

            cell.gameObject.SetActive(shouldShow);
            cell.SetImage(null);
            cell.SetLoading(true);
            cell.clickSound = _clickSound;
            cell.premiumPanel = _premiumPanel;
            cell.fullPanel = _fullPanel;
            cell.SetPremium(isPremium);
            _cells.Add(cell);

            yield return null;
        }

        UpdateContentSize();
        _isLoadingMore = false;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();

        foreach (var sprite in _loadedSprites.Values)
        {
            if (sprite != null && sprite.texture != null)
                Destroy(sprite.texture);
            if (sprite != null)
                Destroy(sprite);
        }
        _loadedSprites.Clear();
        _loadingImages.Clear();
    }
}
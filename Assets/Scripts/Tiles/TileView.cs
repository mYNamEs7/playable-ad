using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class TileView : MonoBehaviour
{
    [Header("Data")] [SerializeField] private TileData data;

    [Header("UI")] 
    [SerializeField] private Image image;
    [SerializeField] private Button button;
    [SerializeField] private RectTransform rect;
    [SerializeField] private Image displayedImage;
    [SerializeField] private TMP_Text displayedText;

    [Header("Colors")]
    [SerializeField] private Color normal = Color.white;
    [SerializeField] private Color wrongColor = Color.red;
    [SerializeField] private Color selectedColor;
    
    private int siblingIndex;
    public TileData Data => data;

    private MatchController controller;
    private Tween highlightTween;
    private Tween wrongTween;

    public bool IsAlive { get; private set; } = true;

    // ---------- GEOMETRY ----------
    public float Left => WorldX - Width * 0.5f;
    public float Right => WorldX + Width * 0.5f;
    public float Top => WorldY + Height * 0.5f;
    public float Bottom => WorldY - Height * 0.5f;

    public float Width => rect.rect.width;
    public float Height => rect.rect.height;

    public float WorldX => rect.anchoredPosition.x;
    public float WorldY => rect.anchoredPosition.y;

    private void Awake()
    {
        siblingIndex = rect.GetSiblingIndex();
    }

    public void Setup(MatchController matchController)
    {
        controller = matchController;
        button.onClick.AddListener(OnClick);
        SetVisual();
    }
    
    private void SetVisual()
    {
        if (Data.GetType() == typeof(ImageTileData))
        {
            displayedImage.gameObject.SetActive(true);
            displayedImage.sprite = ((ImageTileData)Data).displayedImage;
        }
        else if (Data.GetType() == typeof(TextTileData))
        {
            displayedText.gameObject.SetActive(true);
            displayedText.text = ((TextTileData)Data).displayedText;
        }
    }

    private void OnClick()
    {
        if (!IsAlive)
            return;

        controller.OnTileClicked(this);
    }
    
    public void HighlightOn()
    {
        highlightTween?.Kill();
        rect.SetAsLastSibling();

        highlightTween = DOTween.Sequence()
            .Append(image.DOColor(selectedColor, 0.12f))
            .Join(transform.DOScale(1.2f, 0.12f).SetEase(Ease.OutBack));
    }
    
    public void HighlightOff()
    {
        highlightTween?.Kill();
        rect.SetSiblingIndex(siblingIndex);

        highlightTween = DOTween.Sequence()
            .Append(image.DOColor(normal, 0.15f))
            .Join(transform.DOScale(1f, 0.15f));
    }

    public void PlaySelect()
    {
        transform
            .DOScale(1.1f, 0.15f)
            .SetLoops(2, LoopType.Yoyo);
    }

    public void PlayWrong()
    {
        wrongTween?.Kill();
        wrongTween = DOTween.Sequence()
            .AppendCallback(() => image.color = wrongColor)
            .Append(transform.DOShakeRotation(0.25f, new Vector3(0, 0, 20)))
            .AppendCallback(() =>
            {
                image.color = normal;
                HighlightOff();
            });
    }

    public void PlayMatch(Vector2 centerPosition)
    {
        IsAlive = false;
        
        highlightTween?.Kill();
        rect.SetAsLastSibling();
        image.color = selectedColor;
        DOTween.Sequence()
            .Append(transform.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack))
            .Append(((RectTransform)transform).DOAnchorPos(centerPosition, 0.6f).SetEase(Ease.InBack, 3f))
            .AppendInterval(0.2f)
            .Append(transform.DOScale(0f, 0.25f).SetEase(Ease.InBack, 2f))
            .OnComplete(() =>
            {
                controller.PlayEffect();
                gameObject.SetActive(false);
            });
    }
}
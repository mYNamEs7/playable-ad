using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class FingerController : MonoBehaviour
{
    [SerializeField] private RectTransform finger;
    [SerializeField] private MatchController matchController;
    [SerializeField] private RectTransform fingerParent; 
    [SerializeField] private Canvas canvas;      
    [SerializeField] private RectTransform fieldRoot;

    [Header("Visual")]
    [SerializeField] private Vector3 fingerOffset = new Vector3(0, -80f, 0);

    private Image image;
    private Coroutine loopRoutine;
    private Vector3 baseFingerScale;

    private Sequence loop;
    private TileView lastA;
    private TileView lastB;

    private void Awake()
    {
        image = GetComponent<Image>();
        baseFingerScale = finger.localScale; 
    }

    private void Start()
    {
        StartLoop();
    }

    private void OnEnable()
    {
        matchController.OnUserClicked += ResetLoop;
    }

    private void OnDisable()
    {
        matchController.OnUserClicked -= ResetLoop;
    }

    private void Update()
    {
        if(matchController.IsTheEnd())
            loop?.Kill();
    }
    

    private float GetFieldScaleFactor()
    {
        return fieldRoot.localScale.x;
    }
    
    private void UpdateFingerScale()
    {
        float factor = GetFieldScaleFactor();
        finger.localScale = baseFingerScale * factor;
        fingerOffset *= factor;
    }

    public void ResetLoop(float waitTime)
    {
        Stop();
        if(loopRoutine != null) StopCoroutine(loopRoutine);
        loopRoutine = StartCoroutine(StartLoopCoroutine(waitTime));
    }

    private IEnumerator StartLoopCoroutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        
        matchController.OffAllHighlights();
        StartLoop();
    }

    private void StartLoop()
    {
        Stop();

        loop = DOTween.Sequence();

        ClearHighlights();

        var pair = matchController.GetFirstAvailablePair();
        if (pair == null)
            return;

        lastA = pair.Value.Item1;
        lastB = pair.Value.Item2;

        UpdateFingerScale();
        finger.anchoredPosition = SetFingerToTile(lastA);
        
        loop.Append(image.DOFade(1f, 0.5f));
        loop.AppendCallback(DoScale);
        loop.AppendCallback(() => Highlight(lastA));
        loop.AppendInterval(0.5f);

        loop.Append(finger.DOAnchorPos(SetFingerToTile(lastB), 0.5f));
        loop.AppendCallback(DoScale);
        loop.AppendCallback(() => Highlight(lastB));
        loop.AppendInterval(0.5f);
        loop.AppendCallback(ClearHighlights);
        loop.Append(image.DOFade(0f, 0.5f));

        loop.SetLoops(-1);
    }
    
    private Vector2 SetFingerToTile(TileView tile)
    {
        RectTransform tileRect = (RectTransform)tile.transform;

        Vector3 worldPos = tileRect.TransformPoint(tileRect.rect.center);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            fingerParent,
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldPos),
            canvas.worldCamera,
            out Vector2 localPoint
        );

        return localPoint + (Vector2)fingerOffset;
    }

    private void DoScale()
    {
        finger
            .DOScale(baseFingerScale * GetFieldScaleFactor() * 0.85f, 0.12f)
            .SetLoops(2, LoopType.Yoyo);
    }

    private void Highlight(TileView tile)
    {
        tile.HighlightOn();
    }

    private void ClearHighlights()
    {
        lastA?.HighlightOff();
        lastB?.HighlightOff();
    }

    public void Stop()
    {
        loop?.Kill();
        ClearHighlights();
        TurnOffAlphaColor();
    }

    private void TurnOffAlphaColor()
    {
        var imageColor = image.color;
        imageColor.a = 0f;
        image.color = imageColor;
    }
}

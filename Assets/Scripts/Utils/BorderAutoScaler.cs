using UnityEngine;

public class BoardAutoScaler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform gameArea; 
    [SerializeField] private RectTransform board; 

    [Header("Base board size")]
    [SerializeField] private Vector2 boardBaseSize = new Vector2(600, 800);

    private void Start()
    {
        ApplyScale();
    }

    private void OnRectTransformDimensionsChange()
    {
        ApplyScale();
    }

    private void ApplyScale()
    {
        if (gameArea == null || board == null)
            return;

        Vector2 areaSize = gameArea.rect.size;

        float scaleX = areaSize.x / boardBaseSize.x;
        float scaleY = areaSize.y / boardBaseSize.y;

        float finalScale = Mathf.Min(scaleX, scaleY);

        board.localScale = Vector3.one * finalScale;
    }
}
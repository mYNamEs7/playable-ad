using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchController : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField] private int maxTaps = 6;
    [SerializeField] private RectTransform parent;
    [SerializeField] private List<ParticleSystem> effects;
    [SerializeField] private List<TileView> tiles;

    [Header("Free Side Detection")]
    [Tooltip("Насколько по X тайл считается соседним")]
    [SerializeField] private float sideCheckDistance = 10f;
    [Header("Free Side Rules")]
    [SerializeField, Range(0.1f, 1f)]
    private float minVerticalOverlap = 0.6f; 

    [SerializeField]
    private float sideSnapDistance = 6f; 

    private TileView selectedTile;
    private int tapCount;

    public Action<float> OnUserClicked;
    
    public bool HasUserInteracted { get; private set; }

    private void Start()
    {
        foreach (var tile in tiles)
        {
            tile.Setup(this);
        }
    }

    public bool IsTheEnd() => tapCount >= maxTaps;

    public void OnTileClicked(TileView tile)
    {
        HasUserInteracted = true;
        
        if (tapCount >= maxTaps) return;

        if (!IsTileFree(tile))
        {
            tapCount++;
            tile.PlayWrong();
            OnUserClicked?.Invoke(3f);
            return;
        }

        if (selectedTile == null)
        {
            OnUserClicked?.Invoke(3f);
            selectedTile = tile;
            tile.HighlightOn();
            return;
        }

        TryMatch(selectedTile, tile);
    }

    private void TryMatch(TileView a, TileView b)
    {
        OnUserClicked?.Invoke(3f);
        tapCount++;

        bool correct =
            a != b &&
            a.Data.groupId == b.Data.groupId &&
            a.Data.id != b.Data.id;

        if (correct)
        {
            var parentPosition = parent.anchoredPosition;
            parentPosition.x -= a.Width/2;
            a.PlayMatch(parentPosition);
            parentPosition.x += (a.Width * 2) / 2;
            b.PlayMatch(parentPosition);
        }
        else if(a == b)
            a.HighlightOff();
        else
        {
            a.PlayWrong();
            b.PlayWrong();
        }

        selectedTile = null;
    }

    public void PlayEffect()
    {
        foreach (var effect in effects)
        {
            var particles = Instantiate(effect);
            particles.Play();
        }
    }

    private bool IsTileFree(TileView tile)
    {
        bool leftBlocked = false;
        bool rightBlocked = false;

        foreach (var other in tiles)
        {
            if (other == tile || !other.IsAlive)
                continue;

            if (!HasEnoughVerticalOverlap(tile, other))
                continue;

            if (Mathf.Abs(other.Right - tile.Left) <= sideSnapDistance)
                leftBlocked = true;

            if (Mathf.Abs(other.Left - tile.Right) <= sideSnapDistance)
                rightBlocked = true;
        }

        return !leftBlocked || !rightBlocked;
    }
    
    private bool HasEnoughVerticalOverlap(TileView a, TileView b)
    {
        float overlap =
            Mathf.Min(a.Top, b.Top) -
            Mathf.Max(a.Bottom, b.Bottom);

        if (overlap <= 0)
            return false;

        float minHeight = Mathf.Min(a.Height, b.Height);
        float overlapPercent = overlap / minHeight;

        return overlapPercent >= minVerticalOverlap;
    }

    public (TileView, TileView)? GetFirstAvailablePair()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = i + 1; j < tiles.Count; j++)
            {
                var a = tiles[i];
                var b = tiles[j];

                if (!a.IsAlive || !b.IsAlive)
                    continue;

                if (!IsTileFree(a) || !IsTileFree(b))
                    continue;

                if (a.Data.groupId == b.Data.groupId &&
                    a.Data.id != b.Data.id)
                {
                    return (a, b);
                }
            }
        }
        return null;
    }

    public void OffAllHighlights()
    {
        foreach (var tile in tiles)
        {
            tile.HighlightOff();
        }
        selectedTile = null;
    }
}

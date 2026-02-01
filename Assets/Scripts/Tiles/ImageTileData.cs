using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Playable/Image Tile Data")]
public class ImageTileData : TileData
{
    [Header("Display")]
    public Sprite displayedImage;
}

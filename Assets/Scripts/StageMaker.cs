using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageMaker : MonoBehaviour
{
    public Texture2D map;
    public GameObject brickPrefab;
    public Color noBrickColor;
    public Color brick1Color;
    public Color brick2Color;
    public Color brick3Color;
    public Color brick4Color;
    public Color brick5Color;
    public Color brickIndestructableColor;
    public Color brickBombColor;

    public void CreateStage()
    {
        for (var y = 0; y < map.height; y++)
        {
            for (var x = 0; x < map.width; x++)
            {
                Color colorOnPixel = map.GetPixel(x, y);

                Debug.Log("Coords: " + x + "," + y + " Color: " + colorOnPixel);

                if (colorOnPixel != noBrickColor)
                {
                    var newBrick = Instantiate(brickPrefab, transform);
                    newBrick.transform.localPosition = new Vector3((x * 2) - 13, y - 0.5f);
                    Brick currentBrick = newBrick.GetComponent<Brick>();

                    if (colorOnPixel == brick1Color)
                    {
                        currentBrick.InitializeBrick(1);
                    }
                    else if (colorOnPixel == brick2Color)
                    {
                        currentBrick.InitializeBrick(2);
                    }
                    else if (colorOnPixel == brick3Color)
                    {
                        currentBrick.InitializeBrick(3);
                    }
                    else if (colorOnPixel == brick4Color)
                    {
                        currentBrick.InitializeBrick(4);
                    }
                    else if (colorOnPixel == brick5Color)
                    {
                        currentBrick.InitializeBrick(5);
                    }
                    else if (colorOnPixel == brickIndestructableColor)
                    {
                        currentBrick.InitializeBrick(0, true);
                    }
                    else if (colorOnPixel == brickBombColor)
                    {
                        currentBrick.InitializeBrick(1, false, true);
                    }
                }
            }
        }
    }
}

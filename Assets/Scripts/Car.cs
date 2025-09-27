using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public enum Orientation { Horizontal, Vertical }

    public int length;
    public Orientation orientation;
    public Vector2Int position; // Position of the car's top-left corner on the grid
}

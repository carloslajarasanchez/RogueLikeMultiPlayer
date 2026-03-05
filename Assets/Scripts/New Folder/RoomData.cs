using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomData
{
    public int X, Y;
    public RoomType Type;
    public bool DoorTop, DoorBottom, DoorLeft, DoorRight;

    public RoomData(int x, int y)
    {
        X = x;
        Y = y;
        Type = RoomType.Normal;
    }

    // Genera una clave string para buscar el prefab correcto
    // Orden siempre: Top, Bottom, Left, Right
    public string GetPrefabKey()
    {
        string key = "";
        if (DoorTop) key += "T";
        if (DoorBottom) key += "B";
        if (DoorLeft) key += "L";
        if (DoorRight) key += "R";
        return key == "" ? "ClosedR" : key;
    }
}

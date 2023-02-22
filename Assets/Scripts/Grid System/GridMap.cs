using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Personal.Utils;

#region DESCRIPTION
/* GridMap script is base script for creating grid structure, it class is generic, which allow them to receive custom object.
 * This be use as baseline for other custom object (class) to use to create specific custom grid system.
 * 
 * You can create new script as custom object type that contain specific data and algorithm for various kind of grid system you want.
 * Then implement those custom object into grid structure using this script as baseline (eg. gird map, tile map, heat map, navigation map, etc.)
 */
#endregion

public class GridMap<TGridType>
{
    public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
    public class OnGridValueChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    private int width;
    private int height;
    private float cellSize;
    private Vector3 OriginPOS;
    private TGridType[,] gridArray;
    private TextMesh[,] debugTextArray;


    //---> construct for declaring new grid system <---//
    public GridMap(int width, int height, float cellSize, Vector2 OriginPOS, Func<GridMap<TGridType>, int, int, TGridType> createGridObject)
    {

        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.OriginPOS = OriginPOS;

        //---> receive any object type using generic <---//
        gridArray = new TGridType[width, height];


        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                //---> create new grid as 2 dimension array of [custom object] in grid order (row/colum) <---//
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }
            
        bool Debugging = true;
        if (Debugging)
        {
            debugTextArray = new TextMesh[width, height];

            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    Debug.DrawLine(GetWorldPOS(x, y), GetWorldPOS(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPOS(x, y), GetWorldPOS(x + 1, y), Color.white, 100f);
                    debugTextArray[x, y] = Utility.CreateWorldText(gridArray[x, y]?.ToString(), null, GetWorldPOS(x, y) + new Vector3(cellSize, cellSize) * 0.5f, 10, Color.red, TextAnchor.MiddleCenter, TextAlignment.Center, 0);
                }
            }
            Debug.DrawLine(GetWorldPOS(0, height), GetWorldPOS(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPOS(width, 0), GetWorldPOS(width, height), Color.white, 100f);

            OnGridValueChanged += (object sender, OnGridValueChangedEventArgs eventArgs) =>
            { debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString(); };
        }
    }

    #region GET/SET GRID PROPERTY
    public void TriggerGridObjectChanged(int x, int y)
    {
        if (OnGridValueChanged != null)
        {
            Debug.Log("Value Changed: " + OnGridValueChanged);
            OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, y = y });
        }
    }
    public void SetGridObject(int x, int y, TGridType value)
    {
        if(x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
            if (OnGridValueChanged != null) 
            {
                Debug.Log("Value Changed");
                OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, y = y }); 
            }
        }
    }
    public void SetGridObject(Vector3 POS, TGridType value)
    {
        int x, y;
        GetGridXY(POS, out x, out y);
        gridArray[x, y] = value;
    }

    public TGridType GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return default(TGridType);
        }
    }
    public TGridType GetGridObject(Vector3 POS)
    {
        int x, y;
        GetGridXY(POS, out x, out y);
        return (GetGridObject(x, y));
    }


    public Vector3 GetWorldPOS(int x, int y)
    {
        return new Vector3(x, y) * cellSize + OriginPOS;
    }
    public void GetGridXY(Vector3 worldPOS, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPOS - OriginPOS).x / cellSize);
        y = Mathf.FloorToInt((worldPOS - OriginPOS).y / cellSize);
    }
    
    public float GetCellSize()
    {
        return cellSize;
    }
    public int GetWidth()
    {
        return gridArray.GetLength(0);
    }
    public int GetHeigth()
    {
        return gridArray.GetLength(1);
    }
    #endregion

}

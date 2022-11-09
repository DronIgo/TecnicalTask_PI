using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using static MazeGeneration;

/// <summary>
/// MazeData is used to store info about the maze and contains some usefull methods to work with it
/// </summary>
public class MazeData : MonoBehaviour
{
    public WallState[,] map;
    //width and height should ideally be properties with getter only
    public int width;
    public int height;

    public float cellSize;
    public Vector2 lbCorner;

    /// <summary>
    /// Returns the index of the cell in which the point currently is
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Vector2Int GetCellIndex(Vector3 position)
    {
        return new Vector2Int((int)((position.x - lbCorner.x) / cellSize), (int)((position.z - lbCorner.y) / cellSize));
    }

    /// <summary>
    /// Return the position od the center of the cell by index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Vector3 GetCellPosition(Vector2Int index)
    {
        return new Vector3(lbCorner.x + index.x * cellSize + cellSize / 2, 0, lbCorner.y + index.y * cellSize + cellSize / 2);
    }
    
    /// <summary>
    /// Gets the position to which enemy should flee from the player 
    /// (fleePos - position of the enemy, chasePos - position of the player)
    /// </summary>
    /// <param name="fleePos"></param>
    /// <param name="chasePos"></param>
    /// <returns></returns>
    public Vector3 GetFleePosition(Vector3 fleePos, Vector3 chasePos)
    {
        Vector3 fleeDirection = new Vector3(fleePos.x - chasePos.x, 0, fleePos.z - chasePos.z);

        Vector2Int fp = GetCellIndex(fleePos);
        Vector2Int cp = GetCellIndex(chasePos);
        if (fp == cp)
            return fleePos + fleeDirection.normalized * 0.6f;

        Vector2Int cp_furthest = cp;
        {
            int dist = 0;
            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                    if (AllDistances[cp.x, cp.y][i, j] > dist)
                    {
                        dist = AllDistances[cp.x, cp.y][i, j];
                        cp_furthest = new Vector2Int(i, j);
                    }
        }

        Vector2Int fpInit = fp;
        int steps = 5;
        while (steps > 0)
        {
            List<Vector2Int> possibleSteps = new List<Vector2Int>();
            if (fp.x > 0) // left
                if (!map[fp.x, fp.y].HasFlag(WallState.LEFT))
                    if (AllDistances[cp.x, cp.y][fp.x, fp.y] < AllDistances[cp.x, cp.y][fp.x - 1, fp.y])
                        possibleSteps.Add(new Vector2Int(fp.x - 1, fp.y));

            if (fp.x < width - 1) // right
                if (!map[fp.x, fp.y].HasFlag(WallState.RIGHT))
                    if (AllDistances[cp.x, cp.y][fp.x, fp.y] < AllDistances[cp.x, cp.y][fp.x + 1, fp.y])
                        possibleSteps.Add(new Vector2Int(fp.x + 1, fp.y));

            if (fp.y > 0) // down
                if (!map[fp.x, fp.y].HasFlag(WallState.DOWN))
                    if (AllDistances[cp.x, cp.y][fp.x, fp.y] < AllDistances[cp.x, cp.y][fp.x, fp.y - 1])
                        possibleSteps.Add(new Vector2Int(fp.x, fp.y - 1));

            if (fp.y < height - 1) // up
                if (!map[fp.x, fp.y].HasFlag(WallState.UP))
                    if (AllDistances[cp.x, cp.y][fp.x, fp.y] < AllDistances[cp.x, cp.y][fp.x, fp.y + 1])
                        possibleSteps.Add(new Vector2Int(fp.x, fp.y + 1));

            if (possibleSteps.Count > 0)
            {
                int dist = width + height;
                for (int i = 0; i < possibleSteps.Count; i++)
                    if (possibleSteps[i].x - cp_furthest.x + possibleSteps[i].y - cp_furthest.y < dist)
                    {
                        dist = possibleSteps[i].x - cp_furthest.x + possibleSteps[i].y - cp_furthest.y;
                        fp = possibleSteps[i];
                    }
                --steps;
                continue;
            }
            break;
        }
        return GetCellPosition(fp);
    }

    private void Initialise(SaveData saveData)
    {
        width = saveData.width;
        height = saveData.height;
        lbCorner = new Vector2(saveData.lbCorner_x, saveData.lbCorner_y);
        map = saveData.map;
        cellSize = saveData.cellSize;
    }

    public void Load()
    {
        string filepath = Application.persistentDataPath + savePath;

        using (FileStream file = File.Open(filepath, FileMode.Open))
        {
            object loadedData = new BinaryFormatter().Deserialize(file);
            SaveData saveData = (SaveData)loadedData;
            Initialise(saveData);
        }
    }

    private void Awake()
    {
        Load();
        string filepath = Application.persistentDataPath + "/allDist.dat";

        using (FileStream file = File.Open(filepath, FileMode.Open))
        {
            object loadedData = new BinaryFormatter().Deserialize(file);
            int[,][,] saveData = (int[,][,])loadedData;
            if (saveData.GetLength(0) > 0)
                AllDistances = saveData;
            else
                CalculateAllDistances();
        }
    }


    public int[,][,] AllDistances;

    /// <summary>
    /// Calculates the distance between each pair of cells using Floyd-Warshall alghorithm
    /// </summary>
    public void CalculateAllDistances()
    {
        AllDistances = new int[width, height][,];
        for (int i = 0; i < width; ++i)
            for (int j = 0; j < height; ++j)
            {
                AllDistances[i, j] = new int[width, height];
                for (int i2 = 0; i2 < width; ++i2)
                    for (int j2 = 0; j2 < height; ++j2)
                        AllDistances[i, j][i2, j2] = int.MaxValue/2;
                AllDistances[i, j][i, j] = 0;
                if (i > 0) // left
                    if (!map[i, j].HasFlag(WallState.LEFT))
                        AllDistances[i, j][i - 1, j] = 1;
                if (i < width - 1) // right
                    if (!map[i, j].HasFlag(WallState.RIGHT))
                        AllDistances[i, j][i + 1, j] = 1;
                if (j > 0) // down
                    if (!map[i, j].HasFlag(WallState.DOWN))
                        AllDistances[i, j][i, j - 1] = 1;
                if (j < height - 1) // up
                    if (!map[i, j].HasFlag(WallState.UP))
                        AllDistances[i, j][i, j + 1] = 1;
            }
        for (int ik = 0; ik < width; ++ik)
            for (int jk = 0; jk < height; ++jk)
                for (int i1 = 0; i1 < width; ++i1)
                    for (int j1 = 0; j1 < height; ++j1)
                        for (int i2 = 0; i2 < width; ++i2)
                            for (int j2 = 0; j2 < height; ++j2)
                                if (AllDistances[i1, j1][i2, j2] > AllDistances[i1, j1][ik, jk] + AllDistances[ik, jk][i2, j2])
                                {
                                    AllDistances[i1, j1][i2, j2] = AllDistances[i1, j1][ik, jk] + AllDistances[ik, jk][i2, j2];
                                    AllDistances[i2, j2][i1, j1] = AllDistances[i1, j1][ik, jk] + AllDistances[ik, jk][i2, j2];
                                }
    }
}

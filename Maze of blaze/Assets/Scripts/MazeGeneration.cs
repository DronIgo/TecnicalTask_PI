using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The class which handles Maze Generation. Code is taken from here and slightly redacted https://github.com/gamedolphin/youtube_unity_maze/tree/master/Assets/Scripts
/// </summary>
public class MazeGeneration : MonoBehaviour
{
    public static string savePath = "/save.dat";

    [System.Serializable]
    public struct SaveData
    {
        public WallState[,] map;
        public int width;
        public int height;

        public float cellSize;
        public float lbCorner_x;
        public float lbCorner_y;
    };

    [Flags]
    public enum WallState
    {
        LEFT = 1,// 0001
        RIGHT = 2,//0010
        UP = 4,//0100
        DOWN = 8,//1000

        VISITED = 32,//100000
    }

    public struct Neighbour
    {
        public Vector2Int position;
        public WallState sharedWall;
    }

    private static WallState GetOppositeWall(WallState wall)
    {
        switch (wall)
        {
            case WallState.RIGHT: return WallState.LEFT;
            case WallState.LEFT: return WallState.RIGHT;
            case WallState.UP: return WallState.DOWN;
            case WallState.DOWN: return WallState.UP;
            default: return WallState.LEFT;
        }
    }

    public static List<Neighbour> GetUnvisitedNeighbours(Vector2Int p, WallState[,] maze, int width, int height)
    {
        var list = new List<Neighbour>();

        if (p.x > 0) // left
        {
            if (!maze[p.x - 1, p.y].HasFlag(WallState.VISITED))
            {
                list.Add(new Neighbour
                {
                    position = new Vector2Int(p.x - 1, p.y),
                    sharedWall = WallState.LEFT
                }); 
            }
        }

        if (p.y > 0) // DOWN
        {
            if (!maze[p.x, p.y - 1].HasFlag(WallState.VISITED))
            {
                list.Add(new Neighbour
                {
                    position = new Vector2Int(p.x, p.y - 1),
                    sharedWall = WallState.DOWN
                });
            }
        }

        if (p.y < height - 1) // UP
        {
            if (!maze[p.x, p.y + 1].HasFlag(WallState.VISITED))
            {
                list.Add(new Neighbour
                {
                    position = new Vector2Int(p.x, p.y + 1),
                    sharedWall = WallState.UP
                });
            }
        }

        if (p.x < width - 1) // RIGHT
        {
            if (!maze[p.x + 1, p.y].HasFlag(WallState.VISITED))
            {
                list.Add(new Neighbour
                {
                    position = new Vector2Int(p.x + 1, p.y),
                    sharedWall = WallState.RIGHT
                });
            }
        }

        return list;
    }

    /// <summary>
    /// Generates maze structure using recursive backtracker algorithm
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static WallState[,] GenerateMazeStructure(int width, int height)
    {
        WallState[,] maze = new WallState[width, height];
        WallState initial = WallState.RIGHT | WallState.LEFT | WallState.UP | WallState.DOWN;
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                maze[i, j] = initial; 
            }
        }

        var rand = new System.Random();
        Stack<Vector2Int> positionStack = new Stack<Vector2Int>();
        Vector2Int startPosition = new Vector2Int(rand.Next(0, width), rand.Next(0, height));
        maze[startPosition.x, startPosition.y] |= WallState.VISITED;
        positionStack.Push(startPosition);

        while (positionStack.Count > 0)
        {
            var current = positionStack.Peek();
            var neighbours = GetUnvisitedNeighbours(current, maze, width, height);

            if (neighbours.Count > 0)
            {

                var randIndex = rand.Next(0, neighbours.Count);
                var randomNeighbour = neighbours[randIndex];

                var nPosition = randomNeighbour.position;
                maze[current.x, current.y] &= ~randomNeighbour.sharedWall;
                maze[nPosition.x, nPosition.y] &= ~GetOppositeWall(randomNeighbour.sharedWall);
                maze[nPosition.x, nPosition.y] |= WallState.VISITED;

                positionStack.Push(nPosition);
            } else
            {
                positionStack.Pop();
            }
        }

        return maze;
    }
}

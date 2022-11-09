//We want to exlude MazeEditorTool script from our build
#if (UNITY_EDITOR) 

using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine.AI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using static MazeGeneration;

// Tagging a class with the EditorTool attribute and no target type registers a global tool. Global tools are valid for any selection,
// and are accessible through the top left toolbar in the editor.
/// <summary>
/// Maze Editor Tool is a custom editor tool which can be used to generate and populate the maze, 
/// as well as calculate and store information about the maze structure in advance before runtime
/// more information about the tool can be found in readme.txt
/// </summary>
[EditorTool("Maze Generation Tool")]
class MazeEditorTool : EditorTool
{
    //data, maze, enemies and bonuses (currently unused) are empty GameObject used to organise the Scene or store data
    GameObject data;
    GameObject maze;
    GameObject enemies;
    GameObject bonuses;

    [Header("Please fill this prefabs")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject defEnemyPrefab;
    public GameObject patrolEnemyPrefab;
    public GameObject theifEnemyPrefab;

    [Header("Number of enemies by type")]
    [Range(0, 30)]
    public int numOfDef = 3;
    [Range(0, 30)]
    public int numOfPat = 3;
    [Range(0, 30)]
    public int numOfThief = 3;

    [Header("Size of the field")]
    [Range(5,50)]
    public int width = 10;
    [Range(5, 50)]
    public int height = 10;

    /// <summary>
    /// Function which stores all information about the maze,
    /// can only be used directly from GenerateMaze, otherwise map array will be lost
    /// </summary>
    void SaveChanges()
    {
        data = GameObject.Find("Data Object");
        DestroyImmediate(data);

        data = new GameObject("Data Object");
        data.AddComponent<MazeData>();

        float size_z = wallPrefab.transform.localScale.z;
        float lbCorner_x = width % 2 == 0 ? -(float)(width + 1) * size_z / 2.0f : -(float)(width) * size_z / 2.0f;
        float lbCorner_y = height % 2 == 0 ? -(float)(height + 1) * size_z / 2.0f : -(float)(height) * size_z / 2.0f;
        
        SaveData saveData = new SaveData
        {
            width = width,
            height = height,
            cellSize = size_z,
            lbCorner_x = lbCorner_x,
            lbCorner_y = lbCorner_y,
            map = map
        };

        string filepath = Application.persistentDataPath + savePath;

        using (FileStream file = File.Create(filepath))
        {
            new BinaryFormatter().Serialize(file, saveData);
        }

        filepath = Application.persistentDataPath + "/allDist.dat";

        using (FileStream file = File.Create(filepath))
        {
            new BinaryFormatter().Serialize(file, new int[0,0]);
        }

        NavMeshSurface navSurface = GameObject.FindObjectOfType<NavMeshSurface>();
        navSurface.BuildNavMesh();
    }

    //maze structure (check out MazeGeneration class for WallState enum)
    WallState[,] map;

    /// <summary>
    /// Generates the maze, see more in MazeGeneration
    /// </summary>
    void GenerateMaze()
    {
        map = MazeGeneration.GenerateMazeStructure(width, height);

        //find the properties of the wall
        float size_z = wallPrefab.transform.localScale.z;
        float size_y = wallPrefab.transform.localScale.y;

        //clear already generated maze
        if (maze == null)
        {
            maze = GameObject.Find("Maze Object");
        }
        DestroyImmediate(maze);
        maze = new GameObject("Maze Object");
        maze.transform.position = new Vector3(0, 0, 0);

        Transform mazeTransform = maze.transform;
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                var cell = map[i, j];
                var position = new Vector3(-width / 2 + i, 0, -height / 2 + j)*size_z + new Vector3(0, size_y / 2.0f, 0);

                if (cell.HasFlag(WallState.UP))
                {
                    var topWall = Instantiate(wallPrefab, position + new Vector3(0, 0, size_z / 2.0f), Quaternion.Euler(0, 90, 0), mazeTransform);
                }

                if (cell.HasFlag(WallState.LEFT))
                {
                    var leftWall = Instantiate(wallPrefab, position + new Vector3(-size_z / 2.0f, 0, 0), Quaternion.identity, mazeTransform);
                }
            }
        }
        for (int i = 0; i < width; ++i)
        {
            var cell = map[i, 0];
            var position = new Vector3(-width / 2 + i, 0, -height / 2) * size_z + new Vector3(0, size_y / 2, 0);
            if (cell.HasFlag(WallState.DOWN))
            {
                var bottomWall = Instantiate(wallPrefab, position + new Vector3(0, 0, -size_z / 2.0f), Quaternion.Euler(0, 90, 0), mazeTransform);
            }
        }
        for (int j = 0; j < height; ++j)
        {
            var cell = map[width - 1, j];
            var position = new Vector3((width / 2) - ((width + 1) % 2), 0, -height / 2 + j) * size_z + new Vector3(0, size_y / 2, 0);
            if (cell.HasFlag(WallState.RIGHT))
            {
                var rightWall = Instantiate(wallPrefab, position + new Vector3(size_z / 2.0f, 0, 0), Quaternion.identity, mazeTransform);
            }
        }
        SaveChanges();
    }

    System.Random rand = new System.Random();
    private Vector3 GetRandomPosition()
    {
        float size_z = wallPrefab.transform.localScale.z;
        float lbCorner_x = width % 2 == 0 ? -(float)(width + 1) * size_z / 2.0f : -(float)(width) * size_z / 2.0f;
        float lbCorner_y = height % 2 == 0 ? -(float)(height + 1) * size_z / 2.0f : -(float)(height) * size_z / 2.0f;
        int index_x = rand.Next(0, width);
        int index_y = rand.Next(0, height);
        return new Vector3(lbCorner_x + index_x * size_z + size_z / 2, 0, lbCorner_y + index_y * size_z + size_z / 2);
    }

    /// <summary>
    /// CalculateAllDistances can be used in the editor to run lenghty Floyd-Warshall alghorithm in the editor and save it's results
    /// </summary>
    private void CalcualteAllDistances()
    {
        MazeData mData = new MazeData();
        mData.Load();
        mData.CalculateAllDistances();
        string filepath = Application.persistentDataPath + "/allDist.dat";

        using (FileStream file = File.Create(filepath))
        {
            new BinaryFormatter().Serialize(file, mData.AllDistances);
        }
    }
    private void GenerateEnemies()
    {
        if (enemies == null)
        {
            enemies = GameObject.Find("Enemies Object");
        }
        DestroyImmediate(enemies);
        enemies = new GameObject("Enemies Object");
        for (int i = 0; i < numOfDef; ++i)
            Instantiate(defEnemyPrefab, GetRandomPosition(), Quaternion.identity, enemies.transform);
        for (int i = 0; i < numOfPat; ++i)
            Instantiate(patrolEnemyPrefab, GetRandomPosition(), Quaternion.identity, enemies.transform);
        for (int i = 0; i < numOfThief; ++i)
            Instantiate(theifEnemyPrefab, GetRandomPosition(), Quaternion.identity, enemies.transform);
    }

    private void GenerateBonuses()
    {
        if (bonuses == null)
        {
            bonuses = GameObject.Find("Bonuses Object");
        }
        DestroyImmediate(bonuses);
        bonuses = new GameObject("Bonuses Object");
        for (int i = 0; i < numOfDef; ++i)
            Instantiate(defEnemyPrefab, GetRandomPosition(), Quaternion.identity, enemies.transform);
        for (int i = 0; i < numOfPat; ++i)
            Instantiate(patrolEnemyPrefab, GetRandomPosition(), Quaternion.identity, enemies.transform);
        for (int i = 0; i < numOfThief; ++i)
            Instantiate(theifEnemyPrefab, GetRandomPosition(), Quaternion.identity, enemies.transform);
    }

    // Serialize this value to set a default value in the Inspector.
    [SerializeField]
    Texture2D m_ToolIcon;

    GUIContent m_IconContent;


    void Awake()
    {
        m_IconContent = new GUIContent()
        {
            image = m_ToolIcon,
            text = "Maze Editor Tool",
            tooltip = "Maze Editor Tool"
        };
    }

    public override GUIContent toolbarIcon
    {
        get { return m_IconContent; }
    }

    /// <summary>
    /// Tool functionality
    /// </summary>
    /// <param name="window"></param>
    public override void OnToolGUI(EditorWindow window)
    {
        Handles.BeginGUI();
        {
            
            GUILayout.BeginArea(new Rect(0, 0, 140, 300), new GUIStyle("box"));
            {
                if (GUILayout.Button("Generate Maze"))
                {
                    GenerateMaze();
                }
                if (GUILayout.Button("Generate Enemies"))
                {
                    GenerateEnemies();
                }
                if (GUILayout.Button("Calculate all dist"))
                {
                    CalcualteAllDistances();
                }
                GUILayout.Label("Width: " + width);
                width = (int)GUILayout.HorizontalSlider(width, 5, 50);
                GUILayout.Space(20);
                GUILayout.Label("Height: " + height);
                height = (int)GUILayout.HorizontalSlider(height, 5, 50);

                GUILayout.Space(30);
                GUILayout.Label("Soldiers: " + numOfDef);
                numOfDef = (int)GUILayout.HorizontalSlider(numOfDef, 0, 30);
                GUILayout.Space(20);
                GUILayout.Label("Guards: " + numOfPat);
                numOfPat = (int)GUILayout.HorizontalSlider(numOfPat, 0, 30);
                GUILayout.Space(20);
                GUILayout.Label("Theifs: " + numOfThief);
                numOfThief = (int)GUILayout.HorizontalSlider(numOfThief, 0, 30);
            }
            GUILayout.EndArea();
            
        }
        Handles.EndGUI();
    }
}

#endif
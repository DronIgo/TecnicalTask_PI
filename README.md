# TecnicalTask_PI

!!!Since Maze Generation Tool uses Serialization of data to local file, for correct work of the game Generate Maze should be called at least once in order to create the data file!!!

Please Read the following instruction on the Maze Editor Tool
1) If the prefabs in the script are null, you will have to fill them by hand in the editor. All the needed prefabs are in the prefabs folder.
2) Generate Maze generates maze, saves information about it and rebakes the NavMesh.
3) Calculate all distances should only be used after you generate a maze and should ideally always be used to avoid long initial loading time.

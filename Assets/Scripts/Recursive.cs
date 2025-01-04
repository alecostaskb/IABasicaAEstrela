﻿public class Recursive : Maze
{
    public override void Generate()
    {
        Generate(5, 5);
    }

    private void Generate(int x, int z)
    {
        if (CountSquareNeighbours(x, z) >= 2)
        {
            return;
        }

        mapa[x, z] = 0;

        directions.Shuffle();

        Generate(x + directions[0].x, z + directions[0].z);
        Generate(x + directions[1].x, z + directions[1].z);
        Generate(x + directions[2].x, z + directions[2].z);
        Generate(x + directions[3].x, z + directions[3].z);
    }
}
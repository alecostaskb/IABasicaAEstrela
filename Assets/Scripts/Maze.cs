using System.Collections.Generic;
using UnityEngine;

public class MapLocation
{
    public int x;
    public int z;

    public MapLocation(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public Vector2 ToVector()
    {
        return new Vector2(x, z);
    }

    public static MapLocation operator +(MapLocation a, MapLocation b)
       => new MapLocation(a.x + b.x, a.z + b.z);

    public override bool Equals(object obj)
    {
        if ((obj == null) || !GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            return x == ((MapLocation)obj).x && z == ((MapLocation)obj).z;
        }
    }

    public override int GetHashCode()
    {
        return 0;
    }
}

public class Maze : MonoBehaviour
{
    public List<MapLocation> directions = new List<MapLocation>()
    {
        new MapLocation(1,0),
        new MapLocation(0,1),
        new MapLocation(-1,0),
        new MapLocation(0,-1)
    };

    public int ancho = 30; //x length
    public int largo = 30; //z length
    public byte[,] mapa;
    public int escala = 6;

    // Start is called before the first frame update
    private void Start()
    {
        InitialiseMap();

        Generate();

        DrawMap();
    }

    private void InitialiseMap()
    {
        mapa = new byte[ancho, largo];

        for (int z = 0; z < largo; z++)
        {
            for (int x = 0; x < ancho; x++)
            {
                mapa[x, z] = 1; // 1 = wall  0 = corridor
            }
        }
    }

    public virtual void Generate()
    {
        for (int z = 0; z < largo; z++)
        {
            for (int x = 0; x < ancho; x++)
            {
                if (Random.Range(0, 100) < 50)
                {
                    mapa[x, z] = 0; // 1 = wall  0 = corridor
                }
            }
        }
    }

    private void DrawMap()
    {
        for (int z = 0; z < largo; z++)
        {
            for (int x = 0; x < ancho; x++)
            {
                if (mapa[x, z] == 1) // 1 = wall  0 = corridor
                {
                    Vector3 pos = new Vector3(x * escala, 0, z * escala);
                    GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.transform.localScale = new Vector3(escala, escala, escala);
                    wall.transform.position = pos;
                }
            }
        }
    }

    public int CountSquareNeighbours(int x, int z)
    {
        int count = 0;

        if (x <= 0 || x >= ancho - 1 || z <= 0 || z >= largo - 1)
        {
            return 5;
        }

        if (mapa[x - 1, z] == 0) // 1 = wall  0 = corridor
        {
            count++;
        }

        if (mapa[x + 1, z] == 0) // 1 = wall  0 = corridor
        {
            count++;
        }

        if (mapa[x, z + 1] == 0) // 1 = wall  0 = corridor
        {
            count++;
        }

        if (mapa[x, z - 1] == 0) // 1 = wall  0 = corridor
        {
            count++;
        }

        return count;
    }

    public int CountDiagonalNeighbours(int x, int z)
    {
        int count = 0;

        if (x <= 0 || x >= ancho - 1 || z <= 0 || z >= largo - 1)
        {
            return 5;
        }

        if (mapa[x - 1, z - 1] == 0) // 1 = wall  0 = corridor
        {
            count++;
        }

        if (mapa[x + 1, z + 1] == 0) // 1 = wall  0 = corridor
        {
            count++;
        }

        if (mapa[x - 1, z + 1] == 0) // 1 = wall  0 = corridor
        {
            count++;
        }

        if (mapa[x + 1, z - 1] == 0) // 1 = wall  0 = corridor
        {
            count++;
        }

        return count;
    }

    public int CountAllNeighbours(int x, int z)
    {
        return CountSquareNeighbours(x, z) + CountDiagonalNeighbours(x, z);
    }
}
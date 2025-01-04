using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FindPathAStar : MonoBehaviour
{
    // Algoritmo
    //
    // Paso 0 Añadimos la celda origen a la lista abierta.
    // Paso 1 Cogemos el primer elemento de la lista abierta y lo sacamos y lo insertamos en la lista cerrada.
    // Paso 2 Cogemos las celdas adyacentes a la celda extraída.
    // Paso 3 Para cada celda adyacente:
    //   A) Si la celda es la celda destino, hemos terminado.Recorremos inversamente la cadena de padres hasta llegar al origen
    //      para obtener el camino.
    //   B) Si la celda representa un muro o terreno infranqueable; la ignoramos.
    //   C) Si la celda ya está en la lista cerrada, la ignoramos.
    //   D) Si la celda ya está en la lista abierta, comprobamos si su nueva G(lo veremos más adelante) es mejor que la actual,
    //      en cuyo caso recalculamos factores y ponemos como padre de la celda a la celda extraída.
    //      En caso de que no sea mejor, la ignoramos.
    //   E) Para el resto de celdas adyacentes, les establecemos como padre la celda extraída y recalculamos factores.
    //      Después las añadimos a la lista abierta.
    // Paso 4 Ordenamos la lista abierta. La lista abierta es una lista ordenada de forma ascendente en función del factor F de las celdas.
    // Paso 5 Volver al paso 1.
    //
    // Factores
    //
    // Cada celda va a tener 3 factores.G, H y F.
    // G: Es el coste de ir desde la celda origen a la celda actual. Cada vez que nos movamos un paso en horizontal o vertical,
    //    añadiremos 10 puntos de coste. Cada vez que nos movamos en diagonal, añadiremos 14.
    //    ¿Por qué 14? Porque aunque geométricamente la proporción exacta debería ser 14.14213 {sqrt(10*10+10*10)},
    //    14 es una buena aproximación entera que nos hará ganar velocidad al evitar el uso de coma flotante
    // H: Es la distancia mínima y optimista, sin usar diagonales, que queda hasta el destino. La heurística basada en Distancia Manhattan.
    // F: Es la suma de G y H.
    //
    // https://www.lanshor.com/pathfinding-a-estrella/

    public Maze laberinto;

    public Material closedMaterial;
    public Material openMaterial;

    public GameObject start;
    public GameObject end;
    public GameObject pathPoint;

    private PathMarker nodoInicio;
    private PathMarker nodoFin;
    private PathMarker ultimaPosicion;

    private bool empezado = false;
    private bool terminado = false;
    private bool modoAutomatico = false;

    private List<PathMarker> listaMarcadoresAbiertos = new List<PathMarker>();
    private List<PathMarker> listaMarcadoresCerrados = new List<PathMarker>();

    private void Start()
    {
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            // generar inicio y fin

            BeginSearch();

            empezado = true;
        }

        if (empezado)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                // activar modo automático

                modoAutomatico = true;
            }

            if (!terminado && Input.GetKeyDown(KeyCode.C))
            {
                if (!modoAutomatico)
                {
                    // un paso cada vez

                    Search(ultimaPosicion);
                }
                else
                {
                    // automático hasta acabar

                    while (!terminado)
                    {
                        Search(ultimaPosicion);
                    }
                }
            }

            if (terminado && Input.GetKeyDown(KeyCode.M))
            {
                // mostrar camino

                GetPath();
            }
        }
    }

    private void BeginSearch()
    {
        terminado = false;

        RemoveAllMarkers();

        // generar el laberinto
        List<MapLocation> localizaciones = new List<MapLocation>();

        for (int z = 1; z < laberinto.largo - 1; ++z)
        {
            for (int x = 1; x < laberinto.ancho - 1; ++x)
            {
                if (laberinto.mapa[x, z] != 1) // 1 = wall  0 = corridor
                {
                    localizaciones.Add(new MapLocation(x, z));
                }
            }
        }

        localizaciones.Shuffle();

        // localización de inicio
        Vector3 startLocation = new Vector3(localizaciones[0].x * laberinto.escala, 0.0f, localizaciones[0].z * laberinto.escala);
        nodoInicio = new PathMarker(new MapLocation(localizaciones[0].x, localizaciones[0].z),
            0.0f, 0.0f, 0.0f, Instantiate(start, startLocation, Quaternion.identity), null);

        // localización de fin
        Vector3 endLocation = new Vector3(localizaciones[1].x * laberinto.escala, 0.0f, localizaciones[1].z * laberinto.escala);
        nodoFin = new PathMarker(new MapLocation(localizaciones[1].x, localizaciones[1].z),
            0.0f, 0.0f, 0.0f, Instantiate(end, endLocation, Quaternion.identity), null);

        // limpiar las listas de marcadores
        listaMarcadoresAbiertos.Clear();
        listaMarcadoresCerrados.Clear();

        // añadimos el nodo en el que empezamos
        listaMarcadoresAbiertos.Add(nodoInicio);

        // empezamos en el nodo inicial
        ultimaPosicion = nodoInicio;
    }

    private void Search(PathMarker nodoActual)
    {
        if (nodoActual == null)
        {
            return;
        }

        // si hemos llegado al final no hacemos nada
        if (nodoActual.Equals(nodoFin))
        {
            terminado = true;

            Debug.Log("DONE!");

            return;
        }

        // direcciones en las que están als localizaciones vecinas:
        // (1, 0)  - arriba
        // (0, 1)  - derecha
        // (-1, 0) - izquierda
        // (0, -1) - abajo
        foreach (MapLocation dir in laberinto.directions)
        {
            // vamos a comprobar las cuatro localizaciones vecinas a la actual

            MapLocation vecino = dir + nodoActual.location;

            // si la localización vecina es un muro pasar a la siguiente
            if (laberinto.mapa[vecino.x, vecino.z] == 1) // 1 = wall  0 = corridor
            {
                continue;
            }

            // si la localización vecina está en el borde del laberinto pasar a la siguiente
            if (vecino.x < 1 || vecino.x >= laberinto.ancho || vecino.z < 1 || vecino.z >= laberinto.largo)
            {
                continue;
            }

            // si la localización vecina está cerrada pasar a la siguiente
            if (IsClosed(vecino))
            {
                continue;
            }

            float g = Vector2.Distance(nodoActual.location.ToVector(), vecino.ToVector()) + nodoActual.G;
            float h = Vector2.Distance(vecino.ToVector(), nodoFin.location.ToVector());
            float f = g + h;

            GameObject pathBlock = Instantiate(pathPoint, new Vector3(vecino.x * laberinto.escala, 0.0f, vecino.z * laberinto.escala),
                Quaternion.identity);

            // recuperar los objetos de texto del marcador, para mostrar info en ellos
            TextMesh[] textos = pathBlock.GetComponentsInChildren<TextMesh>();

            textos[0].text = "G: " + g.ToString("0.00");
            textos[1].text = "H: " + h.ToString("0.00");
            textos[2].text = "F: " + f.ToString("0.00");

            if (!UpdateMarker(vecino, g, h, f, nodoActual))
            {
                listaMarcadoresAbiertos.Add(new PathMarker(vecino, g, h, f, pathBlock, nodoActual));
            }
        }

        // ----------------

        // pasar el primero de loa marcadores abiertos a la lista de marcadores cerrados

        // se ordenan los marcadores abiertos, por F y por H
        listaMarcadoresAbiertos = listaMarcadoresAbiertos.OrderBy(pm => pm.F).ThenBy(pm => pm.H).ToList();

        // se recupera el primero de los marcadores abiertos
        PathMarker pm = listaMarcadoresAbiertos[0];

        // se pasa a la lista de marcadores cerrados
        listaMarcadoresCerrados.Add(pm);

        // se elimina de la lista de marcadores abiertos
        listaMarcadoresAbiertos.RemoveAt(0);

        // ----------------

        // ponerle material al marcador
        pm.marcadorCamino.GetComponent<Renderer>().material = closedMaterial;

        // el marcador es la última posición
        ultimaPosicion = pm;
    }

    private void RemoveAllMarkers()
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");

        foreach (GameObject m in markers)
        {
            Destroy(m);
        }
    }

    private bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt)
    {
        foreach (PathMarker p in listaMarcadoresAbiertos)
        {
            if (p.location.Equals(pos))
            {
                p.G = g;
                p.H = h;
                p.F = f;

                p.parent = prt;

                return true;
            }
        }

        return false;
    }

    private bool IsClosed(MapLocation marker)
    {
        foreach (PathMarker p in listaMarcadoresCerrados)
        {
            if (p.location.Equals(marker))
            {
                return true;
            }
        }

        return false;
    }

    private void GetPath()
    {
        RemoveAllMarkers();

        PathMarker inicioPM = ultimaPosicion;

        while (inicioPM != null && !nodoInicio.Equals(inicioPM))
        {
            Instantiate(pathPoint, new Vector3(inicioPM.location.x * laberinto.escala, 0.0f, inicioPM.location.z * laberinto.escala), Quaternion.identity);

            inicioPM = inicioPM.parent;
        }

        Instantiate(pathPoint, new Vector3(nodoInicio.location.x * laberinto.escala, 0.0f, nodoInicio.location.z * laberinto.escala), Quaternion.identity);
    }
}

public class PathMarker
{
    public MapLocation location;
    public float G, H, F;
    public GameObject marcadorCamino;
    public PathMarker parent;

    public PathMarker(MapLocation l, float g, float h, float f, GameObject m, PathMarker p)
    {
        location = l;

        G = g;
        H = h;
        F = f;

        marcadorCamino = m;
        parent = p;
    }

    public override bool Equals(object obj)
    {
        if ((obj == null) || !GetType().Equals(obj.GetType()))
        {
            // NO es un PathMarker

            return false;
        }
        else
        {
            // es un PathMarker

            return location.Equals(((PathMarker)obj).location);
        }
    }

    public override int GetHashCode()
    {
        return 0;
    }
}
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class RushHourGrid : MonoBehaviour
{
    public enum TileState { Vacio, Ocupado, RedCar, Salida }
    public TileState[,] grid = new TileState[6, 6];

    [SerializeField] private int obstacleCarsCount = 4;
    [SerializeField] private GameObject maincarPrefab;
    [SerializeField] private List<GameObject> obstaclecarPrefab;
    [SerializeField] private GameObject obstaclecarLongPrefab;

    private List<GameObject> spawnedCars = new List<GameObject>();

    void Start()
    {
        //Definir salida
        grid[5, 1] = TileState.Salida;

        //Instanciar RedCar fijo
        Instantiate(maincarPrefab, new Vector3(0, 0, 1), maincarPrefab.transform.rotation);
        OccupyCells(new Vector2Int(0, 1), Car.Orientation.Horizontal, 2, TileState.RedCar);

        //Generar autos obstáculos
        for (int i = 0; i < obstacleCarsCount; i++)
        {
            //Prefab random (corto o largo)
            int totalPrefabs = obstaclecarPrefab.Count + 1;
            int prefabIndex = Random.Range(0, totalPrefabs);
            GameObject chosenPrefab = (prefabIndex < obstaclecarPrefab.Count) ? obstaclecarPrefab[prefabIndex] : obstaclecarLongPrefab;

            //Orientación random
            Car.Orientation orientation = (Random.value > 0.5f) ? Car.Orientation.Horizontal : Car.Orientation.Vertical;

            //Instanciar fuera de la grilla
            GameObject car = SpawnCar(chosenPrefab, new Vector3(-10f, 0f, -10f), orientation);
            var carComponent = car.GetComponent<Car>();
            if (carComponent == null)
            {
                Destroy(car);
                continue;
            }

            int length = carComponent.length;

            //Buscar posición válida
            Vector2Int gridPos = FindFreeGridPosition(orientation, length);
            Vector3 worldPos = new Vector3(gridPos.x, 0f, gridPos.y);

            //Colocar auto
            car.transform.position = worldPos;
            carComponent.position = gridPos;
            carComponent.length = length;
            carComponent.orientation = orientation;
            spawnedCars.Add(car);

            
            OccupyCells(gridPos, orientation, length, TileState.Ocupado);
        }

        //Mostrar grilla final
        PrintGridToConsole();

        //Probar solver
        RushHourSolver solver = gameObject.AddComponent<RushHourSolver>();
        solver.Solve(grid);
    }

    private void OccupyCells(Vector2Int start, Car.Orientation orientation, int length, TileState type)
    {
        for (int j = 0; j < length; j++)
        {
            int x = start.x + (orientation == Car.Orientation.Horizontal ? j : 0);
            int y = start.y + (orientation == Car.Orientation.Vertical ? j : 0);

            if (x >= 0 && x < 6 && y >= 0 && y < 6 && grid[x, y] == TileState.Vacio)
            {
                grid[x, y] = type;
            }
        }
    }

    private Vector2Int FindFreeGridPosition(Car.Orientation orientation, int length)
    {
        while (true)
        {
            Vector2Int pos = new Vector2Int(Random.Range(0, 6), Random.Range(0, 6));
            bool conflict = false;

            for (int j = 0; j < length; j++)
            {
                int x = pos.x + (orientation == Car.Orientation.Horizontal ? j : 0);
                int y = pos.y + (orientation == Car.Orientation.Vertical ? j : 0);

              
                if (x < 0 || x >= 6 || y < 0 || y >= 6)
                {
                    conflict = true;
                    break;
                }
            
                if (grid[x, y] != TileState.Vacio)
                {
                    conflict = true;
                    break;
                }
            }

            if (!conflict)
            {
                return pos;
            }
        }
    }

    private GameObject SpawnCar(GameObject car, Vector3 worldPos, Car.Orientation orientation)
    {
        return Instantiate(car, worldPos,
            (orientation == Car.Orientation.Horizontal) ? Quaternion.Euler(0, 90, 0) : Quaternion.identity);
    }

    private string GridToString()
    {
        var sb = new StringBuilder();
        sb.Append("   ");
        for (int x = 0; x < 6; x++) sb.Append(x).Append(' ');
        sb.AppendLine();

        for (int y = 5; y >= 0; y--)
        {
            sb.Append(y).Append("  ");
            for (int x = 0; x < 6; x++)
            {
                sb.Append(TileChar(grid[x, y])).Append(' ');
            }
            sb.AppendLine();
        }

        sb.AppendLine();
        sb.AppendLine("Leyenda: 0 = Vacio, X = Ocupado, R = RedCar, S = Salida");
        return sb.ToString();
    }

    private char TileChar(TileState t)
    {
        return t switch
        {
            TileState.Vacio => '0',
            TileState.Ocupado => 'X',
            TileState.RedCar => 'R',
            TileState.Salida => 'S',
            _ => '?',
        };
    }

    private void PrintGridToConsole()
    {
        Debug.Log("Grid:\n" + GridToString());
    }
}

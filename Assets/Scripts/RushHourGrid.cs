using System.Collections.Generic;
using System.Text;
using UnityEngine;
public class RushHourGrid : MonoBehaviour
{
    public enum TileState { Vacio, Ocupado, RedCar, Salida }
    public TileState[,] grid = new TileState[6, 6];

    [SerializeField]
    private int obstacleCarsCount = 4;

    [SerializeField] private GameObject maincarPrefab;

    [SerializeField] private List<GameObject> obstaclecarPrefab;
    [SerializeField] private GameObject obstaclecarLongPrefab;

    List<GameObject> spawnedCars = new List<GameObject>();

    void Start()
    {
        grid[5, 1] = TileState.Salida; // Set exit point

        // Instanciar el coche principal en posición fija (no forma parte del algoritmo de colocación aleatoria)
        Instantiate(maincarPrefab, new Vector3(0, 0, 1), maincarPrefab.transform.rotation);
        grid[0, 1] = TileState.RedCar;
        grid[1, 1] = TileState.RedCar;


        for (int i = 0; i < obstacleCarsCount; i++)
        {
            // Seleccionar aleatoriamente un prefab (incluye prefabs cortos y el largo)
            int totalPrefabs = obstaclecarPrefab.Count + 1; // +1 para obstaclecarLongPrefab
            int prefabIndex = Random.Range(0, totalPrefabs);
            GameObject chosenPrefab = (prefabIndex < obstaclecarPrefab.Count) ? obstaclecarPrefab[prefabIndex] : obstaclecarLongPrefab;

            // Elegir orientación aleatoria antes de instanciar (para que la rotación del prefab sea correcta)
            Car.Orientation orientation = (Random.value > 0.5f) ? Car.Orientation.Horizontal : Car.Orientation.Vertical;

            // Instanciar primero el coche en una posición temporal fuera de la rejilla
            Vector3 tempPos = new Vector3(-10f, 0f, -10f);
            GameObject car = SpawnCar(chosenPrefab, tempPos, orientation);

            var carComponent = car.GetComponent<Car>();
            if (carComponent == null)
            {
                // Si el prefab no tiene componente Car, eliminar y continuar
                Destroy(car);
                continue;
            }

            // Leer la longitud desde el prefab instanciado
            int length = carComponent.length;

            // Ahora buscar una posición válida en la rejilla con la longitud real del coche
            Vector2Int gridPos = FindFreeGridPosition(orientation, length);
            Vector3 worldPos = new Vector3(gridPos.x, 0f, gridPos.y);

            PrintGridToConsole();
            Debug.Log("/////////");

            // Colocar el coche en la posición encontrada y asignar datos al componente
            car.transform.position = worldPos;
            carComponent.position = gridPos;
            carComponent.length = length;
            carComponent.orientation = orientation;

            // Añadir a la lista de coches instanciados
            spawnedCars.Add(car);

            // Marcar las celdas ocupadas en la rejilla (sin sobrescribir Salida)
            for (int j = 0; j < length; j++)
            {
                int x = gridPos.x + (orientation == Car.Orientation.Horizontal ? j : 0);
                int y = gridPos.y + (orientation == Car.Orientation.Vertical ? j : 0);
                if (x >= 0 && x < 6 && y >= 0 && y < 6 && grid[x, y] == TileState.Vacio)
                {
                    grid[x, y] = TileState.Ocupado;
                }
            }
        }

        // Imprimir la rejilla en consola una vez creada
        PrintGridToConsole();

    }

    // Devuelve coordenadas de celda (x,y) libres para colocar un coche de 'length' y 'orientation'
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

                // Fuera de la rejilla o celda ya ocupada --> conflicto
                if (x < 0 || x >= 6 || y < 0 || y >= 6 || grid[x, y] != TileState.Vacio)
                {
                    conflict = true;
                    break;
                }
            }

            if (!conflict)
            {
                return pos;
            }

            // Si hay conflicto, se repite la búsqueda con otra posición aleatoria
        }
    }

    private GameObject SpawnCar(GameObject car, Vector3 worldPos, Car.Orientation orientation)
    {
        GameObject gm;

        if (orientation == Car.Orientation.Horizontal)
        {
            gm = Instantiate(car, worldPos, Quaternion.Euler(0, 90, 0));
        }
        else
        {
            gm = Instantiate(car, worldPos, Quaternion.identity);
        }

        return gm;
    }

    // Construye una representación textual de la rejilla y la devuelve
    private string GridToString()
    {
        var sb = new StringBuilder();
        // Encabezado de columnas
        sb.Append("   ");
        for (int x = 0; x < 6; x++) sb.Append(x).Append(' ');
        sb.AppendLine();

        // Filas: imprimimos y desde 5 (arriba) a 0 (abajo) para ver el grid con origen abajo
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
        sb.AppendLine("Leyenda: . = Vacio, X = Ocupado, R = RedCar, S = Salida");
        return sb.ToString();
    }

    // Mapea TileState a carácter
    private char TileChar(TileState t)
    {
        switch (t)
        {
            case TileState.Vacio: return '.';
            case TileState.Ocupado: return 'X';
            case TileState.RedCar: return 'R';
            case TileState.Salida: return 'S';
            default: return '?';
        }
    }

    // Imprime la rejilla usando Debug.Log (mantiene saltos de línea)
    private void PrintGridToConsole()
    {
        Debug.Log("Grid:\n" + GridToString());
    }
}
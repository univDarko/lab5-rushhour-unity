using System.Collections.Generic;
using UnityEngine;

public class RushHourSolver : MonoBehaviour
{
    private int gridSize = 6;
    [SerializeField] private int maxDepth = 50;

    //Estado del tablero como matriz
    public class State
    {
        public RushHourGrid.TileState[,] grid;
        public List<string> moves; //historial de movimientos

        public State(RushHourGrid.TileState[,] g, List<string> m = null)
        {
            grid = (RushHourGrid.TileState[,])g.Clone();
            moves = m == null ? new List<string>() : new List<string>(m);
        }
        public override int GetHashCode()
        {
            int hash = 17;
            foreach (var t in grid)
                hash = hash * 31 + (int)t;
            return hash;
        }
        public override bool Equals(object obj)
        {
            if (obj is not State other) return false;
            if (other.grid.GetLength(0) != grid.GetLength(0) || other.grid.GetLength(1) != grid.GetLength(1))
                return false;

            for (int x = 0; x < grid.GetLength(0); x++)
                for (int y = 0; y < grid.GetLength(1); y++)
                    if (grid[x, y] != other.grid[x, y]) return false;
            return true;
        }
    }

    //BFS
    public void Solve(RushHourGrid.TileState[,] startGrid)
{
    var start = new State(startGrid);
    Queue<State> frontier = new Queue<State>();
    HashSet<State> visited = new HashSet<State>();

    frontier.Enqueue(start);
    visited.Add(start);

    while (frontier.Count > 0)
    {
        var current = frontier.Dequeue();

        if (RedCarAtExit(current.grid))
        {
            Debug.Log($"Nivel resoluble en {current.moves.Count} movimientos.");
            return;
        }
        if (current.moves.Count >= maxDepth)
        {
            Debug.Log($"Se superó el límite de {maxDepth} movimientos. Nivel considerado irresoluble.");
            return;
        }
        foreach (var next in GetNextStates(current))
        {
            if (!visited.Contains(next))
            {
                frontier.Enqueue(next);
                visited.Add(next);
            }
        }
    }

    Debug.Log("No se encontró solución para este nivel.");
}

    private bool RedCarAtExit(RushHourGrid.TileState[,] grid)
    {
        int targetX = gridSize - 1;
        int targetY = 1;
        if (targetX < 0 || targetX >= gridSize) return false;
        if (targetY < 0 || targetY >= gridSize) return false;

        return grid[targetX, targetY] == RushHourGrid.TileState.RedCar;
    }

    private List<State> GetNextStates(State state)
    {
        List<State> neighbors = new List<State>();
        bool[,] processed = new bool[gridSize, gridSize];

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                //ignoramos vacio, salida y casillas ya procesadas
                if (processed[x, y]) continue;
                if (state.grid[x, y] == RushHourGrid.TileState.Vacio || state.grid[x, y] == RushHourGrid.TileState.Salida)
                    continue;

                RushHourGrid.TileState carType = state.grid[x, y];

                //Detectar orientación y largo del auto
                int length = 1;
                bool horizontal = false;

                //Miramos a la derecha para ver si forma segmento horizontal
                if (x + 1 < gridSize && state.grid[x + 1, y] == carType)
                {
                    horizontal = true;
                    int xx = x;
                    length = 0;
                    while (xx < gridSize && state.grid[xx, y] == carType)
                    {
                        processed[xx, y] = true;
                        length++;
                        xx++;
                    }
                }
                //Si no horizontal, miramos hacia arriba para vertical (y+1)
                else if (y + 1 < gridSize && state.grid[x, y + 1] == carType)
                {
                    horizontal = false;
                    int yy = y;
                    length = 0;
                    while (yy < gridSize && state.grid[x, yy] == carType)
                    {
                        processed[x, yy] = true;
                        length++;
                        yy++;
                    }
                }
                int startX = x;
                int startY = y;
                int endX = horizontal ? x + length - 1 : x;
                int endY = horizontal ? y : y + length - 1;

                //=========================
                //MOVIMIENTOS HORIZONTALES
                //=========================
                if (horizontal)
                {
                    //mover a la izquierda: comprobar celda startX - 1
                    int checkLeftX = startX - 1;
                    int checkY = startY;
                    if (checkLeftX >= 0)
                    {
                        //permitir moverse si la celda está vacía
                        if (state.grid[checkLeftX, checkY] == RushHourGrid.TileState.Vacio)
                        {
                            var newGrid = (RushHourGrid.TileState[,])state.grid.Clone();
                            //vaciar la celda del extremo derecho
                            newGrid[endX, startY] = RushHourGrid.TileState.Vacio;
                            //desplazar: poner carType en la nueva celda izquierda
                            newGrid[checkLeftX, checkY] = carType;
                            //las demás celdas del segmento quedan igual (no las tocamos)
                            var moves = new List<string>(state.moves);
                            moves.Add($"{carType} ({startX},{startY}) left");
                            neighbors.Add(new State(newGrid, moves));
                        }
                    }

                    //mover a la derecha: comprobar celda endX + 1
                    int checkRightX = endX + 1;
                    if (checkRightX < gridSize)
                    {
                        //permitir moverse si la celda está vacía OR es la salida y el coche es RedCar
                        bool targetIsFree = state.grid[checkRightX, startY] == RushHourGrid.TileState.Vacio ||
                                            (state.grid[checkRightX, startY] == RushHourGrid.TileState.Salida && carType == RushHourGrid.TileState.RedCar);

                        if (targetIsFree)
                        {
                            var newGrid = (RushHourGrid.TileState[,])state.grid.Clone();
                            //vaciar la celda del extremo izquierdo
                            newGrid[startX, startY] = RushHourGrid.TileState.Vacio;
                            //poner carType en la nueva celda derecha
                            newGrid[checkRightX, startY] = carType;
                            var moves = new List<string>(state.moves);
                            moves.Add($"{carType} ({startX},{startY}) right");
                            neighbors.Add(new State(newGrid, moves));
                        }
                    }
                }
                //=========================
                //MOVIMIENTOS VERTICALES
                //=========================
                else
                {
                    //mover hacia abajo: comprobar celda startY - 1
                    int checkDownY = startY - 1;
                    int checkX = startX;
                    if (checkDownY >= 0)
                    {
                        if (state.grid[checkX, checkDownY] == RushHourGrid.TileState.Vacio)
                        {
                            var newGrid = (RushHourGrid.TileState[,])state.grid.Clone();
                            //vaciar extremo superior
                            newGrid[startX, endY] = RushHourGrid.TileState.Vacio; // careful: endY es mayor
                            //poner car en la nueva celda inferior
                            newGrid[checkX, checkDownY] = carType;
                            var moves = new List<string>(state.moves);
                            moves.Add($"{carType} ({startX},{startY}) down");
                            neighbors.Add(new State(newGrid, moves));
                        }
                    }

                    //mover hacia arriba: comprobar celda endY + 1
                    int checkUpY = endY + 1;
                    if (checkUpY < gridSize)
                    {
                        bool targetIsFree = state.grid[startX, checkUpY] == RushHourGrid.TileState.Vacio ||
                                            (state.grid[startX, checkUpY] == RushHourGrid.TileState.Salida && carType == RushHourGrid.TileState.RedCar);

                        if (targetIsFree)
                        {
                            var newGrid = (RushHourGrid.TileState[,])state.grid.Clone();
                            //vaciar extremo inferior (startY)
                            newGrid[startX, startY] = RushHourGrid.TileState.Vacio;
                            //poner car en nueva celda superior
                            newGrid[startX, checkUpY] = carType;
                            var moves = new List<string>(state.moves);
                            moves.Add($"{carType} ({startX},{startY}) up");
                            neighbors.Add(new State(newGrid, moves));
                        }
                    }
                }
            }
        }
        return neighbors;
    }
}

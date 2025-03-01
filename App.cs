using System;

public class App
{
    public static void Main(string[] args)
    {
        TestGrid grid = new TestGrid(new List<string> {
            "########################################",
            "#           #           #              #",
            "#           #           #              #",
            "#           #           #              #",
            "#   #########           ########   #####",
            "#                ####                  #",
            "#       #        #  #                  #",
            "#       #        ####                  #",
            "###############            #############",
            "#             #            #           #",
            "#      ########            #           #",
            "#                          #           #",
            "########################################"
        });

        int cursorX = 0;
        int cursorY = 0;
        bool gettingEnd = false;
        Vector2Int start = new Vector2Int(0, 0);
        Vector2Int end = new Vector2Int(0, 0);
        List<Vector2Int>? path = null;
        
        GridPathFinder pathFinder = new GridPathFinder(grid);

        Console.Clear();
        Console.CursorVisible = false;
        bool done = false;
        while (!done) {
            Console.SetCursorPosition(0, 0);
            Console.Write($"({cursorX}, {cursorY}) - q=quit - Arrows=Move - Space=Select {(gettingEnd ? "end" : "start")}");
            grid.Draw(0, 1);
            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.White;
            if (path != null) {
                foreach (Vector2Int pathPos in path) {
                    Console.SetCursorPosition(pathPos.x, pathPos.y + 1);
                    Console.Write(" ");
                }
            }
            Console.ResetColor();
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.SetCursorPosition(cursorX, cursorY + 1);
            Console.Write("X");
            Console.ResetColor();

            ConsoleKeyInfo key = Console.ReadKey();
            Console.Clear();
            Console.SetCursorPosition(0, grid.Height + 1);
            if (key.KeyChar == 'q') {
                done = true;
            } else if (key.Key == ConsoleKey.UpArrow && cursorY > 0) {
                cursorY -= 1;
            } else if (key.Key == ConsoleKey.DownArrow && cursorY < grid.Height - 1) {
                cursorY += 1;
            } else if (key.Key == ConsoleKey.LeftArrow && cursorX > 0) {
                cursorX -= 1;
            } else if (key.Key == ConsoleKey.RightArrow && cursorX < grid.Width - 1) {
                cursorX += 1;
            } else if (key.Key == ConsoleKey.Spacebar) {
                if (!gettingEnd) {
                    start = new Vector2Int(cursorX, cursorY);
                    gettingEnd = true;
                } else {
                    end = new Vector2Int(cursorX, cursorY);
                    path = pathFinder.FindPath(start, end);
                    if (path != null) {
                        Console.WriteLine($"Found path with {path.Count} steps");
                    } else {
                        Console.WriteLine($"{end} can't be reached from {start}");
                    }
                    gettingEnd = false;
                }
            }
        }
        Console.CursorVisible = true;
    }
}

/*
This is out rest grid that we populate with a list of
strings defining what is blocked or free.
*/
public class TestGrid : IGridPathable
{
    private List<string> GridDef;
    public int Width;
    public int Height;
    private List<List<bool>> Blocked;

    // Take the list of strings for each grid row, and turn
    // them into the list of blocked grid cells.
    public TestGrid(List<string> gridDef)
    {
        GridDef = gridDef;
        Height = GridDef.Count;
        Width = 0;
        Blocked = new List<List<bool>>();
        foreach (string gridRow in GridDef)
        {
            if (gridRow.Length > Width)
            {
                Width = gridRow.Length;
            }
            List<bool> blockedRow = new List<bool>();
            foreach (char space in gridRow)
            {
                if (space == ' ')
                {
                    blockedRow.Add(false);
                }
                else
                {
                    blockedRow.Add(true);
                }
            }
            Blocked.Add(blockedRow);
        }
    }

    // Implement IGridPathable providing the list of neighbors
    // for a cell.  We're just returning the up, down, left, and
    // right neighbors if they are not blocked.
    public List<Vector2Int> GetPathableNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        if (cell.x >= 0 && cell.x < Width && cell.y >= 0 && cell.y < Height)
        {
            // Up
            if (cell.y > 0 && !Blocked[cell.y - 1][cell.x])
            {
                neighbors.Add(new Vector2Int(cell.x, cell.y - 1));
            }
            // Down
            if (cell.y < Height - 1 && !Blocked[cell.y + 1][cell.x])
            {
                neighbors.Add(new Vector2Int(cell.x, cell.y + 1));
            }
            // Left
            if (cell.x > 0 && !Blocked[cell.y][cell.x - 1])
            {
                neighbors.Add(new Vector2Int(cell.x - 1, cell.y));
            }
            // Right
            if (cell.x < Width - 1 && !Blocked[cell.y][cell.x + 1])
            {
                neighbors.Add(new Vector2Int(cell.x + 1, cell.y));
            }
        }
        return neighbors;
    }

    // Simple draw method that will print out the grid definition
    // to the console with the blocked cells as red.
    public void Draw(int x, int y)
    {
        int rowNum = 0;
        foreach (string gridRow in GridDef)
        {
            int colNum = 0;
            foreach (char space in gridRow)
            {
                if (Blocked[rowNum][colNum])
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.SetCursorPosition(x + colNum, y + rowNum);
                Console.Write(space);
                Console.ResetColor();
                colNum += 1;
            }
            rowNum += 1;
        }
    }
}
using System.Runtime.CompilerServices;

Pit[] pits;

ChooseMapSize();
Console.Clear();

Map.InitializeTiles();
Map.Mark("X", Player.Location);
Intro.ColoredMessage();

while (!Player.PlayerWins)
{
    Player.Move();
}

Console.ReadKey();

void ChooseMapSize()
{
    Console.WriteLine("Choose map size: ");
    Console.WriteLine("1 - Small map (4x4)");
    Console.WriteLine("2 - Medium map (6x6)");
    Console.WriteLine("3 - Large map (8x8)");
    string? input = Console.ReadLine();
    if (int.TryParse(input, out int num) && num >= 1 && num <= 3)
    {
        if (num == 1)
        {
            Map.Size = (4, 4);
            Fountain.Location = (0, 2);
            pits = [new Pit(3, 2)];
        }
        else if (num == 2)
        {
            Map.Size = (6, 6);
            Fountain.Location = (2, 4);
            Pit[] pits = [new Pit(3, 2), new Pit(0, 5)];
        }
        /*Map.Size = num switch
        {
            1 => (4,4),
            2 => (6,6),
            3 => (8,8),
            _ => (0,0)
        };*/
    }
    else
    {
        Console.Clear();
        ChooseMapSize();
    }
}


static class Map
{
    public static (int row, int column) Size { get; set; }
    private static string[,] Tiles { get; set; } = new string[Size.row, Size.column];
    public static (int row, int column) Entrance { get; } = (0, 0);


    public static void InitializeTiles()
    {
        Tiles=new string[Size.row, Size.column];
        for(int i=0; i<Size.row;i++)
        {
            for (int j = 0; j < Size.column; j++)     
            {
                Tiles[i, j] = " ";
            }
        }
    }

    public static void Mark(string mark, (int row, int col) location)
    {
        Console.WriteLine();
        int tilenum = 0;
        for (int i = 0; i < Size.row; i++)
        {
            Console.Write("|");
            for (int j = 0; j < Size.column; j++)
            {
                if (i == location.row && j == location.col)
                {
                    Tiles[i,j]=mark;
                    Console.Write($" {Tiles[i,j]} |");
                }
                else Console.Write($" {Tiles[i,j]} |");
                tilenum++;
            }
            Console.WriteLine();
            for(int k=0; k<=Size.column*4; k++)
            {
                Console.Write("-");
            }
            Console.WriteLine();
        }
    }

    public static void RemovePlayerMark()
    {
        Tiles[Player.Location.row, Player.Location.column] = " ";
    }
}



static class Player
{
    public static bool PlayerWins { get; private set; } = false;
    public static bool IsDead { get; private set; } = false;
    public static (int row, int column) Location { get; private set; } = (0, 0);
    public static void Move()
    {
        int row = Location.row;
        int column = Location.column;
        ConsoleKey key=Console.ReadKey().Key;
        Map.RemovePlayerMark();
        Console.Clear();
        if (key == ConsoleKey.RightArrow && (column + 1) < Map.Size.column) Location=(row,++column);
        else if (key == ConsoleKey.LeftArrow && (column - 1) >= 0) Location= (row,--column);
        else if (key == ConsoleKey.UpArrow && (row - 1) >= 0) Location=(--row,column);
        else if (key == ConsoleKey.DownArrow && (row + 1) < Map.Size.row) Location= (++row,column);
        Map.Mark("X", Location);
        CheckRoom();
    }

    private static void CheckRoom()
    {
        
        Object[] objects = [Fountain.];


        /*if (Player.Location == Map.Entrance)
        {
            if (!Fountain.IsActivated) Entrance.ColoredMessage();
            else
            {
                PlayerSuccess.ColoredMessage();
                Player.PlayerWins = true;
            }
        }
        else if (Player.Location == Fountain.Location)
        {
            if (!Fountain.IsActivated)
            {
                FountainFound.ColoredMessage();
                Fountain.Reactivate();
            }
            if (Fountain.IsActivated)
            {
                FountainActivated.ColoredMessage();
            }
        }*/
    }
}


static class Fountain
{
    public static bool IsActivated { get; private set; } = false;
    public static (int row, int col) Location { get; set; }

    public static void Reactivate()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Press ENTER to reactivate fountain.");
        Console.ForegroundColor = ConsoleColor.White;
        if (Console.ReadKey().Key == ConsoleKey.Enter)
        {
            IsActivated = true;
        }
        else Console.WriteLine("Fountain not activated.");
    }
}

class Pit
{
    public (int row, int col) Location { get; set; }
    public Pit(int _row,int _col)
    {
        Location=(_row, _col);
    }
}


public abstract class Narrative
{
    public static ConsoleColor TextColor { get; protected set; }
    public static string? Message { get; protected set; }
    protected static void ShowMessage() 
    {
        Console.ForegroundColor = TextColor;
        Console.WriteLine(Message);
        Console.ForegroundColor = ConsoleColor.White;
    }
}

public class Intro : Narrative
{
    public static void ColoredMessage()
    {
        TextColor = ConsoleColor.Magenta;
        Message= "\nYou have entered the Cavern of Objects.\nUse UP, DOWN, LEFT or RIGHT arrow buttons to move.";
        ShowMessage();
    }
    
}

class Entrance : Narrative
{
    public static void ColoredMessage()
    {
        TextColor = ConsoleColor.Yellow;
        Message = "\nYou see light in this room coming from outside the cavern.\nThis is the entrance.";
        ShowMessage();
    }
}

class FountainFound:Narrative
{
    public static void ColoredMessage()
    {
        TextColor = ConsoleColor.Blue;
        Message = "\nYou hear water dripping in this room.\nThe Fountain of Objects is here!";
        ShowMessage();
    }
}

class FountainActivated:Narrative
{

    public static void ColoredMessage()
    {
        TextColor = ConsoleColor.Blue;
        Message = "\nYou hear the rushing waters from the Fountain of Objects. It has been reactivated!\nGo back to cave entrance to exit.";
        ShowMessage();
    }
}

class PlayerSuccess : Narrative
{
    public static void ColoredMessage()
    {
        TextColor = ConsoleColor.Yellow;
        Message = "\nSafely exited the cave! You win!";
        ShowMessage();
    }
}
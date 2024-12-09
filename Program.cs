

bool exitGame=false;

ChooseMapSize();
Console.Clear();


Map.Mark("X", Player.Location);
Intro.ColoredMessage();
if (Player.IfNearbyRoom(RoomType.Pit)) NearPit.ColoredMessage();

while (!exitGame)
{
    Player.Move();
    if(Player.PlayerWins==true || Player.IsDead==true)
    {
        exitGame = true;
    }
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
        Map.InitializeTiles(num);
        Rooms.Generate(num);
    }
    else
    {
        Console.Clear();
        ChooseMapSize();
    }
}


static class Map
{
    public static (int row, int column) Size { get; private set; }
    private static string[,] Tiles { get; set; } = new string[Size.row, Size.column];
    public static (int row, int column) Entrance { get; } = (0, 0);


    public static void InitializeTiles(int num)
    {
        if (num == 1)
        {
            Map.Size = (4, 4);
        }
        else if (num == 2)
        {
            Map.Size = (6, 6);
        }
        else if (num == 3)
        {
            Map.Size = (8, 8);
        }

        Tiles =new string[Size.row, Size.column];
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

    public static void MarkRoom(string mark, (int row, int col)location)
    {
        Tiles[location.row, location.col] = mark;
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
        if (IfNearbyRoom(RoomType.Pit) && PlayerWins==false && IsDead==false) NearPit.ColoredMessage();
    }

    private static void CheckRoom()
    {
        if (Rooms.RoomDescription[Location.row,Location.column]==RoomType.Entrance)
        {
            if (!Fountain.IsActivated) Entrance.ColoredMessage();
            else
            {
                PlayerSuccess.ColoredMessage();
                PlayerWins = true;
                return;
            }
        }
        else if (Rooms.RoomDescription[Location.row, Location.column] == RoomType.FountainRoom)
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
        }
        else if (Rooms.RoomDescription[Location.row, Location.column] == RoomType.Pit)
        {
            IsDead = true;
            PlayerDied.ColoredMessage();
            return;
        }
    }
    public static bool IfNearbyRoom(RoomType roomType)
    {
        (int, int)[] sequence = new(int, int) []{(1,0),(-1,0),(0,1),(0,-1),(1,1),(-1,-1),(1,-1),(-1,1) };
        foreach((int x, int y) seq in sequence)
        {
            try 
            {
                if (Rooms.RoomDescription[Location.row + seq.x, Location.column + seq.y] == roomType) return true;
            }
            catch (IndexOutOfRangeException)
            {
                continue;
            }
        }
        return false;
    }
}


static class Fountain
{
    public static bool IsActivated { get; private set; } = false;
    public static (int row, int col) Location { get; private set; }

    public static void SetLocation()
    {
        Random random = new Random();
        Location = (random.Next(0, Map.Size.row-1), random.Next(0, Map.Size.column-1));
    }

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


struct Rooms
{
    public static RoomType[,] RoomDescription { get; private set; } = new RoomType[0, 0];
    public static (int row, int col)[] Pits { get; private set; }=new (int row, int col)[0];

    public static void Generate(int numberOfPits)
    {
        RoomDescription = new RoomType[Map.Size.row, Map.Size.column];
        for (int i = 0; i < Map.Size.row; i++)
        {
            for (int j = 0; j < Map.Size.column; j++)
            {
                RoomDescription[i, j] = RoomType.Blank;
            }
        }
        RoomDescription[Map.Entrance.row, Map.Entrance.column] = RoomType.Entrance;
        GeneratePits(numberOfPits);
        GenerateFountainRoom();
    }   
     
    private static void GeneratePits (int numberOfPits)
    { 
        Random random = new Random();
        Pits = new (int row, int col)[numberOfPits];
        for (int i = 0; i < numberOfPits; i++)
        {
            int x=random.Next(0,Map.Size.row-1);
            int y=random.Next(0, Map.Size.column-1);
            if (RoomDescription[x,y]==RoomType.Blank)
            {
                RoomDescription[x,y] = RoomType.Pit;
                Pits[numberOfPits-1] = (x, y);
                Map.MarkRoom("!",(x, y));
            }
        }
    }

    private static void GenerateFountainRoom()
    {
        Fountain.SetLocation();
        if (RoomDescription[Fountain.Location.row, Fountain.Location.col] == RoomType.Blank)
        {
            RoomDescription[Fountain.Location.row, Fountain.Location.col] = RoomType.FountainRoom;
        }
        else GenerateFountainRoom();
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

class NearPit:Narrative
{
    public static void ColoredMessage()
    {
        TextColor = ConsoleColor.Red;
        Message = "\nYou feel a draft.\nThere is a pit in a nearby room.";
        ShowMessage();
    }
}

class PlayerDied : Narrative
{
    public static void ColoredMessage()
    {
        TextColor = ConsoleColor.DarkRed;
        Message = "\nYou fell into a deep pit!.\nYou died.";
        ShowMessage();
    }
}

enum RoomType { Blank, Entrance, FountainRoom, Pit}
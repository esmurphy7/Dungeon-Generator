/* TODO:
   Optimization: 
 * Restrictions: Each dimension must be at least 3
 */

using System;
using System.Collections;

/* Struct that represents each room */
struct Room
{
    public int x;
    public int y;
    public int w;
    public int h;

    public void print()
    {
        Console.WriteLine(" x->{0}\n y->{1}\n w->{2}\n h->{3}", this.x, this.y, this.w, this.h);
    }
};
/* Struct that represents a specific point in the map array*/
struct Point
{
    public int x;
    public int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
};
/* Encodings for different cell types */
enum cellTypes
{
    EMPTY = 0,
    CORNER,
    FLOOR,
    CORRIDOR,
    VERT_WALL,
    HORZ_WALL,
    START,
    DEBUG
};

public class DungeonGenerator
{
    private bool DEBUG = false;
    private int seed;
    Random rand;

    private int map_width;
    private int map_height;
    private int[,] map;

    private ArrayList rooms;
    private int room_count = 5;
    private int min_wall_length = 3;
    private int max_room_width;
    private int max_room_height;

    public DungeonGenerator(int map_width, int map_height, int seed)
	{
        this.seed = seed;
        this.rand = new Random(seed);
        this.map_width = map_width;
        this.map_height = map_height;
        this.map = new int[map_width, map_height];
     
        this.room_count = (int)((map_width+map_height)*0.10);
        
        this.max_room_width = (int)(map_width/4)+1;
        this.max_room_height = (int)(map_height/4)+1;
        if (this.max_room_width < 2)
        {
            this.min_wall_length = 2;
            this.max_room_width = 2;
        }
        if (this.max_room_height < 2)
        {
            this.min_wall_length = 2;
            this.max_room_height = 2;
        }
        //Console.WriteLine("room_count->{0}\nmax_room_width->{1}\nmax_room_height->{2}\n", room_count, max_room_width, max_room_height);
        this.rooms = new ArrayList();
	}

    /* Generate a set of random rooms with non-intersecting walls */
    public void generateRooms()
    {
        for (int i = 0; i < room_count; i++)
        {
            Room newRoom = create_randomRoom();
            // If the room intersects with any others, or it is off the map, go back and try again

            if (DEBUG)
            {
                int original = map[newRoom.x, newRoom.y];
                map[newRoom.x, newRoom.y] = (int)cellTypes.DEBUG;
                encodeRooms();
                Console.WriteLine();
                Console.WriteLine("Attempting to add Room...");
                newRoom.print();
                printColoredMap();
                map[newRoom.x, newRoom.y] = original;
                System.Threading.Thread.Sleep(1000);
            }
            if (roomIntersects(newRoom) || roomOffMap(newRoom))
            {
                i--;
                continue;
            }
            // Shrink the room to ensure it doesn't share a wall with another
            newRoom.w--;
            newRoom.h--;
            rooms.Add(newRoom);
            if (DEBUG)
            {
                newRoom.print();
                Console.WriteLine("Room {0} added", i);
                encodeRooms();
                printColoredMap();
            }
             
        }

    }

    /* Connect rooms with corridors */
    public void connectRooms()
    {
        foreach (Room roomA in rooms)
        {
            Room roomB = findNearestRoom(roomA);
            // Create 2 random points in the room and its nearest room
            Point pointA = new Point(rand.Next(roomA.x, roomA.x + roomA.w),
                                     rand.Next(roomA.y, roomA.y + roomA.h));
            Point pointB = new Point(rand.Next(roomB.x, roomB.x + roomB.w),
                                     rand.Next(roomB.y, roomB.y + roomB.h));
            // Move the pointB closer and closer until it reaches pointA, encoding a corridor along the way
            while(pointA.x != pointB.x || pointA.y != pointB.y)
            {
                if(pointA.x != pointB.x)
                {
                    if (pointA.x < pointB.x) pointB.x--;
                    else pointB.x++;
                }
                else if(pointA.y != pointB.y)
                {
                    if (pointA.y < pointB.y) pointB.y--;
                    else pointB.y++;
                    
                }
                map[pointB.x, pointB.y] = (int)cellTypes.CORRIDOR;
            }
        }
    }

    /* Identify cells that should be a wall, and set them to type "HORZ_WALL" by default */
    public void addWalls()
    {
        // For each cell
        for (int x=0; x < map_width; x++)
        {
            for (int y = 0; y < map_height; y++)
            {          
                // Assert that the current cell is a floor, corridor, corner, or start cell
                if(map[x,y] == (int)cellTypes.FLOOR || 
                    map[x,y] == (int)cellTypes.CORRIDOR ||
                    map[x,y] == (int)cellTypes.CORNER ||
                    map[x, y] == (int)cellTypes.START)
                { 
                    // For each cell adjacent the current cell
                    for (int xx = x-1; xx <= x+1; xx++)
                    {
                        for(int yy = y-1; yy <= y+1; yy++)
                        {
                            if (xx < map_width && xx >= 0 &&
                                yy < map_height && yy >= 0)
                            {
                                // If the adjacent cell is empty, make a horizontal wall
                                if (map[xx, yy] == (int)cellTypes.EMPTY)
                                {
                                    map[xx, yy] = (int)cellTypes.HORZ_WALL;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /* Check if parameter room intersects with any others in the list of rooms created so far */
    private bool roomIntersects(Room room)
    {
        foreach (Room curRoom in rooms)
        {
            // If the current room is the parameter room, ignore it
            if(room.Equals(curRoom)) continue;
            /* //Debug code
            Console.WriteLine("room's bottom edge->{0}",(room.x + room.w)-1);
            Console.WriteLine("room's top edge->{0}", room.x);
            Console.WriteLine("room's right edge->{0}", (room.y + room.h)-1);
            Console.WriteLine("room's left edge->{0}", room.y);

            Console.WriteLine("curRoom's top edge->{0}", curRoom.x);
            Console.WriteLine("curRoom's bottom edge->{0}", (curRoom.x + curRoom.w)-1);
            Console.WriteLine("curRoom's left edge->{0}", curRoom.y);
            Console.WriteLine("curRoom's right edge->{0}", (curRoom.y + curRoom.h)-1);
             * */
            
            // Box collision checking
            if(!((room.x + room.w)-1 < curRoom.x) &&    // room's right edge is left of curRoom's left
                 !(room.x > (curRoom.x + curRoom.w)-1) && // room's left edge is right of curRoom's right
                 !((room.y + room.h)-1 < curRoom.y) &&   // room's bottom edge is above curRoom's top
                 !(room.y > (curRoom.y + curRoom.h)-1 )) // room's top edge is below curRoom's bottom
            {
                return true;
            }
        }
        return false;
    }

    /* Check if given room has at least one cell off the map */
    private bool roomOffMap(Room room)
    {
        if(room.x < 0 ||
            room.x + room.w > map_width ||
            room.y < 0 ||
            room.y + room.h > map_height)
        {
            return true;
        }
        return false;
    }

    /* Create and return a room with random co-ordinates and dimensions */
    private Room create_randomRoom()
    {
        Room room = new Room();
        try
        {
            // Init random room
            room.x = rand.Next(1, map_width);
            room.y = rand.Next(1, map_height);
            room.w = rand.Next(min_wall_length, max_room_width);
            room.h = rand.Next(min_wall_length, max_room_height);
        }
        catch (ArgumentOutOfRangeException e)
        {
            // Shut up compiler warning
            e.ToString();
            /*
            Console.WriteLine();
            Console.WriteLine("x min {0}, x max {1}", 1, map_width);
            Console.WriteLine("width min {0}, width max {1}", min_wall_length, max_room_width + 1);
            Console.WriteLine("y min {0}, y max {1}", 1, map_height);
            Console.WriteLine("height min {0}, height max {1}", min_wall_length, max_room_height + 1);
             * */
            // If the max value is smaller than the min, try again with the max incremented
            room.w = rand.Next(min_wall_length, max_room_width + 1);
            room.h = rand.Next(min_wall_length, max_room_height + 1);     
        }
        return room;
    }

    /* In the list of created rooms, find and return the nearest room to the parameter room */
    private Room findNearestRoom(Room room)
    {
        // Midpoint of parameter room
        Point midPoint = new Point(room.x + (room.w / 2),
                                    room.y + (room.h / 2));
        Room nearestRoom = new Room();
        int nearest_distance = 1000000;
        // For each room
        foreach (Room curRoom in rooms)
        {
            // If the current room is the parameter room, ignore it
            if (curRoom.Equals(room)) continue;
            // Current room's midpoint
            Point cur_midPoint = new Point(curRoom.x + (curRoom.w / 2),
                                            curRoom.y + (curRoom.h / 2));
            // Calculate distance between parameter room and current room
            int distance = (int)Math.Sqrt((midPoint.x - cur_midPoint.x)^2 + (midPoint.y - cur_midPoint.y)^2);
            // Check if the current room qualifies as the new closest room
            if(distance < nearest_distance)
            {
                nearest_distance = distance;
                nearestRoom = curRoom;
            }
        }
        if (nearestRoom.Equals(null))
        {
            Console.WriteLine("Attempted to return null nearest room");
            System.Environment.Exit(1);
        }
        return nearestRoom;
    }

    /* Encode the map array with the set of rooms */
    public void encodeRooms()
    {
        foreach (Room room in rooms)
        {
            for (int x = room.x; x < room.x + room.w; x++)
            {
                for (int y = room.y; y < room.y + room.h; y++)
                {
                    map[x, y] = (int)cellTypes.FLOOR;
                    if (x == room.x && y == room.y)
                    {
                        // Encode top-left corner of the room
                        map[x, y] = (int)cellTypes.CORNER;
                        // If the current room is the first room, encode the start point
                        if (room.Equals((Room)rooms[0]))
                            map[x, y] = (int)cellTypes.START;
                    }
                }
            }
        }
    }

    /* Encode the map array with separate types for vertical and horizontal walls */
    public void encodeWalls()
    {
        int prevCell = (int)cellTypes.EMPTY;
        // For each cell
        for (int x = 0; x < map_width; x++)
        {
            for (int y = 0; y < map_height; y++)
            {
                int curCell = map[x, y];        
                // If the current cell is a horizontal wall (all walls are horizontal by default)
                // And the previous cell is not
                if(curCell == (int)cellTypes.HORZ_WALL &&
                    prevCell != (int)cellTypes.HORZ_WALL)
                {
                    // Check the next cell
                    if (y + 1 < map_height)
                    {
                        int nextCell = map[x, y + 1];
                        // If the next cell is not a horizontal wall, make it a vertical wall
                        if (nextCell != (int)cellTypes.HORZ_WALL)
                            map[x, y] = (int)cellTypes.VERT_WALL;
                    }
                    else
                    {
                        map[x, y] = (int)cellTypes.VERT_WALL;
                    }
                }
                prevCell = curCell;
            }
        }
    }

    /* Print a colorized number version of the map to the console */
    public void printColoredMap()
    {
        for (int i = 0; i < map_width; i++)
        {
            for (int j = 0; j < map_height; j++)
            {
                int cell = map[i,j];
                ConsoleColor color = ConsoleColor.Gray;
                switch(cell)
                {
                    case (int)cellTypes.EMPTY:
                        color = ConsoleColor.Gray;
                        break;
                    case (int)cellTypes.CORNER:
                        color = ConsoleColor.Blue;
                        break;
                    case (int)cellTypes.FLOOR:
                        color = ConsoleColor.Cyan;
                        break;
                    case (int)cellTypes.CORRIDOR:
                        color = ConsoleColor.Green;
                        break;
                    case (int)cellTypes.VERT_WALL:
                        color = ConsoleColor.Yellow;
                        break;
                    case (int)cellTypes.HORZ_WALL:
                        color = ConsoleColor.Yellow;
                        break;
                    case (int)cellTypes.START:
                        color = ConsoleColor.Magenta;
                        break;
                    case (int)cellTypes.DEBUG:
                        color = ConsoleColor.Red;
                        break;
                    default:
                        color = ConsoleColor.Gray;
                        break;
                }
                ColoredConsoleWrite(color, cell);
            }
            Console.WriteLine();
        }
    }

    /* Print the textual representation of the map */
    public void printTextMap()
    {
        for (int i = 0; i < map_width; i++)
        {
            for (int j = 0; j < map_height; j++)
            {
                int cell = map[i, j];
                char character = ' ';
                switch (cell)
                {
                    case (int)cellTypes.EMPTY:
                        character = ' ';
                        break;
                    case (int)cellTypes.CORNER:
                        character = ' ';
                        break;
                    case (int)cellTypes.FLOOR:
                        character = ' ';
                        break;
                    case (int)cellTypes.CORRIDOR:
                        character = ' ';
                        break;
                    case (int)cellTypes.VERT_WALL:
                        character = '|';
                        break;
                    case (int)cellTypes.HORZ_WALL:
                        character = '-';
                        break;
                    case (int)cellTypes.START:
                        character = '*';
                        break;
                    case (int)cellTypes.DEBUG:
                        character = '$';
                        break;
                    default:
                        character = ' ';
                        break;
                }
                Console.Write(character);
            }
            Console.WriteLine();
        }
    }

    /* Print all rooms added to the map so far */
    private void printRooms()
    {
        int i = 0;
        foreach (Room room in rooms)
        {
            Console.WriteLine("Room {0}:\n x->{1} \n y->{2}\n", i, room.x, room.y);
            i++;
        }
    }

    /* Print a cell of a specific color to the console */
    public static void ColoredConsoleWrite(ConsoleColor color, int cellType)
    {
        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(cellType);
        Console.ForegroundColor = originalColor;
    }

    public static void Main(string[] args)
    {
        string usage = "\tUsage: $ DungeonGenerator.exe <width> <height> <seed>\n\tNote: Each dimension must be at least 3.\n\t\tAn average dimension size of 10 is optimal.";
        if (args.Length < 3)
        {
            Console.WriteLine(usage);
            System.Environment.Exit(1);
        }
        int width = Convert.ToInt32(args[0]);
        int height = Convert.ToInt32(args[1]);
        int seed = Convert.ToInt32(args[2]);

        if(width < 3 || height < 3)
        {
            Console.WriteLine(usage);
            System.Environment.Exit(1);
        }

        DungeonGenerator generator = new DungeonGenerator(width, height, seed);
        generator.generateRooms();
        generator.connectRooms();
        generator.encodeRooms();
        generator.addWalls();
        generator.encodeWalls();
        //generator.printColoredMap();
        generator.printTextMap();
    }
}

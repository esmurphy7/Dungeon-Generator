/* TODO: Dynamically set room_count, min_wall_length, max_wall_length, and default nearest_distance based on command line options 
 *      Encode separate types for vertical and horizontal walls
 *      Variable Corridor size
 *      Place an entrance
        Optimization: 
 */

using System;
using System.Collections;

// map[y][x], y = row (height), x = column (width)

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

enum directions : int 
{ 
    W = 1,
    E,
    N, 
    S 
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
    private int min_wall_length = 2;
    private int max_room_width;
    private int max_room_height;

    public DungeonGenerator(int map_width, int map_height, int seed)
	{
        this.seed = seed;
        this.rand = new Random(seed);
        this.map_width = map_width;
        this.map_height = map_height;
        this.map = new int[map_width, map_height];
        this.room_count = (int)((map_width+map_height)*0.18);
        this.max_room_width = (int)(map_width/4)+1;
        this.max_room_height = (int)(map_height/4)+1;
        Console.WriteLine("room_count->{0}\nmax_room_width->{1}\nmax_room_height->{2}\n", room_count, max_room_width, max_room_height);
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
                encodeMap();
                //Console.WriteLine();
                //Console.WriteLine("Attempting to add Room...");
                newRoom.print();
                printMap();
                map[newRoom.x, newRoom.y] = original;
                System.Threading.Thread.Sleep(1000);
            }
            if (roomIntersects(newRoom) || roomOffMap(newRoom))
            {
                //Console.WriteLine("Room intersects", i);
                //if (roomOffMap(newRoom)) Console.WriteLine("Room is off Map", i);
                i--;
                continue;
            }
            // Shrink the room to ensure it doesn't share a wall with another (resulting in one big room)
            newRoom.w--;
            newRoom.h--;
            rooms.Add(newRoom);
            if (DEBUG)
            {
                newRoom.print();
                Console.WriteLine("Room {0} added", i);
                encodeMap();
                printMap();
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
            // Move the pointB closer and closer until it reaches pointA, drawing a corridor along the way
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
        encodeMap();
    }

    public void addWalls()
    {
        //Console.WriteLine("Adding walls...");
        for (int x=0; x < map_width; x++)
        {
            for (int y = 0; y < map_height; y++)
            {                 
                if(map[x,y] == (int)cellTypes.FLOOR || 
                    map[x,y] == (int)cellTypes.CORRIDOR ||
                    map[x,y] == (int)cellTypes.CORNER ||
                    map[x, y] == (int)cellTypes.START)
                { 
                    for (int xx = x-1; xx <= x+1; xx++)
                    {
                        for(int yy = y-1; yy <= y+1; yy++)
                        {
                            /*
                            Console.WriteLine("Original cell: [{0},{1}]", x, y);
                            Console.WriteLine("Current cell: [{0},{1}]",xx,yy);
                            int original = map[x, y];
                            int originalxxyy = map[xx, yy];
                            map[x, y] = (int)cellTypes.DEBUG;
                            map[xx, yy] = (int)cellTypes.DEBUG;
                            printMap();
                            map[x, y] = original;
                            map[xx, yy] = originalxxyy;
                             * */
                            //System.Threading.Thread.Sleep(2000);
                            if (map[xx, yy] == (int)cellTypes.EMPTY)
                            {
                                /*
                                // check if the empty cell found is N,E,S,or W of original cell
                                int dir = 0;
                                int type = 0;
                                if (x - xx > 0) dir = (int)directions.W;
                                else if (x - xx < 0) dir = (int)directions.E;
                                else if (y - yy > 0) dir = (int)directions.N;
                                else if (y - yy < 0) dir = (int)directions.S;
                                // N or S, HORZ_WALL
                                if (dir == (int)directions.N || dir == (int)directions.S) type = (int)cellTypes.HORZ_WALL;
                                // W or E, VERT_WALL
                                else if (dir == (int)directions.W || dir == (int)directions.E) type = (int)cellTypes.VERT_WALL;
                                map[xx,yy] = type;
                                    */
                                map[xx, yy] = (int)cellTypes.HORZ_WALL;

                            }
                        }
                    }
                }
            }
        }
    }

    /* Check if given room intersects with any others in the list of rooms created so far */
    private bool roomIntersects(Room room)
    {
        foreach (Room curRoom in rooms)
        {
            // If the current room is the parameter room, ignore it
            if(room.Equals(curRoom)) continue;
            /*
            Console.WriteLine("room's bottom edge->{0}",(room.x + room.w)-1);
            Console.WriteLine("room's top edge->{0}", room.x);
            Console.WriteLine("room's right edge->{0}", (room.y + room.h)-1);
            Console.WriteLine("room's left edge->{0}", room.y);

            Console.WriteLine("curRoom's top edge->{0}", curRoom.x);
            Console.WriteLine("curRoom's bottom edge->{0}", (curRoom.x + curRoom.w)-1);
            Console.WriteLine("curRoom's left edge->{0}", curRoom.y);
            Console.WriteLine("curRoom's right edge->{0}", (curRoom.y + curRoom.h)-1);
             * */
            
            
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

    /* Create and return a room with random co-odinates and dimensions */
    private Room create_randomRoom()
    {
        Room room = new Room();
        try
        {
            
            room.x = rand.Next(1, map_width);
            room.y = rand.Next(1, map_height);
            room.w = rand.Next(min_wall_length + 1, max_room_width);
            room.h = rand.Next(min_wall_length + 1, max_room_height);
            //Console.WriteLine("Created room:\n w->{0}\n h->{1}", room.w, room.h);
        }
        catch (ArgumentOutOfRangeException e)
        {
            Console.WriteLine("Exception: {0}", e);
            
            
        }
        //Console.WriteLine("Created room:");
        //room.print();
        return room;
    }

    private Room findNearestRoom(Room room)
    {
        Point midPoint = new Point(room.x + (room.w / 2),
                                    room.y + (room.h / 2));
        Room nearestRoom = new Room();
        int nearest_distance = 100000;

        foreach (Room curRoom in rooms)
        {
            if (curRoom.Equals(room)) continue;
            Point cur_midPoint = new Point(curRoom.x + (curRoom.w / 2),
                                            curRoom.y + (curRoom.h / 2));
            int distance = (int)Math.Sqrt((midPoint.x - cur_midPoint.x)^2 + (midPoint.y - cur_midPoint.y)^2);
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

    public void encodeMap()
    {
        int i = 0;
        foreach (Room room in rooms)
        {
            for (int x = room.x; x < room.x + room.w; x++)
            {
                for (int y = room.y; y < room.y + room.h; y++)
                {
                    //Console.WriteLine("x: {0}, y: {1}", x, y);
                    map[x, y] = (int)cellTypes.FLOOR;
                    if (x == room.x && y == room.y)
                        map[x, y] = (int)cellTypes.CORNER;
                }
            }
            i++;
        }
    }

    public void printMap()
    {
        /*Console.WriteLine("\nEmpty-{0}\nFloor-{1}\nCorridor-{2}\nVert_Wall-{3}\nHorz_Wall-{4}\nStart-{5}\nTopleft Room Corner-{6}",
                            (int)cellTypes.EMPTY, (int)cellTypes.FLOOR, (int)cellTypes.CORRIDOR,
                            (int)cellTypes.VERT_WALL, (int)cellTypes.HORZ_WALL, (int)cellTypes.START, (int)cellTypes.CORNER);
         */
        for (int i = 0; i < map_width; i++)
        {
            for (int j = 0; j < map_height; j++)
            {
                if (map[i, j] == (int)cellTypes.CORNER)
                {
                    ColoredConsoleWrite(ConsoleColor.Blue, map[i, j]);
                }
                else if(map[i,j] == (int)cellTypes.FLOOR)
                {
                    ColoredConsoleWrite(ConsoleColor.Cyan, map[i, j]);
                }
                else if(map[i,j] == (int)cellTypes.CORRIDOR)
                {
                    ColoredConsoleWrite(ConsoleColor.Green, map[i, j]);
                }
                else if(map[i,j] == (int)cellTypes.DEBUG)
                {
                    ColoredConsoleWrite(ConsoleColor.Red, map[i, j]);
                }
                else if (map[i, j] == (int)cellTypes.HORZ_WALL)
                {
                    ColoredConsoleWrite(ConsoleColor.Yellow, map[i, j]);
                }
                else
                {
                    Console.Write(map[i, j]);
                }
            }
            Console.WriteLine();
        }
    }

    private void printRooms()
    {
        int i = 0;
        foreach (Room room in rooms)
        {
            Console.WriteLine("Room {0}:\n x->{1} \n y->{2}\n", i, room.x, room.y);
            i++;
        }
    }

    public static void ColoredConsoleWrite(ConsoleColor color, int mapCode)
    {
        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(mapCode);
        Console.ForegroundColor = originalColor;
    }

    public static void Main(string[] args)
    {
        int width = Convert.ToInt32(args[0]);
        int height = Convert.ToInt32(args[1]);
        int seed = Convert.ToInt32(args[2]);
        //Console.WriteLine("width: {0}, height: {0}", width, height);
        DungeonGenerator generator = new DungeonGenerator(width, height, seed);
        generator.generateRooms();
        generator.connectRooms();
        generator.addWalls();
        generator.printMap();
    }
}

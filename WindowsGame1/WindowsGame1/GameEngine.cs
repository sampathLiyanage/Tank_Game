using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace WindowsGame1
{

    public class Paths {
        private Dictionary<int, Location[,]> paths;
        private Location[,] warfield;
        private int mapSize;
        private bool[,] checkedLocs;

        public Paths(Location[,] wf, int mSize) {
            mapSize=mSize;
            paths = new Dictionary<int, Location[,]>();
            warfield = wf;
            checkedLocs = new bool[mapSize,mapSize];
        }

        public void addPath(int x, int y){
            Location[,] locs = new Location[mapSize,mapSize];
            for (int i = 0; i < mapSize; i++) {
                for (int j = 0; j < mapSize; j++) {
                    locs[i, j] = new Location(warfield[i,j].type,warfield[i,j].x,warfield[i,j].y);
                }
            }

            int key = y * mapSize + x;
            configPath(locs[x, y],locs);
            paths.Add(key, locs);
        }

        private void configPath(Location src, Location[,] locs)
        {
            String neighbrType;
            Location tempLoc;
            Location tempLoc1 = null;
            Location tempLoc2 = null;
            Location tempLoc3 = null;
            Location tempLoc4 = null;

            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                    checkedLocs[i, j] = false;
            }

            Queue<Location> q = new Queue<Location>();
            q.Enqueue(src);
            checkedLocs[src.x, src.y] = true;
            while (q.Count != 0)
            {
                tempLoc = q.Dequeue();

                if (tempLoc.x > 0)
                {

                    tempLoc1 = locs[tempLoc.x - 1, tempLoc.y];
                    if (!checkedLocs[tempLoc1.x, tempLoc1.y])
                    {
                        neighbrType = tempLoc1.type;
                        if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone") && !neighbrType.Equals("tank"))
                        {
                            tempLoc1.parentLoc = tempLoc;
                            tempLoc1.distFromSrc = tempLoc.distFromSrc + 1;
                            checkedLocs[tempLoc1.x, tempLoc1.y] = true;
                            q.Enqueue(tempLoc1);
                        }
                    }
                }

                if (tempLoc.x < (mapSize - 1))
                {
                    tempLoc2 = locs[tempLoc.x + 1, tempLoc.y];
                    if (!checkedLocs[tempLoc2.x, tempLoc2.y])
                    {
                        neighbrType = tempLoc2.type;
                        if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone") && !neighbrType.Equals("tank"))
                        {
                            tempLoc2.parentLoc = tempLoc;
                            tempLoc2.distFromSrc = tempLoc.distFromSrc + 1;
                            checkedLocs[tempLoc2.x, tempLoc2.y] = true;
                            q.Enqueue(tempLoc2);
                        }
                    }
                }

                if (tempLoc.y > 0)
                {
                    tempLoc3 = locs[tempLoc.x, tempLoc.y - 1];
                    if (!checkedLocs[tempLoc3.x, tempLoc3.y])
                    {
                        neighbrType = tempLoc3.type;
                        if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone") && !neighbrType.Equals("tank"))
                        {
                            tempLoc3.parentLoc = tempLoc;
                            tempLoc3.distFromSrc = tempLoc.distFromSrc + 1;
                            checkedLocs[tempLoc3.x, tempLoc3.y] = true;
                            q.Enqueue(tempLoc3);
                        }
                    }
                }

                if (tempLoc.y < (mapSize - 1))
                {
                    tempLoc4 = locs[tempLoc.x, tempLoc.y + 1];
                    if (!checkedLocs[tempLoc4.x, tempLoc4.y])
                    {
                        neighbrType = tempLoc4.type;
                        if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone") && !neighbrType.Equals("tank"))
                        {
                            tempLoc4.parentLoc = tempLoc;
                            tempLoc4.distFromSrc = tempLoc.distFromSrc + 1;
                            checkedLocs[tempLoc4.x, tempLoc4.y] = true;
                            q.Enqueue(tempLoc4);
                        }
                    }
                }

            }
        }

        private Location[,] getPath(int x, int y) {
            int key = mapSize * y + x;
            return paths[key];
        }

        public Location getNextLoc(Location src, Location dest) {
            Location target = getPath(src.x, src.y)[dest.x, dest.y];
            while (target.parentLoc != null && !(src.x == target.parentLoc.x && src.y == target.parentLoc.y))
            {
                target = target.parentLoc;
            }
            return target;
        }

        public int getDistance(Location src, Location dest)
        {
            Location target = getPath(src.x, src.y)[dest.x, dest.y];
            return target.distFromSrc;
        }
    }
    /*
    * ######################
    * Game engine components
    * ######################
    * 
    */


    public class Commandor
    {

        private int mapSize;
        private WarField warField;
        private Tank myTank;
        private Response response;
        private Thread listnerTrd;
        private Timer myTimer;
        private String bestCommand;
        private bool[,] checkedLocs;
        private int[,] distanceMatrix;
        private int brainLevelToFindCoins;
        private Paths paths;
        private static Commandor instant;


        private Commandor(WarField wf)
        {
            mapSize = Convert.ToInt16(ConfigurationSettings.AppSettings.Get("MapSize"));
            brainLevelToFindCoins = Convert.ToInt16(ConfigurationSettings.AppSettings.Get("brainLevelToFindCoins"));
            warField = wf;
            myTank = warField.getMyTank();
            response = Response.Instance;
            checkedLocs = new bool[mapSize, mapSize];

            bestCommand = "nothing";
            distanceMatrix = new int[mapSize, mapSize];

            //initiate paths
            paths = new Paths(warField.getField(), mapSize);
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    paths.addPath(i, j);
                    System.Console.WriteLine(i + " , " + j + " added");
                }
            }

            //setting timer for send responses to server after every second
            TimerCallback tcb = this.sendCommand;
            AutoResetEvent autoEvent = new AutoResetEvent(false);
            myTimer = new Timer(tcb, autoEvent, 0, Convert.ToInt16(ConfigurationSettings.AppSettings.Get("responseTimeInterval")) / 4);

            //starting a thread to calculate the next best command
            listnerTrd = new Thread(new ThreadStart(this.act));
            listnerTrd.Priority = ThreadPriority.Highest;
            listnerTrd.Start();


        }

        public static Commandor getInstant(WarField wf)
        {
            if (instant == null)
            {
                instant = new Commandor(wf);
            }
            return instant;
        }

        //to be called once a second
        public void sendCommand(Object stateInfo)
        {
            lock (bestCommand)
            {
                if (!bestCommand.Equals("nothing"))
                {
                    response.sendData(bestCommand);
                    System.Console.WriteLine("ssssssssssssssssssssssssssseeeeeeeeeeeeeeeeeennnnnnnnnnnnnnddddddddddd");
                }
            }
        }

        //calculate best next command
        public void act()
        {
            while (true)
            {

                bestCommand = getCommand();
            }

        }

        //breadth first search to get next best command
        private String getCommand()
        {
            setLocDangerLevels();
            for (int i = 0; i < mapSize; i++)
             {
                 for (int j = 0; j < mapSize; j++)
                 {
                     System.Console.Write(warField.getField()[i, j].safeLevel + "-" + warField.getField()[i, j].type + "-" );

                 }
                 System.Console.WriteLine();
             }
             System.Console.WriteLine();

            Location safestLoc = getSafestLoc();
            if (safestLoc == null)
            {
                if (isShoot()) {
                    return "SHOOT#";
                }

                Location target = nextMoveToNearestCoins();
                if (target == null)
                    return "nothing";

                return myTank.getCommand(target.x, target.y);
            }
            else
            {
                return myTank.getCommand(safestLoc.x, safestLoc.y);
            }

        }

        
        public Location getSafestLoc() {
            
            if (myTank.tankLoc.safeLevel == mapSize)
                return null;
            else {
                Location safestLoc = null;
                int safeValue = 0;
                if (myTank.tankLoc.safeLevel>safeValue) {
                    safeValue = myTank.tankLoc.safeLevel;
                    safestLoc = myTank.tankLoc;
                }

                if (myTank.tankLoc.x > 0) {
                    Location tempLoc = warField.getField()[(myTank.tankLoc.x) - 1, myTank.tankLoc.y];
                    if (tempLoc.safeLevel > safeValue)
                    {
                        safeValue = tempLoc.safeLevel;
                        safestLoc = tempLoc;
                    }
                }
                if (myTank.tankLoc.x < mapSize)
                {
                    Location tempLoc = warField.getField()[(myTank.tankLoc.x) + 1, myTank.tankLoc.y];
                    if (tempLoc.safeLevel > safeValue)
                    {
                        safeValue = tempLoc.safeLevel;
                        safestLoc = tempLoc;
                    }
                }
                if (myTank.tankLoc.y > 0)
                {
                    Location tempLoc = warField.getField()[myTank.tankLoc.x, (myTank.tankLoc.y)-1];
                    if (tempLoc.safeLevel > safeValue)
                    {
                        safeValue = tempLoc.safeLevel;
                        safestLoc = tempLoc;
                    }
                }
                if (myTank.tankLoc.x < mapSize)
                {
                    Location tempLoc = warField.getField()[myTank.tankLoc.x, myTank.tankLoc.y+1];
                    if (tempLoc.safeLevel > safeValue)
                    {
                        safeValue = tempLoc.safeLevel;
                        safestLoc = tempLoc;
                    }
                }
                return safestLoc;
            }
        }

        //next move to nearest coins
        public Location nextMoveToNearestCoins()
        {
            Location nearest = null;
            int min = 1000;
            int tempInt;
            Location tempLoc;
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    tempLoc = warField.getField()[i, j];
                    if (tempLoc.type == "coins")
                    {
                        tempInt = paths.getDistance(myTank.tankLoc, tempLoc);
                        if (tempInt < min)
                        {
                            min = tempInt;
                            nearest = tempLoc;
                        }
                    }

                }
            }
            if (nearest == null)
                return null;
            return paths.getNextLoc(myTank.tankLoc, nearest);
        }

        //check if next move is to shoot
        private bool isShoot() {
            int x = myTank.tankLoc.x;
            int y = myTank.tankLoc.y;
            int dir = myTank.direction;

            if (dir == 0 && y != 0)
            {
                 for (int i = (y - 1); i > -1; i--)
                 {
                     if (warField.getField()[x, i].type != "empty" && warField.getField()[x, i].type != "water" && warField.getField()[x, i].type != "tank")
                         break;
                     else if (warField.getField()[x, i].type == "tank")
                         return true;
                 }
            }
            else if (dir == 1 && x != mapSize)
                {
                    for (int i = (x + 1); i < mapSize; i++)
                    {
                        if (warField.getField()[i, y].type != "empty" && warField.getField()[i, y].type != "water" && warField.getField()[i, y].type != "tank")
                            break;
                        else if (warField.getField()[i, y].type == "tank")
                            return true;
                    }
            }
            else if (dir == 2 && y != mapSize)
                {
                    for (int i = (y + 1); i < mapSize; i++)
                    {
                        if (warField.getField()[x, i].type != "empty" && warField.getField()[x, i].type != "water" && warField.getField()[x, i].type != "tank")
                            break;
                        else if (warField.getField()[x, i].type == "tank")
                            return true;
                    }
            }
            else if (dir == 3 && x != 0)
            {
                    for (int i = (x - 1); i > -1; i--)
                    {
                        if (warField.getField()[i, y].type != "empty" && warField.getField()[i, y].type != "water" && warField.getField()[i, y].type != "tank")
                            break;
                        else if (warField.getField()[i, y].type == "tank")
                            return true;
                    }
            }
            return false;
        }

        //update location damage levels
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void setLocDangerLevels()
        {
            Location temp;
            int x, y, dir, safeLevel;
            Tank tank;
            //reset location safe levels
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    temp = warField.getField()[i, j];
                    
                        temp.safeLevel = mapSize;
                   

                }
            }

            //set location safe levels

            System.Console.WriteLine("no Of Tanks" + warField.tanks.Count);
            foreach (KeyValuePair<String, Tank> t in warField.tanks)
            {
                tank = t.Value;
                dir = tank.direction;
                x = tank.tankLoc.x;
                y = tank.tankLoc.y;
                safeLevel = 0;
                if (tank == myTank)
                    continue;
                else if (dir == 0 && y != 0)
                {
                    for (int i = (y - 1); i > -1; i--)
                    {
                        safeLevel++;
                        if (warField.getField()[x, i].type != "empty" && warField.getField()[x, i].type != "water")
                        {
                            break;
                        }
                        else if (warField.getField()[x, i].safeLevel <= safeLevel)
                        {
                            continue;
                        }
                        warField.getField()[x, i].safeLevel = safeLevel;
                    }
                }
                else if (dir == 1 && x != mapSize)
                {
                    for (int i = (x + 1); i < mapSize; i++)
                    {
                        safeLevel++;
                        if (warField.getField()[i, y].type != "empty" && warField.getField()[i, y].type != "water")
                        {
                            break;
                        }
                        else if (warField.getField()[i, y].safeLevel <= safeLevel)
                        {
                            continue;
                        }
                        warField.getField()[i, y].safeLevel = safeLevel;
                    }
                }
                else if (dir == 2 && y != mapSize)
                {
                    for (int i = (y + 1); i < mapSize; i++)
                    {
                        safeLevel++;
                        if (warField.getField()[x, i].type != "empty" && warField.getField()[x, i].type != "water")
                        {
                            break;
                        }
                        else if (warField.getField()[x, i].safeLevel <= safeLevel)
                        {
                            continue;
                        }
                        warField.getField()[x, i].safeLevel = safeLevel;
                    }
                }
                else if (dir == 3 && x != 0)
                {
                    for (int i = (x - 1); i > -1; i--)
                    {
                        safeLevel++;
                        if (warField.getField()[i, y].type != "empty" && warField.getField()[i, y].type != "water")
                        {
                            break;
                        }
                        else if (warField.getField()[i, y].safeLevel <= safeLevel)
                        {
                            continue;
                        }
                        warField.getField()[i, y].safeLevel = safeLevel;
                    }
                }
            }
        }
    }
        /*
         * 
         * if input type is string, compare the destination to type attribute of the location
         * if input type is Location, compare the destination to Location
         * 
         */

       

       

    public class priorityLocationQueue
    {
        private SortedDictionary<int, Queue<Location>> priorities;
        public priorityLocationQueue()
        {
            priorities = new SortedDictionary<int, Queue<Location>>();
        }

        public void enqueue(Location loc, int priority)
        {
            if (priority == (-1)) ;
            else if (priorities.ContainsKey(priority))
            {
                priorities[priority].Enqueue(loc);
            }
            else
            {
                priorities.Add(priority, new Queue<Location>());
                priorities[priority].Enqueue(loc);
            }
        }

        public Location dequeue()
        {
            if (priorities.Count == 0)
                return null;
            else
            {
                int min = priorities.Keys.Min();
                Location temp = priorities[min].Dequeue();
                if (priorities[min].Count == 0)
                {
                    priorities.Remove(min);
                }
                return temp;
            }
        }

        public int count()
        {
            return priorities.Count;
        }
    }

}

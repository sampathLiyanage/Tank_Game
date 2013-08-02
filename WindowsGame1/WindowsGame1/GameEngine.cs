﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace WindowsGame1
{
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

        public Commandor(WarField wf)
        {
            mapSize = Convert.ToInt16(ConfigurationSettings.AppSettings.Get("MapSize"));
            brainLevelToFindCoins = Convert.ToInt16(ConfigurationSettings.AppSettings.Get("brainLevelToFindCoins"));
            warField = wf;
            myTank = warField.getMyTank();
            response = Response.Instance;
            checkedLocs = new bool[mapSize, mapSize];

            bestCommand = "nothing";
            distanceMatrix = new int[mapSize,mapSize];


            //setting timer for send responses to server after every second
            TimerCallback tcb = this.sendCommand;
            AutoResetEvent autoEvent = new AutoResetEvent(false);
            myTimer = new Timer(tcb, autoEvent, 0, Convert.ToInt16(ConfigurationSettings.AppSettings.Get("responseTimeInterval")));

            //starting a thread to calculate the next best command
            listnerTrd = new Thread(new ThreadStart(this.act));
            listnerTrd.Priority = ThreadPriority.Highest;
            listnerTrd.Start();

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
                
                if (myTank.newCoins && brainLevelToFindCoins==2) {
                    int dst= distToTankIfNearest(myTank.target.coinLoc,myTank);
                    if (dst != -1)
                    {
                        myTank.destToTarget = dst;
                        myTank.targetLock = true;

                        findLocationAndConfigPath(myTank.tankLoc, myTank.target);
                    }
                    myTank.newCoins = false;
                }
                bestCommand = getCommand();
            }
            
        }

        //breadth first search to get next best command
        private String getCommand()
        {////////////////////////////////////////////////
            setLocDangerLevels();
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    System.Console.Write(warField.getField()[i, j].safeLevel +"-" +warField.getField()[i, j].type+ "  ");

                }
                System.Console.WriteLine();
            }
            System.Console.WriteLine();

            if (myTank.target != null && !(warField.getField())[myTank.target.coinLoc.x, myTank.target.coinLoc.y].type.Equals("coins") && brainLevelToFindCoins==2)
            {
                myTank.targetLock = false;
                myTank.target = null;
            }

        
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                    checkedLocs[i, j] = false;
            }

            myTank.tankLoc.parentLoc = null;
            myTank.tankLoc.distFromSrc = 0;
            int distCount;

            Location targetCoins;
            if (myTank.targetLock && (warField.getField())[myTank.target.coinLoc.x, myTank.target.coinLoc.y].type.Equals("coins") && brainLevelToFindCoins == 2)
            {
                targetCoins = findLocationAndConfigPath(myTank.tankLoc, myTank.target.coinLoc);
            }
            else
            {
                targetCoins = findLocationAndConfigPath(myTank.tankLoc, "coins") ;
            }

            Location tempLoc;
            if (targetCoins != null)
            {
                distCount = 0;
                tempLoc = targetCoins;
                while (tempLoc.parentLoc!=null && tempLoc.parentLoc != myTank.tankLoc)
                {
                    tempLoc = tempLoc.parentLoc;
                    distCount++;
                }
                myTank.destToTarget = distCount;
                return myTank.getCommand(tempLoc.x, tempLoc.y);
            }

            else
            {
                return "nothing";
            }
            
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
                    if (temp.type == "empty")
                    {
                        temp.safeLevel = mapSize;
                    }

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
                else if (dir == 0 && y!=0)
                {
                    for (int i = (y-1); i > 0; i--)
                    {
                        safeLevel++;
                        if (warField.getField()[x, i].type != "empty")
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
                else if (dir == 1 && x!=mapSize)
                {
                    for (int i = (x+1); i < mapSize; i++)
                    {
                        safeLevel++;
                        if (warField.getField()[i, y].type != "empty")
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
                else if (dir == 2 && y!=mapSize)
                {
                    for (int i = (y+1); i < mapSize; i++)
                    {
                        safeLevel++;
                        if (warField.getField()[x, i].type != "empty")
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
                else if (dir == 3 && x!=0)
                {
                    for (int i = (x-1); i > 0; i--)
                    {
                        safeLevel++;
                        if (warField.getField()[i, y].type != "empty")
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

        /*
         * 
         * if input type is string, compare the destination to type attribute of the location
         * if input type is Location, compare the destination to Location
         * 
         */
        
        private Location findLocationAndConfigPath(Location src, Object objTocompare)
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

            priorityLocationQueue q = new priorityLocationQueue();
            q.enqueue(src, heuristic(src));
            checkedLocs[src.x, src.y] = true;
            while (q.count() != 0)
            {
                tempLoc = q.dequeue();

                if (objTocompare.GetType()==typeof(String))
                {
                    if (tempLoc.type.Equals("coins"))
                    {
                        return tempLoc;
                    }
                }

                else if (objTocompare.GetType()==typeof(EmptyLoc))
                {
                     if (tempLoc==objTocompare)
                        {
                            return tempLoc;
                        }
                }
            
                if (tempLoc.x > 0)
                {

                    tempLoc1 = warField.getField()[tempLoc.x - 1, tempLoc.y];
                    if (!checkedLocs[tempLoc1.x, tempLoc1.y])
                    {
                        neighbrType = tempLoc1.type;
                        if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone") && !neighbrType.Equals("tank"))
                        {
                            tempLoc1.parentLoc = tempLoc;
                            tempLoc1.distFromSrc = tempLoc.distFromSrc + 1;
                            checkedLocs[tempLoc1.x, tempLoc1.y] = true;
                            q.enqueue(tempLoc1, heuristic(tempLoc1));
                        }
                    }
                }

                if (tempLoc.x < (mapSize - 1))
                {
                    tempLoc2 = warField.getField()[tempLoc.x + 1, tempLoc.y];
                    if (!checkedLocs[tempLoc2.x, tempLoc2.y])
                    {
                        neighbrType = tempLoc2.type;
                        if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone") && !neighbrType.Equals("tank"))
                        {
                            tempLoc2.parentLoc = tempLoc;
                            tempLoc2.distFromSrc = tempLoc.distFromSrc + 1;
                            checkedLocs[tempLoc2.x, tempLoc2.y] = true;
                            q.enqueue(tempLoc2, heuristic(tempLoc2));
                        }
                    }
                }

                if (tempLoc.y > 0)
                {
                    tempLoc3 = warField.getField()[tempLoc.x, tempLoc.y - 1];
                    if (!checkedLocs[tempLoc3.x, tempLoc3.y])
                    {
                        neighbrType = tempLoc3.type;
                        if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone") && !neighbrType.Equals("tank"))
                        {
                            tempLoc3.parentLoc = tempLoc;
                            tempLoc3.distFromSrc = tempLoc.distFromSrc + 1;
                            checkedLocs[tempLoc3.x, tempLoc3.y] = true;
                            q.enqueue(tempLoc3, heuristic(tempLoc3));
                        }
                    }
                }

                if (tempLoc.y < (mapSize - 1))
                {
                    tempLoc4 = warField.getField()[tempLoc.x, tempLoc.y + 1];
                    if (!checkedLocs[tempLoc4.x, tempLoc4.y])
                    {
                        neighbrType = tempLoc4.type;
                        if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone") && !neighbrType.Equals("tank"))
                        {
                            tempLoc4.parentLoc = tempLoc;
                            tempLoc4.distFromSrc = tempLoc.distFromSrc + 1;
                            checkedLocs[tempLoc4.x, tempLoc4.y] = true;
                            q.enqueue(tempLoc4, heuristic(tempLoc1));
                        }
                    }
                }

            }

            return null;
        }

        //to find nearest coins
        [MethodImpl(MethodImplOptions.Synchronized)]
        private int heuristic(Location src)
        {
            if (src == null)
                return -1;
            int min = 1000;
            Location loc=null;

            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (warField.getField()[i, j].type == "coins")
                    {
                        Location tempLoc = warField.getField()[i, j];
                        int temp = Math.Abs(src.x - tempLoc.x) + Math.Abs(src.y - tempLoc.y) + 1;
                        if (min > temp)
                        {
                            min = temp;
                            loc = tempLoc;
                        }
                    }
                }
            }

           if (loc == null)
               return -1;
           return Math.Abs(src.x - loc.x) + Math.Abs(src.y - loc.y) + 1; ;
        }


        private int distToTankIfNearest(Location src, Tank dest)
        {
            Location tempLoc;
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                    distanceMatrix[i, j] = -1;
            }

            priorityLocationQueue q = new priorityLocationQueue();
            q.enqueue(src,heuristic(src,dest.tankLoc));
            distanceMatrix[src.x, src.y] = 0;
            while (q.count() != 0)
            {
                tempLoc = q.dequeue();
                if (tempLoc.type.Equals("tank") || distanceMatrix[tempLoc.x, tempLoc.y] >= dest.destToTarget)
                {
                    if (tempLoc == dest.tankLoc)
                    {
                        return distanceMatrix[tempLoc.x, tempLoc.y];
                    }
                    else
                    {
                        return -1;
                    }
                }

                else
                {
                    String neighbrType;
                    if (tempLoc.x > 0 && -1 == distanceMatrix[tempLoc.x - 1, tempLoc.y])
                    {
                        neighbrType = warField.getField()[tempLoc.x - 1, tempLoc.y].type;
                        if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone"))
                        {
                            Location temp=warField.getField()[tempLoc.x - 1, tempLoc.y];
                            q.enqueue(temp, heuristic(temp, dest.tankLoc));
                            distanceMatrix[tempLoc.x - 1, tempLoc.y] = distanceMatrix[tempLoc.x, tempLoc.y] + 1;
                        }
                    }

                    if (tempLoc.x < (mapSize - 1) && -1 == distanceMatrix[tempLoc.x + 1, tempLoc.y])
                    {
                        neighbrType = warField.getField()[tempLoc.x + 1, tempLoc.y].type;
                        if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone"))
                        {
                            Location temp = warField.getField()[tempLoc.x + 1, tempLoc.y];
                            q.enqueue(temp, heuristic(temp, dest.tankLoc));
                            distanceMatrix[tempLoc.x + 1, tempLoc.y] = distanceMatrix[tempLoc.x, tempLoc.y] + 1;
                        }
                    }

                    if (tempLoc.y > 0 && -1 == distanceMatrix[tempLoc.x, tempLoc.y - 1])
                    {
                        neighbrType = warField.getField()[tempLoc.x, tempLoc.y - 1].type;
                        if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone"))
                        {
                            Location temp = warField.getField()[tempLoc.x, tempLoc.y - 1];
                            q.enqueue(temp, heuristic(temp, dest.tankLoc));
                            distanceMatrix[tempLoc.x, tempLoc.y - 1] = distanceMatrix[tempLoc.x, tempLoc.y] + 1;
                        }
                    }

                    if (tempLoc.y < (mapSize - 1) && -1 == distanceMatrix[tempLoc.x, tempLoc.y + 1])
                    {
                        neighbrType = warField.getField()[tempLoc.x, tempLoc.y + 1].type;
                        if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone"))
                        {
                            Location temp = warField.getField()[tempLoc.x, tempLoc.y + 1];
                            q.enqueue(temp, heuristic(temp, dest.tankLoc));
                            distanceMatrix[tempLoc.x, tempLoc.y + 1] = distanceMatrix[tempLoc.x, tempLoc.y] + 1;
                        }
                    }

                }

            }

            return -1;

        }

        private int heuristic(Location src, Location dest) {
            return (Math.Abs(src.x - dest.x) + Math.Abs(src.y - dest.y) + 1);
        }



    }

    public class priorityLocationQueue {
        private SortedDictionary<int, Queue<Location>> priorities;
        public priorityLocationQueue() {
            priorities = new SortedDictionary<int, Queue<Location>>();
        }

        public void enqueue(Location loc, int priority){
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

        public Location dequeue() { 
            if (priorities.Count==0)
                return null;
            else{
                int min=priorities.Keys.Min();
                Location temp=priorities[min].Dequeue();
                if (priorities[min].Count == 0) {
                    priorities.Remove(min);
                }
                return temp;
            }
        }

        public int count() {
            return priorities.Count;
        }
    }

}

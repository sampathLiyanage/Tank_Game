using System;
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

        public Commandor(WarField wf)
        {
            mapSize = Convert.ToInt16(ConfigurationSettings.AppSettings.Get("MapSize"));
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
            if (!bestCommand.Equals("nothing"))
                response.sendData(bestCommand);
        }

        //calculate best next command
        public void act()
        {
            while (true)
            {
                
                if (myTank.newCoins) {
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
        {
            if (myTank.target != null && !(warField.getField())[myTank.target.coinLoc.x, myTank.target.coinLoc.y].type.Equals("coins"))
            {
                myTank.targetLock = false;
                myTank.target = null;
                myTank.destToTarget = 1000;
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
            if (myTank.targetLock && (warField.getField())[myTank.target.coinLoc.x, myTank.target.coinLoc.y].type.Equals("coins"))
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

            Queue<Location> q = new Queue<Location>();
            q.Enqueue(src);
            checkedLocs[src.x, src.y] = true;
            while (q.Count != 0)
            {
                tempLoc = q.Dequeue();

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
                            q.Enqueue(tempLoc1);
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
                            q.Enqueue(tempLoc2);
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
                            q.Enqueue(tempLoc3);
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
                            q.Enqueue(tempLoc4);
                        }
                    }
                }

            }

            return null;
        }

        private int distToTankIfNearest(Location src, Tank dest)
        {
            Location tempLoc;
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                    distanceMatrix[i, j] = -1;
            }

            Queue<Location> q = new Queue<Location>();
            q.Enqueue(src);
            distanceMatrix[src.x, src.y] = 0;
            while (q.Count != 0)
            {
                tempLoc = q.Dequeue();
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
                            q.Enqueue(warField.getField()[tempLoc.x - 1, tempLoc.y]);
                            distanceMatrix[tempLoc.x - 1, tempLoc.y] = distanceMatrix[tempLoc.x, tempLoc.y] + 1;
                        }
                    }

                    if (tempLoc.x < (mapSize - 1) && -1 == distanceMatrix[tempLoc.x + 1, tempLoc.y])
                    {
                        neighbrType = warField.getField()[tempLoc.x + 1, tempLoc.y].type;
                        if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone"))
                        {
                            q.Enqueue(warField.getField()[tempLoc.x + 1, tempLoc.y]);
                            distanceMatrix[tempLoc.x + 1, tempLoc.y] = distanceMatrix[tempLoc.x, tempLoc.y] + 1;
                        }
                    }

                    if (tempLoc.y > 0 && -1 == distanceMatrix[tempLoc.x, tempLoc.y - 1])
                    {
                        neighbrType = warField.getField()[tempLoc.x, tempLoc.y - 1].type;
                        if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone"))
                        {
                            q.Enqueue(warField.getField()[tempLoc.x, tempLoc.y - 1]);
                            distanceMatrix[tempLoc.x, tempLoc.y - 1] = distanceMatrix[tempLoc.x, tempLoc.y] + 1;
                        }
                    }

                    if (tempLoc.y < (mapSize - 1) && -1 == distanceMatrix[tempLoc.x, tempLoc.y + 1])
                    {
                        neighbrType = warField.getField()[tempLoc.x, tempLoc.y + 1].type;
                        if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone"))
                        {
                            q.Enqueue(warField.getField()[tempLoc.x, tempLoc.y + 1]);
                            distanceMatrix[tempLoc.x, tempLoc.y + 1] = distanceMatrix[tempLoc.x, tempLoc.y] + 1;
                        }
                    }

                }

            }

            return -1;

        }

    }

}

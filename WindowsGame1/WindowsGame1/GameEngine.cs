using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;

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
        private Location myTank;
        private Response response;
        private bool pathFound;
        private Coins followingCoins;
        private Thread listnerTrd;
        private Timer myTimer;
        private String bestCommand;
        private bool[,] checkedLocs;

        public Commandor(WarField wf)
        {
            mapSize = Convert.ToInt16(ConfigurationSettings.AppSettings.Get("MapSize"));
            warField = wf;
            myTank = warField.getMyTank();
            response = Response.Instance;
            pathFound = false;
            checkedLocs = new bool[mapSize, mapSize];

            bestCommand = "nothing";

            //setting timer for send responses to server after every second
            TimerCallback tcb = this.sendCommand;
            AutoResetEvent autoEvent = new AutoResetEvent(false);
            myTimer = new Timer(tcb, autoEvent, 0, Convert.ToInt16(ConfigurationSettings.AppSettings.Get("responseTimeInterval")));

            //starting a thread to calculate the next best command
            listnerTrd = new Thread(new ThreadStart(this.act));
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
                bestCommand = getCommand();
            }
        }

        //breadth first search to get next best command
        private String getCommand()
        {
            if (((Tank)myTank).target != null && !(warField.getField())[((Tank)myTank).target.x, ((Tank)myTank).target.y].type.Equals("coins"))
            {
                ((Tank)myTank).targetLock = false;
                ((Tank)myTank).destToTarget = 1000;
            }

            if (((Tank)myTank).targetLock && (warField.getField())[((Tank)myTank).target.x, ((Tank)myTank).target.y].type.Equals("coins"))
            {


                pathFound = false;
                for (int i = 0; i < mapSize; i++)
                {
                    for (int j = 0; j < mapSize; j++)
                        checkedLocs[i, j] = false;
                }
                myTank.parentLoc = null;
                myTank.distFromSrc = 0;
                checkedLocs[myTank.x, myTank.y] = true;
                Queue<Location> q = new Queue<Location>();
                q.Enqueue(myTank);
                configPath(q, ((Tank)myTank).target);

                if (pathFound && followingCoins.parentLoc != null)
                {
                    Location loc;

                    int distCount = 0;
                    loc = followingCoins;
                    while (loc.parentLoc.parentLoc != null)
                    {
                        loc = loc.parentLoc;
                        distCount++;
                    }

                    ((Tank)myTank).destToTarget = distCount;
                    System.Console.WriteLine("target is " + ((Tank)myTank).target.x + " " + ((Tank)myTank).target.y + "; distance is " + distCount);
                    return ((Tank)myTank).getCommand(loc.x, loc.y);
                }

                else
                {
                    return "nothing";
                }
            }
            else
            {
                pathFound = false;
                for (int i = 0; i < mapSize; i++)
                {
                    for (int j = 0; j < mapSize; j++)
                        checkedLocs[i, j] = false;
                }
                myTank.parentLoc = null;
                myTank.distFromSrc = 0;
                checkedLocs[myTank.x, myTank.y] = true;
                Queue<Location> q = new Queue<Location>();
                q.Enqueue(myTank);
                configPath(q);

                if (pathFound && followingCoins.parentLoc != null)
                {
                    Location loc;
                    loc = followingCoins;
                    while (loc.parentLoc.parentLoc != null)
                    {
                        loc = loc.parentLoc;
                    }

                    return ((Tank)myTank).getCommand(loc.x, loc.y);
                }

                else
                {
                    return "nothing";
                }
            }
        }
        //creating a path
        private void configPath(Queue<Location> q, Location loc)
        {
            String neighbrType;
            Location tempLoc1 = null;
            Location tempLoc2 = null;
            Location tempLoc3 = null;
            Location tempLoc4 = null;
            if (!pathFound && q.Count != 0)
            {
                Location src = q.Dequeue();
                if (src == loc)
                {
                    followingCoins = (Coins)src;
                    pathFound = true;
                }

                else
                {

                    if (src.x > 0)
                    {

                        tempLoc1 = warField.getField()[src.x - 1, src.y];
                        if (!checkedLocs[tempLoc1.x, tempLoc1.y])
                        {
                            neighbrType = tempLoc1.type;
                            if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone") && !neighbrType.Equals("tank"))
                            {
                                tempLoc1.parentLoc = src;
                                tempLoc1.distFromSrc = src.distFromSrc + 1;
                                checkedLocs[tempLoc1.x, tempLoc1.y] = true;
                                q.Enqueue(tempLoc1);
                            }
                        }
                    }

                    if (src.x < mapSize - 1)
                    {
                        tempLoc2 = warField.getField()[src.x + 1, src.y];
                        if (!checkedLocs[tempLoc2.x, tempLoc2.y])
                        {
                            neighbrType = tempLoc2.type;
                            if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone") && !neighbrType.Equals("tank"))
                            {
                                tempLoc2.parentLoc = src;
                                tempLoc2.distFromSrc = src.distFromSrc + 1;
                                checkedLocs[tempLoc2.x, tempLoc2.y] = true;
                                q.Enqueue(tempLoc2);
                            }
                        }
                    }

                    if (src.y > 0)
                    {
                        tempLoc3 = warField.getField()[src.x, src.y - 1];
                        if (!checkedLocs[tempLoc3.x, tempLoc3.y])
                        {
                            neighbrType = tempLoc3.type;
                            if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone") && !neighbrType.Equals("tank"))
                            {
                                tempLoc3.parentLoc = src;
                                tempLoc3.distFromSrc = src.distFromSrc + 1;
                                checkedLocs[tempLoc3.x, tempLoc3.y] = true;
                                q.Enqueue(tempLoc3);
                            }
                        }
                    }

                    if (src.y < mapSize - 1)
                    {
                        tempLoc4 = warField.getField()[src.x, src.y + 1];
                        if (!checkedLocs[tempLoc4.x, tempLoc4.y])
                        {
                            neighbrType = tempLoc4.type;
                            if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone") && !neighbrType.Equals("tank"))
                            {
                                tempLoc4.parentLoc = src;
                                tempLoc4.distFromSrc = src.distFromSrc + 1;
                                checkedLocs[tempLoc4.x, tempLoc4.y] = true;
                                q.Enqueue(tempLoc4);
                            }
                        }
                    }
                    configPath(q);

                }
            }
        }

        //creating a path
        private void configPath(Queue<Location> q)
        {
            String neighbrType;
            Location tempLoc1 = null;
            Location tempLoc2 = null;
            Location tempLoc3 = null;
            Location tempLoc4 = null;
            if (!pathFound && q.Count != 0)
            {
                Location src = q.Dequeue();
                if (src.type.Equals("coins"))
                {
                    followingCoins = (Coins)src;
                    pathFound = true;
                }

                else
                {

                    if (src.x > 0)
                    {

                        tempLoc1 = warField.getField()[src.x - 1, src.y];
                        if (!checkedLocs[tempLoc1.x, tempLoc1.y])
                        {
                            neighbrType = tempLoc1.type;
                            if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone") && !neighbrType.Equals("tank"))
                            {
                                tempLoc1.parentLoc = src;
                                tempLoc1.distFromSrc = src.distFromSrc + 1;
                                checkedLocs[tempLoc1.x, tempLoc1.y] = true;
                                q.Enqueue(tempLoc1);
                            }
                        }
                    }

                    if (src.x < mapSize - 1)
                    {
                        tempLoc2 = warField.getField()[src.x + 1, src.y];
                        if (!checkedLocs[tempLoc2.x, tempLoc2.y])
                        {
                            neighbrType = tempLoc2.type;
                            if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone") && !neighbrType.Equals("tank"))
                            {
                                tempLoc2.parentLoc = src;
                                tempLoc2.distFromSrc = src.distFromSrc + 1;
                                checkedLocs[tempLoc2.x, tempLoc2.y] = true;
                                q.Enqueue(tempLoc2);
                            }
                        }
                    }

                    if (src.y > 0)
                    {
                        tempLoc3 = warField.getField()[src.x, src.y - 1];
                        if (!checkedLocs[tempLoc3.x, tempLoc3.y])
                        {
                            neighbrType = tempLoc3.type;
                            if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone") && !neighbrType.Equals("tank"))
                            {
                                tempLoc3.parentLoc = src;
                                tempLoc3.distFromSrc = src.distFromSrc + 1;
                                checkedLocs[tempLoc3.x, tempLoc3.y] = true;
                                q.Enqueue(tempLoc3);
                            }
                        }
                    }

                    if (src.y < mapSize - 1)
                    {
                        tempLoc4 = warField.getField()[src.x, src.y + 1];
                        if (!checkedLocs[tempLoc4.x, tempLoc4.y])
                        {
                            neighbrType = tempLoc4.type;
                            if (!neighbrType.Equals("water") && !neighbrType.Equals("brick") && !neighbrType.Equals("stone") && !neighbrType.Equals("tank"))
                            {
                                tempLoc4.parentLoc = src;
                                tempLoc4.distFromSrc = src.distFromSrc + 1;
                                checkedLocs[tempLoc4.x, tempLoc4.y] = true;
                                q.Enqueue(tempLoc4);
                            }
                        }
                    }
                    configPath(q);

                }
            }
        }

    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Configuration;

namespace WindowsGame1
{

    /*
     * #######################
     *the game model component
     * #######################
    */

    //represent a location (ie: 10*10 locations in the game)
    public abstract class Location
    {
        public int x, y;
        public Location parentLoc;
        public int distFromSrc;
        public String type;
        public Location(String str, int X, int Y)
        {
            type = str;
            x = X;
            y = Y;
        }
    }

    //represents an Empty Location (ie: locations without any object like tank, water, wall)
    public class EmptyLoc : Location
    {
        public EmptyLoc(int x, int y, Location[,] wf)
            : base("empty", x, y)
        {
            wf[x, y] = this;
        }
    }

    //represents a tank
    public class Tank
    {
        public int direction; //0=N, 1=E, 2=S, 3=W
        private bool whetherShot;
        private int helth, coins, points;
        private Location[,] warfield;
        private String name;
        private bool initiated;
        public bool targetLock;
        public int destToTarget;
        public Coins target;
        public bool newCoins;
        public Location tankLoc;

        public Tank(String nm, int x, int y, Location[,] wf)
        {
            warfield = wf;
            tankLoc = warfield[x, y];
            warfield[x, y].type = "tank";
            name = nm;
            targetLock = false;
            destToTarget = 1000;
            newCoins = false;
        }

        //set location of the tank
        public void Set(int locX, int locY, int dir, bool ws, int h, int c, int p)
        {
            if (initiated)
            {
                tankLoc.type="empty";
            }
            else
                initiated = true;

            
            tankLoc = warfield[locX, locY];
            tankLoc.type = "tank";
            direction = dir;
            whetherShot = ws;
            helth = h;
            coins = c;
            points = p;
        }

        //input: next location cordinate calculated by game engine (supposed to be an adjesent location)
        //output: the command or string "nothing"
        public String getCommand(int crdX, int crdY)
        {
            if (crdY == tankLoc.y + 1 && crdX == tankLoc.x)
                return "DOWN#";
            else if ((crdY == tankLoc.y - 1) && crdX == tankLoc.x)
                return "UP#";
            else if ((crdX == tankLoc.x + 1) && crdY == tankLoc.y)
                return "RIGHT#";
            else if ((crdX == tankLoc.x - 1) && crdY == tankLoc.y)
                return "LEFT#";
            else
                return "nothing";
        }
    }

    //represent coins
    public class Coins : Location
    {
        private int mapSize;
        int value;
        DateTime startedTime;
        int time;
        private Location[,] warfield;
        public bool taken;
        public bool expired;
        public Coins(int x, int y, int v, int t, Location[,] wf, Tank myTank)
            : base("coins", x, y)
        {
            mapSize = Convert.ToInt16(ConfigurationSettings.AppSettings.Get("MapSize"));
            value = v;
            taken = false;
            expired = false;
            startedTime = DateTime.Now;
            time = t;
            wf[x, y] = this;
            warfield = wf;

            myTank.target = this;
            myTank.newCoins = true;

        }

        public bool isExpired()
        {
            DateTime now = DateTime.Now;
            TimeSpan diff = now.Subtract(startedTime);
            if (diff.TotalMilliseconds >= time || taken == true)
                return true;
            return false;
        }


        
    }

    //represents war field (game environment)
    //this is a singleton class
    public class WarField
    {
        private int mapSize;

        private Location[,] wfield;
        public Dictionary<String,Tank> tanks;
        private List<Coins> coinList;
        private Listner listner;
        private Response response;
        private Char[] dataIn;
        private String mytankName;
        public bool initiated;

        static WarField instance = null;
        static readonly object padlock = new object();

        private WarField()
        {
            mapSize = Convert.ToInt16(ConfigurationSettings.AppSettings.Get("MapSize"));
            wfield = new Location[mapSize, mapSize];
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    new EmptyLoc(i, j, wfield);
                }
            }
            tanks = new Dictionary<String, Tank>();
            coinList = new List<Coins>();
            listner = Listner.Instance;
            listner.start(this);
            initiated = false;
            response = Response.Instance;

            //join game
            response.sendData("JOIN#");
        }

        //singleton instance method
        public static WarField Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new WarField();
                    }
                    return instance;
                }
            }
        }

        //input from the listner instance
        public void inputData(Byte[] input)
        {
            dataIn = new Char[input.Length];
            for (int i = 0; input.Length > i; i++)
            {
                dataIn[i] = Convert.ToChar(input[i]);
                System.Console.Write(dataIn[i]);
            }
            System.Console.WriteLine();
            if (dataIn[0].Equals('I') && dataIn[1].Equals(':'))
            {  //if game is being intiated
                initiateGame(dataIn);
            }

            else if (dataIn[0].Equals('G') && dataIn[1].Equals(':'))
            {  //if game is being intiated
                updateView(dataIn);
            }

            //if coins appear
            else if (dataIn[0].Equals('C') && dataIn[1].Equals(':'))
            {
                updateCoins(dataIn);
            }

        }

        //initiate game
        private void initiateGame(Char[] dataInput)
        {
            System.Console.WriteLine("game is initiating");
            String operands = new String(dataInput);
            operands = operands.Replace("#", "");
            string[] operands_1 = Regex.Split(operands, ":");

            mytankName = operands_1[1];
            this.newTank(operands_1[1]);
            string[] operands_12 = Regex.Split(operands_1[2], ";");
            for (int i = 0; i < operands_12.Length; i++)
            {
                String[] operands_12i = Regex.Split(operands_12[i], ",");
                this.newBrick(Convert.ToInt16(operands_12i[0]), Convert.ToInt16(operands_12i[1]));
            }

            string[] operands_13 = Regex.Split(operands_1[3], ";");
            for (int i = 0; i < operands_13.Length; i++)
            {
                String[] operands_13i = Regex.Split(operands_13[i], ",");
                this.newStone(Convert.ToInt16(operands_13i[0]), Convert.ToInt16(operands_13i[1]));
            }

            string[] operands_14 = Regex.Split(operands_1[4], ";");
            for (int i = 0; i < operands_14.Length; i++)
            {
                String[] operands_14i = Regex.Split(operands_14[i], ",");

                this.newWater(Convert.ToInt16(operands_14i[0]), Convert.ToInt16(operands_14i[1]));
            }

         

            initiated = true;
        }

        //update game view according to the server responces
        private void updateView(Char[] dataInput)
        {
            System.Console.WriteLine("game is updating");
            String operands = new String(dataInput);
            operands = operands.Replace("#", "");
            string[] operands_1 = Regex.Split(operands, ":");
            String[] operands_2;
            for (int i = 1; i < operands_1.Length - 1; i++)
            {
                operands_2 = Regex.Split(operands_1[i], ";");
                String nm = Convert.ToString(operands_2[0]);
                String[] operands_21 = Regex.Split(operands_2[1], ",");
                int X = Convert.ToInt32(operands_21[0]);
                int Y = Convert.ToInt32(operands_21[1]);
                int dir = Convert.ToInt32(operands_2[2]);
                bool ws = Convert.ToBoolean(Convert.ToInt16(operands_2[3]));
                int h = Convert.ToInt32(operands_2[4]);
                int c = Convert.ToInt32(operands_2[5]);
                int p = Convert.ToInt32(operands_2[6]);
                this.setTank(nm, X, Y, dir, ws, h, c, p);
            }

            String[] operands_3 = Regex.Split(operands_1[operands_1.Length - 1], ";");
            for (int i = 0; i < operands_3.Length; i++)
            {
                String[] operands_31 = Regex.Split(operands_3[i], ",");
                int X = Convert.ToInt32(operands_31[0]);
                int Y = Convert.ToInt32(operands_31[1]);
                int damage = Convert.ToInt32(operands_31[2]);
                this.setDamage(X, Y, damage);
            }


            this.update(); //updating coins and life packs


        }

        //update coins according to the server responses
        private void updateCoins(Char[] dataInput)
        {
            System.Console.WriteLine("new coins appear");
            String operands = new String(dataInput);
            operands = operands.Replace("#", "");
            string[] operands_1 = Regex.Split(operands, ":");
            String[] operands_2 = Regex.Split(operands_1[1], ",");
            int X = Convert.ToInt16(operands_2[0]);
            int Y = Convert.ToInt16(operands_2[1]);
            int lt = Convert.ToInt32(operands_1[2]);
            int vl = Convert.ToInt32(operands_1[3]);
            this.newCoins(X, Y, vl, lt);
        }

        //represents stone
        private class Stone : Location
        {
            public Stone(int x, int y, Location[,] wf)
                : base("stone", x, y)
            {
                wf[x, y] = this;
            }
        }

        //represents brick
        private class Brick : Location
        {
            private int damageLevel;
            public Brick(int x, int y, Location[,] wf)
                : base("brick", x, y)
            {
                damageLevel = 0; //0% damage
                wf[x, y] = this;
            }

            public void setDamage(int damage, Location[,] wf)
            {
                damageLevel = damage;
                if (damageLevel == 4)
                {
                    wf[x, y] = new EmptyLoc(x, y, wf);
                }
            }
        }

        //represents water
        private class Water : Location
        {
            public Water(int x, int y, Location[,] wf)
                : base("water", x, y)
            {
                wf[x, y] = this;
            }
        }


        private void newEmptyLoc(int X, int Y)
        {
            new EmptyLoc(X, Y, wfield);
        }

        private void newStone(int X, int Y)
        {
            new Stone(X, Y, wfield);
        }

        private void newBrick(int X, int Y)
        {
            new Brick(X, Y, wfield);
        }

        private void newWater(int X, int Y)
        {
            new Water(X, Y, wfield);
        }

        private void newTank(String nm)
        {
            tanks.Add(nm, new Tank(nm, 0, 0, wfield));
        }



        private void setTank(String name, int X, int Y, int dir, bool ws, int h, int c, int p)
        {
            if (!tanks.ContainsKey(name))
            {
                newTank(name);
            }
            ((Tank)tanks[name]).Set(X, Y, dir, ws, h, c, p);
        }

        private void setDamage(int X, int Y, int damage)
        {
            ((Brick)wfield[X, Y]).setDamage(damage, this.wfield);
        }

        private void newCoins(int X, int Y, int v, int t)
        {
            coinList.Add(new Coins(X, Y, v, t, wfield, (Tank)tanks[mytankName]));
        }

        //should be called to update coins and life packs when they are expired
        private void update()
        {
            //updating coins
            List<Coins> newCList = new List<Coins>();
            foreach (Coins c in coinList)
            {
                if (c.isExpired())
                {
                    c.expired = true;
                    new EmptyLoc(c.x, c.y, wfield);
                }
                else
                    newCList.Add(c);
            }

            coinList = newCList;
            //updating life packs(to be implemented)


        }



        private void removeCoins(Coins c)
        {
            coinList.Remove(c);
        }


        //functions to be called by game engine
        public Tank getTank(String nm)
        {
            return (Tank)tanks[nm];
        }

        public List<Coins> getCoins()
        {
            return coinList;
        }

        public Location[,] getField()
        {
            return wfield;
        }

        public Tank getMyTank()
        {
            while (!initiated) ;
            return (Tank)tanks[mytankName];
        }
    }

}

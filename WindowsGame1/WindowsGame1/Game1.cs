using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections;

namespace WindowsGame1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        Communicator com;
        Operator opr;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            com = new Communicator();
            opr = new Operator(com);
            opr.join();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }

    public class Communicator {
        private Stream stm;
        private Thread listner;
        private NetworkStream serverStream;
        private TcpClient client;
        private Operator opr;
        public Communicator()
        {
            

            try
            {
                listner = new Thread(new ThreadStart(this.listen));
                listner.Start();

            }

            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
        }
        
        public void addOperator(Operator o){
            opr = o;
        }

        private void listen() {

            IPAddress ipAd = IPAddress.Parse("127.0.0.1");
                // use local m/c IP address, and 
                // use the same in the client

                /* Initializes the Listener */
                TcpListener myList = new TcpListener(ipAd, 7000);

                /* Start Listeneting at the specified port */
                myList.Start();

                Console.WriteLine("The server is running at port 8001...");
                Console.WriteLine("The local End point is  :" +
                                  myList.LocalEndpoint);
                Console.WriteLine("Waiting for a connection.....");

                Socket soc;
 
                while (true)
                {
                    //connection is connected socket
                    soc = myList.AcceptSocket();
                    if (soc.Connected)
                    {
                        //To read from socket create NetworkStream object associated with socket
                        serverStream = new NetworkStream(soc);

                        SocketAddress sockAdd = soc.RemoteEndPoint.Serialize();
                        string s = soc.RemoteEndPoint.ToString();
                        List<Byte> inputStr = new List<byte>();

                        int asw = 0;
                        while (asw != -1)
                        {
                            asw = this.serverStream.ReadByte();
                            inputStr.Add((Byte)asw);
                            if (Convert.ToChar(asw).Equals('#'))
                            {
                                opr.inputData(inputStr.ToArray());
                                break;
                            }
                        }
                    }
                    serverStream.Close();
                    soc.Close();
                }
            
           
        }

        public void SendData(String msg)
        {
            //Opening the connection
            client = new TcpClient();
            IPAddress ipAd = IPAddress.Parse("127.0.0.1");
            BinaryWriter writer;
            try
            {
                

                    client.Connect(ipAd, 6000);

                    if (client.Connected)
                    {
                        //To write to the socket
                        stm = client.GetStream();

                        //Create objects for writing across stream
                        writer = new BinaryWriter(stm);
                        Byte[] tempStr = Encoding.ASCII.GetBytes(msg);

                        //writing to the port                
                        writer.Write(tempStr);
                        writer.Flush();
                        writer.Close();
                        this.stm.Close();
                    }
                
            }
            catch (Exception e)
            {
                
            }
            finally
            {
                client.Close();
            }
        }

       
    }

    public class Operator
    {
        private bool started;
        private Communicator com;
        private String mytankName;
        private WarField warfield;
        private Char[] dataIn;
        private Commandor commander;
        public Operator(Communicator c) {
            com = c;
            com.addOperator(this);
            warfield = new WarField();
            started = false;
        }

        public void join() {
            com.SendData("JOIN#");
        }

        public void inputData(Byte[] input) {
            dataIn = new Char[input.Length];
            for (int i = 0; input.Length > i; i++ )
            {
                dataIn[i] = Convert.ToChar(input[i]);
                System.Console.Write(dataIn[i]);
            }
            System.Console.WriteLine();
            if (dataIn[0].Equals('I') && dataIn[1].Equals(':')) {  //if game is being intiated
                initiateGame(dataIn);
            }

            else if (dataIn[0].Equals('G') && dataIn[1].Equals(':'))
            {  //if game is being intiated
                updateView(dataIn);
            }

            //if coins appear
            else if (dataIn[0].Equals('C') && dataIn[1].Equals(':')) {
                updateCoins(dataIn);
            }

        }

        public void outputData(String output) {
            com.SendData(output);
        }

        public void initiateGame(Char[] dataInput){
            System.Console.WriteLine("game is initiating");
                String operands = new String(dataInput);
                operands=operands.Replace("#","");
                string[] operands_1 = Regex.Split(operands, ":");
       
                mytankName=operands_1[1];
                warfield.newTank(operands_1[1]);
                string[] operands_12 = Regex.Split(operands_1[2], ";");
                for (int i = 0; i < operands_12.Length; i++) {
                    String[] operands_12i = Regex.Split(operands_12[i], ",");
                    warfield.newBrick(Convert.ToInt16(operands_12i[0]), Convert.ToInt16(operands_12i[1]));
                }

                string[] operands_13 = Regex.Split(operands_1[3], ";");
                for (int i = 0; i < operands_13.Length; i++)
                {
                    String[] operands_13i = Regex.Split(operands_13[i], ",");
                    warfield.newStone(Convert.ToInt16(operands_13i[0]), Convert.ToInt16(operands_13i[1]));
                }

                string[] operands_14 = Regex.Split(operands_1[4], ";");
                for (int i = 0; i < operands_14.Length; i++)
                {
                    String[] operands_14i = Regex.Split(operands_14[i], ",");

                    warfield.newWater(Convert.ToInt16(operands_14i[0]), Convert.ToInt16(operands_14i[1]));
                }

                for (int i = 0; i < 10; i++) {
                    for (int j = 0; j < 10; j++) {
                        System.Console.Write(warfield.getField()[i,j].type);
                        System.Console.Write(" ");
                    }
                    System.Console.WriteLine();
                }

                commander = new Commandor(warfield, com, warfield.getTank(mytankName));
                Thread thread = new Thread(new ThreadStart(commander.act));
                thread.Start();
        }


        public void updateView(Char[] dataInput)
        {
            System.Console.WriteLine("game is updating");
            String operands = new String(dataInput);
            operands = operands.Replace("#", "");
            string[] operands_1 = Regex.Split(operands, ":");
            String[] operands_2;
            for (int i = 1; i < operands_1.Length - 1; i++) {
                operands_2 = Regex.Split(operands_1[i], ";");
                String nm = Convert.ToString(operands_2[0]);
                String[] operands_21= Regex.Split(operands_2[1], ",");
                int X = Convert.ToInt32(operands_21[0]);
                int Y = Convert.ToInt32(operands_21[1]);
                int dir = Convert.ToInt32(operands_2[2]);
                bool ws = Convert.ToBoolean(Convert.ToInt16(operands_2[3]));
                int h = Convert.ToInt32(operands_2[4]);
                int c = Convert.ToInt32(operands_2[5]);
                int p = Convert.ToInt32(operands_2[6]);
                warfield.setTank(nm, X, Y, dir, ws, h, c, p);
            }

            String[] operands_3 = Regex.Split(operands_1[operands_1.Length-1], ";");
            for (int i = 0; i < operands_3.Length; i++) {
                String[] operands_31 = Regex.Split(operands_3[i], ",");
                int X = Convert.ToInt32(operands_31[0]);
                int Y = Convert.ToInt32(operands_31[1]);
                int damage = Convert.ToInt32(operands_31[2]);
                warfield.setDamage(X,Y,damage);
            }


            warfield.update(); //updating coins and life packs


            //printing map on console
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    System.Console.Write(warfield.getField()[i, j].type);
                    System.Console.Write(" ");
                }
                System.Console.WriteLine();
            }


        }

        public void updateCoins(Char[] dataInput) {
            System.Console.WriteLine("new coins appear");
            String operands = new String(dataInput);
            operands = operands.Replace("#", "");
            string[] operands_1 = Regex.Split(operands, ":");
            String[] operands_2 = Regex.Split(operands_1[1], ",");
            int X = Convert.ToInt16(operands_2[0]);
            int Y = Convert.ToInt16(operands_2[1]);
            int lt = Convert.ToInt32(operands_1[2]);
            int vl = Convert.ToInt32(operands_1[3]);
            warfield.newCoins(X, Y, vl, lt);
        }


                
    }


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

    public class EmptyLoc : Location
    {
        public EmptyLoc(int x, int y, Location[,] wf)
            : base("empty", x, y)
        {
            wf[x, y] = this;
        }
    }

    public class Tank :Location{
            private int direction; //0=N, 1=E, 2=S, 3=W
            private bool whetherShot;
            private int helth, coins, points;
            private Location[,] warfield;
            private String name;
            private bool initiated;

            public Tank(String nm, int x, int y, Location[,] wf) :base("tank", x, y){
                warfield=wf;
                name=nm;
            }

            public void Set(int locX, int locY, int dir, bool ws, int h, int c, int p){
                if (initiated==true){
                    warfield[x,y] = new EmptyLoc(x,y,warfield);
                }
                else
                    initiated=true;

                x=locX;
                y=locY;

                direction=dir;
                whetherShot=ws;
                helth=h;
                coins=c;
                points=p;
                warfield[x,y]=this;
            }

            public String getCommand(int crdX, int crdY) {
                if (crdY == y + 1 && crdX == x)
                    return "DOWN#";
                else if ((crdY == y - 1) && crdX == x)
                    return "UP#";
                else if ((crdX == x + 1) && crdY == y)
                    return "RIGHT#";
                else if ((crdX == x - 1) && crdY == y)
                    return "LEFT#";
                else
                    return "nothing";
            }
        }

    public class Coins : Location
    {
        int value;
        DateTime startedTime;
        int time;
        public bool taken;
        public bool expired;
        public Coins(int x, int y, int v, int t, Location[,] wf)
            : base("coins", x, y)
        {
            value = v;
            taken = false;
            expired = false;
            startedTime = DateTime.Now;
            time = t;
            wf[x, y] = this;
        }

        public bool isExpired()
        {
            DateTime now = DateTime.Now;
            TimeSpan diff = now.Subtract(startedTime);
            if (diff.TotalMilliseconds >= time || taken==true)
                return true;
            return false;
        }
    }


    public class WarField{
        private Location[,] wfield;
        public Hashtable tanks;
        private List<Coins> coinList;

        public WarField(){
            wfield= new Location[10,10];
            for (int i=0; i<10; i++){
                for(int j=0; j<10; j++){
                    new EmptyLoc(i,j,wfield);
                }
            }
            tanks = new Hashtable();
            coinList = new List<Coins>();
        }

       

        

        private class Stone :Location{
            public Stone(int x, int y,Location[,] wf) :base("stone", x, y){
                wf[x,y]=this;
            }
        }
         
        private class Brick :Location{
            private int damageLevel;
            public Brick(int x, int y, Location[,] wf) :base("brick", x, y){
                damageLevel=0; //0% damage
                wf[x,y]=this;
            }

            public void setDamage(int damage, Location[,] wf){
                damageLevel=damage;
                if (damageLevel == 4) {
                    wf[x, y] = new EmptyLoc(x,y,wf);
                }
            }
        }
         
        private class Water :Location{
            public Water(int x, int y, Location[,] wf) :base("water", x, y){
                wf[x,y]=this;
            }
        }


        public void newEmptyLoc(int X, int Y){
            new EmptyLoc(X,Y,wfield);
        }

        public void newStone(int X, int Y){
            new Stone(X,Y,wfield);
        }

        public void newBrick(int X, int Y){
            new Brick(X,Y,wfield);
        }

        public void newWater(int X, int Y){
            new Water(X,Y,wfield);
        }

        public void newTank(String nm){
            tanks.Add(nm,new Tank(nm, 0,0,wfield));
        }

        public Location[,] getField() {
            return wfield;
        }

        public void setTank (String name, int X, int Y, int dir, bool ws, int h, int c, int p){
            if (!tanks.ContainsKey(name)) {
                newTank(name);
            }
            ((Tank)tanks[name]).Set(X, Y, dir, ws, h, c, p);
        }

        public void setDamage(int X, int Y, int damage) {
            ((Brick)wfield[X, Y]).setDamage(damage, this.wfield);
        }

        public void newCoins(int X, int Y, int v, int t) {
            coinList.Add(new Coins(X, Y, v, t, wfield));
        }

        //should be called to update coins and life packs when they are expired
        public void update() {
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

        public List<Coins> getCoins() {
            return coinList;
        }

        public void removeCoins(Coins c) {
            coinList.Remove(c);
        }

        public Location getTank(String nm) {
            return (Location)tanks[nm];
        }

    }

    public class Commandor {


        private bool[,] checkedLocs;
        private WarField warField;
        private Location myTank;
        private Communicator communicator;
        private bool pathFound;
        private DateTime lastCommandTime;
        private Coins followingCoins;

        public Commandor(WarField wf, Communicator com, Location aTank)
        {
            warField = wf;
            myTank = aTank;
            communicator = com;
            pathFound = false;
            checkedLocs = new bool[10, 10];
            lastCommandTime = DateTime.Now;
        }

        public void act()
        {
            while (true)
            {
                DateTime now = DateTime.Now;
                TimeSpan diff = now.Subtract(lastCommandTime);
                while (diff.TotalMilliseconds < 1000)
                {
                    now = DateTime.Now;
                    diff = now.Subtract(lastCommandTime);
                }

                String command = getCommand();
                if (!command.Equals("nothing")){
                    communicator.SendData(getCommand());
                    lastCommandTime = DateTime.Now;
                }
            }
        }

        public String getCommand() {
            pathFound = false;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                    checkedLocs[i, j] = false;
            }
            myTank.parentLoc = null;
            myTank.distFromSrc = 0;
            checkedLocs[myTank.x, myTank.y] = true;
            Queue<Location> q = new Queue<Location>();
            q.Enqueue(myTank);
            configPath(q);

            if (pathFound && followingCoins.parentLoc!=null)
            {
                Location loc;
                loc = followingCoins;
                while(loc.parentLoc.parentLoc!=null){
                    loc = loc.parentLoc;
                }
                return ((Tank)myTank).getCommand(loc.x, loc.y);
            }

            else {
                return "nothing";
            }
        }

        

        private void configPath(Queue<Location> q) {
            String neighbrType;
            Location tempLoc1=null;
            Location tempLoc2=null;
            Location tempLoc3=null;
            Location tempLoc4=null;
            if (!pathFound && q.Count != 0)
            {
                Location src = q.Dequeue();
                if (src.type.Equals("coins"))
                {
                    followingCoins = (Coins) src;
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

                    if (src.x < 9)
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

                    if (src.y < 9)
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

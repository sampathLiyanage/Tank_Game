using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Runtime.CompilerServices;

namespace WindowsGame1
{
    /*
     * ######################
     * Communicator Component 
     *#######################
     */


    /*
     *A singleton class that start a connection to the server and listen 
     * 
     * First get an instatnce calling "Listner.Instance"
     * Then call start(WarField wf) method
     * All the responses from server will be sent to WarField.inputData(Byte[] b) method.
     */
    public class Listner
    {

        private Thread listnerTrd;
        private NetworkStream serverStream;
        private WarField warfield;

        static Listner instance = null;
        static readonly object padlock = new object();

        private Listner()
        {
            //empty
        }

        //Singleton Instatnce method
        public static Listner Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Listner();
                    }
                    return instance;
                }
            }
        }

        //start listning
        public void start(WarField wf)
        {
            warfield = wf;
            try
            {
                //starting a new thread for listning
                listnerTrd = new Thread(new ThreadStart(this.listen));
                listnerTrd.Start();
            }

            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
        }

        private void listen()
        {

            IPAddress ipAd = IPAddress.Parse(ConfigurationSettings.AppSettings.Get("ClientIp"));
            // use local m/c IP address, and 
            // use the same in the client

            /* Initializes the Listener */
            TcpListener myList = new TcpListener(ipAd, Convert.ToInt16(ConfigurationSettings.AppSettings.Get("ClientPort")));

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
                            warfield.inputData(inputStr.ToArray());
                            break;
                        }
                    }
                }
                serverStream.Close();
                soc.Close();
            }


        }
    }


    /*
     *A singleton class that start a connection to the server and response
     * 
     * First get an instatnce calling "Response.Instance"
     * Then call sendData(String response) method to send responses to server
     */
    public class Response
    {
        private Stream stm;
        private TcpClient client;

        static Response instance = null;
        static readonly object padlock = new object();

        private Response()
        {
        }

        //Syngleton Instatnce method
        public static Response Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Response();
                    }
                    return instance;
                }
            }
        }

        //sending response to server
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void sendData(String msg)
        {
            //Opening the connection
            client = new TcpClient();
            IPAddress ipAd = IPAddress.Parse(ConfigurationSettings.AppSettings.Get("ServerIp"));
            BinaryWriter writer;
            try
            {


                client.Connect(ipAd, Convert.ToInt16(ConfigurationSettings.AppSettings.Get("ServerPort")));

                if (client.Connected)
                {
                    //To write to the socket
                    stm = client.GetStream();

                    //Create objects for writing across stream
                    writer = new BinaryWriter(stm);
                    Byte[] tempStr = Encoding.ASCII.GetBytes(msg);

                    //writing to the port                
                    writer.Write(tempStr);
                    System.Console.WriteLine("command send: " + msg + " thread " + Thread.CurrentThread.ManagedThreadId);
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


}

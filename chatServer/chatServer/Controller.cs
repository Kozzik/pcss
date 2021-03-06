﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chatServer
{
    public class Controller
    {
        private TcpListener server;
        private Boolean isRunning;
        private Form1 mainWindow;
        private int UID = 0;
        private int RID = 0;
        private Dictionary<int, NetworkStream> clients = new Dictionary<int, NetworkStream>();
        private List<User> users = new List<User>();
        private List<Room> rooms = new List<Room>();
        private List<string> blast = new List<string>();
        private Boolean bClientConnected;
        private String sData;
        private StreamWriter sWriter;
        private StreamReader sReader;
        private Commands commands;
        private int counter;
        private int roomCounter = 0;

        public Controller()
        {
            fillBlastIt();
            var GUImain = new Thread(initGUI);
            GUImain.Start();

        }

        [Serializable]private enum Commands
        {
            createRoom, joinRoom, handleMessage
        }
        private void initGUI()
        {
            mainWindow = new Form1();
            Application.Run(mainWindow);
        }

        public void serverSetup(int port)
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            isRunning = true;

            LoopClients();
        }
        public void LoopClients()
        {
            while (isRunning)
            {
                TcpClient newClient = server.AcceptTcpClient();
                Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                t.Start(newClient);
            }
        }
        public void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream networkStream = ((TcpClient)obj).GetStream();
            clients.Add(counter, ((TcpClient)obj).GetStream());
            counter++;
            sWriter = new StreamWriter(networkStream, Encoding.ASCII);
            string inputString = "";
            sReader = new StreamReader(networkStream, Encoding.ASCII);
            bClientConnected = true;
            sData = null;

            if (bClientConnected)
            {
                addUser(client);

                while (bClientConnected)
                {
                    inputString = sReader.ReadLine();

                    if (Enum.TryParse(inputString, out commands))
                    {
                        switch (commands)
                        {
                            case Commands.createRoom:
                                createRoom("Room"+roomCounter, RID);
                                roomCounter++;
                                inputString = "";
                                break;

                            case Commands.joinRoom:
                                User u = null;
                                Room r = null;
                                for(int i = 0; i<users.Count; i++)
                                {
                                    User tmp = users[i];
                                    if (tmp.getID().ToString() == sReader.ReadLine())
                                    {
                                        u = tmp;
                                        break;
                                    }
                                }

                                for (int i = 0; i < rooms.Count; i++)
                                {
                                    Room tmp = rooms[i];
                                    if (tmp.getID().ToString() == sReader.ReadLine())
                                    {
                                        r = tmp;
                                        break;
                                    }
                                }
                                joinRoom(u, r);
                                inputString = "";
                                break;

                            case Commands.handleMessage:
                                rooms.ForEach(x => x.broadcastMessage());
                                break;
                        }
                    }
                }
            }

        }

        public void addUser(TcpClient tcp)
        {
            sData = sReader.ReadLine();
            User u = new User(sData, UID, tcp);
            users.Add(u);
            UID++;
            Console.WriteLine(u.getName());
            sWriter.WriteLine(u.getID());
            sWriter.Flush();
        }

        public void createRoom(string n, int i)
        {
            Room r = new Room(n,i, this);
            rooms.Add(r);
            RID++;
            foreach(KeyValuePair<int, NetworkStream> entry in clients)
            {
                StreamWriter writer = new StreamWriter(entry.Value, Encoding.ASCII);
                writer.WriteLine(n);
                writer.Flush();
            }
            Console.WriteLine(r.getName());
        }

        public void joinRoom(User u, Room r)
        {
            r.joinUser(u);
        }
        public void fillBlastIt()
        {
            blast.Add("Gum");
            blast.Add("Blast it?");
            blast.Add("Another");
            blast.Add("Brick");
            blast.Add("In");
            blast.Add("The");
            blast.Add("Wall");
            blast.Add("Time");
            blast.Add("is");
            blast.Add("Passing by");

        }

        public IList getBlasted()
        {
            return blast;
        }

        public string randomBlast(int n)
        {
            return blast[n];
        }
    }
}

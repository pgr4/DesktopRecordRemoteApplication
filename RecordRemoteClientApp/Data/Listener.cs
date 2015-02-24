using RecordRemoteClientApp.Enumerations;
using RecordRemoteClientApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RecordRemoteClientApp.Data
{
    public class Listener
    {
        #region Members

        public static BusyStatus? BusyStatus;

        public static bool IsSystemBusy = false;

        public static IPAddress ThisIpAddress;

        private static Listener instance;

        private Listener() { }

        public static Listener Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Listener();
                }
                return instance;
            }
        }

        #endregion

        #region Events

        public delegate void NewAlbumDetected(NewAlbum na);
        public event NewAlbumDetected NewAlbumEvent;

        public delegate void NewCurrentAlbumDetected(NewAlbum na);
        public event NewCurrentAlbumDetected NewCurrentAlbum;

        public delegate void StatusEvent(string status, string extra = null);
        public event StatusEvent SetStatus;

        #endregion

        private const int listenPort = 30003;

        public void Speak()
        {
            SetIpAddress();

            UdpClient listener = new UdpClient(listenPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);
            IPEndPoint sendPoint = new IPEndPoint(IPAddress.Broadcast, listenPort);

            byte[] b = GetStatusBytes();
            listener.Send(b, b.Length, sendPoint);

            try
            {
                while (true)
                {
                    int pointer = 0;

                    byte[] bytes = listener.Receive(ref groupEP);

                    MessageHeader mh = MessageParser.ParseHeader(bytes, ref pointer);

                    if (mh != null)
                    {
                        switch (mh.Command)
                        {
                            case MessageCommand.None:
                            {
                                break;
                            }
                            case MessageCommand.NewAlbum:
                            {
                                NewAlbum na = MessageParser.ParseNewAlbum(bytes, ref pointer);

                                if (NewAlbumEvent != null)
                                {
                                    NewAlbumEvent(na);
                                }
                                break;
                            }
                            case MessageCommand.CurrentAlbum:
                            {
                                //Todo: Change to use CurrentAlbum methods and members later
                                NewAlbum na = MessageParser.ParseNewAlbum(bytes, ref pointer);

                                if (NewCurrentAlbum != null)
                                {
                                    NewCurrentAlbum(na);
                                }
                                break;
                            }
                            case MessageCommand.Status:
                            {
                                SendStatus();
                                break;
                            }
                            case  MessageCommand.Busy:
                            {
                                if (SetStatus != null)
                                {
                                    byte bByte = MessageParser.GetByte(bytes, ref pointer);
                                    BusyStatus = (BusyStatus)bByte;
                                    IsSystemBusy = true;
                                    SetStatus("Busy");
                                }
                                break;
                            }
                            case MessageCommand.Ready:
                            {
                                if (SetStatus != null)
                                {
                                    IsSystemBusy = false;
                                    BusyStatus = null;
                                    SetStatus("Ready");
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        //Message was not parsed correctly
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                listener.Close();
            }

        }

        public void SendStatus()
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPAddress broadcast = IPAddress.Parse("192.168.1.255");

            IPEndPoint ep = new IPEndPoint(broadcast, 30003);

            //Sender IP
            byte[] sourceIP = new byte[4];
            sourceIP[0] = 192;
            sourceIP[1] = 168;
            sourceIP[2] = 1;
            sourceIP[3] = 2;

            //Destination IP
            byte[] destinationIP = new byte[4];
            destinationIP[0] = 192;
            destinationIP[1] = 168;
            destinationIP[2] = 1;
            destinationIP[3] = 5;

            //Command
            byte command = (byte)(IsSystemBusy ? 20 : 21);

            //Signal end of Header Info
            byte[] cutoffSequence = new byte[6];
            cutoffSequence[0] = 111;
            cutoffSequence[1] = 111;
            cutoffSequence[2] = 111;
            cutoffSequence[3] = 111;
            cutoffSequence[4] = 111;
            cutoffSequence[5] = 111;
            
            int len = sourceIP.Length + destinationIP.Length + cutoffSequence.Length + 1;

            if (IsSystemBusy)
            {
                len++;
            }

            byte[] SendArray = new byte[len];
            sourceIP.CopyTo(SendArray, 0);
            destinationIP.CopyTo(SendArray, 4);
            SendArray[8] = command;
            cutoffSequence.CopyTo(SendArray, 9);
            if (IsSystemBusy)
            {
                SendArray[len - 1] = (byte)BusyStatus;
            }

            s.SendTo(SendArray, ep);
        }

        private byte[] GetStatusBytes()
        {
            int pointer = 0;

            byte[] buf = new byte[256];

            for (int i = 0; i < 4; i++)
            {
                buf[i] = ThisIpAddress.GetAddressBytes()[i];
            }

            pointer = 4;

            for (int i = pointer; i < pointer + 4; i++)
            {
                buf[i] = 12;
            }
            pointer = 8;

            buf[pointer++] = 14;

            for (int i = pointer; i < pointer + 6; i++)
            {
                buf[i] = 111;
            }

            return buf;
        }

        /// <summary>
        /// TODO:I am getting 2 valid IPs one that doesn't make any sense (.56.1)
        /// </summary>
        private void SetIpAddress()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }

            ThisIpAddress = IPAddress.Parse(localIP);
        }

    }
}

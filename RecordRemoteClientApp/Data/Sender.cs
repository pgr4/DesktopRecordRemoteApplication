using RecordRemoteClientApp.Enumerations;
using System;
using System.Net;
using System.Net.Sockets;

namespace RecordRemoteClientApp.Data
{
    public static class Sender
    {
        private static byte[] GetHeader(byte command)
        {
            byte[] ret = new byte[15];
            int i = 0;

            byte[] ip = Listener.ThisIpAddress.GetAddressBytes();

            //Sender IP
            ret[i++] = ip[0];
            ret[i++] = ip[1];
            ret[i++] = ip[2];
            ret[i++] = ip[3];

            //Destination IP
            ret[i++] = 192;
            ret[i++] = 168;
            ret[i++] = 1;
            ret[i++] = 5;

            //Command
            ret[i++] = command;

            //Signal end of Header Info
            ret[i++] = 111;
            ret[i++] = 111;
            ret[i++] = 111;
            ret[i++] = 111;
            ret[i++] = 111;
            ret[i++] = 111;

            return ret;
        }

        public static void SendPlayPauseMessage(bool play)
        {
            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPAddress broadcast = IPAddress.Parse("192.168.1.255");

                IPEndPoint ep = new IPEndPoint(broadcast, 30003);

                byte[] header = GetHeader((byte)(play ? 11 : 12));

                s.SendTo(header, ep);
            }
            catch (Exception e)
            {

            }
        }

        public static void SendGoToTrackMessage(byte b)
        {
            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPAddress broadcast = IPAddress.Parse("192.168.1.255");

                IPEndPoint ep = new IPEndPoint(broadcast, 30003);

                byte[] header = GetHeader((byte)10);

                byte[] message = new byte[header.Length + 1];
                header.CopyTo(message, 0);
                message[header.Length] = b;

                s.SendTo(message, ep);
            }
            catch (Exception e)
            {

            }
        }

        public static void SendStatusMessage(BusyStatus bs)
        {
            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPAddress broadcast = IPAddress.Parse("192.168.1.255");

                IPEndPoint ep = new IPEndPoint(broadcast, 30003);

                byte[] header = GetHeader((byte)bs);

                s.SendTo(header, ep);
            }
            catch (Exception e)
            {

            }
        }

        public static void SendStatusMessage()
        {
            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPAddress broadcast = IPAddress.Parse("192.168.1.255");

                IPEndPoint ep = new IPEndPoint(broadcast, 30003);

                byte[] header = GetHeader(3);

                s.SendTo(header, ep);
            }
            catch (Exception e)
            {

            }
        }

        public static void SendScanMessage()
        {
            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPAddress broadcast = IPAddress.Parse("192.168.1.255");

                IPEndPoint ep = new IPEndPoint(broadcast, 30003);

                byte[] header = GetHeader(4);

                s.SendTo(header, ep);
            }
            catch (Exception e)
            {

            }
        }

        public static void SendSyncMessage(byte[] b)
        {
            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPAddress broadcast = IPAddress.Parse("192.168.1.255");

                IPEndPoint ep = new IPEndPoint(broadcast, 30003);

                byte[] header = GetHeader(5);

                byte[] message = new byte[header.Length + b.Length + 6];

                header.CopyTo(message, 0);
                b.CopyTo(message, header.Length);

                for (int i = header.Length + b.Length; i < message.Length; i++)
                {
                    message[i] = 111;
                }
                    
               s.SendTo(message, ep);
            }
            catch (Exception e)
            {

            }
        }
    }
}

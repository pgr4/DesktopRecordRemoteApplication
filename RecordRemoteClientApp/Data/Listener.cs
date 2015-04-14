using RecordRemoteClientApp.Enumerations;
using RecordRemoteClientApp.Misc;
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

        public static BusyStatus BusyStatusType = BusyStatus.Unknown;

        public static PowerStatus PowerStatusType = PowerStatus.Unknown;

        public static IPAddress ThisIpAddress;

        private const int listenPort = 30003;

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

        public delegate void SyncMessageDetected(int[] key);
        public event SyncMessageDetected SyncMessage;

        public delegate void BusyStatusEvent(BusyStatus bs);
        public event BusyStatusEvent SetBusyStatus;

        public delegate void PowerStatusEvent(PowerStatus bs);
        public event PowerStatusEvent SetPowerStatus;

        public delegate void PositionUpdateEvent(int? b);
        public event PositionUpdateEvent EventPositionUpdate;

        public delegate void PlayingUpdate(bool b);
        public event PlayingUpdate EventPlayingUpdate;

        #endregion

        public void Listen()
        {
            SetIpAddress();

            UdpClient listener = new UdpClient(listenPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);

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
                                break;
                            case MessageCommand.NewAlbum:
                                if (mh.DestinationAddress.Equals(ThisIpAddress))
                                {
                                    Application.Current.Dispatcher.Invoke(new Action(() =>
                                    {
                                        NewAlbum na = MessageParser.ParseNewAlbum(bytes, ref pointer);

                                        if (NewAlbumEvent != null)
                                        {
                                            NewAlbumEvent(na);
                                        }
                                    }));
                                }
                                break;
                            case MessageCommand.CurrentAlbum:
                                break;
                            case MessageCommand.Status:
                                if (!mh.SourceAddress.Equals(ThisIpAddress))
                                {
                                    Sender.SendStatusMessage(BusyStatusType);
                                }
                                break;
                            case MessageCommand.Sync:
                                if (!mh.SourceAddress.Equals(ThisIpAddress))
                                {
                                    Application.Current.Dispatcher.Invoke(new Action(() =>
                                    {
                                        int[] bKey = MessageParser.ParseKey(bytes, ref pointer);
                                        if (SyncMessage != null)
                                        {

                                            SyncMessage(bKey);

                                        }
                                    }));
                                }
                                break;
                            case MessageCommand.Scan:
                                //Ignore
                                break;
                            case MessageCommand.GetPower:
                                if (!mh.SourceAddress.Equals(ThisIpAddress))
                                {
                                    Sender.SendGenericMessage((MessageCommand)PowerStatusType + 14);
                                }
                                break;
                            case MessageCommand.SwitchPowerOn:
                            case MessageCommand.SwitchPowerOff:
                            case MessageCommand.MediaGoToBeginning:
                            case MessageCommand.QueueGoToBeginning:
                            case MessageCommand.MediaGoToTrack:
                            case MessageCommand.QueueGoToTrack:
                            case MessageCommand.MediaPlay:
                            case MessageCommand.MediaStop:
                                //Ignore
                                break;
                            case MessageCommand.On:
                            case MessageCommand.Off:
                            case MessageCommand.PowerUnknown:
                                if (!mh.SourceAddress.Equals(ThisIpAddress))
                                {
                                    if (SetPowerStatus != null)
                                    {
                                        Application.Current.Dispatcher.Invoke(new Action(() =>
                                        {
                                            PowerStatus ps = (PowerStatus)mh.Command - 14;
                                            PowerStatusType = ps;
                                            SetPowerStatus(ps);
                                        }));
                                    }
                                }
                                break;
                            case MessageCommand.PositionUpdate:
                                if (EventPositionUpdate != null)
                                {
                                    Application.Current.Dispatcher.Invoke(new Action(() =>
                                    {
                                        EventPositionUpdate(MyConverters.BytesToInt(MessageParser.GetByte(bytes, ref pointer), MessageParser.GetByte(bytes, ref pointer)));
                                    }));
                                }
                                break;
                            case MessageCommand.AtBeginning:
                                if (EventPositionUpdate != null)
                                {
                                    Application.Current.Dispatcher.Invoke(new Action(() =>
                                    {
                                        EventPositionUpdate(null);
                                    }));
                                }
                                break;
                            case MessageCommand.Play:
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    if (SetBusyStatus != null)
                                    {
                                        SetBusyStatus((BusyStatus)mh.Command);
                                        BusyStatusType = (BusyStatus)mh.Command;
                                    }
                                    if (EventPlayingUpdate != null)
                                    {
                                        EventPlayingUpdate(true);
                                    }
                                }));
                                break;
                            case MessageCommand.GoToTrack:
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    if (SetBusyStatus != null)
                                    {
                                        SetBusyStatus((BusyStatus)mh.Command);
                                        BusyStatusType = (BusyStatus)mh.Command;
                                    }
                                    if (EventPlayingUpdate != null)
                                    {
                                        EventPlayingUpdate(false);
                                    }
                                }));
                                break;
                            case MessageCommand.Pause:
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    if (SetBusyStatus != null)
                                    {
                                        SetBusyStatus((BusyStatus)mh.Command);
                                        BusyStatusType = (BusyStatus)mh.Command;
                                    }
                                    if (EventPlayingUpdate != null)
                                    {
                                        EventPlayingUpdate(false);
                                    }
                                }));
                                break;
                            case MessageCommand.Stop:
                            case MessageCommand.sScan:
                            case MessageCommand.Unknown:
                            case MessageCommand.Ready:
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    if (SetBusyStatus != null)
                                    {
                                        SetBusyStatus((BusyStatus)mh.Command);
                                        BusyStatusType = (BusyStatus)mh.Command;
                                    }
                                }));
                                break;
                            default:
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    if (SetBusyStatus != null)
                                    {
                                        SetBusyStatus((BusyStatus)mh.Command);
                                        BusyStatusType = (BusyStatus)mh.Command;
                                    }
                                }));
                                break;
                        }
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

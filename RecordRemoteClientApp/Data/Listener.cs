﻿using RecordRemoteClientApp.Enumerations;
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

        public delegate void SyncMessageDetected(byte[] key);
        public event SyncMessageDetected SyncMessage;

        public delegate void BusyStatusEvent(BusyStatus bs);
        public event BusyStatusEvent SetBusyStatus;

        public delegate void PowerStatusEvent(PowerStatus bs);
        public event PowerStatusEvent SetPowerStatus;

        public delegate void PositionUpdateEvent(byte? b);
        public event PositionUpdateEvent EventPositionUpdate;

        #endregion

        public void Speak()
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
                                    NewAlbum na = MessageParser.ParseNewAlbum(bytes, ref pointer);

                                    if (NewAlbumEvent != null)
                                    {
                                        NewAlbumEvent(na);
                                    }
                                }
                                break;
                            case MessageCommand.CurrentAlbum:
                                //Todo: THIS IS TEST
                                NewAlbum na1 = MessageParser.ParseNewAlbum(bytes, ref pointer);

                                if (NewCurrentAlbum != null)
                                {
                                    NewCurrentAlbum(na1);
                                }
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
                                    byte[] bKey = MessageParser.ParseKey(bytes, ref pointer);
                                    if (SyncMessage != null)
                                    {
                                        SyncMessage(bKey);
                                    }
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
                            case MessageCommand.MediaSkip:
                            case MessageCommand.MediaGoToTrack:
                            case MessageCommand.MediaPlay:
                            case MessageCommand.MediaStop:
                            case MessageCommand.MediaRewind:
                                //Ignore
                                break;
                            case MessageCommand.On:
                            case MessageCommand.Off:
                            case MessageCommand.PowerUnknown:
                                if (!mh.SourceAddress.Equals(ThisIpAddress))
                                {
                                    if (SetPowerStatus != null)
                                    {
                                        PowerStatus ps = (PowerStatus)mh.Command - 14;
                                        PowerStatusType = ps;
                                        SetPowerStatus(ps);
                                    }
                                }
                                break;
                            case MessageCommand.PositionUpdate:
                                if (EventPositionUpdate != null)
                                {
                                    EventPositionUpdate(MessageParser.GetByte(bytes, ref pointer));
                                }
                                break;
                            case MessageCommand.AtBeginning:
                                if (EventPositionUpdate != null)
                                {
                                    EventPositionUpdate(null);
                                }
                                break;
                            default:
                                //TODO: MAKE SURE ALL COMMANDS ARE BUSY IF LEFT DEFAULT
                                if (SetBusyStatus != null)
                                {
                                    SetBusyStatus((BusyStatus)mh.Command);
                                    BusyStatusType = (BusyStatus)mh.Command;
                                }
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

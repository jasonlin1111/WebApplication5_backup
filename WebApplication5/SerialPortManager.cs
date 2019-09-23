using System;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace Bezel8PlusApp
{

    public enum PktType
    {
        SI = 0,
        STX = 1
    }

    public sealed class SerialPortManager
    {
        private static readonly Lazy<SerialPortManager> lazy = new Lazy<SerialPortManager>(() => new SerialPortManager());
        public static SerialPortManager Instance { get { return lazy.Value; } }

        private SerialPort _serialPort;
        private static bool _stopTransaction = false;

        private SerialPortManager()
        {
            _serialPort = new SerialPort();
        }

        /// <summary>
        /// Update the serial port status to the event subscriber
        /// </summary>
        public event EventHandler<string> OnStatusChanged;

        /// <summary>
        /// Update received data from the serial port to the event subscriber
        /// </summary>
        public event EventHandler<byte[]> OnDataSent;

        /// <summary>
        /// Update received data from the serial port to the event subscriber
        /// </summary>
        public event EventHandler<byte[]> OnDataReceived;


        /// <summary>
        /// Update TRUE/FALSE for the serial port connection to the event subscriber
        /// </summary>
        public event EventHandler<bool> OnSerialPortOpened;

        /// <summary>
        /// Return TRUE if the serial port is currently connected
        /// </summary>
        public bool IsOpen { get { return _serialPort.IsOpen; } }

        public int GetWriteBufferSize() { return _serialPort.WriteBufferSize; }

        /// <summary>
        /// Open the serial port connection using basic serial port settings
        /// </summary>
        /// <param name="portname">COM1 / COM3 / COM4 / etc.</param>
        /// <param name="baudrate">0 / 100 / 300 / 600 / 1200 / 2400 / 4800 / 9600 / 14400 / 19200 / 38400 / 56000 / 57600 / 115200 / 128000 / 256000</param>
        /// <param name="parity">None / Odd / Even / Mark / Space</param>
        /// <param name="databits">5 / 6 / 7 / 8</param>
        /// <param name="stopbits">None / One / Two / OnePointFive</param>
        /// <param name="handshake">None / XOnXOff / RequestToSend / RequestToSendXOnXOff</param>
        /// </summary>
        public void Open(
            string portname = "COM1",
            int baudrate = 9600,
            Parity parity = Parity.None,
            int databits = 8,
            StopBits stopbits = StopBits.One,
            Handshake handshake = Handshake.None)
        {
            if (_serialPort.IsOpen)
            {
                Close();
            }
            try
            {
                _serialPort.PortName = portname;
                _serialPort.BaudRate = baudrate;
                _serialPort.Parity = parity;
                _serialPort.DataBits = databits;
                _serialPort.StopBits = stopbits;
                _serialPort.Handshake = handshake;

                _serialPort.ReadTimeout = 5000;
                //_serialPort.WriteTimeout = 200;

                _serialPort.Open();
                //StartReading();
            }
            catch (Exception ex)
            {
                throw ex;
                if (OnStatusChanged != null)
                    OnStatusChanged(this, "Error: " + ex.Message);
            }

            if (_serialPort.IsOpen)
            {
                string sb = StopBits.None.ToString().Substring(0, 1);
                switch (_serialPort.StopBits)
                {
                    case StopBits.One:
                        sb = "1"; break;
                    case StopBits.OnePointFive:
                        sb = "1.5"; break;
                    case StopBits.Two:
                        sb = "2"; break;
                    default:
                        break;
                }

                string p = _serialPort.Parity.ToString().Substring(0, 1);
                string hs = _serialPort.Handshake == Handshake.None ? "No Handshake" : _serialPort.Handshake.ToString();

                if (OnStatusChanged != null)
                    OnStatusChanged(this, string.Format(
                    "Connected to {0}: {1} bps, {2}{3}{4}, {5}.",
                    _serialPort.PortName,
                    _serialPort.BaudRate,
                    _serialPort.DataBits,
                    p,
                    sb,
                    hs));

                if (OnSerialPortOpened != null)
                    OnSerialPortOpened(this, true);
            }
            else
            {
                if (OnStatusChanged != null)
                    OnStatusChanged(this, string.Format(
                    "{0} already in use.",
                    portname));

                if (OnSerialPortOpened != null)
                    OnSerialPortOpened(this, false);
            }
        }

        /// <summary>
        /// Close the serial port connection
        /// </summary>
        public void Close()
        {
            _serialPort.Close();

            if (OnStatusChanged != null)
                OnStatusChanged(this, "Connection closed.");

            if (OnSerialPortOpened != null)
                OnSerialPortOpened(this, false);
        }

        public void CancelTransaction()
        {
            _stopTransaction = true;
        }

        public void StartTransaction()
        {
            _stopTransaction = false;
        }

        public void WriteAndReadMessage(PktType type, string head, string body, out string responseOut, bool keepWaitting = true, int readTimeOut = 0)
        {
            string prefix = String.Empty;
            string suffix = String.Empty;
            responseOut = String.Empty;

            if (type == PktType.SI)
            {
                prefix = Convert.ToChar(0x0F).ToString();
                suffix = Convert.ToChar(0x0E).ToString();
            }
            else if (type == PktType.STX)
            {
                prefix = Convert.ToChar(0x02).ToString();
                suffix = Convert.ToChar(0x03).ToString();
            }


            string packed_meaasge = prefix + head + body + suffix;
            byte lrc = DataManager.LRCCalculator(Encoding.ASCII.GetBytes(packed_meaasge), packed_meaasge.Length);

            if (_serialPort.BytesToRead > 0)
                _serialPort.DiscardInBuffer();

            if (_serialPort.BytesToWrite > 0)
                _serialPort.DiscardOutBuffer();

            // Sending message
            try
            {
                _serialPort.Write(packed_meaasge + Convert.ToChar(lrc).ToString());
                if (OnDataSent != null)
                {
                    byte[] data_sent = Encoding.ASCII.GetBytes(packed_meaasge + Convert.ToChar(lrc).ToString());
                    OnDataSent(this, data_sent);
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //throw new System.Exception("Sending message failed.");
            }

            // Check if ACK is received
            try
            {
                byte[] controlCode = new byte[1];
                _serialPort.Read(controlCode, 0, 1);

                if (OnDataReceived != null)
                {
                    OnDataReceived(this, controlCode);
                }

                switch (controlCode[0])
                {
                    case 0x06:
                    case 0x04:
                        break;

                    case 0x15:
                        throw new System.Exception("Received NAK from reader: Incorrect LRC.");

                    default:
                        // Unknown
                        throw new System.Exception("Unknown response: 0x" + controlCode[0].ToString("X2"));
                }
            }
            catch (Exception ex)
            {
                throw ex;
                throw new System.Exception(packed_meaasge + "Waitting ACK failed.");
            }

            if (!keepWaitting)
                return;

            // Waitting for reply from reader
            Stopwatch s = new Stopwatch();
            if (readTimeOut > 0)
                s.Start();
            while (s.Elapsed <= TimeSpan.FromMilliseconds(readTimeOut))
            {
                if (_stopTransaction)
                {
                    _stopTransaction = false;
                    string t6CMeaasge = Convert.ToChar(0x02).ToString() + "T6C" + Convert.ToChar(0x03).ToString();
                    byte t6cLrc = DataManager.LRCCalculator(Encoding.ASCII.GetBytes(t6CMeaasge), t6CMeaasge.Length);
                    Thread.Sleep(500);
                    _serialPort.Write(t6CMeaasge + Convert.ToChar(t6cLrc).ToString());
                }

                if (_serialPort.BytesToRead == 1)
                {
                    _serialPort.DiscardInBuffer();
                }   
                else if (_serialPort.BytesToRead > 1)
                    break;

                /*
                if (_serialPort.BytesToRead > 0)
                    break;
                */
            }
            s.Reset();

            if (_serialPort.BytesToRead > 0)
            {
                int bytes = _serialPort.BytesToRead;
                byte[] readBuffer = new byte[bytes];
                _serialPort.Read(readBuffer, 0, bytes);
                if (OnDataReceived != null)
                {
                    OnDataReceived(this, readBuffer);
                }

                // Well recevied, check LRC
                if (readBuffer[bytes - 1] == DataManager.LRCCalculator(readBuffer, bytes - 1))
                {
                    // Send ACK
                    _serialPort.Write(Convert.ToChar(0x06).ToString());
                    if (OnDataSent != null)
                    {
                        byte[] ack = Encoding.ASCII.GetBytes(Convert.ToChar(0x06).ToString());
                        OnDataSent(this, ack);
                    }
                    responseOut = Encoding.ASCII.GetString(readBuffer, 1, bytes - 3);
                }
                else
                {
                    // Send NAK
                    _serialPort.Write(Convert.ToChar(0x15).ToString());
                    if (OnDataSent != null)
                    {
                        byte[] nak = Encoding.ASCII.GetBytes(Convert.ToChar(0x15).ToString());
                        OnDataSent(this, nak);
                    }
                }
            }
            else
            {
                throw new System.TimeoutException("Timeout: No response");
            }

        }
    }
}

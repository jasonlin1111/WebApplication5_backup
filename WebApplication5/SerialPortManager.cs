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
            int baudrate = 115200,
            Parity parity = Parity.None,
            int databits = 8,
            StopBits stopbits = StopBits.One,
            Handshake handshake = Handshake.None)
        {
            if (_serialPort.IsOpen)
            {
                return;
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
                /*
                if (OnDataSent != null)
                {
                    byte[] data_sent = Encoding.ASCII.GetBytes(packed_meaasge + Convert.ToChar(lrc).ToString());
                    OnDataSent(this, data_sent);
                }
                */
            }
            catch (Exception ex)
            {
                throw new System.Exception(string.Format("Sending message failed: {0}", ex.Message));
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
                        // NAK
                        throw new System.Exception("Received NAK from reader: Incorrect LRC.");

                    default:
                        // Unknown
                        throw new System.Exception("Unknown response: 0x" + controlCode[0].ToString("X2"));
                }
            }
            catch (TimeoutException)
            {
                // Reader no response;
                throw new System.TimeoutException("Timeout: No ACK");
            }
            catch (Exception ex)
            {
                throw new System.Exception(string.Format("Waitting ACK failed: {0}", ex.Message));
            }

            if (!keepWaitting)
                return;

            int response_length = 0;
            byte[] readBuffer = new byte[_serialPort.ReadBufferSize];
            byte bPrefix, bSuffix;

            if (type == PktType.SI)
            {
                bPrefix = 0x0F;
                bSuffix = 0x0E;
            }
            else
            {
                bPrefix = 0x02;
                bSuffix = 0x03;
            }

            try
            {
                // Waitting for reply from reader
                Stopwatch s = new Stopwatch();
                if (readTimeOut > 0)
                    s.Start();
                while (s.Elapsed <= TimeSpan.FromMilliseconds(readTimeOut))
                {
                    if (_serialPort.BytesToRead == 0)
                        continue;
                    int bytes = _serialPort.BytesToRead;
                    _serialPort.Read(readBuffer, response_length, bytes);
                    response_length += bytes;

                    if (response_length > 2 && readBuffer[0] == bPrefix && readBuffer[response_length - 2] == bSuffix)
                    {
                        if (OnDataReceived != null)
                        {
                            byte[] localBuffer = new byte[response_length];
                            Array.Copy(readBuffer, localBuffer, response_length);
                            OnDataReceived(this, localBuffer);
                        }

                        // LRC check
                        if (readBuffer[response_length - 1] == DataManager.LRCCalculator(readBuffer, response_length - 1))
                        {
                            // Send ACK
                            _serialPort.Write(Convert.ToChar(0x06).ToString());
                            if (OnDataSent != null)
                            {
                                byte[] ack = Encoding.ASCII.GetBytes(Convert.ToChar(0x06).ToString());
                                OnDataSent(this, ack);
                            }
                            responseOut = Encoding.ASCII.GetString(readBuffer, 1, response_length - 3);
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
                        return;
                    }
                }
                s.Reset();

                // Timeout
                throw new System.TimeoutException("Timeout: No response");
            }
            catch (Exception ex)
            {
                throw new System.Exception(string.Format("Waitting response failed: {0}", ex.Message));
            }
        }
    }
}

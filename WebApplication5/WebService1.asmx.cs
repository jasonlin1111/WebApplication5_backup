using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Bezel8PlusApp;
using System.Configuration;

namespace WebApplication5
{
    /// <summary>
    ///WebService1 的摘要描述
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允許使用 ASP.NET AJAX 從指令碼呼叫此 Web 服務，請取消註解下列一行。
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService, IBasicHttpBinding_ITestService
    {

        private SerialPortManager _serialPort;
        private ParameterHandler _parameterHandler;
        private DebugLogHandler _debugLogHandler;
        private string _portName;
        private int _buadrate;
        StackTrace stackTrace;

        public WebService1()
        {
            _parameterHandler = new ParameterHandler();
            _debugLogHandler = new DebugLogHandler();
            _serialPort = SerialPortManager.Instance;
            _buadrate = Int32.Parse(ConfigurationManager.AppSettings["buadrate"]);
            _portName = ConfigurationManager.AppSettings["portName"];
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open(_portName);
            }
            stackTrace = new StackTrace();
        }

        public ReturnData GetConfig()
        {
            Trace.WriteLine(string.Format("{0}  GetConfig(): Call", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
            ReturnData returnData = new ReturnData
            {
                ReturnCode = ReturnCode.Successful,
                ReturnValue = _parameterHandler.GetCurrentParameters()
            };
            Trace.WriteLine(string.Format("{0}  GetConfig(): Return", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
            return returnData;
        }

        public ReturnData SetConfigToDefault()
        {
            Trace.WriteLine(string.Format("{0}  SetConfigToDefault(): Call", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
            ReturnData returnData = new ReturnData
            {
                ReturnCode = ReturnCode.Successful
            };

            _parameterHandler.SetupConfigToDefault();
            try
            {
                SetupApplicationConfigToReader();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("{0}   Exception: {1}", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff"), ex.Message));
                returnData.ReturnCode = ReturnCode.Error;
                returnData.ReturnCodeSpecified = true;
            }
            Trace.WriteLine(string.Format("{0}  SetConfigToDefault(): Return", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
            return returnData;
        }

        public ReturnData SetConfig(ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary[] configuration)
        {
            Trace.WriteLine(string.Format("{0}  SetConfig(): Call", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
            ReturnData returnData = new ReturnData
            {
                ReturnCode = ReturnCode.Successful
            };

            if (configuration == null || configuration.Length == 0)
                return returnData;

            try
            {
                foreach (ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary config in configuration)
                {
                    _parameterHandler.SetParameter(config);
                }
                SetupApplicationConfigToReader();
            }

            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("{0}   Exception: {1}", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff"), ex.Message));
                returnData.ReturnCode = ReturnCode.Error;
                returnData.ReturnCodeSpecified = true;
            }
            Trace.WriteLine(string.Format("{0}  SetConfig(): Return", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
            return returnData;
        }

        public ReturnData PrepareTransaction()
        {
            Trace.WriteLine(string.Format("{0}  PrepareTransaction(): Call", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
            ReturnData returnData = new ReturnData { ReturnCode = ReturnCode.Successful };

            try
            {
                if (!_serialPort.IsOpen)
                {
                    _serialPort.Open(_portName);
                    Thread.Sleep(300);
                }

                if (!IsReaderOnIdle())
                {
                    Trace.WriteLine("0");
                    _serialPort.WriteAndReadMessage(PktType.STX, "T6C", "", out string t6Cresponse, false);
                    Trace.WriteLine("1");
                }
                    
                // Setting Timeout for Transaction
                int timeout = _parameterHandler.GetTimeoutPeriod();
                _serialPort.WriteAndReadMessage(PktType.STX, "V09", Convert.ToChar(0x1A).ToString() + timeout.ToString(), out string v09response);
                if (!v09response.ToUpper().Equals("V0A0"))
                    throw new Exception(v09response);

            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("{0}   Exception: {1}", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff"), ex.Message));
                returnData.ReturnCode = ReturnCode.Error;
                returnData.ReturnCodeSpecified = true;
            }
            Trace.WriteLine(string.Format("{0}  PrepareTransaction(): Return", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
            return returnData;

        }

        public ReturnData StartTransaction(double amount, bool amountSpecified)
        {
            Trace.WriteLine(string.Format("{0}  StartTransaction(): Call", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));

            ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary txnResult = new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary
            {
                Key = new byte[] { 0xC1, 0x01 }
            };
            ReturnData returnData = new ReturnData
            {
                ReturnCode = ReturnCode.Successful,
                ReturnValue = new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary[] { txnResult }
            };

            byte[] statusWord = null;
            try
            {
                // Initial Transaction
                string t61Message = BuildTxnDataStream(amount);
                //Thread.Sleep(100);
                Trace.WriteLine(string.Format("{0}  StartTransaction(): Sending T61", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
                _serialPort.WriteAndReadMessage(PktType.STX, "T61", t61Message, out string t61Response, true, 30000);
                Trace.WriteLine(string.Format("{0}  StartTransaction(): T61 sent", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));

                _debugLogHandler.ClearDebugLog();

                _serialPort.WriteAndReadMessage(PktType.STX, "V03", "", out string v03Response, true);
                
                if (v03Response.ToUpper().StartsWith("V040"))
                {
                    string command = v03Response.Substring(v03Response.LastIndexOf(Convert.ToChar(0x1A)) + 1, 4);
                    statusWord = DataManager.HexStringToByteArray(v03Response.Substring(v03Response.Length - 4));

                    _debugLogHandler.Add(
                        new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary
                        {
                            Key = DataManager.HexStringToByteArray(command),
                            Value = DataManager.HexStringToByteArray(v03Response.Substring(9))
                        }
                        );
                }
                GetTxnData();
                returnData.ReturnValue[0].Value = TransactionResultAnalyze(t61Response, statusWord);
            }
            catch (Exception ex)
            { 
                Trace.WriteLine(string.Format("{0}   Exception: {1}", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff"), ex.Message));
                txnResult.Value = new byte[] { 0xEF, 0x00 };
                returnData.ReturnCode = ReturnCode.Error;
                returnData.ReturnCodeSpecified = true;
            }
            Trace.WriteLine(string.Format("{0}  StartTransaction(): Return", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
            return returnData;

        }

        public ReturnData StartTransactionAsync(double amount, bool amountSpecified)
        {
            return StartTransaction(amount, amountSpecified);
        }

        public ReturnData StopCurrentTransaction()
        {
            Thread thread = Thread.CurrentThread;
            Trace.WriteLine(string.Format("{0}  StopCurrentTransaction(): Call", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
            ReturnData returnData = new ReturnData { ReturnCode = ReturnCode.Successful };
            //_serialPort.CancelTransaction();

            try
            {
                Thread.Sleep(100);
                Trace.WriteLine(string.Format("{0}  StopCurrentTransaction(): Sending T6C", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
                _serialPort.WriteAndReadMessage(PktType.STX, "T6C", "", out string t6CResponse, false);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("{0}   Exception: {1}", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff"), ex.Message));
                returnData.ReturnCode = ReturnCode.Error;
                returnData.ReturnCodeSpecified = true;
            }

            Trace.WriteLine(string.Format("{0}  StopCurrentTransaction(): Return", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
            return returnData;
        }

        public ReturnData GetDebugLog()
        {
            Trace.WriteLine(string.Format("{0}  GetDebugLog(): Call", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
            ReturnData returnData = new ReturnData { ReturnCode = ReturnCode.Successful };
            returnData.ReturnValue = _debugLogHandler.GetDebugLog();
            Trace.WriteLine(string.Format("{0}  GetDebugLog(): Return", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
            return returnData;
        }

        public ReturnData ClearLogs()
        {
            Trace.WriteLine(string.Format("{0}  ClearLogs(): Call", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
            ReturnData returnData = new ReturnData { ReturnCode = ReturnCode.Successful };
            _debugLogHandler.ClearDebugLog();
            Trace.WriteLine(string.Format("{0}  ClearLogs(): Return", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
            return returnData;
        }

        public ReturnData GetDeviceState()
        {
            Trace.WriteLine(string.Format("{0}  GetDeviceState(): Call", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
            ReturnData returnData = new ReturnData
            {
                ReturnCode = ReturnCode.Successful
            };
            int count = 0;
            try
            {
                while (!_serialPort.IsOpen && count < 3)
                {
                    _serialPort.Open(_portName);
                    Thread.Sleep(500);
                    count += 1;
                }

                if (!_serialPort.IsOpen)
                    throw new Exception();

                _serialPort.WriteAndReadMessage(PktType.STX, "V01", "", out string response, true, 1000);
                if (string.IsNullOrEmpty(response) || !response.Equals("V020"))
                    throw new Exception(response);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("{0}   Exception: {1}", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff"), ex.Message));
                returnData.ReturnCode = ReturnCode.Error;
                returnData.ReturnCodeSpecified = true;
            }
            Trace.WriteLine(string.Format("{0}  GetDeviceState(): Return", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
            return returnData;
        }

        public ReturnData ResetDevice()
        {
            Trace.WriteLine(string.Format("{0}  ResetDevice(): Call", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
            ReturnData returnData = new ReturnData { ReturnCode = ReturnCode.Successful };

            try
            {
                _serialPort.WriteAndReadMessage(PktType.STX, "U8", "", out string response, false);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("{0}   Exception: {1}", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff"), ex.Message));
                returnData.ReturnCode = ReturnCode.Error;
                returnData.ReturnCodeSpecified = true;
            }
            Trace.WriteLine(string.Format("{0}  ResetDevice(): Return", DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff")));
            return returnData;
        }


        // Helper Functions
        private bool IsReaderOnIdle()
        {
            try
            {
                _serialPort.WriteAndReadMessage(PktType.STX, "V01", "", out string response, true, 1000);
                if (!string.IsNullOrEmpty(response) && response.Equals("V020"))
                    return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
        }

        private void SetupApplicationConfigToReader()
        {
            ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary[] currentConfig = _parameterHandler.GetCurrentParameters();
            string t55Message = String.Empty;
            string s1A = Convert.ToChar(0x1A).ToString();
            string s1C = Convert.ToChar(0x1C).ToString();
            string txnType = currentConfig[0x1B].Value[0].ToString("X2");

            t55Message += s1A + txnType + s1A + "030000" + s1A + "A000000003";
            t55Message += s1A + "DF810C" + s1C + "2" + s1C + "030000";

            foreach (ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary config in currentConfig)
            {

                if (DataHandler.tagTableVcasToUIC.TryGetValue(DataManager.ToHexString(config.Key), out string tagUIC))
                {
                    t55Message += s1A + tagUIC + s1C + "2" + s1C + DataManager.ToHexString(config.Value);
                }

            }

            // Zero Amount Option
            if (currentConfig[0x09].Value[0] != 0x00)
                t55Message += s1A + "FFFF8008" + s1C + "2" + s1C + currentConfig[0x09].Value[0].ToString("X2");
            else
                t55Message += s1A + "FFFF8008" + s1C + "2" + s1C + "01";

            try
            {
                _serialPort.WriteAndReadMessage(PktType.STX, "T5511", t55Message, out string t55Response, true, 5000);
                if (string.IsNullOrEmpty(t55Response) || !t55Response.Equals("T560"))
                {
                    throw new Exception("Setup Applciation Config to Reader Response: " + t55Response);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private byte[] TransactionResultAnalyze(string result, byte[] statusWord)
        {
            if (result.StartsWith("T620"))
            {
                // Success
                switch (result.Substring(4))
                {
                    case TxnResult.OnlineApprove:
                        return new byte[] { 0x30, 0x30 };

                    case TxnResult.OfflineApprove:
                    case TxnResult.OfflineApproveSign:
                        return new byte[] { 0x59, 0x31 };

                    case TxnResult.OfflineDecline:
                        return new byte[] { 0x5A, 0x31 };

                    case TxnResult.TryAnotherInterface:
                        return new byte[] { 0xEF, 0x06 };

                    case TxnResult.OnlineAuthorizeReq:
                    case TxnResult.ExternalPinBlockReq:
                        Thread.Sleep(700);
                        try
                        {
                            string s1A = Convert.ToChar(0x1A).ToString();
                            _serialPort.WriteAndReadMessage(PktType.STX, "T71", "1" + s1A + "00" + s1A, out string t71Response);
                            return TransactionResultAnalyze(t71Response, null);
                        }
                        catch (Exception ex)
                        {
                            break;
                        }

                    default:
                        break;

                }
            }
            else if (result.StartsWith("T6211"))
            {
                switch (result.Substring(5))
                {
                    case "F1111111":
                        // Cancelled
                        _debugLogHandler.ClearDebugLog();
                        return new byte[] { 0xEF, 0x00 };

                    case "80000103":
                        // Collision
                        _debugLogHandler.ClearDebugLog();
                        return new byte[] { 0xEF, 0x04 };

                    case "F1111114":
                        // Timeout
                        _debugLogHandler.ClearDebugLog();
                        return new byte[] { 0xEF, 0x03 };

                    default:
                        break;
                }
            }
            if (statusWord != null && statusWord[0] != 0x90 && statusWord[1] != 0x00)
                return new byte[] { 0xEF, 0x01 };
            else
                return new byte[] { 0xEF, 0x02 };

        }

        private string BuildTxnDataStream(double amount)
        {
            ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary[] currentConfig = _parameterHandler.GetCurrentParameters();
            string s1A = Convert.ToChar(0x1A).ToString();
            string sAmount = amount.ToString();
            int decimalPoint = sAmount.IndexOf('.');
            int currencyExp = 2;

            if (decimalPoint < 0)
                sAmount += "00";
            else
                currencyExp = sAmount.Length - decimalPoint - 1;

            if (decimalPoint >= 0)
                sAmount = sAmount.Remove(decimalPoint, 1);

            sAmount = sAmount.PadLeft(12, '0');

            // Txn Type
            string txnType = currentConfig[0x1C - 1].Value[0].ToString("X2");

            // Currency Code
            string curcode = currentConfig[0x20 - 1].Value[0].ToString("X2") + currentConfig[0x20 - 1].Value[1].ToString("X2");

            // Transaction Info
            string txnInfo = "40";

            // Account Type
            string accType = "00";

            // Force Online
            string forceOnline = "0";

            //[AmtAuth]<1A>[AmtOther]<1A>[CurExponent + CurCode]<1A>[TranType]<1A>[TranInfo]<1A>[Account Type]<1A>[Force Online]
            return
                s1A + sAmount + s1A + s1A + currencyExp.ToString() + curcode.Substring(1) +
                s1A + txnType + s1A + txnInfo + s1A + accType + s1A + forceOnline + s1A;
        }

        private bool SetupTerminalConfigToReader()
        {
            string t51Message = String.Empty;
            string s1A = Convert.ToChar(0x1A).ToString();
            string s1C = Convert.ToChar(0x1C).ToString();

            t51Message += 
                s1A + "9F15" + s1C + "6" + s1C + "0001" +
                s1A + "9F16" + s1C + "4" + s1C + "VMNY_SY00000001" +
                s1A + "9F1C" + s1C + "3" + s1C + "00001768" +
                s1A + "9F1E" + s1C + "3" + s1C + "00000001" +
                s1A + "9F35" + s1C + "6" + s1C + "25" +
                s1A + "9F4E" + s1C + "4" + s1C + "UIC_PDR";

            try
            {
                _serialPort.WriteAndReadMessage(PktType.STX, "T5111", t51Message, out string t51Response, true, 5000);
                if (string.IsNullOrEmpty(t51Response) || !t51Response.Equals("T520"))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }

        private void GetTxnData()
        {
            try
            {
                // Time Stamps
                _debugLogHandler.Add(
                    new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary
                    {
                        Key = new byte[] { 0xC0, 0x1E },
                        Value = DataManager.HexStringToByteArray(DateTime.Now.ToString("HHmmss"))
                    }
                    );

                string[] logTags = new string[] { "9F27", "9F10", "9F66" };
                _serialPort.WriteAndReadMessage(PktType.STX, "T63", string.Join(Convert.ToChar(0x1A).ToString(), logTags), out string t63Response, true, 2000);

                Dictionary<string, string> dicTagValue = new Dictionary<string, string>();
                string[] dataObj = t63Response.Split(Convert.ToChar(0x1A));
                foreach (string data in dataObj)
                {
                    string[] tlv = data.Split(Convert.ToChar(0x1C));
                    if (tlv.Length == 3)
                    {
                        dicTagValue.Add(tlv[0], tlv[2]);
                    }
                }

                // CID 9F27
                if (dicTagValue.TryGetValue("9F27", out string value9F27))
                {
                    _debugLogHandler.Add(
                        new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary
                        {
                            Key = new byte[] { 0x9F, 0x27 },
                            Value = DataManager.HexStringToByteArray(value9F27)
                        }
                        );
                }

                // IAD 9F10
                if (dicTagValue.TryGetValue("9F10", out string value9F10))
                {
                    _debugLogHandler.Add(
                        new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary
                        {
                            Key = new byte[] { 0x9F, 0x10 },
                            Value = DataManager.HexStringToByteArray(value9F10)
                        }
                        );
                }

                // TTQ 9F66
                if (dicTagValue.TryGetValue("9F66", out string value9F66))
                {
                    _debugLogHandler.Add(
                        new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary
                        {
                            Key = new byte[] { 0x9F, 0x66 },
                            Value = DataManager.HexStringToByteArray(value9F66)
                        }
                        );
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

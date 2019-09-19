using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bezel8PlusApp
{
    public enum Resident
    {
        App,
        Terminal,
        Na
    }

    class DataManager
    {
    
        public static char CalculateLRC(string toEncode)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(toEncode);
            byte LRC = 0;
            for (int i = 1; i < bytes.Length; i++)
            {
                LRC ^= bytes[i];
            }
            return Convert.ToChar(LRC);
        }

        static public byte LRCCalculator(byte[] input, int length)
        {
            byte LRC = 0;

            if (length > input.Length)
            {
                length = input.Length;
            }

            for (int i = 1; i < length; i++)
            {
                LRC ^= input[i];
            }

            return LRC;
        }

        public static byte[] HexStringToByteArray(string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
                return null;

            if (hexString.Length % 2 != 0)
                hexString = hexString.PadLeft(hexString.Length + 1, '0');

            byte[] byteOUT = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i = i + 2)
            {
                byteOUT[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }
            return byteOUT;
        }

        public static string ToHexString(byte[] bytes)
        {
            string hexString = String.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    strB.Append(bytes[i].ToString("X2"));
                }
                hexString = strB.ToString();
            }
            return hexString;
        }
    }

    public class DebugLogHandler
    {
        private static List<ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary> debugLog = 
            new List<ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary>();

        public void ClearDebugLog()
        {
            debugLog.Clear();
        }

        public ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary[] GetDebugLog()
        {
            return debugLog.ToArray();
        }

        public void Add(ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary data)
        {
            debugLog.Add(data);
        }
    }

    public class ParameterHandler
    {
        private readonly ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary[] _defaultConfig =
        {
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x01 }, new byte[] { 0xB0, 0x00, 0x40, 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x02 }, new byte[] { 0x01 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x03 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x04 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x05 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x06 }, new byte[] { 0x01 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x07 }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x80, 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x08 }, new byte[] { 0x01 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x09 }, new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x0A }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x0B }, new byte[] { 0x01 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x0C }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x60, 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x0D }, new byte[] { 0x00, 0x00, 0x1F, 0x40 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x0E }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x0F }, new byte[] { 0x01 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x10 }, new byte[] { 0x01 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x11 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x12 }, new byte[] { 0x01 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x13 }, new byte[] { 0x01 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x14 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x15 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x16 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x17 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x18 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x19 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x1A }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x1B }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x1C }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x1D }, new byte[] { 0x10, 0x01, 0x01 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x1E }, new byte[] { 0x09, 0x00, 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x1F }, new byte[] { 0x08, 0x40 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x20 }, new byte[] { 0x08, 0x40 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x21 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x22 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x23 }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x01, 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x24 }, new byte[] { 0x0A })
        };

        private static ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary[] _currentConfig =
        {
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x01 }, new byte[] { 0xB0, 0x00, 0x40, 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x02 }, new byte[] { 0x01 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x03 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x04 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x05 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x06 }, new byte[] { 0x01 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x07 }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x80, 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x08 }, new byte[] { 0x01 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x09 }, new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x0A }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x0B }, new byte[] { 0x01 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x0C }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x60, 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x0D }, new byte[] { 0x00, 0x00, 0x1F, 0x40 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x0E }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x0F }, new byte[] { 0x01 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x10 }, new byte[] { 0x01 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x11 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x12 }, new byte[] { 0x01 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x13 }, new byte[] { 0x01 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x14 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x15 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x16 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x17 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x18 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x19 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x1A }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x1B }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x1C }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x1D }, new byte[] { 0x10, 0x01, 0x01 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x1E }, new byte[] { 0x09, 0x00, 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x1F }, new byte[] { 0x08, 0x40 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x20 }, new byte[] { 0x08, 0x40 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x21 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x22 }, new byte[] { 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x23 }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x01, 0x00 }),
            new ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary(new byte[] { 0xC0, 0x24 }, new byte[] { 0x0A })
        };

        public ParameterHandler() { }

        public void SetupConfigToDefault()
        {
            Array.Copy(_defaultConfig, _currentConfig, _currentConfig.Length);
        }

        public void SetParameter(ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary config)
        {
            
            if (config.Key[0] != 0xC0 ||
                config.Key[1] < 0x01 ||
                config.Key[1] > 0x24)
            {
                throw new ArgumentOutOfRangeException();
            }

            try
            {
                var idx = config.Key[1] - 1;
                Array.Copy(config.Value, _currentConfig[idx].Value, config.Value.Length);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ArrayOfKeyValueOfbase64Binarybase64BinaryKeyValueOfbase64Binarybase64Binary[] GetCurrentParameters()
        {
            return _currentConfig;
        }

        public int GetTimeoutPeriod()
        {
            return _currentConfig[0x23].Value[0];
        }

    }


    public class ConfigData
    {
        
        private string tag;
        private string format;
        private string value;

        public ConfigData() { }

        public ConfigData(string tag, string format, string value)
        {
            this.tag = tag;
            this.format = format;
            this.value = value;
        }

        public string GetDataStream()
        {
            if (String.IsNullOrEmpty(tag) || String.IsNullOrEmpty(format) || String.IsNullOrEmpty(value))
                return null;
            return tag + Convert.ToChar(0x1C).ToString() + format + Convert.ToChar(0x1C).ToString() + value;
        }

        public string Tag
        {
            get
            {
                return this.tag;
            }
            set
            {
                this.tag = value;
            }
        }

        public string Format
        {
            get
            {
                return this.format;
            }
            set
            {
                this.format = value;
            }
        }

        public string Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }

    }

    public class TagInfo
    {
        private Resident _resident;
        private int _index;

        public TagInfo(Resident res, int index)
        {
            this._resident = res;
            this._index = index;
        }

        public TagInfo()
        {
            _resident = Resident.Na;
            _index = -1;
        }

        public Resident Resident
        {
            get
            {
                return this._resident;
            }
            set
            {
                this._resident = value;
            }
        }

        public int Index
        {
            get
            {
                return this._index;
            }
            set
            {
                this._index = value;
            }
        }
    }

    public class DataHandler
    {
        private Dictionary<string, byte[]> tagConvertTable = new Dictionary<string, byte[]>();

        private void InitTagConvertTable()
        {
            // According to VCAS specification Table 3-12 Default Terminal Configuration
            tagConvertTable.Add("", new byte[] { 0xC0, 0x01});
        }

        public static readonly Dictionary<string, string> tagTableVcasToUIC = new Dictionary<string, string>()
        {
            { "C001", "9F66" },
            { "C003", "FFFF8007" },
            { "C004", "FFFF8217" },
            { "C005", "FFFF800C" },
            { "C006", "FFFF800A" },
            { "C007", "DF8123" },
            { "C008", "FFFF8004" },
            { "C009", "DF8124" },
            { "C00A", "FFFF8005" },
            { "C00B", "FFFF8009" },
            { "C00C", "DF8126" },
            { "C00D", "9F1B" },
            { "C011", "FFFF800F" },
            { "C019", "FFFF8006" },
            { "C01F", "9F1A" }
        };

    }

    public class TxnResult
    {
        public const string OnlineApprove = "Y4";
        public const string OfflineApprove = "Y1";
        public const string OfflineDecline = "Z1";
        public const string OnlineDecline = "Z4";
        public const string OfflineApproveSign = "Y7";
        public const string TryAnotherInterface = "B0";
        public const string OnlineAuthorizeReq = "A1";
        public const string ExternalPinBlockReq = "A5";
    }
}

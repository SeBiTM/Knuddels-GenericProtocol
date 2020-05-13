using System;
using System.Text;
using  Knuddels.Network.Generic.IStream;

namespace  Knuddels.Network.Generic.DataStream
{
    internal class DataOutput : IStreamOutput
    {
        #region " FELDER "

        private readonly StringBuilder _StringBuilder;

        #endregion

        #region " EIGENSCHAFTEN "

        public string Data
        {
            get { return _StringBuilder.ToString(); }
        }

        #endregion

        #region " KONSTRUKTOR "

        public DataOutput() : this(new StringBuilder())
        {
        }

        public DataOutput(string pString) : this(new StringBuilder(pString))
        {
        }

        public DataOutput(StringBuilder pStringBuilder)
        {
            _StringBuilder = pStringBuilder;
        }

        #endregion

        #region " WRITE "

        #region " WRITE | INT | BYTEArray | BYTEArrayExtended "
        
        public void Write(int pInteger)
        {
            _StringBuilder.Append(pInteger & 0xff);
        }

        public void Write(byte[] pByteArray)
        {
            Write(pByteArray, 0, pByteArray.Length);
        }

        public void Write(byte[] pByteArray, int pStartPosition, int pLength)
        {
            for (int i = 0; i < pLength; i++)
            {
                Write(pByteArray[i + pStartPosition]);
            }
        }

        #endregion

        #region " WRITEBYTE "
        
        public void WriteByte(int pByte)
        {
            _StringBuilder.Append((char) (pByte & 0xff));
        }

        public void WriteBytes(string pString)
        {
            // http://download.oracle.com/javase/1.4.2/docs/api/java/lang/String.html#getBytes%28%29
            // Die Standard-Kodierung vom Computer wird benutzt. 
            Write(Encoding.Default.GetBytes(pString));
        }

        #endregion

        #region " WRITEBOOL "
        
        public void WriteBoolean(bool pBool)
        {
            Write(pBool ? 1 : 0);
        }

        #endregion

        #region " WRITECHAR "

        public void WriteChar(int pChar)
        {
            _StringBuilder.Append((char) pChar);
        }

        #endregion

        #region " WRITECHARS "

        public void WriteChars(string pString)
        {
            _StringBuilder.Append(pString);
        }

        #endregion

        #region " WRITE | DOUBLE | FLOAT "
        
        public void WriteDouble(double pDouble)
        {
            WriteLong(BitConverter.DoubleToInt64Bits(pDouble));
        }

        public void WriteFloat(float pFloat)
        {
            WriteInt(BitConverter.ToInt32(BitConverter.GetBytes(pFloat), 0));
        }

        #endregion

        #region " WRITEINT "
       
        public void WriteInt(int pInt)
        {
            WriteByte(pInt >> 24);
            WriteByte(pInt >> 16);
            WriteByte(pInt >> 8);
            WriteByte(pInt);
        }

        #endregion

        #region " WRITELONG "

        public void WriteLong(long pLong)
        {
            WriteByte((int) (pLong >> 56));
            WriteByte((int) (pLong >> 48));
            WriteByte((int) (pLong >> 40));
            WriteByte((int) (pLong >> 32));
            WriteByte((int) (pLong >> 24));
            WriteByte((int) (pLong >> 16));
            WriteByte((int) (pLong >> 8));
            WriteByte((int) pLong);
        }

        #endregion

        #region " WRITESHORT "

        public void WriteShort(short pShort)
        {
            WriteByte(pShort >> 8);
            WriteByte(pShort);
        }

        #endregion

        #region " WRITEUTF "

        public void WriteUTF(string pString)
        {
            if (pString == null)
                pString = String.Empty;
            // -------------------------
            int strlen = pString.Length;
            int utflen = 0;
            int c;
            // -------------------------
            for (int i = 0; i < strlen; i++)
            {
                c = pString[i];
                if ((c >= 0x0001) && (c <= 0x007F))
                {
                    utflen++;
                }
                else if (c > 0x07FF)
                {
                    utflen += 3;
                }
                else
                {
                    utflen += 2;
                }
            }
            // -------------------------
            if (utflen > 65535)
                throw new Exception("encoded string too long: " + utflen + " bytes");
            WriteShort((short) utflen);
            // -------------------------
            int i1 = 0;
            for (; i1 < strlen; i1++)
            {
                c = pString[i1];
                if (!((c >= 0x0001) && (c <= 0x007F)))
                    break;
                WriteByte((byte) c);
            }
            // -------------------------
            for (; i1 < strlen; i1++)
            {
                c = pString[i1];
                if ((c >= 0x0001) && (c <= 0x007F))
                {
                    WriteByte((byte) c);
                }
                else if (c > 0x07FF)
                {
                    WriteByte((byte) (0xe0 | ((c >> 12) & 0xf)));
                    WriteByte((byte) (0x80 | ((c >> 6) & 0x3f)));
                    WriteByte((byte) (0x80 | c & 0x3f));
                }
                else
                {
                    WriteByte((byte) (0xc0 | ((c >> 6) & 0x1f)));
                    WriteByte((byte) (0x80 | ((c >> 0) & 0x3f)));
                }
            }
            // -------------------------
        }

        #endregion

        #endregion
    }
}
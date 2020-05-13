using System;
using System.IO;
using  Knuddels.Network.Generic.IStream;
using  Network.Opcodes;

namespace  Knuddels.Network.Generic.DataStream
{
    internal class DataOutputStream : MemoryStream, IStreamOutput
    {
        public DataOutputStream() { }
        public DataOutputStream(SendOpcode op)
        {
            this.WriteShort((short)op);
        }
        #region IStreamOutput Members

        #region " WRITE "

        public void Write(int pInteger)
        {
            WriteByte(pInteger);
        }

        public void Write(byte[] pByteArray)
        {
            base.Write(pByteArray, 0, pByteArray.Length);
        }

        #endregion

        #region " WRITEBYTE "

        public void WriteByte(int pByte)
        {
            base.WriteByte((byte) pByte);
        }

        #endregion

        #region " WRITEBYTES "

        public void WriteBytes(string pString)
        {
            int len = pString.Length;
            for (int i = 0; i < len; i++)
            {
                WriteByte((byte) pString[i]);
            }
        }

        #endregion

        #region " WRITEBOOLEAN "

        public void WriteBoolean(bool pBool)
        {
            WriteByte(pBool ? 1 : 0);
        }

        #endregion

        #region " WRITE | CHAR | CHARS "

        public void WriteChar(int pChar)
        {
            WriteByte((pChar >> 8) & 0xFF);
            WriteByte((pChar >> 0) & 0xFF);
        }

        public void WriteChars(string pString)
        {
            int len = pString.Length;
            for (int i = 0; i < len; i++)
            {
                int v = pString[i];
                WriteByte((v >> 8) & 0xFF);
                WriteByte((v >> 0) & 0xFF);
            }
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
            WriteByte((pInt >> 24) & 0xFF);
            WriteByte((pInt >> 16) & 0xFF);
            WriteByte((pInt >> 8) & 0xFF);
            WriteByte((pInt >> 0) & 0xFF);
        }

        #endregion

        #region " WRITELONG "

        public void WriteLong(long pLong)
        {
            var writeBuffer = new byte[8];
            writeBuffer[0] = (byte) (pLong >> 56);
            writeBuffer[1] = (byte) (pLong >> 48);
            writeBuffer[2] = (byte) (pLong >> 40);
            writeBuffer[3] = (byte) (pLong >> 32);
            writeBuffer[4] = (byte) (pLong >> 24);
            writeBuffer[5] = (byte) (pLong >> 16);
            writeBuffer[6] = (byte) (pLong >> 8);
            writeBuffer[7] = (byte) (pLong >> 0);
            Write(writeBuffer, 0, 8);
        }

        #endregion

        #region " WRITESHORT "

        public void WriteShort(short pShort)
        {
            WriteByte((pShort >> 8) & 0xFF);
            WriteByte((pShort >> 0) & 0xFF);
        } 

        #endregion

        #region WriteUTF

        public void WriteUTF(string pString)
        {
            if (pString == null)
                pString = string.Empty;
            // ------------------------------
            int strLength = pString.Length;
            int utfLength = 0;
            int c, count = 0;
            // ------------------------------
            for (int i = 0; i < strLength; i++)
            {
                c = pString[i];
                if ((c >= 0x0001) && (c <= 0x007F))
                {
                    utfLength++;
                }
                else if (c > 0x07FF)
                {
                    utfLength += 3;
                }
                else
                {
                    utfLength += 2;
                }
            }
            // ------------------------------
            if (utfLength > 65535)
                throw new Exception("encoded string too long: " + utfLength + " bytes");
            // ------------------------------
            var bytearr = new byte[utfLength + 2];
            bytearr[count++] = (byte)((utfLength >> 8) & 0xFF); // >>>
            bytearr[count++] = (byte)((utfLength >> 0) & 0xFF); // >>>  
            // ------------------------------
            int i1 = 0;
            for (; i1 < strLength; i1++)
            {
                c = pString[i1];
                if (!((c >= 0x0001) && (c <= 0x007F)))
                    break;
                bytearr[count++] = (byte)c;
            }
            // ------------------------------
            for (; i1 < strLength; i1++)
            {
                c = pString[i1];
                if ((c >= 0x0001) && (c <= 0x007F))
                {
                    bytearr[count++] = (byte)c;
                }
                else if (c > 0x07FF)
                {
                    bytearr[count++] = (byte)(0xE0 | ((c >> 12) & 0x0F));
                    bytearr[count++] = (byte)(0x80 | ((c >> 6) & 0x3F));
                    bytearr[count++] = (byte)(0x80 | ((c >> 0) & 0x3F));
                }
                else
                {
                    bytearr[count++] = (byte)(0xC0 | ((c >> 6) & 0x1F));
                    bytearr[count++] = (byte)(0x80 | ((c >> 0) & 0x3F));
                }
            }
            // ------------------------------
            Write(bytearr, 0, utfLength + 2);
        }
        
        #endregion


        public void WriteArray(byte[] decKey)
        {
            WriteShort((short)decKey.Length);
            Write(decKey);
        }
        #endregion
    }
}
namespace  Knuddels.Network.Generic.IStream
{
    public interface IStreamOutput
    {
        void Write(int pInteger);
        void Write(byte[] pByteArray);
        void Write(byte[] pByteArray, int pStartPosition, int pLength);
        void WriteByte(int pByte);
        void WriteBytes(string pString);
        void WriteBoolean(bool pBool);
        void WriteChar(int pChar);
        void WriteChars(string pString);
        void WriteDouble(double pDouble);
        void WriteFloat(float pFloat);
        void WriteInt(int pInt);
        void WriteLong(long pLong);
        void WriteShort(short pShort);
        void WriteUTF(string pString);
    }
}
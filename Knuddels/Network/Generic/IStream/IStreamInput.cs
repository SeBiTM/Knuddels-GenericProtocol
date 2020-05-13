namespace  Knuddels.Network.Generic.IStream
{
    public interface IStreamInput
    {
        int Read();
        byte ReadByte();
        int ReadUnsignedByte();
        bool ReadBoolean();
        char ReadChar();
        double ReadDouble();
        float ReadFloat();
        void ReadFully(byte[] pByteArray, int pStartPosition, int pLength);
        int ReadInt();
        long ReadLong();
        short ReadShort();
        string ReadUTF();
        int ReadUnsignedShort();
    }
}
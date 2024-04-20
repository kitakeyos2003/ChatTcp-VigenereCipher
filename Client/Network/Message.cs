using System.Text;

public class Message
{

    public byte command;
    private MemoryStream writer;
    private MemoryStream reader;

    public Message(byte command)
    {
        this.command = command;
        this.writer = new MemoryStream();
    }

    public Message(byte command, byte[] data)
    {
        this.command = command;
        reader = new MemoryStream(data);

    }

    public byte GetCommand()
    {
        return command;
    }

    public void SetCommand(int cmd)
    {
        SetCommand((byte)cmd);
    }

    public void SetCommand(byte cmd)
    {
        command = cmd;
    }

    public byte[] GetData()
    {
        return writer.ToArray();
    }
    public byte ReadByte()
    {
        return (byte)reader.ReadByte();
    }

    public short ReadShort()
    {
        byte[] data = new byte[2];
        data[0] = ReadByte();
        data[1] = ReadByte();
        return BitConverter.ToInt16(data, 0);
    }

    public int ReadInt()
    {
        byte[] data = new byte[4];
        data[0] = ReadByte();
        data[1] = ReadByte();
        data[2] = ReadByte();
        data[3] = ReadByte();
        return BitConverter.ToInt32(data, 0);
    }

    public long ReadLong()
    {
        byte[] data = new byte[8];
        data[0] = ReadByte();
        data[1] = ReadByte();
        data[2] = ReadByte();
        data[3] = ReadByte();
        data[4] = ReadByte();
        data[5] = ReadByte();
        data[6] = ReadByte();
        data[7] = ReadByte();
        return BitConverter.ToInt64(data, 0);
    }

    public void Read(ref byte[] data)
    {
        reader.Read(data, 0, data.Length);
    }

    public string ReadUTF()
    {
        short num = ReadShort();
        byte[] array = new byte[num];
        Read(ref array);
        return Encoding.UTF8.GetString(array);
    }

    public void WriteByte(byte value)
    {
        writer.WriteByte(value);
    }

    public void WriteShort(short value)
    {
        byte[] data = BitConverter.GetBytes(value);
        Write(data, 0, 2);
    }

    public void WriteInt(int value)
    {
        byte[] data = BitConverter.GetBytes(value);
        Write(data, 0, 4);
    }

    public void WriteLong(long value)
    {
        byte[] data = BitConverter.GetBytes(value);
        Write(data, 0, 8);
    }

    public void Write(byte[] data, int offset, int length)
    {
        writer.Write(data, offset, length);
    }

    public void WriteUTF(string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        short num = (short)bytes.Length;
        WriteShort(num);
        Write(bytes, 0, num);
    }
    public void Flush()
    {
        writer.Flush();
    }

    public void CleanUp()
    {
        try
        {
            if (reader != null)
            {
                reader.Close();
            }
            if (writer != null)
            {
                writer.Close();
            }
        }
        catch (IOException e)
        {
        }
    }

}

using Client;
using System.Net.Sockets;
using System.Text;

public class Session
{
    private TcpClient client;
    private NetworkStream stream;
    private BinaryReader reader;
    private BinaryWriter writer;
    private bool connected;
    private bool closed;
    public string SessionName { get; set; }
    private ChatForm ChatForm;

    public Session(ChatForm chatForm)
    {
        this.ChatForm = chatForm;
    }

    public void Connect(string host, int port)
    {
        client = new TcpClient();
        client.Connect(host, port);
        stream = client.GetStream();
        reader = new BinaryReader(stream, new UTF8Encoding());
        writer = new BinaryWriter(stream, new UTF8Encoding());
        connected = true;
        Thread thread = new Thread(() =>
        {
            while (connected && !closed)
            {
                Message ms = ReceiveMessage();
                if (ms != null)
                {
                    OnMessage(ms);
                } else
                {
                    break;
                }
            }
            Close();
        });
        thread.Start();

    }

    public void SetName(string name)
    {
        SessionName = name;
        Message ms = new Message(0);
        ms.WriteUTF(name);
        ms.Flush();
        SendMessage(ms);
        ms.CleanUp();
    }

    public void OnMessage(Message ms)
    {
        switch (ms.command)
        {
            case 1:
                string name = ms.ReadUTF();
                int length = ms.ReadInt();
                byte[] data = new byte[length];
                ms.Read(ref data);
                ChatForm.ReceiveMessage(name, data);
                break;
        }
    }

    public void Disconnect()
    {
        this.closed = true;
    }

    public void Close()
    {
        if (client != null)
        {
            ChatForm.SetStatusConnection(false);
            client.Close();
            client = null;
        }
    }

    public void SendMessage(Message message)
    {
        byte[] data = message.GetData();
        byte[] byteArray = BitConverter.GetBytes(data.Length);
        writer.Write(message.command);
        writer.Write(byteArray);
        writer.Write(data);
        writer.Flush();
    }

    public Message ReceiveMessage()
    {
        try
        {
            byte command = reader.ReadByte();
            byte[] byteArray = new byte[4];
            reader.Read(byteArray);
            int length = BitConverter.ToInt32(byteArray, 0);
            byte[] data = new byte[length];
            int len = 0;
            int byteRead = 0;
            while (len != -1 && byteRead < length)
            {
                len = reader.Read(data, byteRead, length - byteRead);
                if (len > 0)
                {
                    byteRead += len;
                }
            }
            return new Message(command, data);
        } catch
        {
        }
        return null;
    }
}

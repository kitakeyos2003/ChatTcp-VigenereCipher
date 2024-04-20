using System.Net.Sockets;

public class ServerManager
{
    private List<Session> sessions;

    private TcpListener listener;
    private bool running;


    public ServerManager(int port)
    {
        this.sessions = new List<Session>();
        this.listener = new TcpListener(port);
    }

    public List<Session> Sessions { get; }

    public void Start()
    {
        listener.Start();
        Thread thread = new Thread(() =>
        {
            running = true;
            while (running)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Session session = new Session(client);
                    session.Start();
                    AddSession(session);
                }
                catch (SocketException)
                {

                }
            }
        });
        thread.Start();

    }

    public void AddSession(Session session)
    {
        sessions.Add(session);
    }

    public void RemoveSession(Session session)
    {
        sessions.Remove(session);
    }

    public void SendMessage(string name, byte[] data)
    {
        Message ms = new Message(1);
        ms.WriteUTF(name);
        ms.WriteInt(data.Length);
        ms.Write(data, 0, data.Length);
        ms.Flush();
        BroadCast(ms);
        ms.CleanUp();
    }

    public void SendMessage(string name, byte[] data, Session except)
    {
        Message ms = new Message(1);
        ms.WriteUTF(name);
        ms.WriteInt(data.Length);
        ms.Write(data, 0, data.Length);
        ms.Flush();
        BroadCast(ms, except);
        ms.CleanUp();
    }

    public void BroadCast(Message ms)
    {
        foreach (Session session in sessions)
        {
            session.SendMessage(ms);
        }
    }

    public void BroadCast(Message ms, Session except)
    {
        foreach (Session session in sessions)
        {
            if (session != except)
            {
                session.SendMessage(ms);
            }
        }
    }

    public void DisconnectAll()
    {
        foreach (Session session in sessions)
        {
            session.Disconnect();
        }
    }

    public void Stop()
    {
        running = false;
        DisconnectAll();
        listener.Stop();
    }
}
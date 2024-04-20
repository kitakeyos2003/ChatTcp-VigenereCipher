using Server.Encrypt;
using System.Text;

namespace Client
{
    public partial class ChatForm : Form
    {

        private Session session;
        private VigenereCipher cipher;
        public ChatForm()
        {
            InitializeComponent();

        }

        public void ReceiveMessage(string name, byte[] message)
        {
            string decryptedMessage = Encoding.UTF8.GetString(cipher.Decrypt(message));
            AddMessage(name, decryptedMessage);
        }

        public void SendMessage(string message)
        {
            byte[] encryptedMessage = cipher.Encrypt(Encoding.UTF8.GetBytes(message));
            Message ms = new Message(1);
            ms.WriteInt(encryptedMessage.Length);
            ms.Write(encryptedMessage, 0, encryptedMessage.Length);
            ms.Flush();
            session.SendMessage(ms);
            ms.CleanUp();
        }

        public void AddMessage(string name, string text)
        {
            if (view.InvokeRequired)
            {
                view.Invoke(new Action(() =>
                {
                    view.AppendText(string.Format("{0}: {1}", name.ToUpper(), text));
                    view.AppendText(Environment.NewLine);
                }));
            } 
            else
            {
                view.AppendText(string.Format("{0}: {1}", name.ToUpper(), text));
                view.AppendText(Environment.NewLine);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string text = txtMessage.Text; 
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            text = text.Trim();
            txtMessage.Text = string.Empty;
            AddMessage("You", text);
            SendMessage(text);
        }


        private void btnConnectOrClose_Click(object sender, EventArgs e)
        {
            if (session != null)
            {
                session.Close();
                SetStatusConnection(false);
            }
            else
            {
                string host = txtHost.Text;
                string strPort = txtPort.Text;
                string name = txtName.Text;
                string key = txtKey.Text;
                int port;
                if (string.IsNullOrEmpty(strPort) || !int.TryParse(strPort, out port))
                {
                    MessageBox.Show("Port not valid!");
                    return;
                }
                if (string.IsNullOrEmpty(host))
                {
                    MessageBox.Show("Host not valid!");
                    return;
                }
                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show("Name not valid!");
                    return;
                }
                if (string.IsNullOrEmpty(key) || key.Length < 6)
                {
                    MessageBox.Show("Key not valid!");
                    return;
                }
                cipher = new VigenereCipher(Encoding.UTF8.GetBytes(key));
                session = new Session(this);
                btnConnectOrClose.Enabled = false;
                try
                {
                    session.Connect(host, port);
                } catch (Exception ex)
                {
                    btnConnectOrClose.Enabled = true;
                    MessageBox.Show(ex.Message);
                    return;
                }
                session.SetName(name);
                view.Text = string.Empty;
                SetStatusConnection(true);
            }

        }

        public void SetStatusConnection(bool status)
        {
            if (status)
            {
                txtHost.Enabled = false;
                txtPort.Enabled = false;
                txtName.Enabled = false;
                txtKey.Enabled = false;
                btnConnectOrClose.Text = "Stop";
                btnConnectOrClose.Enabled = true;
                panelMain.Enabled = true;
            }
            else
            {
                txtHost.Enabled = true;
                txtPort.Enabled = true;
                txtName.Enabled = true;
                txtKey.Enabled = true;
                panelMain.Enabled = false;
                session = null;
                btnConnectOrClose.Text = "Connect";

            }
        }

        public void Form_Closing(object sender, FormClosingEventArgs e)
        {
            if (session != null)
            {
                session.Disconnect();
            }
        }
    }
}
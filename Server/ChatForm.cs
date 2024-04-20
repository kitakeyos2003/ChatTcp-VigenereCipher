using Server.Encrypt;
using System.Text;

namespace Server
{
    public partial class ChatForm : Form
    {
        private static readonly ChatForm instance = new ChatForm();
        public static ChatForm Instance()
        {
            return instance;
        }

        private VigenereCipher cipher;
        public ServerManager Server { get; set; }
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
            Server.SendMessage("Server", encryptedMessage);
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
            AddMessage("Server", text);
            SendMessage(text);
        }

        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void btnStartOrStop_Click(object sender, EventArgs e)
        {
            if (Server == null)
            {
                int port;
                string key = txtKey.Text;
                if (!int.TryParse(txtPort.Text, out port))
                {
                    MessageBox.Show("Port is not valid!");
                    return;
                }
                if (string.IsNullOrEmpty(key) || key.Length < 6)
                {
                    MessageBox.Show("Key not valid!");
                    return;
                }
                cipher = new VigenereCipher(Encoding.UTF8.GetBytes(key));
                Server = new ServerManager(port);
                try
                {
                    Server.Start();
                    view.Text = string.Empty;
                    txtPort.Enabled = false;
                    txtKey.Enabled = false;
                    btnGenerateKey.Enabled = false;
                    btnStartOrStop.Text = "Stop";
                    panelMain.Enabled = true;
                }
                catch
                {
                    Server = null;
                    MessageBox.Show($"Failed to start server {port}");
                }
            }
            else
            {
                Server.Stop();
                txtPort.Enabled = true;
                txtKey.Enabled = true;
                btnGenerateKey.Enabled = true;
                btnStartOrStop.Text = "Start";
                panelMain.Enabled = false;
                Server = null;
            }
        }

        public void Form_Closing(object sender, FormClosingEventArgs e)
        {
            if (Server != null)
            {
                Server.Stop();
            }
        }

        private void btnGenerateKey_Click(object sender, EventArgs e)
        {
            txtKey.Text = GenerateRandomString(6);
        }

        private static string GenerateRandomString(int length)
        {
            StringBuilder sb = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                char c = (char)random.Next(32, 127);
                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}

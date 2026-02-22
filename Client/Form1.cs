using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        private Dictionary<int, string> _onlineUsers = new Dictionary<int, string>();

        public Form1()
        {
            InitializeComponent();
            var connector = ChatServerConnector.GetInstance();

            connector.OnMessageReceived += (msg) => {
                this.Invoke(new Action(() => AddMessage(msg)));
            };

            connector.OnUsersUpdated += (users) => {
                this.Invoke(new Action(() => UpdateOnlineList(users)));
            };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text;
            System.Threading.Tasks.Task.Run(() => {
                try
                {
                    ChatServerConnector.GetInstance().Connect(name);
                    this.Invoke(new Action(() => {
                        button1.Enabled = false;
                        textBox1.Enabled = false;
                    }));
                }
                catch (Exception ex)
                {
                    this.Invoke(new Action(() => MessageBox.Show(ex.Message)));
                }
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int? targetId = null;

            if (listBox2.SelectedIndex > 0)
            {
                var selectedName = listBox2.SelectedItem.ToString();
                targetId = _onlineUsers.FirstOrDefault(x => x.Value == selectedName).Key;
            }

            ChatServerConnector.GetInstance().SendMessageToServer(textBox2.Text, targetId);
            textBox2.Clear();
        }

        public void AddMessage(string message)
        {
            listBox1.Items.Add(message);
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }

        public void UpdateOnlineList(Dictionary<int, string> users)
        {
            _onlineUsers = users;
            listBox2.Items.Clear();

            listBox2.Items.Add("Усі (Груповий чат)");

            foreach (var name in users.Values)
            {
                listBox2.Items.Add(name);
            }
            if (listBox2.Items.Count > 0)
                listBox2.SelectedIndex = 0;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
      
        }
    }
}
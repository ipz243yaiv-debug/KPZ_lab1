using Client.ChatServiceReference;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ServiceModel;

namespace Client
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class ChatServerConnector : IChatServiceCallback
    {
        private static ChatServerConnector Instance = null;
        private ChatServiceClient Client;
        private int UserId;

        private ChatServerConnector()
        {
            InstanceContext context = new InstanceContext(this);
            Client = new ChatServiceClient(context);
        }

        public static ChatServerConnector GetInstance()
        {
            if (Instance == null) Instance = new ChatServerConnector();
            return Instance;
        }

        public void Connect(string username)
        {
            UserId = Client.Connect(username);
        }

        public void SendMessageToServer(string message, int? targetId)
        {
            if (Client.State == CommunicationState.Opened)
            {
                Client.SendMessage(message, UserId, targetId);
            }
        }

        public void SendMessageToClient(string message)
        {
            var form = Application.OpenForms["Form1"] as Form1;
            form?.Invoke(new Action(() => {
                form.AddMessage(message);
            }));
        }

        public void UpdateUsersList(Dictionary<int, string> users)
        {
            var form = Application.OpenForms["Form1"] as Form1;
            form?.Invoke(new Action(() => {
                form.UpdateOnlineList(users);
            }));
        }

        public void Disconnect()
        {
            if (Client.State == CommunicationState.Opened)
            {
                Client.Disconnect(UserId);
            }
        }
    }
}
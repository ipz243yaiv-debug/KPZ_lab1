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

        public event Action<string> OnMessageReceived;
        public event Action<Dictionary<int, string>> OnUsersUpdated;

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
            OnMessageReceived?.Invoke(message);
        }

        public void UpdateUsersList(Dictionary<int, string> users)
        {
            OnUsersUpdated?.Invoke(users);
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
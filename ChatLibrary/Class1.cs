using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace ChatLibrary
{
    [ServiceContract(CallbackContract = typeof(IChatServiceCallback))]
    public interface IChatService
    {
        [OperationContract]
        int Connect(string username);

        [OperationContract(IsOneWay = true)]
        void Disconnect(int id);

        [OperationContract(IsOneWay = true)]
        void SendMessage(string message, int senderId, int? targetId);
    }

    public interface IChatServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void SendMessageToClient(string message);

        [OperationContract(IsOneWay = true)]
        void UpdateUsersList(Dictionary<int, string> users);
    }

    public class ChatUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public OperationContext Context { get; set; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ChatService : IChatService
    {
        private List<ChatUser> usersList = new List<ChatUser>();
        private int nextUserId = 1;

        public int Connect(string username)
        {
            usersList.RemoveAll(u => u.Name == username);

            ChatUser user = new ChatUser()
            {
                Id = nextUserId++,
                Name = username,
                Context = OperationContext.Current
            };

            usersList.Add(user);
            NotifyUsersUpdated();
            return user.Id;
        }

        public void Disconnect(int id)
        {
            var user = usersList.FirstOrDefault(x => x.Id == id);
            if (user != null)
            {
                usersList.Remove(user);
                NotifyUsersUpdated();
            }
        }

        public void SendMessage(string message, int senderId, int? targetId)
        {
            var sender = usersList.FirstOrDefault(u => u.Id == senderId);
            if (sender == null) return;

            string formattedMsg = $"{DateTime.Now.ToShortTimeString()} {sender.Name}: {message}";

            if (targetId.HasValue)
            {
                var receiver = usersList.FirstOrDefault(u => u.Id == targetId.Value);
                if (receiver != null)
                {
                    try
                    {
                        receiver.Context.GetCallbackChannel<IChatServiceCallback>().SendMessageToClient("[Приватно] " + formattedMsg);
                        sender.Context.GetCallbackChannel<IChatServiceCallback>().SendMessageToClient("[Ви для " + receiver.Name + "] " + message);
                    }
                    catch { }
                }
            }
            else
            {
                foreach (var user in usersList.ToList())
                {
                    try
                    {
                        user.Context.GetCallbackChannel<IChatServiceCallback>().SendMessageToClient(formattedMsg);
                    }
                    catch { }
                }
            }
        }

        private void NotifyUsersUpdated()
        {
            var dict = usersList.ToDictionary(u => u.Id, u => u.Name);

            foreach (var user in usersList.ToList())
            {
                try
                {
                    user.Context.GetCallbackChannel<IChatServiceCallback>().UpdateUsersList(dict);
                }
                catch
                {
                    usersList.Remove(user);
                }
            }
        }
    }
}
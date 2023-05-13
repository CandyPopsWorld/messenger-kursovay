using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System.Linq;

namespace messenger
{
    public static class MessageManager
    {
        public class Message
        {
            public string MessageChatId;
            public string MessageUniqueId { get; set; }
            public string SenderId { get; set; }
            public string ReceiverId { get; set; }
            public string Text { get; set; }
            public DateTime TimeSent { get; set; }

            public void SendNewMessage(string chatId, Message newMessage)
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(GlobalData.connectionString))
                {
                    conn.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand("UPDATE chats_messages SET messages = array_append(messages, @message) WHERE chat_unique_id = @chatId", conn))
                    {
                        cmd.Parameters.AddWithValue("message", NpgsqlTypes.NpgsqlDbType.Jsonb, JsonConvert.SerializeObject(newMessage, Newtonsoft.Json.Formatting.None));
                        cmd.Parameters.AddWithValue("chatId", chatId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public static async Task CreateMessage(string message, string reciever_unique_id, string chatId)
        {
            Message newMessage = new Message();
            newMessage.Text = message;
            var networkTime = await Utils.GetNetworkTime();

            newMessage.TimeSent = networkTime;
            newMessage.SenderId = GlobalData.user.UniqueId;
            newMessage.ReceiverId = reciever_unique_id;
            newMessage.MessageChatId = chatId;
            newMessage.MessageUniqueId = Guid.NewGuid().ToString();


            newMessage.SendNewMessage(chatId, newMessage);
        }

        public static Message[] GetAllMessages(string chatId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(GlobalData.connectionString))
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT array_to_json(messages) FROM chats_messages WHERE chat_unique_id = @chatId", conn))
                {
                    cmd.Parameters.AddWithValue("chatId", chatId);
                    string messagesJson = (string)cmd.ExecuteScalar();
                    if (messagesJson != null)
                    {
                        Message[] messages = JsonConvert.DeserializeObject<Message[]>(messagesJson);
                        return messages;
                    }
                    else
                    {
                        return new Message[0];
                    }

                }
            }
        }

        public static void DeleteMessage(string chatId, string messageUniqueId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(GlobalData.connectionString))
            {
                conn.Open();

                // ѕолучаем текущий список сообщений
                MessageManager.Message[] messages = MessageManager.GetAllMessages(chatId);

                // »щем нужное сообщение и удал€ем его из списка
                var messageToRemove = messages.FirstOrDefault(m => m.MessageUniqueId == messageUniqueId);
                if (messageToRemove != null)
                {
                    messages = messages.Where(m => m.MessageUniqueId != messageUniqueId).ToArray();

                    // —охран€ем изменени€ в базе данных
                    using (NpgsqlCommand updateCmd = new NpgsqlCommand("UPDATE chats_messages SET messages = @messages WHERE chat_unique_id = @chatId", conn))
                    {
                        updateCmd.Parameters.AddWithValue("messages", NpgsqlTypes.NpgsqlDbType.Jsonb | NpgsqlTypes.NpgsqlDbType.Array, messages);
                        //updateCmd.Parameters.AddWithValue("chatId", chatId);
                        updateCmd.Parameters.Add("chatId", NpgsqlDbType.Text).Value = chatId;
                        updateCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        async public static void UpdateMessageText(string chatId, string messageUniqueId, string newText)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(GlobalData.connectionString))
            {
                conn.Open();

                // ѕолучаем текущий список сообщений
                MessageManager.Message[] messages = MessageManager.GetAllMessages(chatId);

                // Ќаходим нужное сообщение и обновл€ем его текст
                var messageToUpdate = messages.FirstOrDefault(m => m.MessageUniqueId == messageUniqueId);
                if (messageToUpdate != null)
                {
                    messageToUpdate.Text = newText;
                    messageToUpdate.TimeSent = await Utils.GetNetworkTime();

                    // —охран€ем изменени€ в базе данных
                    using (NpgsqlCommand updateCmd = new NpgsqlCommand("UPDATE chats_messages SET messages = @messages WHERE chat_unique_id = @chatId", conn))
                    {
                        updateCmd.Parameters.AddWithValue("messages", NpgsqlTypes.NpgsqlDbType.Jsonb | NpgsqlTypes.NpgsqlDbType.Array, messages);
                        updateCmd.Parameters.Add("chatId", NpgsqlDbType.Text).Value = chatId;
                        updateCmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
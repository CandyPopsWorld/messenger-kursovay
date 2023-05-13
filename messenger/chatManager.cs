using Npgsql;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static messenger.Form2;

namespace messenger
{
    public static class ChatManager
    {
        public class Chat
        {
            public string Chat_Unique_Id { get; set; }
            public string User1_Id { get; set; }
            public string User2_Id { get; set; }
            public List<Message> Messages { get; set; }

            public string GenerateChatId()
            {
                NpgsqlConnection connection = new NpgsqlConnection(GlobalData.connectionString);
                connection.Open();
                // Генерируем новый уникальный идентификатор
                string newUserId = Guid.NewGuid().ToString();

                // Проверяем, что пользователь с таким id еще не существует
                using (var command = new NpgsqlCommand($"SELECT COUNT(*) FROM chats WHERE chat_unique_id='{newUserId}'", connection))
                {
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    while (count > 0)
                    {
                        newUserId = Guid.NewGuid().ToString();
                        command.CommandText = $"SELECT COUNT(*) FROM users WHERE chat_unique_id='{newUserId}'";
                        count = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
                return newUserId;
            }

            public void AddChatToDatabase()
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(GlobalData.connectionString))
                {
                    connection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "INSERT INTO chats(chat_unique_id, user1_id, user2_id) VALUES (@chat_unique_id, @user1_id, @user2_id)";
                        command.Parameters.AddWithValue("chat_unique_id", this.Chat_Unique_Id);
                        command.Parameters.AddWithValue("user1_id", this.User1_Id);
                        command.Parameters.AddWithValue("user2_id", this.User2_Id);
                        command.ExecuteNonQuery();
                    }
                }

                using (NpgsqlConnection connection = new NpgsqlConnection(GlobalData.connectionString))
                {
                    connection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "INSERT INTO chats_messages(chat_unique_id) VALUES (@chat_unique_id)";
                        command.Parameters.AddWithValue("chat_unique_id", this.Chat_Unique_Id);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
        public class Chats
        {
            private List<Chat> _chats;

            public Chats()
            {
                _chats = new List<Chat>();
            }

            public void Add(Chat chat)
            {
                _chats.Add(chat);
            }

            public void Remove(Chat chat)
            {
                _chats.Remove(chat);
            }

            public Chat GetById(string id)
            {
                return _chats.FirstOrDefault(c => c.Chat_Unique_Id == id);
            }

            public IEnumerable<Chat> GetAll()
            {
                return _chats;
            }
        }
        public static void RemoveHiddenChat(string hiddenChatId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(GlobalData.connectionString))
            {
                conn.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand("UPDATE users SET hiddenChats = array_remove(hiddenChats, @chatId) WHERE unique_id = @uniqueId", conn))
                {
                    cmd.Parameters.AddWithValue("chatId", NpgsqlTypes.NpgsqlDbType.Text, hiddenChatId);
                    cmd.Parameters.AddWithValue("uniqueId", GlobalData.user.UniqueId);
                    cmd.ExecuteNonQuery();
                }
            }
            Application.Restart();
        }

        public static string GetUserIdChat(string chat_unique_id)
        {
            string user1_id = "";
            string user2_id = "";
            NpgsqlConnection connection = new NpgsqlConnection(GlobalData.connectionString);
            connection.Open();
            using (var command = new NpgsqlCommand($"SELECT user1_id, user2_id FROM chats WHERE chat_unique_id='{chat_unique_id}'", connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user1_id = reader.GetString(0);
                        user2_id = reader.GetString(1);
                    }
                }
            }
            if (user1_id == GlobalData.user.UniqueId)
            {
                return user2_id;
            }
            return user1_id; ;

        }

        public static void HideChat(string chatId, string additionalUsername)
        {
            AddHiddenChat(chatId);
            MessageBox.Show("Чат скрыт!");
            Application.Restart();
        }

        public static void AddHiddenChat(string chatId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(GlobalData.connectionString))
            {
                conn.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand("UPDATE users SET hiddenChats = array_append(hiddenChats, @chatId) WHERE unique_id = @uniqueId", conn))
                {
                    cmd.Parameters.AddWithValue("chatId", NpgsqlTypes.NpgsqlDbType.Text, chatId);
                    cmd.Parameters.AddWithValue("uniqueId", GlobalData.user.UniqueId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<string> GetHiddenChats()
        {
            List<string> chats = new List<string>();
            using (NpgsqlConnection connection = new NpgsqlConnection(GlobalData.connectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand("SELECT hiddenchats FROM users WHERE unique_id = @unique_id", connection))
                {
                    command.Parameters.AddWithValue("unique_id", GlobalData.user.UniqueId);
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                string[] chatIds = (string[])reader["hiddenchats"];
                                chats.AddRange(chatIds);
                            }
                        }
                    }
                }
            }
            return chats;
        }

        public static List<string> GetAllChatsIds()
        {
            List<string> chats = new List<string>();
            using (NpgsqlConnection connection = new NpgsqlConnection(GlobalData.connectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand("SELECT chats FROM users WHERE unique_id = @unique_id", connection))
                {
                    command.Parameters.AddWithValue("unique_id", GlobalData.user.UniqueId);
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                string[] chatIds = (string[])reader["chats"];
                                chats.AddRange(chatIds);
                            }
                        }
                    }
                }
            }
            return chats;
        }

        public static void AddChatUniqueIdToUserArray(Chat newChat, string user_id)
        {
            NpgsqlConnection conn;
            NpgsqlCommand cmd;
            conn = new NpgsqlConnection(GlobalData.connectionString);
            conn.Open();
            cmd = new NpgsqlCommand("UPDATE users SET chats = array_append(chats, @chat_id) WHERE unique_id = @user_id", conn);
            cmd.Parameters.AddWithValue("chat_id", newChat.Chat_Unique_Id);
            cmd.Parameters.AddWithValue("user_id", user_id);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteChat(string chatId, string userUniqueId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(GlobalData.connectionString))
            {
                conn.Open();
                // Удаляем строку из таблицы chats
                using (NpgsqlCommand cmd = new NpgsqlCommand("DELETE FROM chats WHERE chat_unique_id = @chatId", conn))
                {
                    cmd.Parameters.AddWithValue("chatId", chatId);
                    cmd.ExecuteNonQuery();
                }
                // Удаляем строки из таблицы chats_messages
                using (NpgsqlCommand cmd = new NpgsqlCommand("DELETE FROM chats_messages WHERE chat_unique_id = @chatId", conn))
                {
                    cmd.Parameters.AddWithValue("chatId", chatId);
                    cmd.ExecuteNonQuery();
                }
                // Получаем данные пользователя из таблицы users
                UserManager.User user = new UserManager.User();
                user.LoadFromDatabase(userUniqueId);
                // Удаляем chatId из массива chats в записи пользователя
                List<string> chatsIds = ChatManager.GetAllChatsIds();
                if (chatsIds.Contains(chatId))
                {
                    chatsIds.Remove(chatId);
                    GlobalData.globalChats.Remove(chatId);
                    GlobalData.globalMessages = null;
                    GlobalData.currentOpenChatId = "";
                    using (NpgsqlCommand cmd = new NpgsqlCommand("UPDATE users SET chats = @chats WHERE unique_id = @userUniqueId", conn))
                    {
                        cmd.Parameters.AddWithValue("chats", chatsIds.ToArray());
                        cmd.Parameters.AddWithValue("userUniqueId", userUniqueId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void DeleteChatDialog(string chatId)
        {
            DialogResult result = MessageBox.Show("Вы действительно хотите удалить чат с этим пользователем? Будьте осторожны вы не сможете его восстановить!", "Вы действительно хотите удалить чат?", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                DeleteChat(chatId, GlobalData.user.UniqueId);
                Application.Restart();
            }
            else if (result == DialogResult.Cancel)
            {
            }
        }

        public static void DownloadChatMessages(string chatId, string filePath)
        {
            // Получаем все сообщения чата
            MessageManager.Message[] messages = MessageManager.GetAllMessages(chatId);

            // Создаем поток для записи в файл
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Проходимся по всем сообщениям чата
                foreach (MessageManager.Message message in messages)
                {
                    // Загружаем данные об отправителе
                    UserManager.User sender = new UserManager.User();
                    sender.LoadFromDatabase(message.SenderId);

                    // Загружаем данные о получателе
                    UserManager.User receiver = new UserManager.User();
                    receiver.LoadFromDatabase(message.ReceiverId);

                    // Записываем в файл информацию об отправителе, сообщении и времени отправки
                    writer.WriteLine($"{sender.Username} {message.TimeSent}");
                    writer.WriteLine(message.Text);
                    writer.WriteLine();
                }
            }
        }
        public static void DownloadChooseFolder(string chatId)
        {
            var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                DownloadChatMessages(chatId, folderDialog.SelectedPath + "\\messages-" + chatId.Substring(0, 6) + ".txt");
            }
        }
        public static bool IsChatIdInArray(string whereUserId, string whatUserId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(GlobalData.connectionString))
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT array_position(chats_with_users_id, @what_user_id) FROM users WHERE unique_id = @where_user_id", conn))
                {
                    cmd.Parameters.AddWithValue("what_user_id", whatUserId);
                    cmd.Parameters.AddWithValue("where_user_id", whereUserId);
                    var result = cmd.ExecuteScalar();
                    if (Convert.IsDBNull(result))
                    {
                        return false;
                    }
                    else
                    {
                        return Convert.ToInt32(result) > 0;
                    }
                }
            }
        }
        public static string GetChatId(string user1Id, string user2Id)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(GlobalData.connectionString))
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT chat_unique_id FROM chats WHERE (user1_id = @user1_id AND user2_id = @user2_id) OR (user1_id = @user2_id AND user2_id = @user1_id)", conn))
                {
                    cmd.Parameters.AddWithValue("user1_id", user1Id);
                    cmd.Parameters.AddWithValue("user2_id", user2Id);
                    var result = cmd.ExecuteScalar();
                    if (Convert.IsDBNull(result))
                    {
                        return null;
                    }
                    else
                    {
                        return result.ToString();
                    }
                }
            }
        }
    }
}
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO;
using System.Reflection.Emit;
using System.Runtime.Remoting.Contexts;
using System.Data.Entity;
using static messenger.Form2;
using MailKit.Search;
using MailKit;
using System.Runtime.Remoting.Messaging;
using MimeKit;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Xml;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.PeerToPeer;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using NpgsqlTypes;

namespace messenger
{
    public partial class Form2 : Form
    {
        string userId = ConfigurationManager.AppSettings["UserId"];
        static string connectionString = "Server=localhost;Port=5432;Database=messenger;User Id=postgres;Password=regular123;";
        public static User user; // объявляем переменную класса User
        public static User globalAdditionalUser;
        public static Chats chats; // объявляем переменную класса Chats
        // Создаем таймер и настраиваем его
        Timer timer = new Timer();

        Timer chatTimer = new Timer();

        Timer statusTimer = new Timer();

        static Message[] globalMessages;
        static List<string> globalChats;
        //bool initChats = false;
        bool globalStatusAdditionalUser = false;
        //string[] hiddenChats;
        static List<string> hiddenChats;

        public static string currentOpenChatId = "";
        public Form2()
        {
            InitializeComponent();

            user = new User(); // создаем экземпляр класса User
            globalAdditionalUser = new User();
            chats = new Chats();
            user.LoadFromDatabase(userId); // вызываем метод LoadFromDatabase для получения данных из базы данных и заполнения полей класса

            hiddenChats = GetHiddenChats();

            DisplayUserData(user); // отображаем полученные данные на форме
            InitLoadAllChaUser();
            SetUserOnlineStatus(user.UniqueId, true);

            DisplayHiddenChats();


            if (chatTimer.Enabled)
            {
                timer.Stop();
                chatTimer.Stop();
            }
            chatTimer = new Timer();
            chatTimer.Tick += (sender, e) => LoadAllChatsUser();
            chatTimer.Interval = 3000; // 3 секунду
            chatTimer.Start();
        }

        public void DisplayHiddenChats()
        {
            if (hiddenChats.Count > 0)
            {
                foreach (string hiddenChatId in hiddenChats)
                {
                    string additionalUserId = GetUserIdChat(hiddenChatId);
                    User additionalUser = new User();
                    additionalUser.LoadFromDatabase(additionalUserId);
                    CreateHiddenChatWidget(additionalUser, hiddenChatId);

                }
            }
            else
            {
                System.Windows.Forms.Label notFoundHiddenChats = new System.Windows.Forms.Label();
                notFoundHiddenChats.Text = "У вас нет скрытых чатов!";
                notFoundHiddenChats.Font = new Font("Arial", 12, FontStyle.Bold);
                notFoundHiddenChats.Width = 450;
                notFoundHiddenChats.Location = new Point(270, 100);
                panel6.Controls.Add(notFoundHiddenChats);
            }
        }

        public void CreateHiddenChatWidget(User additionalUser, string hiddenChatId)
        {
            string username = additionalUser.Username;
            byte[] photo = additionalUser.Photo;
            string unique_id = additionalUser.UniqueId;

            Panel hiddenChatPanel = new Panel();
            hiddenChatPanel.BorderStyle = BorderStyle.FixedSingle;
            hiddenChatPanel.Width = panel6.Width - 10;
            hiddenChatPanel.Height = 50;
            hiddenChatPanel.Padding = new Padding(5);
            hiddenChatPanel.Location = new Point(0, panel9.Controls.Count * hiddenChatPanel.Height);

            PictureBox photoBox = new PictureBox();
            photoBox.Width = 40;
            photoBox.Height = 40;
            photoBox.SizeMode = PictureBoxSizeMode.StretchImage;
            using (MemoryStream memoryStream = new MemoryStream(photo))
            {
                Image image = Image.FromStream(memoryStream);
                photoBox.Image = image;
            }

            System.Windows.Forms.Button showChatBtn = new System.Windows.Forms.Button();
            showChatBtn.Text = "Показать чат";
            showChatBtn.Click += (sender, e) => RemoveHiddenChat(hiddenChatId);

            System.Windows.Forms.Label nameLabel = new System.Windows.Forms.Label();
            nameLabel.Text = username;
            nameLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            nameLabel.Width = 150;
            nameLabel.Location = new Point(50, 10);

            hiddenChatPanel.Controls.Add(photoBox);
            photoBox.Location = new Point(5, 5);

            hiddenChatPanel.Controls.Add(nameLabel);
            nameLabel.Location = new Point(photoBox.Right + 5, 5);

            hiddenChatPanel.Controls.Add(showChatBtn);
            showChatBtn.Width = 150;
            showChatBtn.Height = 30;
            showChatBtn.Location = new Point(640, 10);

            // Добавляем новый элемент интерфейса на панель результатов поиска
            panel6.Controls.Add(hiddenChatPanel);
        }

        public void RemoveHiddenChat(string hiddenChatId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand("UPDATE users SET hiddenChats = array_remove(hiddenChats, @chatId) WHERE unique_id = @uniqueId", conn))
                {
                    cmd.Parameters.AddWithValue("chatId", NpgsqlTypes.NpgsqlDbType.Text, hiddenChatId);
                    cmd.Parameters.AddWithValue("uniqueId", user.UniqueId);
                    cmd.ExecuteNonQuery();
                }
            }
            Application.Restart();
        }

        public void LoadAllChatsUser()
        {
            List<string> chatsIds = GetAllChatsIds();
            chatsIds.Reverse();

            if (chatsIds.Count == globalChats.Count)
            {
                return;
            }
            panel3.Controls.Clear();
            globalChats = chatsIds;
            foreach (string chatId in chatsIds)
            {
                bool isChatIdExist = hiddenChats.Contains(chatId);
                if (!isChatIdExist)
                {
                    //panel3.Controls.Clear();
                    string additionalUserId = GetUserIdChat(chatId);
                    User additionalUser = new User();
                    additionalUser.LoadFromDatabase(additionalUserId);
                    CreateNewChatPanel(additionalUser, chatId);
                }
            }
        }

        public void InitLoadAllChaUser()
        {
            List<string> chatsIds = GetAllChatsIds();
            chatsIds.Reverse();
            globalChats = chatsIds;

            bool equalsHidden = false;
            int countEqualsHiddenAndChats = 0;
            
            foreach (var chatId in chatsIds)
            {
                foreach (var hiddenId in hiddenChats)
                {
                    if(chatId == hiddenId)
                    {
                        countEqualsHiddenAndChats++;
                    }
                }
            }
            if(countEqualsHiddenAndChats == chatsIds.Count)
            {
                equalsHidden = true;
            }

            if (chatsIds.Count == 0 || equalsHidden)
            {
                System.Windows.Forms.Label label = new System.Windows.Forms.Label();
                if (equalsHidden)
                {
                    label.Text = "Не скрытые чаты отсутствуют!";
                    label.Width = 220;
                    label.Font = new Font("Arial", 9, FontStyle.Bold);
                }
                else
                {
                    label.Text = "У вас еще нет чатов!";
                    label.Width = 200;
                    label.Font = new Font("Arial", 12, FontStyle.Bold);
                }

                label.Location = new Point(30, 10);
                panel3.Controls.Add(label);
                return;
            }
            foreach (string chatId in chatsIds)
            {
                bool isChatIdExist = hiddenChats.Contains(chatId);
                if (!isChatIdExist)
                {
                string additionalUserId = GetUserIdChat(chatId);
                
                User additionalUser = new User();
                additionalUser.LoadFromDatabase(additionalUserId);
                CreateNewChatPanel(additionalUser, chatId);
                }
            }
        }

        public void CreateNewChatPanel(User additionalUser, string chatId)
        {
            string username = additionalUser.Username;
            int age = additionalUser.Age;
            byte[] photo = additionalUser.Photo;
            string unique_id = additionalUser.UniqueId;

           // Создаем новый элемент интерфейса для вывода информации о пользователе
            Panel chatPanel = new Panel();
            chatPanel.BorderStyle = BorderStyle.FixedSingle;
            chatPanel.Width = 227;
            chatPanel.Height = 59;
            chatPanel.Padding = new Padding(5);
            chatPanel.Location = new Point(13, panel3.Controls.Count * chatPanel.Height);
            chatPanel.BackColor = Color.Gray;

            /*System.Windows.Forms.Button hiddenButton = new System.Windows.Forms.Button();
            hiddenButton.Visible = false;
            hiddenButton.Name = "hiddenButton";
            hiddenButton.Click += (sender, e) => ClickToChatWidget(additionalUser, chatId);*/

            PictureBox photoBox = new PictureBox();
            
            if(photo != null)
            {
                photoBox.Width = 44;
                photoBox.Height = 52;
                photoBox.SizeMode = PictureBoxSizeMode.StretchImage;
                using (MemoryStream memoryStream = new MemoryStream(photo))
                {
                    Image image = Image.FromStream(memoryStream);
                    photoBox.Image = image;
                }
                chatPanel.Controls.Add(photoBox);
            }

            System.Windows.Forms.Label usernameLabel = new System.Windows.Forms.Label();
            usernameLabel.Text = username;
            usernameLabel.Width = 150;
            usernameLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            usernameLabel.Location = new Point(50, 10);

            chatPanel.Click += (sender, e) => ClickToChatWidget(additionalUser, chatId);
            usernameLabel.Click += (sender, e) => ClickToChatWidget(additionalUser, chatId);
            photoBox.Click += (sender, e) => ClickToChatWidget(additionalUser, chatId);

            chatPanel.MouseEnter += (sender, e) => panel3.Focus();
            chatPanel.MouseLeave += (sender, e) => this.ActiveControl = null;
            usernameLabel.MouseEnter += (sender, e) => panel3.Focus();
            usernameLabel.MouseLeave += (sender, e) => this.ActiveControl = null;
            photoBox.MouseEnter += (sender, e) => panel3.Focus();
            photoBox.MouseLeave += (sender, e) => this.ActiveControl = null;

            chatPanel.Controls.Add(usernameLabel);
            //chatPanel.Controls.Add(hiddenButton);
            panel3.Controls.Add(chatPanel);
        }

        public void OutChatIsEmpty(Message[] messages)
        {
            if (messages.Length == 0 || panel5.Controls.Count == 0)
            {
                System.Windows.Forms.Label notFoundMessages = new System.Windows.Forms.Label();
                notFoundMessages.Text = "ЧАТ ПУСТОЙ! ВЫ МОЖЕТЕ НАПИСАТЬ ПЕРВОЕ СООБЩЕНИЕ";
                notFoundMessages.Width = 550;
                notFoundMessages.Font = new Font("Arial", 12, FontStyle.Bold);
                notFoundMessages.Location = new Point(30, 100);
                notFoundMessages.Name = "notFoundMessages";
                notFoundMessages.ForeColor = Color.Gray;
                panel5.Controls.Add(notFoundMessages);
            }
        }

        public void ClickToChatWidget(User additionalUser, string chatId)
        {
            if(currentOpenChatId == chatId)
            {
                MessageBox.Show("Данный чат уже открыт!");
                return;
            }


            currentOpenChatId = chatId;

            panel5.Controls.Clear();

            Message[] messages = GetAllMessages(chatId);
            globalMessages = messages;

            foreach (Message message in messages)
            {
                CreateWidgetMessage(message);
            }

            OutChatIsEmpty(messages);

            //ПОВТОРЕНИЕ КОДА(НАЧАЛО)
            string username = additionalUser.Username;
            int age = additionalUser.Age;
            byte[] photo = additionalUser.Photo;
            string unique_id = additionalUser.UniqueId;

            // Создаем новый элемент интерфейса для вывода информации о пользователе
            Panel chatPanel = new Panel();
            chatPanel.BorderStyle = BorderStyle.FixedSingle;
            chatPanel.Width = 227;
            chatPanel.Height = 59;
            chatPanel.Padding = new Padding(5);
            chatPanel.Location = new Point(13, 10);
            chatPanel.BackColor = Color.Gray;

            System.Windows.Forms.Button downloadChatHistoryBtn = new System.Windows.Forms.Button();
            downloadChatHistoryBtn.Text = "Скачать историю сообщений!";
            downloadChatHistoryBtn.Click += (sender, e) => DownloadChooseFolder(chatId);
            downloadChatHistoryBtn.Width = 210;
            downloadChatHistoryBtn.Height = 30;
            downloadChatHistoryBtn.Location = new Point(380, 50);

            System.Windows.Forms.Button hideChatBtn = new System.Windows.Forms.Button();
            hideChatBtn.Text = "Скрыть чат";
            hideChatBtn.Click += (sender, e) => HideChat(chatId, additionalUser.Username);
            hideChatBtn.Width = 210;
            hideChatBtn.Height = 30;
            hideChatBtn.Location = new Point(380, 20);

            System.Windows.Forms.Button deleteChatBtn = new System.Windows.Forms.Button();
            if (unique_id == null)
            {
                deleteChatBtn.Text = "Удалить чат!";
                deleteChatBtn.Click += (sender, e) => DeleteChatDialog(chatId);
                deleteChatBtn.Width = 210;
                deleteChatBtn.Height = 30;
                deleteChatBtn.ForeColor = Color.Red;
                deleteChatBtn.Location = new Point(380, 20);
            }

            PictureBox photoBox = new PictureBox();

            System.Windows.Forms.Label usernameLabel = new System.Windows.Forms.Label();
            usernameLabel.Text = username;
            usernameLabel.Width = 150;
            usernameLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            usernameLabel.Location = new Point(50, 10);

            System.Windows.Forms.Label statusLabel = new System.Windows.Forms.Label();

            if (unique_id != null)
            {

                photoBox.Width = 44;
                photoBox.Height = 52;
                photoBox.SizeMode = PictureBoxSizeMode.StretchImage;
                using (MemoryStream memoryStream = new MemoryStream(photo))
                {
                    Image image = Image.FromStream(memoryStream);
                    photoBox.Image = image;
                }
                chatPanel.Controls.Add(photoBox);

                //статус
                if (GetUserOnlineStatus(unique_id))
                {
                    globalStatusAdditionalUser = true;
                    statusLabel.Text = "(ONLINE)";
                    statusLabel.ForeColor = Color.Green;
                }
                else
                {
                    globalStatusAdditionalUser = false;
                    statusLabel.Text = "(OFFLINE)";
                    statusLabel.ForeColor = Color.Red;
                }
                statusLabel.Width = 150;
                statusLabel.Font = new Font("Arial", 9, FontStyle.Bold);
                statusLabel.Location = new Point(50, 30);
                chatPanel.Controls.Add(statusLabel);

                if (statusTimer.Enabled)
                {
                    statusTimer.Stop();
                }
                statusTimer = new Timer();
                statusTimer.Tick += (sender, e) => status_Tick(sender, e, unique_id, statusLabel);
                statusTimer.Interval = 1000; // 1 секунду
                statusTimer.Start();
            }

            chatPanel.Controls.Add(usernameLabel);
            

            //ПОВТОРЕНИЕ КОДА(КОНЕЦ)

            //ДОБАВЛЕНИЕ TEXTBOX И КНОПКИ ДЛЯ ПЕЧАТИ И ОТПРАВКИ СООБЩЕНИЙ
            panel11.Controls.Clear();

            
            
            System.Windows.Forms.TextBox messageTextBox = new System.Windows.Forms.TextBox();
            System.Windows.Forms.Label countSymbol = new System.Windows.Forms.Label();
            System.Windows.Forms.Button write_user = new System.Windows.Forms.Button();
            System.Windows.Forms.Label userDeleteLabel = new System.Windows.Forms.Label();
            if (unique_id != null)
            {
                messageTextBox.Multiline = true;
                messageTextBox.MaxLength = 4096;
                messageTextBox.Width = 455;
                messageTextBox.Height = 74;
                messageTextBox.Location = new Point(10, 0);
                messageTextBox.ScrollBars = ScrollBars.Both;

                countSymbol.Text = "0/4096";
                countSymbol.Width = 300;
                countSymbol.Location = new Point(423, 80);

                messageTextBox.TextChanged += (sender, e) => countSymbol.Text = messageTextBox.Text.Length + "/4096";

                write_user.Text = "Отправить";
                write_user.Click += (sender, e) => SendMessageToUser(messageTextBox, unique_id, chatId);
                write_user.Width = 142;
                write_user.Height = 54;
                write_user.Location = new Point(480, 0);

                panel11.Controls.Add(messageTextBox);
                panel11.Controls.Add(write_user);
                panel11.Controls.Add(countSymbol);
            } else
            {
                userDeleteLabel.Text = "(Пользователь удалил свой аккаунт! Вы больше не можете ему написать!)";
                userDeleteLabel.Width = 700;
                userDeleteLabel.Font = new Font("Arial", 11, FontStyle.Bold);
                userDeleteLabel.ForeColor = Color.Red;
                userDeleteLabel.Location = new Point(40, 40);

                panel11.Controls.Add(userDeleteLabel);
            }



            //
            panel2.Controls.Clear();
            panel2.Controls.Add(chatPanel);
            panel2.Controls.Add(downloadChatHistoryBtn);
            panel2.Controls.Add(hideChatBtn);

            if(unique_id == null)
            {
                panel2.Controls.Add(deleteChatBtn);
            }


            //ListenForNewMessages(chatId);
            if (timer.Enabled) {
                timer.Stop();
            }
            timer = new Timer();
            timer.Tick += (sender, e) => timer_Tick(sender, e, chatId);
            timer.Interval = 1000; // 1 секунду
            timer.Start();
        }

        public void HideChat(string chatId, string additionalUsername)
        {
            AddHiddenChat(chatId);
            MessageBox.Show("Чат скрыт!");
            Application.Restart();
        }

        /*public static void AddHiddenChat(string chatId, string additionalUsername)
        {
            var appSettings = ConfigurationManager.AppSettings;
            string hiddenChats = appSettings["hiddenChats"];

            if (string.IsNullOrEmpty(hiddenChats))
            {
                // Если строка пустая, то создаем новую строку с chatId
                appSettings["hiddenChats"] = chatId;
            }
            else
            {
                // Иначе добавляем chatId к существующей строке через запятую
                appSettings["hiddenChats"] = $"{hiddenChats},{chatId}";
            }

            // Сохраняем изменения в конфигурационном файле
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["hiddenChats"].Value = appSettings["hiddenChats"];
            config.Save(ConfigurationSaveMode.Modified);
        }*/

        public static void AddHiddenChat(string chatId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand("UPDATE users SET hiddenChats = array_append(hiddenChats, @chatId) WHERE unique_id = @uniqueId", conn))
                {
                    cmd.Parameters.AddWithValue("chatId", NpgsqlTypes.NpgsqlDbType.Text, chatId);
                    cmd.Parameters.AddWithValue("uniqueId", user.UniqueId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<string> GetHiddenChats()
        {
            List<string> chats = new List<string>();
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand("SELECT hiddenchats FROM users WHERE unique_id = @unique_id", connection))
                {
                    command.Parameters.AddWithValue("unique_id", user.UniqueId);
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

        private void status_Tick(object sender, EventArgs e, string additionalUniqueId, System.Windows.Forms.Label statusLabel)
        {
            bool status = GetUserOnlineStatus(additionalUniqueId);

            if(status == globalStatusAdditionalUser)
            {
                return;
            }
            globalStatusAdditionalUser = status;

            if (status)
            {
                statusLabel.Text = "(ONLINE)";
                statusLabel.ForeColor = Color.Green;
            } else
            {
                statusLabel.Text = "(OFFLINE)";
                statusLabel.ForeColor = Color.Red;
            }
        }


        private void timer_Tick(object sender, EventArgs e, string chatId)
        {
            Message[] messages = GetAllMessages(chatId);
            
            int lastMessageIndex = messages.Length - 1;

            bool equal = true;

            if(messages.Length > 0 && panel5.Controls.Find("notFoundMessages", true).Length > 0){
                panel5.Controls.Clear();
            }
           
            if(messages.Length == globalMessages.Length) { 
                if(messages.Length == 0)
                {
                    OutChatIsEmpty(messages);
                }
                equal = true;
            } else
            {
                equal = false;
            }


            if (equal)
            {
                bool equalText = true;
                for(int i = 0; i < messages.Length; i++)
                {
                    if (messages[i].Text == globalMessages[i].Text)
                    {

                    } else
                    {
                        equalText = false;
                    }
                }

                if(equalText == false)
                {
                    globalMessages = messages;
                    panel5.Controls.Clear();
                    foreach (Message message in messages)
                    {
                        CreateWidgetMessage(message);
                    }
                }

            } else {
                if(messages.Length > globalMessages.Length)
                {
                    globalMessages = messages;
                    //panel5.Controls.Clear();
                    foreach (Message message in messages)
                    {
                        if (Array.IndexOf(messages, message) == lastMessageIndex)
                        {
                            // Это последняя итерация
                            CreateWidgetMessage(message);
                        }

                    }
                } else
                {
                    globalMessages = messages;
                    panel5.Controls.Clear();
                    foreach (Message message in messages)
                    {
                        CreateWidgetMessage(message);
                    }
                }

            }
        }
        public void SetUserOnlineStatus(string userUniqueId, bool isOnline)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand("UPDATE users_status SET isOnline = @isOnline WHERE user_unique_id = @userUniqueId", conn))
                {
                    cmd.Parameters.AddWithValue("isOnline", isOnline);
                    cmd.Parameters.AddWithValue("userUniqueId", userUniqueId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public bool GetUserOnlineStatus(string userUniqueId)
        {
            bool isOnline = false;

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT isOnline FROM users_status WHERE user_unique_id = @userUniqueId", conn))
                {
                    cmd.Parameters.AddWithValue("userUniqueId", userUniqueId);

                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        isOnline = (bool)result;
                    }
                }
            }

            return isOnline;
        }



        public static async Task<DateTime> GetNetworkTime()
        {
            // Настройка NTP-сервера
            string ntpServer = "pool.ntp.org";
            int ntpPort = 123;

            // Создание сокета
            using (var ntpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                // Установка таймаута в 5 секунд
                ntpSocket.ReceiveTimeout = 5000;

                // Получение адреса NTP-сервера
                var addresses = await Dns.GetHostAddressesAsync(ntpServer);

                // Отправка запроса на NTP-сервер
                var ntpData = new byte[48];
                ntpData[0] = 0x1B;
                var time = await Task.Run(() =>
                {
                    ntpSocket.SendTo(ntpData, new IPEndPoint(addresses[0], ntpPort));
                    ntpSocket.Receive(ntpData);
                    var intpart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | ntpData[43];
                    var fractpart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | ntpData[47];
                    var milliseconds = (intpart * 1000) + ((fractpart * 1000) / 0x100000000L);
                    return milliseconds;
                });

                // Конвертация времени из NTP в DateTime
                var dateTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(time);
                return dateTime.ToLocalTime();
            }
        }

        async public void SendMessageToUser(System.Windows.Forms.TextBox messageTextBox, string reciever_unique_id, string chatId)
        {
            if(panel5.Controls.Find("notFoundMessages", true).Length > 0)
            {
                panel5.Controls.Clear();
            }
            string message = messageTextBox.Text;
            if(message.Length > 0)
            {
                await CreateMessage(message, reciever_unique_id, chatId);
                messageTextBox.Text = "";
            }
        }

        private static async Task CreateMessage(string message, string reciever_unique_id, string chatId)
        {
            Message newMessage = new Message();
            newMessage.Text = message;
            var networkTime = await GetNetworkTime();

            newMessage.TimeSent = networkTime;
            //newMessage.TimeSent = DateTime.Now;
            newMessage.SenderId = user.UniqueId;
            newMessage.ReceiverId = reciever_unique_id;
            newMessage.MessageChatId = chatId;
            newMessage.MessageUniqueId = Guid.NewGuid().ToString();


            newMessage.SendNewMessage(chatId, newMessage);
            //CreateWidgetMessage(newMessage);
            //MessageBox.Show("Сообщение отправлено!");
        }

        public static Message[] GetAllMessages(string chatId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT array_to_json(messages) FROM chats_messages WHERE chat_unique_id = @chatId", conn))
                {
                    cmd.Parameters.AddWithValue("chatId", chatId);
                    string messagesJson = (string)cmd.ExecuteScalar();
                    if(messagesJson != null)
                    {
                        Message[] messages = JsonConvert.DeserializeObject<Message[]>(messagesJson);
                        return messages;
                    } else
                    {
                        return new Message[0];
                    }

                }
            }
        }

        public void CreateWidgetMessage(Message newMessage)
        {
            string messageText = newMessage.Text;
            string messageSendDate = newMessage.TimeSent.ToString();
            string senderId = newMessage.SenderId;
            string recieverId = newMessage.ReceiverId;

            bool isSenderMainUser = false;

            if (senderId == user.UniqueId)
            {
                isSenderMainUser = true;
            } else
            {
                isSenderMainUser = false;
            }

            // Создаем новую панель для сообщения
            TableLayoutPanel messagePanel = new TableLayoutPanel();
            messagePanel.AutoSize = true;
            messagePanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            messagePanel.Margin = new Padding(0, 5, 0, 5);
            messagePanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
            if (isSenderMainUser)
            {
                messagePanel.BackColor = Color.LightSeaGreen;
                //КОНТЕКСТНОЕ МЕНЮ ДЛЯ messagePanel
                // Создание контекстного меню

                // Создание элементов меню
                ToolStripMenuItem deleteMessageItem = new ToolStripMenuItem("Удалить сообщение");
                deleteMessageItem.Click += (sender, e) => DeleteMessageMenuClick(newMessage.MessageUniqueId);

                ToolStripMenuItem changeMessageItem = new ToolStripMenuItem("Изменить сообщение");
                changeMessageItem.Click += (sender, e) => ChangeMessageMenuClick(newMessage);

                // Добавление элементов в меню
                contextMenuStrip.Items.Add(deleteMessageItem);
                contextMenuStrip.Items.Add(changeMessageItem);

            } else
            {
                messagePanel.BackColor = Color.Gray;
            }

            ToolStripMenuItem copyMessageItem = new ToolStripMenuItem("Копировать текст сообщения");
            copyMessageItem.Click += (sender, e) => CopyMessageMenuClick(newMessage.Text);
            contextMenuStrip.Items.Add(copyMessageItem);
            messagePanel.ContextMenuStrip = contextMenuStrip;

            // Добавляем ячейку для текста сообщения
            System.Windows.Forms.Label messageLabel = new System.Windows.Forms.Label();
            messageLabel.Text = messageText;
            messageLabel.AutoSize = true;
            messageLabel.Margin = new Padding(5);
            messageLabel.MaximumSize = new Size(300, 0);
            messagePanel.Controls.Add(messageLabel, 0, 0);
            if (isSenderMainUser)
            {
                messageLabel.BackColor = Color.LightSeaGreen;
            }
            else
            {
                messageLabel.BackColor = Color.Gray;
            }


            // Добавляем ячейку для даты отправки сообщения
            System.Windows.Forms.Label timeSentLabel = new System.Windows.Forms.Label();
            timeSentLabel.Text = messageSendDate.ToString();
            timeSentLabel.AutoSize = true;
            timeSentLabel.Margin = new Padding(5);
            timeSentLabel.Anchor = AnchorStyles.Bottom;

            if (isSenderMainUser)
            {
                timeSentLabel.BackColor = Color.LightSeaGreen;
            }
            else
            {
                timeSentLabel.BackColor = Color.Gray;
            }
            messagePanel.Controls.Add(timeSentLabel, 1, 0);

            // Рассчитываем позицию панели сообщения на основе высоты предыдущей панели и отступов
            int messagePanelY = 0;
            if (panel5.Controls.Count > 0)
            {
                TableLayoutPanel lastMessagePanel = panel5.Controls[panel5.Controls.Count - 1] as TableLayoutPanel;
                messagePanelY = lastMessagePanel.Location.Y + lastMessagePanel.Height + 10;
            }

            // Устанавливаем позицию панели сообщения и добавляем ее на panel4
            messagePanel.Location = new Point(0, messagePanelY);
            panel5.Controls.Add(messagePanel);
            panel5.AutoScrollPosition = new Point(0, panel5.VerticalScroll.Maximum);
        }

        public void DeleteMessageMenuClick(string MessageUniqueId)
        {
            DeleteMessage(currentOpenChatId, MessageUniqueId);
            panel5.Controls.Clear();

            Message[] messages = GetAllMessages(currentOpenChatId);
            globalMessages = messages;
            panel5.Controls.Clear();
            foreach (Message message1 in messages)
            {
                CreateWidgetMessage(message1);
            }
        }
        public void ChangeMessageMenuClick(Message message)
        {
            EditMessageMenuClick(message);
        }

        public void CopyMessageMenuClick(string message)
        {
            CopyToClipboard(message);
        }

        public static void CopyToClipboard(string text)
        {
            Clipboard.SetText(text);
        }

        private void EditMessageMenuClick(Message message)
        {
            // Создаем новое всплывающее окно
            Form popupForm = new Form();
            popupForm.Text = "Изменение сообщения";
            popupForm.Width = 430;
            popupForm.Height = 330;
            //popupForm.StartPosition = FormStartPosition.CenterParent;
            //popupForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            //popupForm.MaximizeBox = false;

            // Добавляем элементы управления на форму
            System.Windows.Forms.Label label = new System.Windows.Forms.Label();
            label.Text = "Осталось символов: " + (4096 - message.Text.Length);
            label.Location = new Point(10, 10);
            label.AutoSize = true;

            System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox();
            textBox.Text = message.Text;
            textBox.Multiline = true;
            textBox.ScrollBars = ScrollBars.Vertical;
            textBox.WordWrap = true;
            textBox.MaxLength = 4096;
            textBox.Location = new Point(10, 30);
            textBox.Size = new Size(400, 200);
            textBox.ScrollBars = ScrollBars.Both;
            textBox.TextChanged += (sender, e) =>
            {
                label.Text = "Осталось символов: " + (4096 - textBox.Text.Length);
            };

            System.Windows.Forms.Button cancelButton = new System.Windows.Forms.Button();
            cancelButton.Text = "Отмена";
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(335, 240);
            cancelButton.Size = new Size(75, 23);

            System.Windows.Forms.Button editButton = new System.Windows.Forms.Button();
            editButton.Text = "Изменить";
            editButton.DialogResult = DialogResult.OK;
            editButton.Location = new Point(255, 240);
            editButton.Size = new Size(75, 23);

            // Добавляем элементы управления на форму
            popupForm.Controls.Add(label);
            popupForm.Controls.Add(textBox);
            popupForm.Controls.Add(cancelButton);
            popupForm.Controls.Add(editButton);

            // Отображаем всплывающее окно
            DialogResult result = popupForm.ShowDialog();

            // Если была нажата кнопка "Изменить"
            if (result == DialogResult.OK)
            {
                if(message.Text == textBox.Text)
                {
                    return;
                }

                if(textBox.Text.Length == 0)
                {
                    MessageBox.Show("Сообщение не может быть пустым!");
                    textBox.Text = message.Text;
                }
                // Изменяем текст сообщения и сохраняем изменения в базе данных
                message.Text = textBox.Text;
                UpdateMessageText(currentOpenChatId, message.MessageUniqueId, message.Text);

                panel5.Controls.Clear();

                Message[] messages = GetAllMessages(currentOpenChatId);
                globalMessages = messages;
                foreach (Message message1 in messages)
                {
                    CreateWidgetMessage(message1);
                }
            }
        }

        public string GetUserIdChat(string chat_unique_id)
        {
            string user1_id = "";
            string user2_id = "";
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
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
            if(user1_id == user.UniqueId)
            {
                return user2_id;
            }
            return user1_id; ;
            
        }

        public static List<string> GetAllChatsIds()
        {
            List<string> chats = new List<string>();
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand("SELECT chats FROM users WHERE unique_id = @unique_id", connection))
                {
                    command.Parameters.AddWithValue("unique_id", user.UniqueId);
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

        public void DisplayUserData(User user)
        {
            // отображаем данные пользователя на форме в табе чат
            label1.Text = user.Username + "(ВЫ)";

            if(user.Age == 0)
            {
                label22.Text = "(Возраст не указан)";
                label24.Text = "";
            } else
            {
                label22.Text = user.Age.ToString();
            }
            label23.Text = user.Email;
            SetImageFromBytes(user.Photo, pictureBox2);
            // отображаем данные пользователя в настройках
            label28.Text = user.Username;
            textBox4.Text = user.Email;
            numericUpDown1.Value = user.Age;
            FillPasswordTextBoxLengthSymbols();
            SetImageFromBytes(user.Photo, pictureBox7);
            label31.Text += user.RegistrationDate.ToString();

            //

            System.Windows.Forms.Label label = new System.Windows.Forms.Label();
            label.Text = "Выберите чат чтобы начать переписку!";
            label.Font = new Font("Arial", 12, FontStyle.Bold);
            label.Width = 400;
            label.Location = new Point(130, 110);
            label.ForeColor = Color.Gray;

            panel5.Controls.Add(label);
        }

        private void FillPasswordTextBoxLengthSymbols()
        {
            int passwordLength = user.GetPasswordLengthFromDatabase();
            textBox5.Text = new String('*', passwordLength);
        }

        private void SetImageFromBytes(byte[] imageBytes, PictureBox pictureBox)
        {
            using (MemoryStream memoryStream = new MemoryStream(imageBytes))
            {
                Image image = Image.FromStream(memoryStream);
                pictureBox.Image = image;
            }
        }

        public class Chat
        {
            public string Chat_Unique_Id { get; set; }
            public string User1_Id { get; set; }
            public string User2_Id { get; set; }
            public List<Message> Messages { get; set; }

            public string GenerateChatId()
            {
                NpgsqlConnection connection = new NpgsqlConnection(connectionString);
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
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
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

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
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

        public void DeleteChatDialog(string chatId)
        {
            DialogResult result = MessageBox.Show("Вы действительно хотите удалить чат с этим пользователем? Будьте осторожны вы не сможете его восстановить!", "Вы действительно хотите удалить чат?", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                DeleteChat(chatId, user.UniqueId);
                Application.Restart();
            }
            else if (result == DialogResult.Cancel)
            {
            }
        }

        public static void DeleteChat(string chatId, string userUniqueId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
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
                User user = new User();
                user.LoadFromDatabase(userUniqueId);
                // Удаляем chatId из массива chats в записи пользователя
                List<string> chatsIds = GetAllChatsIds();
                if (chatsIds.Contains(chatId))
                {
                    chatsIds.Remove(chatId);
                    globalChats.Remove(chatId);
                    globalMessages = null;
                    currentOpenChatId = "";
                    using (NpgsqlCommand cmd = new NpgsqlCommand("UPDATE users SET chats = @chats WHERE unique_id = @userUniqueId", conn))
                    {
                        cmd.Parameters.AddWithValue("chats", chatsIds.ToArray());
                        cmd.Parameters.AddWithValue("userUniqueId", userUniqueId);
                        cmd.ExecuteNonQuery();
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
                using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
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

        public void DownloadChooseFolder(string chatId)
        {
            var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                DownloadChatMessages(chatId, folderDialog.SelectedPath + "\\messages-" + chatId.Substring(0, 6) + ".txt");
            }
        }

        public static void DownloadChatMessages(string chatId, string filePath)
        {
            // Получаем все сообщения чата
            Message[] messages = GetAllMessages(chatId);

            // Создаем поток для записи в файл
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Проходимся по всем сообщениям чата
                foreach (Message message in messages)
                {
                    // Загружаем данные об отправителе
                    User sender = new User();
                    sender.LoadFromDatabase(message.SenderId);

                    // Загружаем данные о получателе
                    User receiver = new User();
                    receiver.LoadFromDatabase(message.ReceiverId);

                    // Записываем в файл информацию об отправителе, сообщении и времени отправки
                    writer.WriteLine($"{sender.Username} {message.TimeSent}");
                    writer.WriteLine(message.Text);
                    writer.WriteLine();

                    // Делаем то же самое для получателя
                    /*writer.WriteLine($"{receiver.Username} {message.TimeSent}");
                    writer.WriteLine(message.Text);
                    writer.WriteLine();*/
                }
            }
        }

        public void DeleteMessage(string chatId, string messageUniqueId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // Получаем текущий список сообщений
                Message[] messages = GetAllMessages(chatId);

                // Ищем нужное сообщение и удаляем его из списка
                var messageToRemove = messages.FirstOrDefault(m => m.MessageUniqueId == messageUniqueId);
                if (messageToRemove != null)
                {
                    messages = messages.Where(m => m.MessageUniqueId != messageUniqueId).ToArray();

                    // Сохраняем изменения в базе данных
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

        async public void UpdateMessageText(string chatId, string messageUniqueId, string newText)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // Получаем текущий список сообщений
                Message[] messages = GetAllMessages(chatId);

                // Находим нужное сообщение и обновляем его текст
                var messageToUpdate = messages.FirstOrDefault(m => m.MessageUniqueId == messageUniqueId);
                if (messageToUpdate != null)
                {
                    messageToUpdate.Text = newText;
                    messageToUpdate.TimeSent = await GetNetworkTime();

                    // Сохраняем изменения в базе данных
                    using (NpgsqlCommand updateCmd = new NpgsqlCommand("UPDATE chats_messages SET messages = @messages WHERE chat_unique_id = @chatId", conn))
                    {
                        updateCmd.Parameters.AddWithValue("messages", NpgsqlTypes.NpgsqlDbType.Jsonb | NpgsqlTypes.NpgsqlDbType.Array, messages);
                        updateCmd.Parameters.Add("chatId", NpgsqlDbType.Text).Value = chatId;
                        updateCmd.ExecuteNonQuery();
                    }
                }
            }
        }



        public class User
        {
            public string Username { get; set; }
            public int Age { get; set; }
            public string Email { get; set; }
            public byte[] Photo { get; set; }
            public string UniqueId { get; set; }
            public DateTime RegistrationDate { get; set; }

            public void LoadFromDatabase(string uniqueId)
            {
                NpgsqlConnection connection = new NpgsqlConnection(connectionString);
                connection.Open();
                using (var command = new NpgsqlCommand($"SELECT username, age, email, photo, unique_id, registration_date FROM users WHERE unique_id='{uniqueId}'", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            this.Username = reader.GetString(0);
                            this.Age = reader.GetInt32(1);
                            this.Email = reader.GetString(2);
                            this.Photo = (byte[])reader["photo"];
                            this.UniqueId = reader.GetString(4);
                            this.RegistrationDate = reader.GetDateTime(5);
                        } else
                        {
                            this.Username = "(Аккаунт удален)";
                            this.Age = 0; 
                            this.Email = null;
                            this.Photo = null;
                            this.UniqueId = null;
                            this.RegistrationDate = DateTime.MinValue;
                        }
                    }
                }
            }

            public bool DeleteUser()
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand($"DELETE FROM users WHERE unique_id = '{this.UniqueId}'", connection))
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public void ChangePhoto(byte[] newPhoto)
            {
                NpgsqlConnection connection = new NpgsqlConnection(connectionString);
                connection.Open();

                using (var command = new NpgsqlCommand($"UPDATE users SET photo=@photo WHERE unique_id='{this.UniqueId}'", connection))
                {
                    command.Parameters.AddWithValue("@photo", newPhoto);
                    command.ExecuteNonQuery();
                }

                this.Photo = newPhoto;
            }

            public void ChangePassword(string newPassword)
            {
                NpgsqlConnection connection = new NpgsqlConnection(connectionString);
                connection.Open();

                using (var command = new NpgsqlCommand($"UPDATE users SET password='{newPassword}' WHERE unique_id='{this.UniqueId}'", connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            public void ChangeAge(int newAge)
            {
                NpgsqlConnection connection = new NpgsqlConnection(connectionString);
                connection.Open();

                using (var command = new NpgsqlCommand($"UPDATE users SET age='{newAge}' WHERE unique_id='{this.UniqueId}'", connection))
                {
                    command.ExecuteNonQuery();
                }
                this.Age = newAge;
            }

            public void ChangeEmail(string newEmail)
            {
                NpgsqlConnection connection = new NpgsqlConnection(connectionString);
                connection.Open();

                using (var command = new NpgsqlCommand($"UPDATE users SET email='{newEmail}' WHERE unique_id='{this.UniqueId}'", connection))
                {
                    command.ExecuteNonQuery();
                }

                this.Email = newEmail;
            }

            public bool IsEmailExists(string email)
            {
                int count = 0;
                NpgsqlConnection connection = new NpgsqlConnection(connectionString);
                connection.Open();
                using (var command = new NpgsqlCommand($"SELECT COUNT(*) FROM users WHERE email='{email}'", connection))
                {
                    object result = command.ExecuteScalar();
                    count = Convert.ToInt32(result);
                }
                return count > 0 ? true : false;
            }

            public int GetPasswordLengthFromDatabase()
            {
                int passwordLength = 0;
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand($"SELECT length(password) FROM users WHERE unique_id='{this.UniqueId}'", connection))
                    {
                        passwordLength = (int)command.ExecuteScalar();
                    }
                }
                return passwordLength;
            }
        }

        public class Notification
        {
            private Timer timer;
            private System.Windows.Forms.Label label;
            //private NotificationType _type;

            public Notification(string text, int duration, Control parentControl, NotificationType type, Point location)
            {
                // Создаем новую метку с текстом и добавляем на форму
                label = new System.Windows.Forms.Label();
                label.AutoSize = true;
                label.Text = text;
                //label.BackColor = Color.FromArgb(0, 0, 0, 0);
                //label.ForeColor = Color.Black;
                label.Font = new Font("Arial", 12, FontStyle.Bold);
                label.Padding = new Padding(10);
                label.BorderStyle = BorderStyle.FixedSingle;
                label.Location = location;
                switch (type)
                {
                    case NotificationType.Error:
                        label.ForeColor = Color.White;
                        label.BackColor = Color.Red;
                        label.BorderStyle = BorderStyle.Fixed3D;
                        break;
                    case NotificationType.Warning:
                        label.ForeColor = Color.Black;
                        label.BackColor = Color.Yellow;
                        break;
                    case NotificationType.Info:
                        label.ForeColor = Color.Black;
                        label.BackColor = Color.LightGray;
                        break;
                }


                parentControl.Controls.Add(label);

               if (timer != null)
               {
                // использование таймера
                // Создаем таймер для удаления метки через указанное время
                timer = new Timer();
                timer.Interval = duration;
                timer.Tick += new EventHandler(Timer_Tick);
                timer.Start();
                //type = type;
                }
            }

            private void Timer_Tick(object sender, EventArgs e)
            {
                if (timer != null)
                {
                timer.Stop();
                timer.Dispose();
                timer = null;
                label.Parent.Controls.Remove(label);
                }
            }
        }

        public enum NotificationType
        {
            Error,
            Warning,
            Info
        }

        public enum ChangeDataUserModalType
        {
            Email,
            Age,
            Password
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ExitFromApp();
        }

        private static void ExitFromApp()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["IsRegistered"].Value = "false";
            config.AppSettings.Settings["UserId"].Value = "";
            config.Save(ConfigurationSaveMode.Modified);
            Application.Restart();
        }

        async public void create_chat_with_user(string unique_id_receiver)
        {
            if(user.UniqueId == unique_id_receiver)
            {
                MessageBox.Show("Вы не можете написать самому себе!");
                return;
            }
            //Очищаем вводы и выводы пользователей
            panel9.Controls.Clear();
            textBox3.Clear();

            //Проверка существует ли уже чат между пользователями
            if(IsChatIdInArray(user.UniqueId, unique_id_receiver))
            {
                globalAdditionalUser.LoadFromDatabase(unique_id_receiver);
                ClickToChatWidget(globalAdditionalUser, GetChatId(user.UniqueId, unique_id_receiver));
                tabControl1.SelectedTab = tabControl1.TabPages[0];
                return;
            }
            //Создаем обьект класса чата
            Chat newChat = new Chat();
            newChat.Chat_Unique_Id = newChat.GenerateChatId();
            newChat.User1_Id = user.UniqueId;
            newChat.User2_Id = unique_id_receiver;

            chats.Add(newChat);
            //Записываем в базу данных
            newChat.AddChatToDatabase();
            //Добавляем chat_unique_id в поле chats[] в таблице users

            AddChatUniqueIdToUserArray(newChat, user.UniqueId);
            AddChatUniqueIdToUserArray(newChat, unique_id_receiver);

            AddUserIdToArrayChatsWithUsers(user.UniqueId, unique_id_receiver);
            AddUserIdToArrayChatsWithUsers(unique_id_receiver, user.UniqueId);

            string messageCreate = "(" + user.Username + " создал чат)";
            await CreateMessage(messageCreate, unique_id_receiver, newChat.Chat_Unique_Id);

            //MessageBox.Show("Чат создан!");
            
            LoadAllChatsUser();

            // Переключаемся в меню чатов
            tabControl1.SelectedTab = tabControl1.TabPages[0];

            globalAdditionalUser.LoadFromDatabase(unique_id_receiver);
            ClickToChatWidget(globalAdditionalUser, newChat.Chat_Unique_Id);

        }


        public bool IsChatIdInArray(string whereUserId, string whatUserId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
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

        public string GetChatId(string user1Id, string user2Id)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
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

        private static void AddUserIdToArrayChatsWithUsers(string where_user_id, string what_user_id)
        {
            NpgsqlConnection conn;
            NpgsqlCommand cmd;
            conn = new NpgsqlConnection(connectionString);
            conn.Open();
            cmd = new NpgsqlCommand("UPDATE users SET chats_with_users_id = array_append(chats_with_users_id, @what_user_id) WHERE unique_id = @where_user_id", conn);
            cmd.Parameters.AddWithValue("where_user_id", where_user_id);
            cmd.Parameters.AddWithValue("what_user_id", what_user_id);
            cmd.ExecuteNonQuery();
        }

        private static void AddChatUniqueIdToUserArray(Chat newChat, string user_id)
        {
            NpgsqlConnection conn;
            NpgsqlCommand cmd;
            conn = new NpgsqlConnection(connectionString);
            conn.Open();
            cmd = new NpgsqlCommand("UPDATE users SET chats = array_append(chats, @chat_id) WHERE unique_id = @user_id", conn);
            cmd.Parameters.AddWithValue("chat_id", newChat.Chat_Unique_Id);
            cmd.Parameters.AddWithValue("user_id", user_id);
            cmd.ExecuteNonQuery();
        }

        public void SearchUsers(string searchText)
        {
            bool foundUsers = false;
            string query = "SELECT username, age, photo, unique_id FROM users WHERE username LIKE @searchText LIMIT 5;";
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("searchText", $"%{searchText}%");
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        // Очищаем предыдущие результаты поиска
                        ClearSearchResults();
                        while (reader.Read())
                        {
                            foundUsers = true;
                            // Получаем данные о пользователе
                            string username = reader.GetString(0);
                            int age = reader.GetInt32(1);
                            byte[] photo = (byte[])reader[2];
                            string unique_id = reader.GetString(3);

                            // Создаем новый элемент интерфейса для вывода информации о пользователе
                            Panel userPanel = new Panel();
                            userPanel.BorderStyle = BorderStyle.FixedSingle;
                            userPanel.Width = panel9.Width - 20;
                            userPanel.Height = 50;
                            userPanel.Padding = new Padding(5);
                            userPanel.Location = new Point(0, panel9.Controls.Count * userPanel.Height);

                            PictureBox photoBox = new PictureBox();
                            photoBox.Width = 40;
                            photoBox.Height = 40;
                            photoBox.SizeMode = PictureBoxSizeMode.StretchImage;
                            using (MemoryStream memoryStream = new MemoryStream(photo))
                            {
                                Image image = Image.FromStream(memoryStream);
                                photoBox.Image = image;
                            }

                            System.Windows.Forms.Button write_user = new System.Windows.Forms.Button();
                            write_user.Text = "Написать";
                            write_user.Click += (sender, e) => create_chat_with_user(unique_id);

                            System.Windows.Forms.Label nameLabel = new System.Windows.Forms.Label();
                            if(unique_id == user.UniqueId)
                            {
                                nameLabel.Text = username + "(ВЫ)";
                            } else
                            {
                                nameLabel.Text = username;
                            }
                            nameLabel.Font = new Font("Arial", 12, FontStyle.Bold);
                            nameLabel.Width = 150;
                            nameLabel.Location = new Point(50, 10);

                            System.Windows.Forms.Label ageLabel = new System.Windows.Forms.Label();
                            string displayAge = "";
                            if(age != 0)
                            {
                                displayAge = "Возраст: " + age.ToString();
                            } else
                            {
                                displayAge = "(Возраст не указан)";
                            }
                            ageLabel.Text = displayAge;
                            ageLabel.Font = new Font("Arial", 10);
                            ageLabel.Width = 150;
                            ageLabel.Location = new Point(50, 30);

                            userPanel.Controls.Add(photoBox);
                            photoBox.Location = new Point(5, 5);

                            userPanel.Controls.Add(nameLabel);
                            nameLabel.Location = new Point(photoBox.Right + 5, 5);

                            userPanel.Controls.Add(ageLabel);
                            ageLabel.Location = new Point(photoBox.Right + 5, nameLabel.Bottom + 5);

                            userPanel.Controls.Add(write_user);
                            write_user.Width = 150;
                            write_user.Height = 30;
                            write_user.Location = new Point(700, 10);

                            // Добавляем новый элемент интерфейса на панель результатов поиска
                            panel9.Controls.Add(userPanel);
                        }
                        if (!foundUsers)
                        {
                            Notification notification = new Notification("Пользователя с таким именем не существует!", 3000, panel9, NotificationType.Warning, new Point(250, 50));
                        }
                    }
                }
            }
        }

        private void ClearSearchResults()
        {
            // Удаляем предыдущие результаты поиска
            foreach (Control control in panel9.Controls)
            {
                control.Dispose();
            }
            panel9.Controls.Clear();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string search_term = textBox3.Text;
            if(search_term.Length > 0 )
            {
                SearchUsers(textBox3.Text);
            } else
            {
                ClearSearchResults();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Вы действительно хотите удалить свой аккаунт? Вы не сможете его восстановить", "Вы действительно хотите удалить свой аккаунт?", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                user.DeleteUser();
                ExitFromApp();
            }
            else if (result == DialogResult.Cancel)
            {
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            changeDataUserModal("Введите новую почту", ChangeDataUserModalType.Email, user);
        }
        private void button8_Click(object sender, EventArgs e)
        {
            changeDataUserModal("Введите новый возраст", ChangeDataUserModalType.Age, user);

        }

        private void button9_Click(object sender, EventArgs e)
        {
            changeDataUserModal("Введите новый пароль", ChangeDataUserModalType.Password, user);
        }

        private static void changeDataUserModal(string nameForm, ChangeDataUserModalType parametrData, User user)
        {
            using (var inputBox = new Form())
            {
                // установка свойств формы
                inputBox.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputBox.MaximizeBox = false;
                inputBox.MinimizeBox = false;
                inputBox.StartPosition = FormStartPosition.CenterParent;
                inputBox.Text = nameForm;

                var textBox=new System.Windows.Forms.TextBox();
                var numericUpDown= new System.Windows.Forms.NumericUpDown();
                if (parametrData == ChangeDataUserModalType.Email || parametrData == ChangeDataUserModalType.Password) {
                    // создание текстового поля
                    textBox = new System.Windows.Forms.TextBox()
                    {
                        Left = 10,
                        Top = 10,
                        Width = 200
                    };
                    inputBox.Controls.Add(textBox);
                } else
                {
                    numericUpDown = new System.Windows.Forms.NumericUpDown()
                    {
                        Left = 10,
                        Top = 10,
                        Width = 200
                    };
                    inputBox.Controls.Add(numericUpDown);

                }
                // создание кнопок "Ок" и "Отмена"
                var okButton = new System.Windows.Forms.Button()
                {
                    Text = "Ок",
                    DialogResult = DialogResult.OK,
                    Left = 10,
                    Top = 50,
                    Width = 80
                };
                inputBox.Controls.Add(okButton);

                var cancelButton = new System.Windows.Forms.Button()
                {
                    Text = "Отмена",
                    DialogResult = DialogResult.Cancel,
                    Left = 110,
                    Top = 50,
                    Width = 80
                };
                inputBox.Controls.Add(cancelButton);

                // показываем форму как модальное окно и, если нажата кнопка "Ок", 
                // выводим в MessageBox значение текстового поля
                if (inputBox.ShowDialog() == DialogResult.OK)
                {
                    switch (parametrData) {
                        case ChangeDataUserModalType.Email:
                            if (textBox.Text.Length > 0)
                            {
                                if (Form1.ValidateEmail(textBox.Text))
                                {
                                    if (user.IsEmailExists(textBox.Text.ToString())){
                                        MessageBox.Show("Такой адрес электронной почты уже существует!");
                                        break;
                                    }
                                    else
                                    {

                                        user.ChangeEmail(textBox.Text);
                                        MessageBox.Show("Адрес электронной почты изменен!");
                                        Application.Restart();
                                        break;
                                    }

                                } else
                                {
                                    MessageBox.Show("Неккоректный адрес электронной почты!");
                                }
                            } else
                            {
                                MessageBox.Show("Поле ввода пустое!");
                                break;
                            }
                            break;
                        case ChangeDataUserModalType.Password:
                            if(textBox.Text.Length > 0)
                            {
                                if (Form1.ValidatePassword(textBox.Text))
                                {
                                    user.ChangePassword(textBox.Text);
                                    MessageBox.Show("Пароль изменен, войдите в систему с новым паролем!");
                                    ExitFromApp();
                                    break;
                                } else
                                {
                                    MessageBox.Show("Пароль должен содержать от 8 до 20 символов. Необходимо использовать: латиниские буквы(хотя бы одну в верхнем и нижнем регистре), хотя бы 1 цифру, спец.символ");
                                }
                            }
                            MessageBox.Show("Поле ввода пустое!"); break;
                        case ChangeDataUserModalType.Age:
                            if (Form1.ValidateAge(Convert.ToInt32(numericUpDown.Value)))
                            {
                                user.ChangeAge(Convert.ToInt32(numericUpDown.Value));
                                MessageBox.Show("Возраст изменен!");
                                Application.Restart();
                                break;
                            } 
                            MessageBox.Show("Введен неккоректный возраст!"); break;
                        default:
                            break;
                    }
                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Form1.OpenPhotoAndAddPhotoToPictureBox(pictureBox7);
            user.ChangePhoto(Form1.GetImageBytesFromPictureBox(pictureBox7));
            MessageBox.Show("Фото изменено!");
            Application.Restart();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/CandyPopsWorld/messenger-kursovay");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabControl1.TabPages[1];
        }

        private void panel5_MouseEnter(object sender, EventArgs e)
        {
            panel5.Focus();
        }

        private void panel5_MouseLeave(object sender, EventArgs e)
        {
        }

        private void panel3_MouseEnter(object sender, EventArgs e)
        {
            panel3.Focus();
        }

        private void panel3_MouseLeave(object sender, EventArgs e)
        {
            this.ActiveControl= null;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (textBox3.Text != textBox3.Text.ToLower())
            {
                textBox3.Text = textBox3.Text.ToLower();
                textBox3.SelectionStart = textBox3.Text.Length;
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Получаем индекс текущей выбранной вкладки
            int selectedIndex = tabControl1.SelectedIndex;

            // Устанавливаем заголовок формы в зависимости от выбранной вкладки
            switch (selectedIndex)
            {
                case 0:
                    this.Text = "ЧАТЫ";
                    break;
                case 1:
                    this.Text = "ПОИСК ПОЛЬЗОВАТЕЛЕЙ";
                    break;
                case 2:
                    this.Text = "НАСТРОЙКИ";
                    break;
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            SetUserOnlineStatus(user.UniqueId, false);
        }
    }
}

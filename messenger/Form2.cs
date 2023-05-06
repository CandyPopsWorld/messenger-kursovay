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

namespace messenger
{
    public partial class Form2 : Form
    {
        string userId = ConfigurationManager.AppSettings["UserId"];
        static string connectionString = "Server=localhost;Port=5432;Database=messenger;User Id=postgres;Password=regular123;";
        public static User user; // объявляем переменную класса User
        public static Chats chats; // объявляем переменную класса Chats
        public Form2()
        {
            InitializeComponent();
            user = new User(); // создаем экземпляр класса User
            chats = new Chats();
            user.LoadFromDatabase(userId); // вызываем метод LoadFromDatabase для получения данных из базы данных и заполнения полей класса
            DisplayUserData(user); // отображаем полученные данные на форме
            LoadAllChatsUser();
        }

        public void LoadAllChatsUser()
        {
            panel3.Controls.Clear();
            List<string> chatsIds = GetAllChatsIds();

            foreach (string chatId in chatsIds)
            {
                string additionalUserId = GetUserIdChat(chatId);
                User additionalUser = new User();
                additionalUser.LoadFromDatabase(additionalUserId);
                CreateNewChatPanel(additionalUser, chatId);
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

            PictureBox photoBox = new PictureBox();
            photoBox.Width = 44;
            photoBox.Height = 52;
            photoBox.SizeMode = PictureBoxSizeMode.StretchImage;
            using (MemoryStream memoryStream = new MemoryStream(photo))
            {
                Image image = Image.FromStream(memoryStream);
                photoBox.Image = image;
            }

            System.Windows.Forms.Label usernameLabel = new System.Windows.Forms.Label();
            usernameLabel.Text = username;
            usernameLabel.Width = 150;
            usernameLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            usernameLabel.Location = new Point(50, 10);

            chatPanel.Click += (sender, e) => ClickToChatWidget(additionalUser);
            usernameLabel.Click += (sender, e) => ClickToChatWidget(additionalUser);
            photoBox.Click += (sender, e) => ClickToChatWidget(additionalUser);

            chatPanel.Controls.Add(photoBox);
            chatPanel.Controls.Add(usernameLabel);
            panel3.Controls.Add(chatPanel);
        }

        public void ClickToChatWidget(User additionalUser)
        {
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

            PictureBox photoBox = new PictureBox();
            photoBox.Width = 44;
            photoBox.Height = 52;
            photoBox.SizeMode = PictureBoxSizeMode.StretchImage;
            using (MemoryStream memoryStream = new MemoryStream(photo))
            {
                Image image = Image.FromStream(memoryStream);
                photoBox.Image = image;
            }

            System.Windows.Forms.Label usernameLabel = new System.Windows.Forms.Label();
            usernameLabel.Text = username;
            usernameLabel.Width = 150;
            usernameLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            usernameLabel.Location = new Point(50, 10);

            chatPanel.Click += (sender, e) => ClickToChatWidget(additionalUser);
            usernameLabel.Click += (sender, e) => ClickToChatWidget(additionalUser);
            photoBox.Click += (sender, e) => ClickToChatWidget(additionalUser);

            chatPanel.Controls.Add(photoBox);
            chatPanel.Controls.Add(usernameLabel);
            //ПОВТОРЕНИЕ КОДА(КОНЕЦ)

            //ДОБАВЛЕНИЕ TEXTBOX И КНОПКИ ДЛЯ ПЕЧАТИ И ОТПРАВКИ СООБЩЕНИЙ
            panel11.Controls.Clear();

            System.Windows.Forms.TextBox messageTextBox = new System.Windows.Forms.TextBox();
            messageTextBox.Multiline = true;
            messageTextBox.Width = 455;
            messageTextBox.Height = 54;
            messageTextBox.Location = new Point(10, 0);

            System.Windows.Forms.Button write_user = new System.Windows.Forms.Button();
            write_user.Text = "Отправить";
            write_user.Click += (sender, e) => SendMessageToUser(messageTextBox);
            write_user.Width = 142;
            write_user.Height = 54;
            write_user.Location = new Point(480, 0);

            panel11.Controls.Add(messageTextBox);
            panel11.Controls.Add(write_user);

            //
            panel2.Controls.Clear();
            panel2.Controls.Add(chatPanel);
            //MessageBox.Show(additionalUser.Username);
        }

        public void SendMessageToUser(System.Windows.Forms.TextBox messageTextBox)
        {
            string message = messageTextBox.Text;
            MessageBox.Show(message);
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

        public List<string> GetAllChatsIds()
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
            label1.Text = user.Username;
            label22.Text = user.Age.ToString();
            label23.Text = user.Email;
            SetImageFromBytes(user.Photo, pictureBox2);
            // отображаем данные пользователя в настройках
            label28.Text = user.Username;
            textBox4.Text = user.Email;
            numericUpDown1.Value = user.Age;
            FillPasswordTextBoxLengthSymbols();
            SetImageFromBytes(user.Photo, pictureBox7);
            label31.Text += user.RegistrationDate.ToString();
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

        public void create_chat_with_user(string unique_id_receiver)
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
                MessageBox.Show("Чат между вами уже существует!");
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

            MessageBox.Show("Чат создан!");
            
            LoadAllChatsUser();

            // Переключаемся в меню чатов
            tabControl1.SelectedTab = tabControl1.TabPages[0];
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
                            ageLabel.Text = $"Age: {age}";
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
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/CandyPopsWorld/messenger-kursovay");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabControl1.TabPages[1];
        }
    }
}

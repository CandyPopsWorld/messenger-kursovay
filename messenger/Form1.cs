using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Npgsql;
using System.Configuration;

namespace messenger
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        /*public static void SqlTest()
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(GlobalData.connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    //cmd.CommandText = "CREATE TABLE users (id SERIAL PRIMARY KEY, username VARCHAR(50), password VARCHAR(50), email VARCHAR(50), age INTEGER, photo BYTEA, unique_id VARCHAR(255), registration_date TIMESTAMP WITHOUT TIME ZONE, chats_with_users_id TEXT[] DEFAULT '{}', chats TEXT[] DEFAULT '{}', hiddenchats TEXT[] DEFAULT '{}')";
                    //cmd.CommandText = @"CREATE TABLE users_status (
                    //user_unique_id char(255) PRIMARY KEY,
                    //isonline boolean
                    //)";
                    //cmd.CommandText = @"CREATE TABLE IF NOT EXISTS chats (
                    //chat_unique_id char(255) PRIMARY KEY,
                    // user1_id char(255),
                    // user2_id char(255)
                    //);";
                    //cmd.CommandText = @"CREATE TABLE IF NOT EXISTS chats_messages (
                                   // chat_unique_id character varying(255) NOT NULL,
	                               // messages JSONB[] DEFAULT ARRAY[]::JSONB[],
	                             //   CONSTRAINT fk_chat FOREIGN KEY (chat_unique_id) REFERENCES chats(chat_unique_id) ON DELETE CASCADE,
	                               // PRIMARY KEY(chat_unique_id)
                               // )";
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }*/

        private void registerBtn_Click(object sender, EventArgs e)
        {
            string login = usernameTextBox.Text.Trim();
            string password = textBox3.Text;
            string email = textBox1.Text.Trim();
            int age = 18;
            byte[] imageBytes = ElementHelper.GetImageBytesFromPictureBox(pictureBox1);
            if (!string.IsNullOrWhiteSpace(numericUpDown1.Value.ToString()))
            {
                age = int.Parse(numericUpDown1.Value.ToString());
            }

            if (!Validator.ValidateLogin(login))
            {
                MessageBox.Show("Логин должен содержать от 3 до 20 символов");
                return;
            }

            if (!Validator.ValidatePassword(password))
            {
                MessageBox.Show("Пароль должен содержать от 8 до 20 символов. Необходимо использовать: латиниские буквы(хотя бы одну в верхнем и нижнем регистре), хотя бы 1 цифру, спец.символ");
                return;
            }

            if (!Validator.ValidateEmail(email))
            {
                MessageBox.Show("Некорректный email адрес");
                return;
            }

            if (!Validator.IsUsernameUnique(login))
            {
                MessageBox.Show("Пользователь с таким логином уже существует");
                return;
            }

            if (!Validator.IsEmailUnique(email))
            {
                MessageBox.Show("Пользователь с такой почтой уже существует");
                return;
            }
            AuthenticationManager.RegisterUser(login, email, password, age, imageBytes);
        }

        private void uploadAvatarBtn_Click(object sender, EventArgs e)
        {
            ElementHelper.OpenPhotoAndAddPhotoToPictureBox(pictureBox1);
        }

        private void recoverPasswordBtn_Click(object sender, EventArgs e)
        {
            string email = textBox6.Text;
            // Проверяем наличие пользователя в базе данных по email
            using (var connection = new NpgsqlConnection(GlobalData.connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("SELECT email FROM users WHERE email=@email", connection);
                cmd.Parameters.AddWithValue("email", email);

                var result = cmd.ExecuteScalar();
                if (result != null)// Пользователь найден
                {
                    GlobalData.tabPageForgetPasswordControls = tabPage3.Controls.Cast<Control>().ToArray();
                    string code = Utils.GenerateRandomCode(4); // генерация случайного кода
                    GlobalData.code_global_xren = code;
                    GlobalData.email_user_ch_pass = email;
                    AuthenticationManager.SendCodeByEmail(email, code);
                    GenerateCodeBlock();
                }
                else
                {
                    MessageBox.Show("Пользователь с таким email не найден!");
                }
            }
        }

        private void loginBtn_Click(object sender, EventArgs e)
        {
            string username = usernameLoginTextBox.Text;
            string password = textBox4.Text;
            string user_id = "";

            // Проверяем наличие пользователя в базе данных
            using (var connection = new NpgsqlConnection(GlobalData.connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("SELECT id FROM users WHERE username=@username AND password=@password", connection);
                cmd.Parameters.AddWithValue("username", username);
                cmd.Parameters.AddWithValue("password", password);

                var result = cmd.ExecuteScalar();
                if (result != null) // Пользователь найден
                {
                    using (NpgsqlConnection conn = new NpgsqlConnection(GlobalData.connectionString))
                    {
                        conn.Open();
                        using (NpgsqlCommand command = new NpgsqlCommand($"SELECT unique_id FROM users WHERE username = '{username}' AND password = '{password}'", conn))
                        {
                            var result_id = command.ExecuteScalar();
                            if (result_id != null)
                            {
                                user_id = (string)result_id;
                            }
                        }
                    }

                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["IsRegistered"].Value = "true";
                    config.AppSettings.Settings["UserId"].Value = user_id;
                    config.Save(ConfigurationSaveMode.Modified);
                    Application.Restart();
                }
                else // Пользователь не найден
                {
                    MessageBox.Show("Неправильное имя пользователя или пароль!");
                }
            }
        }

        private void GenerateCodeBlock()
        {
            // Создаем новый TextBox
            var newTextBox = new TextBox();
            newTextBox.Location = new Point(tabPage3.Width / 3, tabPage3.Height / 2);
            //newTextBox.Location = new Point(tabPage3.Width / 2 - newTextBox.Width / 2, tabPage3.Height / 2 - newTextBox.Height / 2);
            newTextBox.Size = new Size(300, 20);
            newTextBox.BackColor = Color.FromName("Menu");
            newTextBox.Name = "codeTextBox";

            // Создаем новую Button
            var newButton = new Button();
            newButton.Location = new Point(tabPage3.Width / 3, (tabPage3.Height / 2) + 20);
            //newButton.Location = new Point(tabPage3.Width / 2 - newButton.Width / 2, tabPage3.Height / 2 - newButton.Height / 2 + 20);
            newButton.Size = new Size(75, 23);
            newButton.Text = "Отправить";

            // Создаем новый label
            var newLabel = new Label();
            newLabel.Location = new Point(tabPage3.Width / 3, (tabPage3.Height / 2) - 20);
            //newLabel.Location = new Point(tabPage3.Width / 2 - newLabel.Width / 2, tabPage3.Height / 2 - newLabel.Height / 2 - 20);
            newLabel.Size = new Size(300, 30);
            newLabel.Text = "Введите код подтверждения:";

            // Подписываемся на событие Click для новой кнопки
            newButton.Click += new EventHandler(EqualsCodeClientClick);

            // Добавляем новые элементы управления на форму
            tabPage3.Controls.Clear();
            tabPage3.Controls.Add(newTextBox);
            tabPage3.Controls.Add(newButton);
            tabPage3.Controls.Add(newLabel);
            
        }
        public void EqualsCodeClientClick(object sender, EventArgs e)
        {
            // получаем ссылку на кнопку, которая вызвала обработчик событий
            Button button = (Button)sender;

            TabPage tabPage = (TabPage)button.Parent;

            // находим дочерний элемент textbox на панели
            TextBox codeTextBox = (TextBox)tabPage.Controls["codeTextBox"];

            // теперь вы можете использовать ссылку на textbox, чтобы получить или задать его свойства

            if (codeTextBox.Text == GlobalData.code_global_xren)
            {
                GenerateChangePasswordBlock();
            }
            else
            {
                MessageBox.Show("Код подтверждения некорректный!");
            }
        }
        private void GenerateChangePasswordBlock()
        {
            // Создаем новый TextBox
            var newTextBox = new TextBox();
            newTextBox.Location = new Point(tabPage3.Width / 3, tabPage3.Height / 2);
            //newTextBox.Location = new Point(tabPage3.Width / 2 - newTextBox.Width / 2, tabPage3.Height / 2 - newTextBox.Height / 2);
            newTextBox.Size = new Size(300, 20);
            newTextBox.BackColor = Color.FromName("Menu");
            newTextBox.Name = "newPasswordTextBox";

            // Создаем новую Button
            var newButton = new Button();
            newButton.Location = new Point(tabPage3.Width / 3, (tabPage3.Height / 2) + 20);
            //newButton.Location = new Point(tabPage3.Width / 2 - newButton.Width / 2, tabPage3.Height / 2 - newButton.Height / 2 + 20);
            newButton.Size = new Size(75, 23);
            newButton.Text = "Изменить";

            // Создаем новый label
            var newLabel = new Label();
            newLabel.Location = new Point(tabPage3.Width / 3, (tabPage3.Height / 2) - 20);
            //newLabel.Location = new Point(tabPage3.Width / 2 - newLabel.Width / 2, tabPage3.Height / 2 - newLabel.Height / 2 - 20);
            newLabel.Size = new Size(300, 30);
            newLabel.Text = "Введите новый пароль:";

            // Подписываемся на событие Click для новой кнопки
            newButton.Click += new EventHandler(ChangePasswordClick);

            // Добавляем новые элементы управления на форму
            tabPage3.Controls.Clear();
            tabPage3.Controls.Add(newTextBox);
            tabPage3.Controls.Add(newButton);
            tabPage3.Controls.Add(newLabel);

        }

        private void ChangePasswordClick(object sender, EventArgs e)
        {
            // получаем ссылку на кнопку, которая вызвала обработчик событий
            Button button = (Button)sender;

            TabPage tabPage = (TabPage)button.Parent;

            // находим дочерний элемент textbox на панели
            TextBox newPasswordTextBox = (TextBox)tabPage.Controls["newPasswordTextBox"];
            if (!Validator.ValidatePassword(newPasswordTextBox.Text))
            {
                MessageBox.Show("Пароль должен содержать от 8 до 20 символов");
                return;
            }

            using (var connection = new NpgsqlConnection(GlobalData.connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("SELECT email FROM users WHERE email=@email", connection);
                cmd.Parameters.AddWithValue("email", GlobalData.email_user_ch_pass);

                var result = cmd.ExecuteScalar();
                if (result != null)
                { // Пользователь найден
                    using (var conn = new NpgsqlConnection(GlobalData.connectionString))
                    {
                        conn.Open();
                        var cmd1 = new NpgsqlCommand("UPDATE users SET password=@password WHERE email=@email", conn);
                        cmd1.Parameters.AddWithValue("@password", newPasswordTextBox.Text);
                        cmd1.Parameters.AddWithValue("@email", GlobalData.email_user_ch_pass);
                        cmd1.ExecuteNonQuery();
                        conn.Close();
                        GlobalData.code_global_xren = "";
                        GlobalData.email_user_ch_pass = "";
                        MessageBox.Show("Пароль изменен!");
                    }

                }
            }
            tabPage3.Controls.Clear();
            tabPage3.Controls.AddRange(GlobalData.tabPageForgetPasswordControls);
        }

        private void usernameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (usernameTextBox.Text != usernameTextBox.Text.ToLower())
            {
                usernameTextBox.Text = usernameTextBox.Text.ToLower();
                usernameTextBox.SelectionStart = usernameTextBox.Text.Length;
            }
        }

        private void usernameLoginTextBox_TextChanged(object sender, EventArgs e)
        {
            if (usernameLoginTextBox.Text != usernameLoginTextBox.Text.ToLower())
            {
                usernameLoginTextBox.Text = usernameLoginTextBox.Text.ToLower();
                usernameLoginTextBox.SelectionStart = usernameLoginTextBox.Text.Length;
            }
        }
    }
}

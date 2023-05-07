using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;
using System.Configuration;
using System.Net.Mail;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Identity.Client;
using System.Data.SqlClient;
using System.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace messenger
{
    public partial class Form1 : Form
    {
        string code_global_xren = "";
        string email_user_ch_pass = "";
        Control[] tabPageForgetPasswordControls;

        static string connectionString = "Server=localhost;Port=5432;Database=messenger;User Id=postgres;Password=regular123;";
        NpgsqlConnection conn = new NpgsqlConnection(connectionString);
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenPhotoAndAddPhotoToPictureBox(pictureBox1);
        }

        public static void OpenPhotoAndAddPhotoToPictureBox(PictureBox pictureBox)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox.Image = Image.FromFile(openFileDialog.FileName);
            }
        }

        public bool ValidateLogin(string login)
        {
            // Проверяем длину логина
            if (login.Length < 3 || login.Length > 20)
            {
                return false;
            }

            // Проверяем символы логина
            foreach (char c in login)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    return false;
                }
            }

            // Если все проверки пройдены, возвращаем true
            return true;
        }

        public static bool ValidatePassword(string password)
        {
            // Проверяем длину пароля
            if (password.Length < 8 || password.Length > 20)
            {
                return false;
            }

            // Проверяем наличие символов разных типов
            bool hasUpper = false;
            bool hasLower = false;
            bool hasDigit = false;
            bool hasSpecial = false;
            foreach (char c in password)
            {
                if (char.IsUpper(c))
                {
                    hasUpper = true;
                }
                else if (char.IsLower(c))
                {
                    hasLower = true;
                }
                else if (char.IsDigit(c))
                {
                    hasDigit = true;
                }
                else if (char.IsSymbol(c) || char.IsPunctuation(c))
                {
                    hasSpecial = true;
                }
            }

            // Если не найдено символов всех типов, возвращаем false
            if (!hasUpper || !hasLower || !hasDigit || !hasSpecial)
            {
                return false;
            }

            // Если все проверки пройдены, возвращаем true
            return true;
        }

        public static bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                var mailAddress = new MailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool ValidateAge(int age)
        {
            if(age >= 0 && age <= 150)
            {
                return true;
            }
            return false;
        }

        /*public bool TestConnection()
        {
            using (conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (var command = new NpgsqlCommand("SELECT 1", conn))
                    {
                        command.ExecuteNonQuery();
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }*/

        public bool IsUsernameUnique(string username)
        {
            bool isUnique = false;
            using (conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM users WHERE username = @username";
                using (NpgsqlCommand command = new NpgsqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("username", username);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count == 0)
                    {
                        isUnique = true;
                    }
                }
            }
            return isUnique;
        }

        public bool IsEmailUnique(string email)
        {
            bool isUnique = false;
            using (conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM users WHERE email = @email";
                using (NpgsqlCommand command = new NpgsqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("email", email);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count == 0)
                    {
                        isUnique = true;
                    }
                }
            }
            return isUnique;
        }
        public void register_user(string login, string email, string password, int age, byte[] imageBytes)
        {
            string unique_id = GenerateUserId();
            using (conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO users (username, password, email, age, photo, unique_id, registration_date) VALUES (@username, @password, @email, @age, @photo, @unique_id, NOW())";
                    cmd.Parameters.AddWithValue("username", login);
                    cmd.Parameters.AddWithValue("password", password);
                    cmd.Parameters.AddWithValue("email", email);
                    cmd.Parameters.AddWithValue("age", age);
                    cmd.Parameters.AddWithValue("photo", imageBytes);
                    cmd.Parameters.AddWithValue("unique_id", unique_id);
                    cmd.ExecuteNonQuery();
                }
            }

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["IsRegistered"].Value = "true";
            config.AppSettings.Settings["UserId"].Value = unique_id;
            config.Save(ConfigurationSaveMode.Modified);


            MessageBox.Show("Регистрация успешна!");
            Application.Restart();
        }
        public static byte[] GetImageBytesFromPictureBox(PictureBox pictureBox)
        {
            if (pictureBox.Image == null) return null;

            using (MemoryStream ms = new MemoryStream())
            {
                string format = pictureBox.Image.RawFormat.ToString();
                ImageFormat imageFormat = ImageFormat.Jpeg;

                if (format.Equals("png", StringComparison.OrdinalIgnoreCase))
                {
                    imageFormat = ImageFormat.Png;
                }
                else if (format.Equals("jpg", StringComparison.OrdinalIgnoreCase))
                {
                    imageFormat = ImageFormat.Jpeg;
                }
                else if (format.Equals("jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    imageFormat = ImageFormat.Jpeg;
                }

                pictureBox.Image.Save(ms, imageFormat);
                return ms.ToArray();
            }
        }

        public static string GenerateUserId()
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open(); 
            // Генерируем новый уникальный идентификатор
            string newUserId = Guid.NewGuid().ToString();

            // Проверяем, что пользователь с таким id еще не существует
            using (var command = new NpgsqlCommand($"SELECT COUNT(*) FROM users WHERE unique_id='{newUserId}'", connection))
            {
                int count = Convert.ToInt32(command.ExecuteScalar());
                while (count > 0)
                {
                    newUserId = Guid.NewGuid().ToString();
                    command.CommandText = $"SELECT COUNT(*) FROM users WHERE unique_id='{newUserId}'";
                    count = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            return newUserId;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string login = textBox2.Text.Trim();
            string password = textBox3.Text;
            string email = textBox1.Text.Trim();
            int age=18;
            byte[] imageBytes = GetImageBytesFromPictureBox(pictureBox1);
            if (!string.IsNullOrWhiteSpace(numericUpDown1.Value.ToString()))
            {
                age = int.Parse(numericUpDown1.Value.ToString());
            }

            if (!ValidateLogin(login))
            {
                MessageBox.Show("Логин должен содержать от 3 до 20 символов");
                return;
            }

            if (!ValidatePassword(password))
            {
                MessageBox.Show("Пароль должен содержать от 8 до 20 символов. Необходимо использовать: латиниские буквы(хотя бы одну в верхнем и нижнем регистре), хотя бы 1 цифру, спец.символ");
                return;
            }

            if (!ValidateEmail(email))
            {
                MessageBox.Show("Некорректный email адрес");
                return;
            }

            if(!IsUsernameUnique(login))
            {
                MessageBox.Show("Пользователь с таким логином уже существует");
                return;
            }

            if (!IsEmailUnique(email))
            {
                MessageBox.Show("Пользователь с такой почтой уже существует");
                return;
            }
            register_user(login, email, password, age, imageBytes);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string username = textBox5.Text;
            string password = textBox4.Text;
            string user_id="";

            // Проверяем наличие пользователя в базе данных
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("SELECT id FROM users WHERE username=@username AND password=@password", connection);
                cmd.Parameters.AddWithValue("username", username);
                cmd.Parameters.AddWithValue("password", password);

                var result = cmd.ExecuteScalar();
                if (result != null) // Пользователь найден
                {
                    using (conn = new NpgsqlConnection(connectionString))
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

        private void button3_Click(object sender, EventArgs e)
        {
            string email = textBox6.Text; 
            // Проверяем наличие пользователя в базе данных по email
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("SELECT email FROM users WHERE email=@email", connection);
                cmd.Parameters.AddWithValue("email", email);

                var result = cmd.ExecuteScalar();
                if (result != null)// Пользователь найден
                {
                    tabPageForgetPasswordControls = tabPage3.Controls.Cast<Control>().ToArray();
                    string code = GenerateRandomCode(4); // генерация случайного кода
                    code_global_xren = code;
                    email_user_ch_pass = email;
                    SendCodeByEmail(email,code);
                    GenerateCodeBlock();
                } else
                {
                    MessageBox.Show("Пользователь с таким email не найден!");
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
            if (!ValidatePassword(newPasswordTextBox.Text))
            {
                MessageBox.Show("Пароль должен содержать от 8 до 20 символов");
                return;
            }

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("SELECT email FROM users WHERE email=@email", connection);
                cmd.Parameters.AddWithValue("email", email_user_ch_pass);

                var result = cmd.ExecuteScalar();
                if (result != null)
                { // Пользователь найден
                    using (var conn = new NpgsqlConnection(connectionString))
                    {
                        conn.Open();
                        var cmd1 = new NpgsqlCommand("UPDATE users SET password=@password WHERE email=@email", conn);
                        cmd1.Parameters.AddWithValue("@password", newPasswordTextBox.Text);
                        cmd1.Parameters.AddWithValue("@email", email_user_ch_pass);
                        cmd1.ExecuteNonQuery();
                        conn.Close();
                        code_global_xren = "";
                        email_user_ch_pass = "";
                        MessageBox.Show("Пароль изменен!");
                    }

                }
            }
            tabPage3.Controls.Clear();
            tabPage3.Controls.AddRange(tabPageForgetPasswordControls);
        }


        private void EqualsCodeClientClick(object sender, EventArgs e)
        {
            // получаем ссылку на кнопку, которая вызвала обработчик событий
            Button button = (Button)sender;

            TabPage tabPage = (TabPage)button.Parent;

            // находим дочерний элемент textbox на панели
            TextBox codeTextBox = (TextBox)tabPage.Controls["codeTextBox"];

            // теперь вы можете использовать ссылку на textbox, чтобы получить или задать его свойства
            
            if(codeTextBox.Text == code_global_xren)
            {
                GenerateChangePasswordBlock();
            } else
            {
                MessageBox.Show("Код подтверждения некорректный!");
            }
        }

        public void SendCodeByEmail(string recipientEmail, string code)
        {
            string fromEmail = ConfigurationManager.AppSettings["fromEmail"];
            string fromEmailPassword = ConfigurationManager.AppSettings["fromEmailPassword"];
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("MESSENGER", fromEmail));
            message.To.Add(new MailboxAddress("", recipientEmail));
            message.Subject = "Your verification code";


            message.Body = new TextPart("plain")
            {
                Text = "Ваш код подтверждения: " + code + " для смены пароля аккаунта в приложении MESSENGER."
            };
            

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Connect("smtp.office365.com", 587, SecureSocketOptions.StartTls);

                // Replace with your email and password.

                client.Authenticate(fromEmail, fromEmailPassword);

                client.Send(message);
                client.Disconnect(true);
            }
            MessageBox.Show("Код подтверждения отправлен на вашу почту!");
        }

        private string GenerateRandomCode(int length)
        {
            const string chars = "0123456789";
            var random = new Random();
            var result = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return result;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text != textBox2.Text.ToLower())
            {
                textBox2.Text = textBox2.Text.ToLower();
                textBox2.SelectionStart = textBox2.Text.Length;
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (textBox5.Text != textBox5.Text.ToLower())
            {
                textBox5.Text = textBox5.Text.ToLower();
                textBox5.SelectionStart = textBox5.Text.Length;
            }
        }
    }
}

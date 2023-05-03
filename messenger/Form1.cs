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

namespace messenger
{
    public partial class Form1 : Form
    {
        static string connectionString = "Server=localhost;Port=5432;Database=messenger;User Id=postgres;Password=regular123;";
        NpgsqlConnection conn = new NpgsqlConnection(connectionString);
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = Image.FromFile(openFileDialog.FileName);
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

        public bool ValidatePassword(string password)
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

        public bool ValidateEmail(string email)
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
            using (conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO users (username, password, email, age, photo) VALUES (@username, @password, @email, @age, @photo)";
                    cmd.Parameters.AddWithValue("username", login);
                    cmd.Parameters.AddWithValue("password", password);
                    cmd.Parameters.AddWithValue("email", email);
                    cmd.Parameters.AddWithValue("age", age);
                    cmd.Parameters.AddWithValue("photo", imageBytes);
                    cmd.ExecuteNonQuery();
                }
            }

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["IsRegistered"].Value = "true";
            config.Save(ConfigurationSaveMode.Modified);
            MessageBox.Show("Registration successful!");
            Application.Restart();
        }
        private byte[] GetImageBytesFromPictureBox(PictureBox pictureBox)
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
                MessageBox.Show("Логин должен содержать от 5 до 20 символов");
                return;
            }

            if (!ValidatePassword(password))
            {
                MessageBox.Show("Пароль должен содержать от 8 до 20 символов");
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
    }
}

using MailKit.Security;
using MimeKit;
using Npgsql;
using System;
using System.Configuration;
using System.Windows.Forms;

namespace messenger
{
    public static class AuthenticationManager
    {
        public static void RegisterUser(string login, string email, string password, int age, byte[] imageBytes)
        {
            string unique_id = GenerateUserId();
            using (NpgsqlConnection conn = new NpgsqlConnection(GlobalData.connectionString))
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
                InitUserOnlineStatus(unique_id, false);
            }

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["IsRegistered"].Value = "true";
            config.AppSettings.Settings["UserId"].Value = unique_id;
            config.Save(ConfigurationSaveMode.Modified);

            MessageBox.Show("Регистрация успешна!");
            Application.Restart();
        }

        public static string GenerateUserId()
        {
            NpgsqlConnection connection = new NpgsqlConnection(GlobalData.connectionString);
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

        public static void InitUserOnlineStatus(string userUniqueId, bool isOnline)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(GlobalData.connectionString))
            {
                conn.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO users_status (user_unique_id, isonline) VALUES (@userUniqueId, @isOnline)", conn))
                {
                    cmd.Parameters.AddWithValue("userUniqueId", userUniqueId);
                    cmd.Parameters.AddWithValue("isOnline", isOnline);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void SendCodeByEmail(string recipientEmail, string code)
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
        
        public static string GetConnectionString()
        {
            var uriString = ConfigurationManager.AppSettings["ELEPHANTSQL_URL"];
            var uri = new Uri(uriString);
            var db = uri.AbsolutePath.Trim('/');
            var user = uri.UserInfo.Split(':')[0];
            var passwd = uri.UserInfo.Split(':')[1];
            var port = uri.Port > 0 ? uri.Port : 5432;
            var connStr = string.Format("Server={0};Database={1};User Id={2};Password={3};Port={4}",
            uri.Host, db, user, passwd, port);
            return connStr;
        }
    }
}
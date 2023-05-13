using Npgsql;
using System;
using System.Windows.Forms;

namespace messenger
{
    public static class UserManager
    {
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
                NpgsqlConnection connection = new NpgsqlConnection(GlobalData.connectionString);
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
                        else
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
                using (NpgsqlConnection connection = new NpgsqlConnection(GlobalData.connectionString))
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
                NpgsqlConnection connection = new NpgsqlConnection(GlobalData.connectionString);
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
                NpgsqlConnection connection = new NpgsqlConnection(GlobalData.connectionString);
                connection.Open();

                using (var command = new NpgsqlCommand($"UPDATE users SET password='{newPassword}' WHERE unique_id='{this.UniqueId}'", connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            public void ChangeAge(int newAge)
            {
                NpgsqlConnection connection = new NpgsqlConnection(GlobalData.connectionString);
                connection.Open();

                using (var command = new NpgsqlCommand($"UPDATE users SET age='{newAge}' WHERE unique_id='{this.UniqueId}'", connection))
                {
                    command.ExecuteNonQuery();
                }
                this.Age = newAge;
            }

            public void ChangeEmail(string newEmail)
            {
                NpgsqlConnection connection = new NpgsqlConnection(GlobalData.connectionString);
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
                NpgsqlConnection connection = new NpgsqlConnection(GlobalData.connectionString);
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
                using (NpgsqlConnection connection = new NpgsqlConnection(GlobalData.connectionString))
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
        public static void SetUserOnlineStatus(string userUniqueId, bool isOnline)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(GlobalData.connectionString))
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

        public static bool GetUserOnlineStatus(string userUniqueId)
        {
            bool isOnline = false;

            using (NpgsqlConnection conn = new NpgsqlConnection(GlobalData.connectionString))
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
        public static void AddUserIdToArrayChatsWithUsers(string where_user_id, string what_user_id)
        {
            NpgsqlConnection conn;
            NpgsqlCommand cmd;
            conn = new NpgsqlConnection(GlobalData.connectionString);
            conn.Open();
            cmd = new NpgsqlCommand("UPDATE users SET chats_with_users_id = array_append(chats_with_users_id, @what_user_id) WHERE unique_id = @where_user_id", conn);
            cmd.Parameters.AddWithValue("where_user_id", where_user_id);
            cmd.Parameters.AddWithValue("what_user_id", what_user_id);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteAccountUser()
        {
            DialogResult result = MessageBox.Show("Вы действительно хотите удалить свой аккаунт? Вы не сможете его восстановить", "Вы действительно хотите удалить свой аккаунт?", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                GlobalData.user.DeleteUser();
                Utils.ExitFromApp();
            }
            else if (result == DialogResult.Cancel)
            {
            }
        }
    }
}
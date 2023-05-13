using Npgsql;
using System;
using System.Net.Mail;
using System.Windows.Forms;

namespace messenger
{
    public static class Validator
    {
        public static bool ValidateLogin(string login)
        {
            // ��������� ����� ������
            if (login.Length < 3 || login.Length > 20)
            {
                return false;
            }

            // ��������� ������� ������
            foreach (char c in login)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    return false;
                }
            }

            // ���� ��� �������� ��������, ���������� true
            return true;
        }

        public static bool ValidatePassword(string password)
        {
            // ��������� ����� ������
            if (password.Length < 8 || password.Length > 20)
            {
                return false;
            }

            // ��������� ������� �������� ������ �����
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

            // ���� �� ������� �������� ���� �����, ���������� false
            if (!hasUpper || !hasLower || !hasDigit || !hasSpecial)
            {
                return false;
            }

            // ���� ��� �������� ��������, ���������� true
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
            if (age >= 0 && age <= 150)
            {
                return true;
            }
            return false;
        }

        public static bool IsUsernameUnique(string username)
        {
            bool isUnique = false;
            using (NpgsqlConnection conn = new NpgsqlConnection(GlobalData.connectionString))
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

        public static bool IsEmailUnique(string email)
        {
            bool isUnique = false;
            using (NpgsqlConnection conn = new NpgsqlConnection(GlobalData.connectionString))
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
    }
}
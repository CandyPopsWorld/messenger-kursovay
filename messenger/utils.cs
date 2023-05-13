using System;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace messenger
{
    public static class Utils
    {
        public static string GenerateRandomCode(int length)
        {
            const string chars = "0123456789";
            var random = new Random();
            var result = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return result;
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

        public static void CopyToClipboard(string text)
        {
            Clipboard.SetText(text);
        }

        public static void ExitFromApp()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["IsRegistered"].Value = "false";
            config.AppSettings.Settings["UserId"].Value = "";
            config.Save(ConfigurationSaveMode.Modified);
            Application.Restart();
        }
    }
}
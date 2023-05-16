using System.Collections.Generic;
using System.Configuration;
using System.Windows.Forms;
using static messenger.Form2;

namespace messenger
{
    public static class GlobalData
    {
        //public static string connectionString = AuthenticationManager.GetConnectionString();
        public static string connectionString = ConfigurationManager.AppSettings["connectionString"];
        public static string fromEmail = ConfigurationManager.AppSettings["fromEmail"];
        public static string fromEmailPassword = ConfigurationManager.AppSettings["fromEmailPassword"];

        public static string code_global_xren = "";
        public static string email_user_ch_pass = "";
        public static Control[] tabPageForgetPasswordControls;

        public static string userId = ConfigurationManager.AppSettings["UserId"];

        public static UserManager.User user;
        public static UserManager.User globalAdditionalUser;
        public static ChatManager.Chats chats;

        public static MessageManager.Message[] globalMessages;
        public static List<string> globalChats;

        public static bool globalStatusAdditionalUser = false;
        public static List<string> hiddenChats;
        public static string currentOpenChatId = "";

        public static Timer timer = new Timer();
        public static Timer chatTimer = new Timer();
        public static Timer statusTimer = new Timer();
    }
}
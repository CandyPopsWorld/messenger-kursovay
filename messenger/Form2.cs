using Npgsql;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace messenger
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            GlobalData.user = new UserManager.User();
            GlobalData.globalAdditionalUser = new UserManager.User();
            GlobalData.chats = new ChatManager.Chats();
            GlobalData.user.LoadFromDatabase(GlobalData.userId); // Отображение метод LoadFromDatabase для получения данных из базы данных и заполнения полей класса
            GlobalData.hiddenChats = ChatManager.GetHiddenChats();

            UserManager.SetUserOnlineStatus(GlobalData.user.UniqueId, true);
            DisplayUserData(GlobalData.user); // Отображение полученные данные на форме
            InitLoadAllChaUser();
            DisplayHiddenChats();


            if (GlobalData.chatTimer.Enabled)
            {
                GlobalData.timer.Stop();
                GlobalData.chatTimer.Stop();
            }
            GlobalData.chatTimer = new Timer();
            GlobalData.chatTimer.Tick += (sender, e) => LoadAllChatsUser();
            GlobalData.chatTimer.Interval = 3000;
            GlobalData.chatTimer.Start();
        }

        public void DisplayHiddenChats()
        {
            if (GlobalData.hiddenChats.Count > 0)
            {
                foreach (string hiddenChatId in GlobalData.hiddenChats)
                {
                    string additionalUserId = UserManager.GetUserIdChat(hiddenChatId);
                    UserManager.User additionalUser = new UserManager.User();
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

        public void CreateHiddenChatWidget(UserManager.User additionalUser, string hiddenChatId)
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
            showChatBtn.Click += (sender, e) => ChatManager.RemoveHiddenChat(hiddenChatId);

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

        public void LoadAllChatsUser()
        {
            List<string> chatsIds = ChatManager.GetAllChatsIds();
            chatsIds.Reverse();

            if (chatsIds.Count == GlobalData.globalChats.Count)
            {
                return;
            }
            panel3.Controls.Clear();
            GlobalData.globalChats = chatsIds;
            foreach (string chatId in chatsIds)
            {
                bool isChatIdExist = GlobalData.hiddenChats.Contains(chatId);
                if (!isChatIdExist)
                {
                    //panel3.Controls.Clear();
                    string additionalUserId = UserManager.GetUserIdChat(chatId);
                    UserManager.User additionalUser = new UserManager.User();
                    additionalUser.LoadFromDatabase(additionalUserId);
                    CreateNewChatPanel(additionalUser, chatId);
                }
            }
        }

        public void InitLoadAllChaUser()
        {
            List<string> chatsIds = ChatManager.GetAllChatsIds();
            chatsIds.Reverse();
            GlobalData.globalChats = chatsIds;

            bool equalsHidden = false;
            int countEqualsHiddenAndChats = 0;
            
            foreach (var chatId in chatsIds)
            {
                foreach (var hiddenId in GlobalData.hiddenChats)
                {
                    if(chatId == hiddenId)
                    {
                        countEqualsHiddenAndChats++;
                    }
                }
            }
            if(countEqualsHiddenAndChats == chatsIds.Count && chatsIds.Count !=0)
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
                bool isChatIdExist = GlobalData.hiddenChats.Contains(chatId);
                if (!isChatIdExist)
                {
                string additionalUserId = UserManager.GetUserIdChat(chatId);

                UserManager.User additionalUser = new UserManager.User();
                additionalUser.LoadFromDatabase(additionalUserId);
                CreateNewChatPanel(additionalUser, chatId);
                }
            }
        }

        public void CreateNewChatPanel(UserManager.User additionalUser, string chatId)
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
            
            if(unique_id != null)
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

        public void OutChatIsEmpty(MessageManager.Message[] messages)
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

        public void ClickToChatWidget(UserManager.User additionalUser, string chatId)
        {
            if(GlobalData.currentOpenChatId == chatId)
            {
                MessageBox.Show("Данный чат уже открыт!");
                return;
            }


            GlobalData.currentOpenChatId = chatId;

            panel5.Controls.Clear();

            MessageManager.Message[] messages = MessageManager.GetAllMessages(chatId);
            GlobalData.globalMessages = messages;

            foreach (MessageManager.Message message in messages)
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
            downloadChatHistoryBtn.Click += (sender, e) => ChatManager.DownloadChooseFolder(chatId);
            downloadChatHistoryBtn.Width = 210;
            downloadChatHistoryBtn.Height = 30;
            downloadChatHistoryBtn.Location = new Point(380, 50);

            System.Windows.Forms.Button hideChatBtn = new System.Windows.Forms.Button();
            hideChatBtn.Text = "Скрыть чат";
            hideChatBtn.Click += (sender, e) => ChatManager.HideChat(chatId, additionalUser.Username);
            hideChatBtn.Width = 210;
            hideChatBtn.Height = 30;
            hideChatBtn.Location = new Point(380, 20);

            System.Windows.Forms.Button deleteChatBtn = new System.Windows.Forms.Button();
            if (unique_id == null || unique_id.Length == 0 || unique_id == "null")
            {
                deleteChatBtn.Text = "Удалить чат!";
                deleteChatBtn.Click += (sender, e) => ChatManager.DeleteChatDialog(chatId);
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
                if (UserManager.GetUserOnlineStatus(unique_id))
                {
                    GlobalData.globalStatusAdditionalUser = true;
                    statusLabel.Text = "(ONLINE)";
                    statusLabel.ForeColor = Color.Green;
                }
                else
                {
                    GlobalData.globalStatusAdditionalUser = false;
                    statusLabel.Text = "(OFFLINE)";
                    statusLabel.ForeColor = Color.Red;
                }
                statusLabel.Width = 150;
                statusLabel.Font = new Font("Arial", 9, FontStyle.Bold);
                statusLabel.Location = new Point(50, 30);
                chatPanel.Controls.Add(statusLabel);

                if (GlobalData.statusTimer.Enabled)
                {
                    GlobalData.statusTimer.Stop();
                }
                GlobalData.statusTimer = new Timer();
                GlobalData.statusTimer.Tick += (sender, e) => status_Tick(sender, e, unique_id, statusLabel);
                GlobalData.statusTimer.Interval = 1000; // 1 секунду
                GlobalData.statusTimer.Start();
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

            if(unique_id == null || unique_id.Length == 0 || unique_id == "null")
            {
                panel2.Controls.Add(deleteChatBtn);
            } else
            {
                panel2.Controls.Add(hideChatBtn);
            }


            //ListenForNewMessages(chatId);
            if (GlobalData.timer.Enabled) {
                GlobalData.timer.Stop();
            }
            GlobalData.timer = new Timer();
            GlobalData.timer.Tick += (sender, e) => timer_Tick(sender, e, chatId);
            GlobalData.timer.Interval = 1000; // 1 секунду
            GlobalData.timer.Start();
        }

        private void status_Tick(object sender, EventArgs e, string additionalUniqueId, System.Windows.Forms.Label statusLabel)
        {
            bool status = UserManager.GetUserOnlineStatus(additionalUniqueId);

            if(status == GlobalData.globalStatusAdditionalUser)
            {
                return;
            }
            GlobalData.globalStatusAdditionalUser = status;

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
            MessageManager.Message[] messages = MessageManager.GetAllMessages(chatId);
            
            int lastMessageIndex = messages.Length - 1;

            bool equal = true;

            if(messages.Length > 0 && panel5.Controls.Find("notFoundMessages", true).Length > 0){
                panel5.Controls.Clear();
            }
           
            if(messages.Length == GlobalData.globalMessages.Length) { 
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
                    if (messages[i].Text == GlobalData.globalMessages[i].Text)
                    {

                    } else
                    {
                        equalText = false;
                    }
                }

                if(equalText == false)
                {
                    GlobalData.globalMessages = messages;
                    panel5.Controls.Clear();
                    foreach (MessageManager.Message message in messages)
                    {
                        CreateWidgetMessage(message);
                    }
                }

            } else {
                if(messages.Length > GlobalData.globalMessages.Length)
                {
                    GlobalData.globalMessages = messages;
                    //panel5.Controls.Clear();
                    foreach (MessageManager.Message message in messages)
                    {
                        if (Array.IndexOf(messages, message) == lastMessageIndex)
                        {
                            // Это последняя итерация
                            CreateWidgetMessage(message);
                        }

                    }
                } else
                {
                    GlobalData.globalMessages = messages;
                    panel5.Controls.Clear();
                    foreach (MessageManager.Message message in messages)
                    {
                        CreateWidgetMessage(message);
                    }
                }

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
                await MessageManager.CreateMessage(message, reciever_unique_id, chatId);
                messageTextBox.Text = "";
            }
        }

        public void CreateWidgetMessage(MessageManager.Message newMessage)
        {
            string messageText = newMessage.Text;
            string messageSendDate = newMessage.TimeSent.ToString();
            string senderId = newMessage.SenderId;
            string recieverId = newMessage.ReceiverId;

            bool isSenderMainUser = false;

            if (senderId == GlobalData.user.UniqueId)
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
            MessageManager.DeleteMessage(GlobalData.currentOpenChatId, MessageUniqueId);
            panel5.Controls.Clear();

            MessageManager.Message[] messages = MessageManager.GetAllMessages(GlobalData.currentOpenChatId);
            GlobalData.globalMessages = messages;
            panel5.Controls.Clear();
            foreach (MessageManager.Message message1 in messages)
            {
                CreateWidgetMessage(message1);
            }
        }
        public void ChangeMessageMenuClick(MessageManager.Message message)
        {
            EditMessageMenuClick(message);
        }

        public void CopyMessageMenuClick(string message)
        {
            Utils.CopyToClipboard(message);
        }

        private void EditMessageMenuClick(MessageManager.Message message)
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
                MessageManager.UpdateMessageText(GlobalData.currentOpenChatId, message.MessageUniqueId, message.Text);

                panel5.Controls.Clear();

                MessageManager.Message[] messages = MessageManager.GetAllMessages(GlobalData.currentOpenChatId);
                GlobalData.globalMessages = messages;
                foreach (MessageManager.Message message1 in messages)
                {
                    CreateWidgetMessage(message1);
                }
            }
        }

        public void DisplayUserData(UserManager.User user)
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
            ElementHelper.SetImageFromBytes(user.Photo, pictureBox2);
            // отображаем данные пользователя в настройках
            label28.Text = user.Username;
            textBox4.Text = user.Email;
            numericUpDown1.Value = user.Age;
            FillPasswordTextBoxLengthSymbols();
            ElementHelper.SetImageFromBytes(user.Photo, pictureBox7);
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
            int passwordLength = GlobalData.user.GetPasswordLengthFromDatabase();
            textBox5.Text = new String('*', passwordLength);
        }

        public class Notification
        {
            private Timer timer;
            private System.Windows.Forms.Label label;
            //private NotificationType _type;

            public Notification(string text, int duration, Control parentControl, Enums.NotificationType type, Point location)
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
                    case Enums.NotificationType.Error:
                        label.ForeColor = Color.White;
                        label.BackColor = Color.Red;
                        label.BorderStyle = BorderStyle.Fixed3D;
                        break;
                    case Enums.NotificationType.Warning:
                        label.ForeColor = Color.Black;
                        label.BackColor = Color.Yellow;
                        break;
                    case Enums.NotificationType.Info:
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

        private void button1_Click(object sender, EventArgs e)
        {
            Utils.ExitFromApp();
        }

        async public void create_chat_with_user(string unique_id_receiver)
        {
            if(GlobalData.user.UniqueId == unique_id_receiver)
            {
                MessageBox.Show("Вы не можете написать самому себе!");
                return;
            }
            //Очищаем вводы и выводы пользователей
            panel9.Controls.Clear();
            textBox3.Clear();

            //Проверка существует ли уже чат между пользователями
            if(ChatManager.IsChatIdInArray(GlobalData.user.UniqueId, unique_id_receiver))
            {
                GlobalData.globalAdditionalUser.LoadFromDatabase(unique_id_receiver);
                ClickToChatWidget(GlobalData.globalAdditionalUser, ChatManager.GetChatId(GlobalData.user.UniqueId, unique_id_receiver));
                tabControl1.SelectedTab = tabControl1.TabPages[0];
                return;
            }
            //Создаем обьект класса чата
            ChatManager.Chat newChat = new ChatManager.Chat();
            newChat.Chat_Unique_Id = newChat.GenerateChatId();
            newChat.User1_Id = GlobalData.user.UniqueId;
            newChat.User2_Id = unique_id_receiver;

            GlobalData.chats.Add(newChat);
            //Записываем в базу данных
            newChat.AddChatToDatabase();
            //Добавляем chat_unique_id в поле chats[] в таблице users

            ChatManager.AddChatUniqueIdToUserArray(newChat, GlobalData.user.UniqueId);
            ChatManager.AddChatUniqueIdToUserArray(newChat, unique_id_receiver);

            UserManager.AddUserIdToArrayChatsWithUsers(GlobalData.user.UniqueId, unique_id_receiver);
            UserManager.AddUserIdToArrayChatsWithUsers(unique_id_receiver, GlobalData.user.UniqueId);

            string messageCreate = "(" + GlobalData.user.Username + " создал чат)";
            await MessageManager.CreateMessage(messageCreate, unique_id_receiver, newChat.Chat_Unique_Id);

            //MessageBox.Show("Чат создан!");
            
            LoadAllChatsUser();

            // Переключаемся в меню чатов
            tabControl1.SelectedTab = tabControl1.TabPages[0];

            GlobalData.globalAdditionalUser.LoadFromDatabase(unique_id_receiver);
            ClickToChatWidget(GlobalData.globalAdditionalUser, newChat.Chat_Unique_Id);

        }

        public void SearchUsers(string searchText)
        {
            bool foundUsers = false;
            string query = "SELECT username, age, photo, unique_id FROM users WHERE username LIKE @searchText LIMIT 5;";
            using (NpgsqlConnection connection = new NpgsqlConnection(GlobalData.connectionString))
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
                            if(unique_id == GlobalData.user.UniqueId)
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
                            Notification notification = new Notification("Пользователя с таким именем не существует!", 3000, panel9, Enums.NotificationType.Warning, new Point(250, 50));
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
            UserManager.DeleteAccountUser();
        }
        private void button7_Click(object sender, EventArgs e)
        {
            changeDataUserModal("Введите новую почту", Enums.ChangeDataUserModalType.Email, GlobalData.user);
        }
        private void button8_Click(object sender, EventArgs e)
        {
            changeDataUserModal("Введите новый возраст", Enums.ChangeDataUserModalType.Age, GlobalData.user);

        }

        private void button9_Click(object sender, EventArgs e)
        {
            changeDataUserModal("Введите новый пароль", Enums.ChangeDataUserModalType.Password, GlobalData.user);
        }

        private static void changeDataUserModal(string nameForm, Enums.ChangeDataUserModalType parametrData, UserManager.User user)
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
                if (parametrData == Enums.ChangeDataUserModalType.Email || parametrData == Enums.ChangeDataUserModalType.Password) {
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
                        case Enums.ChangeDataUserModalType.Email:
                            if (textBox.Text.Length > 0)
                            {
                                if (Validator.ValidateEmail(textBox.Text))
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
                        case Enums.ChangeDataUserModalType.Password:
                            if(textBox.Text.Length > 0)
                            {
                                if (Validator.ValidatePassword(textBox.Text))
                                {
                                    user.ChangePassword(textBox.Text);
                                    MessageBox.Show("Пароль изменен, войдите в систему с новым паролем!");
                                    Utils.ExitFromApp();
                                    break;
                                } else
                                {
                                    MessageBox.Show("Пароль должен содержать от 8 до 20 символов. Необходимо использовать: латиниские буквы(хотя бы одну в верхнем и нижнем регистре), хотя бы 1 цифру, спец.символ");
                                }
                            }
                            MessageBox.Show("Поле ввода пустое!"); break;
                        case Enums.ChangeDataUserModalType.Age:
                            if (Validator.ValidateAge(Convert.ToInt32(numericUpDown.Value)))
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
            ElementHelper.OpenPhotoAndAddPhotoToPictureBox(pictureBox7);
            GlobalData.user.ChangePhoto(ElementHelper.GetImageBytesFromPictureBox(pictureBox7));
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
            UserManager.SetUserOnlineStatus(GlobalData.user.UniqueId, false);
        }
    }
}
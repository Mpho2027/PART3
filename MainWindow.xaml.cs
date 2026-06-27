using System.Windows;

namespace taskChatt
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter your name to continue.", "Input Required",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Session.UserName = name;

            ChatbotWindow chatbot = new ChatbotWindow();
            chatbot.Show();
            this.Close();
        }
    }
}

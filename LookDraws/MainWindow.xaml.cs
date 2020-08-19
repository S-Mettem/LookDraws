using System.Windows;

namespace LookDraws
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ChooseFolderWindow chooseFolderWindow = new ChooseFolderWindow(this);
            chooseFolderWindow.Show();
            this.Visibility = Visibility.Hidden;
        }
    }
}

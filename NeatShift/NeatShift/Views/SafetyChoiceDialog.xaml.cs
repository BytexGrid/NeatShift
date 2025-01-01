using System.Windows;
using NeatShift.ViewModels;

namespace NeatShift.Views
{
    public partial class SafetyChoiceDialog : Window
    {
        public SafetyChoiceViewModel ViewModel { get; }
        public new bool? DialogResult { get; private set; }

        public SafetyChoiceDialog()
        {
            InitializeComponent();
            ViewModel = new SafetyChoiceViewModel();
            DataContext = ViewModel;
            Owner = Application.Current.MainWindow;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        public new bool? ShowDialog()
        {
            base.ShowDialog();
            return DialogResult;
        }
    }
} 
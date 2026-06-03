using System;
using System.Windows;
using System.Windows.Controls;
using Mp3TagFree.ViewModels;

namespace Mp3TagFree
{
    /// <summary>
    /// Interaction logic for RenameWindow.xaml
    /// </summary>
    public partial class RenameWindow : Window
    {
        public RenameWindow(RenameViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void VariableButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is RenameViewModel vm && sender is Button button && button.Tag is string tag)
            {
                int caretIndex = PatternTextBox.CaretIndex;
                string currentText = vm.Pattern ?? string.Empty;

                // Insert the variable token at caret index
                vm.Pattern = currentText.Insert(caretIndex, tag);

                // Restore caret index and focus the textbox
                PatternTextBox.CaretIndex = caretIndex + tag.Length;
                PatternTextBox.Focus();
            }
        }

        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is RenameViewModel vm)
            {
                bool success = vm.ExecuteRename();
                if (success)
                {
                    MessageBox.Show("파일명 변경을 완료했습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("일부 파일의 이름을 변경하는 도중 오류가 발생했습니다. 미리보기 목록의 상태를 확인해 주세요.", 
                        "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

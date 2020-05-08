using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TaskManager.View
{
    /// <summary>
    /// Interaction logic for NewTaskWindow.xaml
    /// </summary>
    public partial class NewTaskWindow : Window
    {
        public NewTaskWindow()
        {
            InitializeComponent();
        }

        private void Ok_Button_Click(object sender, RoutedEventArgs e)
        {
            bool isValid = true;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Не все поля заполнены корректно:\r\n");
            if (string.IsNullOrWhiteSpace(TaskName.Text))
            {
                isValid = false;
                stringBuilder.Append("-Не указано наименование задачи\r\n");
            }
            if (string.IsNullOrWhiteSpace(AwaitedResult.Text))
            {
                isValid = false;
                stringBuilder.Append("-Не указан ожидаемый результат\r\n");
            }
            if (string.IsNullOrWhiteSpace(CommentToTask.Text))
            {
                CommentToTask.Text = "*";
            }
            if (!Reporter.ItemsSource.Contains(Reporter.Text))
            {
                isValid = false;
                stringBuilder.Append("-Указанный инициатор не найден в списке сотрудников\r\n");
            }
            if ((Assignee.Visibility == Visibility.Visible && 
                Assignee.SelectedItems.Count == 0) 
                    ||
                (AssigneeSingle.Visibility == Visibility.Visible && 
                !AssigneeSingle.ItemsSource.Contains(AssigneeSingle.Text)))
            {
                isValid = false;
                stringBuilder.Append("-Не выделено ни одного ответственного сотрудника\r\n");
            }
            if (string.IsNullOrWhiteSpace(Metric.Text))
            {
                isValid = false;
                stringBuilder.Append("-Не указана метрика\r\n");
            }
            if(CreationDate.SelectedDate > DueDate.SelectedDate)
            {
                isValid = false;
                stringBuilder.Append("-Дата создания задачи больше срока исполнения задачи\r\n");
            }
            if (!isValid)
            {
                MessageBox.Show(stringBuilder.ToString(), "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                DialogResult = true;
                Close();
            }
            
        }
    }
}

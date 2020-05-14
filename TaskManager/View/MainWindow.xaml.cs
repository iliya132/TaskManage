using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TaskManager.Services;

namespace TaskManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void EditReporterCommentButton_Click(object sender, RoutedEventArgs e)
        {
            StartEditReporterComment();
        }

        private void EditDonePercentageButton_Click(object sender, RoutedEventArgs e)
        {
            StartEditDonePercentage();
        }

        private void EditAssigneeCommentButton_Click(object sender, RoutedEventArgs e)
        {
            StartEditAssigneeComment();
        }

        private void TaskList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StopEditReporterComment();
            StopEditAssigneeComment();
        }

        private void DoneReporterBtnClick(object sender, RoutedEventArgs e)
        {
            StopEditReporterComment();
        }

        private void DoneAssigneeBtnClick(object sender, RoutedEventArgs e)
        {
            StopEditAssigneeComment();
        }

        private void DonePercentageBtnClick(object sender, RoutedEventArgs e)
        {
            StopEditDonePercent();
        }

        private void StartEditReporterComment()
        {
            ReporterCommentTextBox.IsEnabled = true;
            EditButtonReporter.IsEnabled = false;
            ReportCommentEditApprove.Visibility = Visibility.Visible;
            ReportCommentEditDenie.Visibility = Visibility.Visible;
        }

        private void StartEditDonePercentage()
        {
            EditDonePercentage.IsEnabled = false;
            DonePercentSlider.IsEnabled = true;
            DonePercentageEditApprove.Visibility = Visibility.Visible;
            DonePercentageEditDenie.Visibility = Visibility.Visible;
        }

        private void StartEditAssigneeComment()
        {
            AssigneeCommentTextBox.IsEnabled = true;
            EditButtonAssignee.IsEnabled = false;
            AssigneeCommentEditApprove.Visibility = Visibility.Visible;
            AssigneeCommentEditDenie.Visibility = Visibility.Visible;
        }

        private void StopEditReporterComment()
        {
            if (ReporterCommentTextBox != null)
            {
                ReporterCommentTextBox.IsEnabled = false;
                EditButtonReporter.IsEnabled = true;
                ReportCommentEditApprove.Visibility = Visibility.Collapsed;
                ReportCommentEditDenie.Visibility = Visibility.Collapsed;
            }
        }

        private void StopEditAssigneeComment()
        {
            if (AssigneeCommentTextBox != null)
            {
                AssigneeCommentTextBox.IsEnabled = false;
                EditButtonAssignee.IsEnabled = true;
                AssigneeCommentEditApprove.Visibility = Visibility.Collapsed;
                AssigneeCommentEditDenie.Visibility = Visibility.Collapsed;
            }
        }

        private void StopEditDonePercent()
        {
            if (DonePercentSlider != null)
            {
                DonePercentSlider.IsEnabled = false;
                EditDonePercentage.IsEnabled = true;
                DonePercentageEditApprove.Visibility = Visibility.Collapsed;
                DonePercentageEditDenie.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateService.UpdateIfNewer();

            if (IsUpdateFileExist())
            {
                DeleteUpdateFile();
                ShowHelpFile(null, null);
            }
        }


        private bool IsUpdateFileExist()
        {
            if (File.Exists("updated.txt"))
            {
                return true;
            } 
            return false;
        }

        private void DeleteUpdateFile()
        {
            File.Delete("updated.txt");
        }

        private void ShowHelpFile(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists($"{Environment.ExpandEnvironmentVariables("%appdata%")}\\TasksDK"))
            {
                Directory.CreateDirectory($"{Environment.ExpandEnvironmentVariables("%appdata%")}\\TasksDK");
            }
            File.Copy("\\\\moscow\\hdfs\\WORK\\Архив необычных операций\\ОРППА\\Programs\\Tasks\\Help\\TaskHelp.chm", $"{Environment.ExpandEnvironmentVariables("%appdata%")}\\TasksDK\\TaskHelp.chm", true);
            System.Diagnostics.Process.Start($"{Environment.ExpandEnvironmentVariables("%appdata%")}\\TasksDK\\TaskHelp.chm");
        }
    }
}

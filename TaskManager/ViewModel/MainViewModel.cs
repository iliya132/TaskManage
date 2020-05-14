using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TaskManager.Model.Context;
using TaskManager.Model.Entities;
using TaskManager.Model.Entities.TimeSheet;
using TaskManager.View;

namespace TaskManager.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Технические значения
        private delegate void OnSelectionChanged();
        private event OnSelectionChanged SelectionChanged;
        #endregion

        #region Данные
        TaskContext _taskContext = new TaskContext();
        TimeSheetContext _timeSheetContext = new TimeSheetContext();
        public ObservableCollection<TaskEntity> AllLoadedTasks { get; set; }
        private TaskEntity _nullTask { get; set; }
        public List<Process> AllProcessesFromTimeSheet { get; set; }
        public ObservableCollection<Block> AllBlocksFromTimeSheet { get; set; }
        public ObservableCollection<BlockProxy> BlockProxies { get; set; }
        public ObservableCollection<Employee> CurrentUserSubordinatedEmployees { get; set; }
        #endregion

        #region Выбранные значения
        private TaskEntity _selectedTask = new TaskEntity();
        public TaskEntity SelectedTask
        {
            get
            {
                return _selectedTask;
            }
            set
            {
                _selectedTask = value;
                SelectionChanged?.Invoke();
            }
        }
        public string CurrentlyShownTaskPath
        {
            get
            {
                if (CurrentlyShownTask != null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append("Текущая задача:");
                    Stack<string> path = new Stack<string>();
                    TaskEntity task = CurrentlyShownTask;
                    path.Push($"/{task.Name}");
                    while (task.ParentTask != null && task.ParentTask != _nullTask)
                    {
                        task = task.ParentTask;
                        path.Push($"/{task.Name}");
                    }
                    while (path.Count > 0)
                    {
                        stringBuilder.Append(path.Pop());
                    }
                    return stringBuilder.ToString();
                }
                else return string.Empty;
            }
        }
        #endregion

        #region Текущие значения
        public TaskEntity CurrentlyShownTask { get; set; }
        public TaskEntity CurrentlyEditedTask { get; set; }
        private List<TaskEntity> _currentlyCuttedTasks { get; set; }
        private Analytic _currentAnalytic { get; set; }
        private Employee _currentEmployee { get; set; }
        public ObservableCollection<Employee> AddFormSelectedAnalytics { get; set; }
        public ObservableCollection<Process> AddFormSelectedProcesses { get; set; }
        public ObservableCollection<TaskEntity> AddFormAvailableParentTasks
        {
            get
            {
                return new ObservableCollection<TaskEntity>(AllLoadedTasks.Where(task=>task.ParentTask==null));
            }
        }
        public ObservableCollection<string> SubordinatedEmployeesNames { get; set; }
        private List<TaskEntity> _selectedTasks { get; set; }
        public string AddedTaskReporterFIO { get; set; }
        public string AddedTaskAssigneeFIO { get; set; }
        public string CurrentTaskProcesses
        {
            get
            {
                if (SelectedTask != null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (ProcessProxy processProxy in SelectedTask.Processes)
                    {
                        stringBuilder.Append($"{AllProcessesFromTimeSheet.FirstOrDefault(process => process.id == processProxy.ProcessId)}\r\n");
                    }
                    return stringBuilder.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        public int SelectedTabTag { get; set; }
        private int DonePercentageCache { get; set; }
        private string ReporterCommentCache { get; set; }
        private string AssigneeCommentCache { get; set; }

        public Visibility ReporterCommentEditVisibility
        {
            get
            {

                if (SelectedTask != null && SelectedTask.Reporter?.AnalyticId == _currentEmployee.AnalyticId || _currentAnalytic.RoleTableId==5)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility AssigneeCommentEditVisibility
        {
            get
            {
                if (SelectedTask != null && SelectedTask.Assignee?.AnalyticId == _currentEmployee.AnalyticId || _currentAnalytic.RoleTableId == 5)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility ReporterOrAssigneeEditVisibility
        {
            get
            {
                if (_currentAnalytic.RoleTableId == 5 || SelectedTask != null &&
                    (SelectedTask.Assignee?.AnalyticId == _currentEmployee.AnalyticId ||
                    SelectedTask.Reporter?.AnalyticId == _currentEmployee.AnalyticId))
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility GoToParentVisibility
        {
            get
            {
                if (!IsBlockSelectionActive)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        private bool _isBlockSelectionActive;
        public bool IsBlockSelectionActive
        {
            get { return _isBlockSelectionActive; }
            set
            {
                _isBlockSelectionActive = value;
                RaisePropertyChanged(nameof(IsBlockSelectionActive));
                RaisePropertyChanged(nameof(GoToParentVisibility));
            }
        }
        #endregion

        #region Команды
        public RelayCommand<TaskEntity> ShowTaskChilds { get; set; }
        public RelayCommand<BlockProxy> SelectBlock { get; private set; }
        public RelayCommand GoToParentCommand { get; set; }
        public RelayCommand<TaskEntity> AddTaskCommand { get; set; }
        public RelayCommand<TaskEntity> EditTaskCommand { get; set; }
        public RelayCommand<TaskEntity> DeleteTaskCommand { get; set; }
        public RelayCommand<TaskEntity> CopyTaskCommand { get; set; }
        public RelayCommand CutTaskCommand { get; set; }
        public RelayCommand<TaskEntity> PasteTaskCommand { get; set; }
        public RelayCommand<int> TabChange { get; set; }
        public RelayCommand<System.Collections.IList> StoreSelectedTasks { get; set; }
        public RelayCommand<string> RememberOldValuesCommand { get; set; }
        public RelayCommand<string> AcceptChangesOnComment { get; set; }
        public RelayCommand<string> DeclineChangesOnComment { get; set; }
        #endregion

        public MainViewModel()
        {
            InitializeData();
            InitializeCommands();
            InitializeEvents();
        }

        private void InitializeData()
        {
            AllLoadedTasks = new ObservableCollection<TaskEntity>(_taskContext.Tasks.
                Include("ChildTasks").
                Include("Assignee").
                Include("Reporter").
                Include("ParentTask").
                Include("Processes").
                OrderBy(i => i.Name).
                ToList());
            _nullTask = new TaskEntity
            {
                ChildTasks = new ObservableCollection<TaskEntity>(AllLoadedTasks.Where(task => task.ParentTask == null)),
                Name = "Отсутствует"
            };
            AllLoadedTasks.Add(_nullTask);
            CurrentlyShownTask = _nullTask;
            AllProcessesFromTimeSheet = new List<Process>(_timeSheetContext.Process);
            List<Analytic> allAnalytics = _timeSheetContext.Analytics.ToList();//TODO Добавить логику отбора подчиненных сотрудников
            CurrentUserSubordinatedEmployees = new ObservableCollection<Employee>();
            _selectedTasks = new List<TaskEntity>();
            _currentlyCuttedTasks = new List<TaskEntity>();
            foreach (Analytic analytic in allAnalytics.OrderBy(i=>i.LastName))
            {
                CurrentUserSubordinatedEmployees.Add(new Employee
                {
                    AnalyticId = analytic.Id,
                    Name = $"{analytic.LastName} {analytic.FirstName} {analytic.FatherName}"
                });
            }
            SubordinatedEmployeesNames = new ObservableCollection<string>(
            CurrentUserSubordinatedEmployees.Select(employee => employee.Name));
            AddFormSelectedAnalytics = new ObservableCollection<Employee>();
            AddFormSelectedProcesses = new ObservableCollection<Process>();
            AllBlocksFromTimeSheet = new ObservableCollection<Block>(_timeSheetContext.Blocks.ToList());

            BlockProxies = new ObservableCollection<BlockProxy>();
            FillBlocksProxyWithTasks(AllLoadedTasks);
            IsBlockSelectionActive = true;

            #region Определение текущего пользователя
            _currentAnalytic = _timeSheetContext.Analytics.FirstOrDefault(analytic => analytic.userName.ToLower().Equals(Environment.UserName.ToLower()));
            _currentEmployee = _taskContext.Employees.FirstOrDefault(employee => employee.AnalyticId ==  _currentAnalytic.Id);
            if (_currentEmployee == null)
            {
                _currentEmployee = new Employee
                {
                    Name = $"{_currentAnalytic.LastName} {_currentAnalytic.FirstName} {_currentAnalytic.FatherName}",
                    AnalyticId = _currentAnalytic.Id
                };
            }
            #endregion
        }

        private void InitializeCommands()
        {
            ShowTaskChilds = new RelayCommand<TaskEntity>(ShowTaskChildsMethod);
            SelectBlock = new RelayCommand<BlockProxy>(SelectBlockMethod);
            GoToParentCommand = new RelayCommand(GotoParent);
            AddTaskCommand = new RelayCommand<TaskEntity>(AddNewTask);
            EditTaskCommand = new RelayCommand<TaskEntity>(EditTask);
            DeleteTaskCommand = new RelayCommand<TaskEntity>(DeleteTask);
            TabChange = new RelayCommand<int>(ChangeShownTask);
            StoreSelectedTasks = new RelayCommand<System.Collections.IList>(StoreSelection);
            CutTaskCommand = new RelayCommand(CutTask);
            PasteTaskCommand = new RelayCommand<TaskEntity>(PasteTask);
            RememberOldValuesCommand = new RelayCommand<string>(RememberOldCommentValues);
            AcceptChangesOnComment = new RelayCommand<string>(AcceptChanges);
            DeclineChangesOnComment = new RelayCommand<string>(RevertChanges);
        }

        private void InitializeEvents()
        {
            SelectionChanged += () => RaisePropertyChanged(nameof(SelectedTask));
            SelectionChanged += () => RaisePropertyChanged(nameof(CurrentTaskProcesses));
            SelectionChanged += () => RaisePropertyChanged(nameof(ReporterCommentEditVisibility));
            SelectionChanged += () => RaisePropertyChanged(nameof(AssigneeCommentEditVisibility));
            SelectionChanged += () => RaisePropertyChanged(nameof(ReporterOrAssigneeEditVisibility));
            SelectionChanged += () => RaisePropertyChanged(nameof(CurrentlyShownTaskPath));
        }

        private void SelectBlockMethod(BlockProxy selectedBlock)
        {
            ChangeShownTask(SelectedTabTag);
            List<TaskEntity> exportValues = new List<TaskEntity>();
            foreach (TaskEntity task in _nullTask.ChildTasks)
            {
                foreach (ProcessProxy process in task.Processes)
                {
                    Process timeSheetProcess = AllProcessesFromTimeSheet.FirstOrDefault(i => i.id == process.ProcessId);
                    if (timeSheetProcess.Block_id == selectedBlock.BlockId)
                    {
                        exportValues.Add(task);
                        break;
                    }
                }
            }
            FillNullTask(exportValues);
            IsBlockSelectionActive = false;
        }

        private void RevertChanges(string senderName)
        {
            if (senderName.Equals("ReportCommentEditDenie"))
            {
                SelectedTask.SupervisorComment = ReporterCommentCache;
            }
            else if (senderName.Equals("AssigneeCommentEditDenie"))
            {
                SelectedTask.EmployeeComment = AssigneeCommentCache;
            }
            else if (senderName.Equals("DonePercentageEditDenie"))
            {
                SelectedTask.SupervisorDonePercent = DonePercentageCache;
            }
            SelectionChanged?.Invoke();
        }

        private void AcceptChanges(string senderName)
        {
            if (senderName.Equals("ReportCommentEditApprove"))
            {
                string temp = SelectedTask.EmployeeComment;
                SelectedTask.EmployeeComment = string.IsNullOrEmpty(AssigneeCommentCache) ? SelectedTask.EmployeeComment : AssigneeCommentCache;
                _taskContext.SaveChanges();
                SelectedTask.EmployeeComment = temp;

            }
            else if (senderName.Equals("AssigneeCommentEditApprove"))
            {
                string temp = SelectedTask.SupervisorComment;
                SelectedTask.SupervisorComment = string.IsNullOrEmpty(ReporterCommentCache) ? SelectedTask.SupervisorComment : ReporterCommentCache;
                _taskContext.SaveChanges();
                SelectedTask.SupervisorComment = temp;
            }
            else if (senderName.Equals("DonePercentageEditApprove"))
            {
                string tempAssigneeComment = SelectedTask.EmployeeComment;
                string tempReporterComment = SelectedTask.SupervisorComment;
                SelectedTask.EmployeeComment = string.IsNullOrEmpty(AssigneeCommentCache) ? SelectedTask.EmployeeComment : AssigneeCommentCache;
                SelectedTask.SupervisorComment = string.IsNullOrEmpty(ReporterCommentCache) ? SelectedTask.SupervisorComment : ReporterCommentCache;
                _taskContext.SaveChanges();
                SelectedTask.EmployeeComment = tempAssigneeComment;
                SelectedTask.SupervisorComment = tempReporterComment;
            }
            RaisePropertyChanged(nameof(SelectedTask));

        }

        private void RememberOldCommentValues(string sender)
        {
            if (sender.Equals("EditButtonReporter"))
            {
                ReporterCommentCache = SelectedTask.SupervisorComment;
            }
            else if (sender.Equals("EditButtonAssignee"))
            {
                AssigneeCommentCache = SelectedTask.EmployeeComment;
            }
            else if (sender.Equals("EditDonePercentage"))
            {
                DonePercentageCache = SelectedTask.SupervisorDonePercent;
            }
        }

        private void PasteTask(TaskEntity ParentTask)
        {
            foreach (TaskEntity cuttedTask in _currentlyCuttedTasks)
            {
                cuttedTask.ParentTask = ParentTask;
                if (ParentTask != CurrentlyShownTask && CurrentlyShownTask.ChildTasks.Any(task => task.Id == cuttedTask.Id))
                    CurrentlyShownTask.ChildTasks.Remove(cuttedTask);

                if (ParentTask == _nullTask)
                {
                    CurrentlyShownTask.ChildTasks.Add(cuttedTask);
                    cuttedTask.ParentTask = null;
                }
            }
            _taskContext.SaveChanges();
        }

        private void CutTask()
        {
            _currentlyCuttedTasks.Clear();
            foreach (TaskEntity selectedTask in _selectedTasks)
            {
                _currentlyCuttedTasks.Add(selectedTask);
            }
        }

        private void StoreSelection(System.Collections.IList tasks)
        {
            _selectedTasks.Clear();
            foreach (TaskEntity tas in tasks)
            {
                _selectedTasks.Add(tas);
            }
        }

        private void ChangeShownTask(int Tag)
        {
            IsBlockSelectionActive = true;
            List<TaskEntity> SelectedTasks = new List<TaskEntity>();
            bool filterNullTaskRequired = false;
            switch (Tag)
            {
                //Все задачи
                case (0):
                    SelectedTasks = _taskContext.Tasks.Where(task => task.ParentTask == null).OrderBy(task => task.Name).ToList();
                    filterNullTaskRequired = true;
                    break;
                    //Я - инициатор
                case (1):
                    SelectedTasks = _taskContext.Tasks.Where(task =>
                            task.Reporter.Name.Equals(_currentEmployee.Name) &&
                            (task.ParentTask == null || !task.ParentTask.Reporter.Name.Equals(task.Reporter.Name))).
                        OrderBy(task => task.Name).ToList();
                    filterNullTaskRequired = false;
                    break;
                    //Я - ответственный
                case (2):
                    SelectedTasks = _taskContext.Tasks.Where(task => task.Assignee.Name.Equals(_currentEmployee.Name)).OrderBy(task => task.Name).ToList();
                    filterNullTaskRequired = false;
                    break;
            }
            FillBlocksProxyWithTasks(SelectedTasks, filterNullTaskRequired);
            FillNullTask(SelectedTasks);
            CurrentlyShownTask = _nullTask;

        }

        private void FillNullTask(IEnumerable<TaskEntity> taskSource)
        {
            _nullTask.ChildTasks.Clear();
            foreach (TaskEntity task in taskSource)
            {
                if (task.ParentTask == null || 
                    !task.Name.Equals(task.ParentTask.Name) ||
                    (task.Name.Equals(task.ParentTask.Name) &&  _currentEmployee.Name != task.ParentTask.Reporter.Name))
                {
                    _nullTask.ChildTasks.Add(task);
                }
            }
            CurrentlyShownTask = _nullTask;
            RaisePropertyChanged(nameof(CurrentlyShownTask));
        }

        private void AddNewTask(TaskEntity parentTask)
        {
            #region инициализация новой задачи
            CurrentlyEditedTask = new TaskEntity
            {
                Owner = _currentEmployee,
                Reporter = _currentEmployee,
                CreationDate = DateTime.Now,
                DueDate = DateTime.Now,
                ParentTask = parentTask,
                Weight = 100
            };
            #endregion


            AddedTaskReporterFIO = CurrentlyEditedTask.Reporter.Name;
            #region Настройка формы добавления
            NewTaskWindow newTaskWindow = new NewTaskWindow();
            newTaskWindow.Reporter.IgnoreTextChange = false;

            if (newTaskWindow.ShowDialog() == true)
            {
                #region Добавление новой задачи
                if (_taskContext.Employees.Any(i => i.Name.Equals(AddedTaskReporterFIO)))
                {
                    CurrentlyEditedTask.Reporter = _taskContext.Employees.FirstOrDefault(i => i.Name.Equals(AddedTaskReporterFIO));
                }
                else
                {
                    CurrentlyEditedTask.Reporter = GetEmployeeByName(AddedTaskReporterFIO);
                }

                //Основная задача
                TaskEntity newHeadTask = new TaskEntity
                {
                    Name = CurrentlyEditedTask.Name,
                    Comment = CurrentlyEditedTask.Comment,
                    Assignee = CurrentlyEditedTask.Reporter,
                    ParentTask = CurrentlyEditedTask.ParentTask,
                    AwaitedResult = CurrentlyEditedTask.AwaitedResult,
                    CreationDate = CurrentlyEditedTask.CreationDate,
                    DueDate = CurrentlyEditedTask.DueDate,
                    Meter = CurrentlyEditedTask.Meter,
                    Owner = CurrentlyEditedTask.Reporter,
                    Processes = AddFormSelectedProcesses.
                        Select(process => new ProcessProxy { ProcessId = process.id }).
                        ToList(),
                    Reporter = CurrentlyEditedTask.Reporter,
                    Weight = CurrentlyEditedTask.Weight,
                    ChildTasks = new ObservableCollection<TaskEntity>()
                };
                
                if (AddFormSelectedAnalytics.Count > 1)
                {
                    //Для каждого выбранного ответственного создается новая подзадача
                    foreach (Employee employee in AddFormSelectedAnalytics)
                    {
                        newHeadTask.ChildTasks.Add(new TaskEntity
                        {
                            Name = CurrentlyEditedTask.Name,
                            Comment = CurrentlyEditedTask.Comment,
                            Assignee = employee,
                            ParentTask = newHeadTask,
                            AwaitedResult = CurrentlyEditedTask.AwaitedResult,
                            CreationDate = CurrentlyEditedTask.CreationDate,
                            DueDate = CurrentlyEditedTask.DueDate,
                            Meter = CurrentlyEditedTask.Meter,
                            Owner = CurrentlyEditedTask.Owner,
                            Processes = AddFormSelectedProcesses.
                                Select(process => new ProcessProxy { ProcessId = process.id }).
                                ToList(),
                            Reporter = CurrentlyEditedTask.Reporter,
                            Weight = CurrentlyEditedTask.Weight,
                        });
                    }
                }
                else
                {
                    newHeadTask.Assignee = AddFormSelectedAnalytics[0];
                }
                if (newHeadTask.ParentTask == _nullTask) newHeadTask.ParentTask = null;
                _taskContext.Tasks.Add(newHeadTask);
                _taskContext.SaveChanges();
                if (newHeadTask.ParentTask == null) newHeadTask.ParentTask = _nullTask;
                RaisePropertyChanged(nameof(CurrentlyShownTask));
                if (newHeadTask.ParentTask != null && newHeadTask.ParentTask.Name.Equals(_nullTask.Name))
                {
                    CurrentlyShownTask.ChildTasks.Add(newHeadTask);
                }
                AddFormSelectedAnalytics.Clear();
                AddFormSelectedProcesses.Clear();

                #endregion
            }
            #endregion
        }

        private void EditTask(TaskEntity editedTask)
        {
            CurrentlyEditedTask = editedTask;
            if (CurrentlyEditedTask.ParentTask == null) CurrentlyEditedTask.ParentTask = _nullTask;
            NewTaskWindow taskWindow = new NewTaskWindow();
            taskWindow.Assignee.Visibility = Visibility.Collapsed;
            taskWindow.AssigneeSingle.Visibility = Visibility.Visible;
            List<ProcessProxy> oldProcesses = new List<ProcessProxy>(CurrentlyEditedTask.Processes);

            AddedTaskAssigneeFIO = CurrentlyEditedTask.Assignee.Name;
            AddedTaskReporterFIO = CurrentlyEditedTask.Reporter?.Name;
            RaisePropertyChanged(nameof(AddedTaskAssigneeFIO));
            RaisePropertyChanged(nameof(AddedTaskReporterFIO));
            taskWindow.Reporter.IgnoreTextChange = false;
            taskWindow.AssigneeSingle.IgnoreTextChange = false;

            taskWindow.Processes.SelectedItemsOverride.Clear();
            foreach (ProcessProxy process in CurrentlyEditedTask.Processes)
            {
                AddFormSelectedProcesses.Add(AllProcessesFromTimeSheet.FirstOrDefault(proc => proc.id == process.ProcessId));
            }
            if (taskWindow.ShowDialog() == true)
            {
                if (CurrentlyEditedTask.ParentTask == _nullTask)
                {
                    CurrentlyEditedTask.ParentTask = null;
                }
                else if (_nullTask.ChildTasks.Contains(CurrentlyEditedTask))
                {
                    _nullTask.ChildTasks.Remove(CurrentlyEditedTask);
                    RaisePropertyChanged(nameof(CurrentlyEditedTask.ParentTask));
                }

                if (_taskContext.Employees.Any(i => i.Name.Equals(AddedTaskAssigneeFIO)))
                {
                    CurrentlyEditedTask.Assignee = _taskContext.Employees.FirstOrDefault(i => i.Name.Equals(AddedTaskAssigneeFIO));
                }
                else
                {
                    CurrentlyEditedTask.Assignee = GetEmployeeByName(AddedTaskAssigneeFIO);
                }

                if (_taskContext.Employees.Any(i => i.Name.Equals(AddedTaskReporterFIO)))
                {
                    CurrentlyEditedTask.Reporter = _taskContext.Employees.FirstOrDefault(i => i.Name.Equals(AddedTaskReporterFIO));
                }
                else
                {
                    CurrentlyEditedTask.Reporter = GetEmployeeByName(AddedTaskReporterFIO);
                }

                foreach (ProcessProxy process in oldProcesses)
                {
                    _taskContext.Processes.Remove(process);
                }
                CurrentlyEditedTask.Processes = AddFormSelectedProcesses.Select(proc => new ProcessProxy { ProcessId = proc.id }).ToList();
                _taskContext.SaveChanges();

            }
            RaisePropertyChanged(nameof(SelectedTask));
            taskWindow.Assignee.Visibility = Visibility.Visible;
            taskWindow.AssigneeSingle.Visibility = Visibility.Collapsed;
            AddFormSelectedProcesses.Clear();
        }

        private Employee GetEmployeeByName(string FIO)
        {
            string lastName = FIO.Split(' ')[0];
            string firstName = FIO.Split(' ')[1];
            string fatherName = FIO.Split(' ')[2];
            Analytic analytic = _timeSheetContext.Analytics.FirstOrDefault(i => i.LastName.Equals(lastName) &&
            i.FirstName.Equals(firstName) &&
            i.FatherName.Equals(fatherName));
            return new Employee
            {
                AnalyticId = analytic.Id,
                Name = FIO
            };
        }

        private void DeleteTask(TaskEntity deletedTask)
        {
            string errorDescription = "Удаляемая задача содержит в себе подзадачи, которые также будут удалены. Вы уверены?";
            string errorTitle = "Внимание!";

            if (_selectedTasks.Count == 1 &&
                (deletedTask.ChildTasks.Count == 0 || (deletedTask.ChildTasks.Count > 0 &&
                     MessageBox.Show(errorDescription, errorTitle, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)))
            {
                RemoveTaskRecursive(deletedTask);
                _taskContext.SaveChanges();
                CurrentlyShownTask.ChildTasks.Remove(deletedTask);
            }
            else if (_selectedTasks.Count > 1 &&
               MessageBox.Show($"Вы собираетесь удалить {_selectedTasks.Count} задач(и). Также будут удалены все их подзадачи. Вы уверены?",
               errorTitle, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                while (_selectedTasks.Count > 0)
                {
                    RemoveTaskRecursive(_selectedTasks[0]);
                    CurrentlyShownTask.ChildTasks.Remove(_selectedTasks[0]);
                }
                _taskContext.SaveChanges();
            }
        }

        private void RemoveTaskRecursive(TaskEntity task)
        {
            if (task.ChildTasks.Count > 0)
            {
                while (task.ChildTasks.Count > 0)
                {
                    RemoveTaskRecursive(task.ChildTasks[0]);
                }
            }

            while (task.Processes.Count > 0)
            {
                _taskContext.Processes.Remove(task.Processes[0]);
            }
            _taskContext.Tasks.Remove(task);
        }

        private void GotoParent()
        {
            if (CurrentlyShownTask.ParentTask != null)
            {
                CurrentlyShownTask = CurrentlyShownTask.ParentTask;
                RaisePropertyChanged(nameof(CurrentlyShownTask));
                RaisePropertyChanged(nameof(CurrentlyShownTaskPath));
            }
            else if (CurrentlyShownTask != _nullTask)
            {
                CurrentlyShownTask = _nullTask;
                RaisePropertyChanged(nameof(CurrentlyShownTask));
                RaisePropertyChanged(nameof(CurrentlyShownTaskPath));

            } else if(CurrentlyShownTask == _nullTask)
            {
                IsBlockSelectionActive = true;
            }
            RaisePropertyChanged(nameof(GoToParentVisibility));
        }

        private void ShowTaskChildsMethod(TaskEntity task)
        {
            if (task.ChildTasks != null && task.ChildTasks.Count > 0)
            {
                CurrentlyShownTask = task;
                RaisePropertyChanged(nameof(CurrentlyShownTask));
                RaisePropertyChanged(nameof(GoToParentVisibility));
            }
        }

        private void FillBlocksProxyWithTasks(IEnumerable<TaskEntity> tasks, bool FilterNullTasks = true)
        {
            BlockProxies.Clear();
            foreach (Block block in AllBlocksFromTimeSheet)
            {
                bool BlockAdded = false;

                foreach (TaskEntity task in tasks)
                {
                    if (!FilterNullTasks || FilterNullTasks && task.ParentTask == null)
                    {
                        foreach (ProcessProxy processProxy in task.Processes)
                        {
                            Process process = AllProcessesFromTimeSheet.FirstOrDefault(i => i.id == processProxy.ProcessId);
                            if (process.Block_id == block.Id)
                            {
                                if (!BlockAdded)
                                {
                                    BlockProxies.Add(new BlockProxy
                                    {
                                        BlockId = block.Id,
                                        BlockName = block.BlockName,
                                        ChildTasks = new List<TaskEntity>()
                                    });
                                }
                                BlockAdded = true;
                                BlockProxies[BlockProxies.Count - 1].ChildTasks.Add(task);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
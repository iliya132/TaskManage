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
        #region Данные

        //Вся информация о задачах
        TaskContext _taskContext = new TaskContext();

        //Вся информация из TimeSheet
        TimeSheetContext _timeSheetContext = new TimeSheetContext();

        //Все задачи в БД
        public ObservableCollection<TaskEntity> AllLoadedTasks { get; set; }

        //Задача, содержащая все задачи, у которых отсутствуют родительские задачи
        private TaskEntity _nullTask { get; set; }

        //Все процессы из TimeSheet
        public List<Process> AllProcesses { get; set; }

        //Подчиненные к текущему пользователю сотрудники
        public ObservableCollection<Employee> SubordinatedEmployees { get; set; }

        #endregion

        #region Выбранные значения

        //Выделенная задача
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
                RaisePropertyChanged(nameof(SelectedTask));
                RaisePropertyChanged(nameof(CurrentTaskProcesses));
                RaisePropertyChanged(nameof(ReporterCommentEditVisibility));
                RaisePropertyChanged(nameof(AssigneeCommentEditVisibility));
                RaisePropertyChanged(nameof(ReporterOrAssigneeEditVisibility));
                RaisePropertyChanged(nameof(CurrentlyShownTaskPath));
                
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

        //Текущая визуализированная задача (в листе на приложении отображаются дочерние элементы этой задачи)
        public TaskEntity CurrentlyShownTask { get; set; }

        //Редактируемая задача
        public TaskEntity CurrentlyEditedTask { get; set; }

        //Задача в буфере обмена
        private List<TaskEntity> _currentlyCuttedTasks { get; set; }

        //Текущий аналитик(из TimeSheet)
        private Analytic _currentAnalytic { get; set; }

        //Текущий сотрудник
        private Employee _currentEmployee { get; set; }

        //Выделенные на форме добавления/редактирования аналитики
        public ObservableCollection<Employee> AddFormSelectedAnalytics { get; set; }

        //Выделенные на форме добавления/редактирования процессы
        public ObservableCollection<Process> AddFormSelectedProcesses { get; set; }

        //Доступные для выбора родительские задачи
        public ObservableCollection<TaskEntity> AddFormAvailableParentTasks
        {
            get
            {
                return new ObservableCollection<TaskEntity>(AllLoadedTasks.Where(task=>task.ParentTask==null));
            }
        }

        //ФИО подчинненных сотрудников
        public ObservableCollection<string> SubordinatedEmployeesNames { get; set; }

        //Выделенные в ListView главной формы задачи
        private List<TaskEntity> _selectedTasks { get; set; }

        //Инициатор задачи
        public string AddedTaskReporterFIO { get; set; }
        //Ответственный в задаче
        public string AddedTaskAssigneeFIO { get; set; }
        //Перевод в строковое представление процессов в выделенной задаче
        public string CurrentTaskProcesses
        {
            get
            {
                if (SelectedTask != null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (ProcessProxy processProxy in SelectedTask.Processes)
                    {
                        stringBuilder.Append($"{AllProcesses.FirstOrDefault(process => process.id == processProxy.ProcessId)}\r\n");
                    }
                    return stringBuilder.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

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
                if (CurrentlyShownTask.ParentTask != null || !CurrentlyShownTask.Name.Equals(_nullTask.Name))
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }
        #endregion

        #region Команды
        public RelayCommand<TaskEntity> ShowTaskChilds { get; set; }
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
        }

        /// <summary>
        /// Инициализация переменных при загрузке приложения
        /// </summary>
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

            AllProcesses = new List<Process>(_timeSheetContext.Process);

            List<Analytic> allAnalytics = _timeSheetContext.Analytics.ToList();//TODO Добавить логику отбора подчиненных сотрудников

            SubordinatedEmployees = new ObservableCollection<Employee>();

            _selectedTasks = new List<TaskEntity>();
            _currentlyCuttedTasks = new List<TaskEntity>();


            foreach (Analytic analytic in allAnalytics.OrderBy(i=>i.LastName))
            {
                SubordinatedEmployees.Add(new Employee
                {
                    AnalyticId = analytic.Id,
                    Name = $"{analytic.LastName} {analytic.FirstName} {analytic.FatherName}"
                });
            }

            SubordinatedEmployeesNames = new ObservableCollection<string>(
            SubordinatedEmployees.Select(employee => employee.Name));

            AddFormSelectedAnalytics = new ObservableCollection<Employee>();
            AddFormSelectedProcesses = new ObservableCollection<Process>();

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

        /// <summary>
        /// Инициализация команд при загрузке приложения
        /// </summary>
        private void InitializeCommands()
        {
            ShowTaskChilds = new RelayCommand<TaskEntity>(ShowTaskChildsMethod);
            GoToParentCommand = new RelayCommand(GotoParent);
            AddTaskCommand = new RelayCommand<TaskEntity>(AddTask);
            EditTaskCommand = new RelayCommand<TaskEntity>(EditTask);
            DeleteTaskCommand = new RelayCommand<TaskEntity>(DeleteTask);
            TabChange = new RelayCommand<int>(ChangeShownTask);
            StoreSelectedTasks = new RelayCommand<System.Collections.IList>(StoreSelection);
            CutTaskCommand = new RelayCommand(CutTask);
            PasteTaskCommand = new RelayCommand<TaskEntity>(PasteTask);
            RememberOldValuesCommand = new RelayCommand<string>(RememberOldCommentValues);
            AcceptChangesOnComment = new RelayCommand<string>(AcceptChanges);
            DeclineChangesOnComment = new RelayCommand<string>(DeclineChanges);
        }

        /// <summary>
        /// Отклоняет изменения, внесенные в статус исполнения
        /// </summary>
        /// <param name="senderName"></param>
        private void DeclineChanges(string senderName)
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
            RaisePropertyChanged(nameof(SelectedTask));
        }

        /// <summary>
        /// Применяет изменения вносимые в статус исполнения
        /// </summary>
        /// <param name="senderName"></param>
        private void AcceptChanges(string senderName)
        {
            //Прежде чем сохранить изменения на форме - 
            //восстанавливаем значения всех полей до редактирования, 
            //для того что бы сохранение произошло только на том поле, 
            //где нажата кнопка принять
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


        /// <summary>
        /// Записать изменения в выделенных задачах
        /// </summary>
        /// <param name="obj"></param>
        private void StoreSelection(System.Collections.IList tasks)
        {
            _selectedTasks.Clear();
            foreach (TaskEntity tas in tasks)
            {
                _selectedTasks.Add(tas);
            }
        }

        /// <summary>
        /// Смена вкладки на главной странице
        /// </summary>
        /// <param name="Tag"></param>
        private void ChangeShownTask(int Tag)
        {
            switch (Tag)
            {
                //Все задачи
                case (0):
                    FillNullTask(_taskContext.Tasks.Where(task => task.ParentTask == null).OrderBy(task => task.Name));
                    break;
                    //Я - инициатор
                case (1):
                    FillNullTask(_taskContext.Tasks.
                        Where(task =>
                            task.Reporter.Name.Equals(_currentEmployee.Name) &&
                            (task.ParentTask == null || !task.ParentTask.Reporter.Name.Equals(task.Reporter.Name))).
                        OrderBy(task => task.Name));
                    break;
                    //Я - ответственный
                case (2):
                    FillNullTask(_taskContext.Tasks.Where(task => task.Assignee.Name.Equals(_currentEmployee.Name)).OrderBy(task => task.Name));
                    break;
                case (3):
                    List<Analytic> analyticsWithSameStruct = new List<Analytic>();
                    List<TaskEntity> tasksWithMyStruct = new List<TaskEntity>();
                    int currentAnalyticRole = _currentAnalytic.Role.Id;


                    foreach (Analytic analytic in _timeSheetContext.Analytics)
                    {

                        bool DepartmentsIdentity = analytic.DepartmentsId == _currentAnalytic.DepartmentsId;
                        bool DirectionsIdentity = analytic.DirectionsId == _currentAnalytic.DirectionsId;
                        bool UpravlenieIdentity = analytic.UpravlenieTableId == _currentAnalytic.UpravlenieTableId;
                        bool OtdelIdentity = analytic.OtdelTableId == _currentAnalytic.OtdelTableId;

                        switch (currentAnalyticRole)
                        {
                            case (1):
                                DirectionsIdentity = true;
                                UpravlenieIdentity = true;
                                OtdelIdentity = true;
                                break;
                            case (2):
                                UpravlenieIdentity = true;
                                OtdelIdentity = true;
                                break;
                            case (3):
                                OtdelIdentity = true;
                                break;
                            case (5):
                                DepartmentsIdentity = true;
                                DirectionsIdentity = true;
                                UpravlenieIdentity = true;
                                OtdelIdentity = true;
                                break;
                            case (6):
                                DepartmentsIdentity = false;
                                DirectionsIdentity = false;
                                UpravlenieIdentity = false;
                                OtdelIdentity = false;
                                break;
                            default:
                                break;
                        }

                        foreach (Analytic item in analyticsWithSameStruct)
                        {
                            tasksWithMyStruct.AddRange(_taskContext.Tasks.Where(task => task.Assignee.AnalyticId == item.Id));
                        }

                        if (DepartmentsIdentity && DirectionsIdentity && UpravlenieIdentity && OtdelIdentity)
                        {
                            analyticsWithSameStruct.Add(analytic);
                        }
                    }

                    FillNullTask(tasksWithMyStruct.Distinct().OrderBy(task => task.Name));
                    break;
            }
            CurrentlyShownTask = _nullTask;

        }

        /// <summary>
        /// Заполнить нулевую задачу (при смене вкладки)
        /// </summary>
        /// <param name="taskSource"></param>
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

        /// <summary>
        /// Метод вызывает окно создания новой задачи, где родительской задачей будет являться parentTask
        /// </summary>
        /// <param name="parentTask"></param>
        private void AddTask(TaskEntity parentTask)
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
                    CurrentlyEditedTask.Reporter = GetEmployee(AddedTaskReporterFIO);
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
                if (newHeadTask.ParentTask == _nullTask) newHeadTask.ParentTask = null;
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

                _taskContext.Tasks.Add(newHeadTask);
                _taskContext.SaveChanges();
                RaisePropertyChanged(nameof(CurrentlyShownTask));
                if (newHeadTask.ParentTask.Name.Equals(_nullTask.Name))
                {
                    CurrentlyShownTask.ChildTasks.Add(newHeadTask);
                }
                AddFormSelectedAnalytics.Clear();
                AddFormSelectedProcesses.Clear();

                #endregion
            }
            #endregion
        }

        /// <summary>
        /// Редактировать выбранную задачу
        /// </summary>
        /// <param name="editedTask"></param>
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
                AddFormSelectedProcesses.Add(AllProcesses.FirstOrDefault(proc => proc.id == process.ProcessId));
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
                    CurrentlyEditedTask.Assignee = GetEmployee(AddedTaskAssigneeFIO);
                }

                if (_taskContext.Employees.Any(i => i.Name.Equals(AddedTaskReporterFIO)))
                {
                    CurrentlyEditedTask.Reporter = _taskContext.Employees.FirstOrDefault(i => i.Name.Equals(AddedTaskReporterFIO));
                }
                else
                {
                    CurrentlyEditedTask.Reporter = GetEmployee(AddedTaskReporterFIO);
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

        /// <summary>
        /// Получить Employee
        /// </summary>
        /// <param name="FIO">ФИО сотрудника, которого нужно найти</param>
        /// <returns></returns>
        private Employee GetEmployee(string FIO)
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

        /// <summary>
        /// Удалить выбранную задачу
        /// </summary>
        /// <param name="deletedTask"></param>
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

        /// <summary>
        /// Рекурсивный метод для удаления задачи, а также её подзадач
        /// </summary>
        /// <param name="task"></param>
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

        /// <summary>
        /// Метод присваивает CurrentlyShownTask родителя текущей задачи
        /// </summary>
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

            }
            RaisePropertyChanged(nameof(GoToParentVisibility));
        }

        /// <summary>
        /// Перейти "Внутрь" задачи. Отображает дочерние задачи выбранной задачи
        /// </summary>
        /// <param name="task"></param>
        private void ShowTaskChildsMethod(TaskEntity task)
        {
            if (task.ChildTasks != null && task.ChildTasks.Count > 0)
            {
                CurrentlyShownTask = task;
                RaisePropertyChanged(nameof(CurrentlyShownTask));
                RaisePropertyChanged(nameof(GoToParentVisibility));
            }
        }
    }
}
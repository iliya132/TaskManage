using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Model.Entities
{
    [Table("EmployeeTasks")]
    public class TaskEntity : Base.NamedEntity, INotifyPropertyChanged
    {
        public TaskEntity()
        {
            ChildTasks = new ObservableCollection<TaskEntity>();
            Processes = new List<ProcessProxy>();
        }
        public List<ProcessProxy> Processes { get; set; }
        public virtual TaskEntity ParentTask { get; set; }
        public virtual ObservableCollection<TaskEntity> ChildTasks { get; set; }
        public List<int> ProcessIds { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime DueDate { get; set; }
        public Employee Owner { get; set; }
        public Employee Assignee { get; set; }
        public Employee Reporter { get; set; }
        public string Comment { get; set; }
        public int Weight { get; set; }
        public string EmployeeComment { get; set; }
        public string SupervisorComment { get; set; }
        public int EmployeeDonePercent { get; set; }
        public string AwaitedResult { get; set; }
        public string Meter { get; set; }
        private int _supervisorDonePercent;
        public int SupervisorDonePercent
        {
            get
            {
                int donePercent = 0;
                if (ChildTasks.Count > 0)
                {
                    foreach (TaskEntity subtask in ChildTasks)
                    {
                        donePercent += subtask.SupervisorDonePercent;
                    }
                    return donePercent / ChildTasks.Count;
                }
                else
                {
                    return _supervisorDonePercent;
                }
            }
            
            set
            {
                _supervisorDonePercent = value;
                OnPropertyChanged(nameof(SupervisorDonePercent));
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string Property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Property));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

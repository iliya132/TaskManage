using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Model.Entities
{
    public class BlockProxy
    {
        public int BlockId { get; set; }
        public string BlockName { get; set; }
        public List<TaskEntity> ChildTasks { get; set; }
        public int DonePercent
        {
            get
            {
                int donePercent = 0;
                foreach (TaskEntity subtask in ChildTasks)
                {
                    donePercent += subtask.SupervisorDonePercent;
                }
                return ChildTasks.Count > 0 ? 
                    donePercent/ChildTasks.Count : 0;
            }
        }
    }
}

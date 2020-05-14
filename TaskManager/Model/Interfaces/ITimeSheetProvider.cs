using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Model.Entities;
using TaskManager.Model.Entities.TimeSheet;

namespace TaskManager.Model.Interfaces
{
    public interface ITimeSheetProvider
    {
        IEnumerable<Process> GetProcesses();
        IEnumerable<Block> GetBlocks();

        IEnumerable<Employee> GetSubordinatedEmployees(Analytic HeadAnalytic);
    }
}

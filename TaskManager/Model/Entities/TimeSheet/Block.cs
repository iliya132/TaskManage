using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Model.Entities.TimeSheet
{
    [Table("Block")]
    public class Block
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("blockName")]
        public string BlockName { get; set; }
    }
}

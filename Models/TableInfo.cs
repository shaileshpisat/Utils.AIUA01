using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourNamespace.Models
{
    [Table("TableInfos")]
    public class TableInfo
    {
        [Column("TableName")]
        public string TableName { get; set; }

    }
}

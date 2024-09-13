using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourNamespace.Models
{
    [Table("ColumnInfos")]
    public class ColumnInfo
    {
        [Column("ColumnName")]
        public string ColumnName { get; set; }

        [Column("DataType")]
        public string DataType { get; set; }

        [Column("IsNullable")]
        public string IsNullable { get; set; }

    }
}

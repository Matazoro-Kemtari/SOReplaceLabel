using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq.Mapping;


namespace SOReplaceLabelLib.Data
{
    [Table(Name = "parts_packings")]
    public class PartsPacking
    {
        [Column(Name = "id", IsPrimaryKey = true)]
        public int? Id { get; set; }
        [Column(Name = "parts_no", CanBeNull = false)]
        public string PartsNo { get; set; }
        [Column(Name = "pack_method", CanBeNull = true)]
        public string PackMethod { get; set; }
    }
}

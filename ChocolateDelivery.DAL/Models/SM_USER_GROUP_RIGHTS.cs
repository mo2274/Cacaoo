using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public partial class SM_USER_GROUP_RIGHTS
    {
        [Key]
        public long Right_Id { get; set; }
        public short Group_Cd { get; set; }
        public Nullable<long> Menu_Id { get; set; }
        public bool AllowView { get; set; }
        public bool AllowAdd { get; set; }
        public bool AllowEdit { get; set; }
        public bool AllowDelete { get; set; }
        public System.DateTime User_Date_Time { get; set; }

    }
}

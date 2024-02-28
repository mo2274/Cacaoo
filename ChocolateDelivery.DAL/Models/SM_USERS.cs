using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public partial class SM_USERS
    {
        [Key]
        public short User_Cd { get; set; }
        public string User_Id { get; set; } = String.Empty;
        public short Group_Cd { get; set; }
        public string Password { get; set; } = String.Empty;
        public string User_Name { get; set; } = String.Empty;
        public string? Phone { get; set; } = String.Empty;
        public string Preferred_Language { get; set; } = String.Empty;
        public string? Comments { get; set; } = String.Empty;
        public long Entity_Id { get; set; }
        public string? Email { get; set; } = String.Empty;
        public string? Default_Page { get; set; } = String.Empty;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public partial class SM_LISTS
    {
        [Key]
        public int List_Id { get; set; }
        public string List_Name_E { get; set; } = String.Empty;
        public string? List_Name_A { get; set; }
        public string? From_Clause { get; set; }
        public string? Where_Clause { get; set; }
        public string? Order_By_Clause { get; set; }
        public string? Variable_Clause { get; set; }
        public string? Add_New_URL { get; set; }
        public bool Show { get; set; }
        public Nullable<short> Created_By { get; set; }
        public Nullable<System.DateTime> Created_Datetime { get; set; }
        public Nullable<short> Updated_By { get; set; }
        public Nullable<System.DateTime> Updated_Datetime { get; set; }
        public Nullable<int> Sequence_By_Column { get; set; }
        public Nullable<bool> Contain_Group_By_Clause { get; set; }
        public Nullable<bool> Pre_Load_Report { get; set; }
        public string? Color_Columns { get; set; }
        public Nullable<bool> Show_Footer { get; set; }
        public Nullable<bool> Show_Horizontal_Scrollbar { get; set; }
        public Nullable<int> Horizontal_Scrollbar_Width { get; set; }
        public Nullable<bool> Show_Vertical_Scrollbar { get; set; }
        public Nullable<int> Vertical_Scrollbar_Height { get; set; }
        public Nullable<bool> Is_StoredProcedure { get; set; }
        public string? StoredProcedure_Name { get; set; }
        public string? StoredProcedure_Content { get; set; }
        public Nullable<int> Command_Timeout { get; set; }
        public Nullable<int> Gridview_Type { get; set; }
        public Nullable<bool> Write_SQL_Log { get; set; }
        public string? Update_URL { get; set; }
        public bool Contain_Entity_Id { get; set; }
        public string? Find_Return_Columns { get; set; }
        public string? Entity_Id_Alias { get; set; }
        public short Render_Type { get; set; }
        public string? Control_Name { get; set; }
        public string? Control_Css_Class { get; set; }
        public string? Bar_Chart_Label { get; set; }
        public bool Show_Add_New_URL { get; set; }
    }
}

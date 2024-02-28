using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public partial class SM_LIST_PARAMETERS
    {
        [Key]
        public long Parameter_Id { get; set; }
        public int List_Id { get; set; }
        public string? Parameter_Name { get; set; } = "";
        public string? Parameter_Type { get; set; } = "";
        public string? Value_Type { get; set; } = "";
        public string? Control_Name { get; set; } = "";
        public string? Control_Label_E { get; set; } = "";
        public string? Control_Label_A { get; set; } = "";
        public Nullable<long> Find_Id { get; set; }
        public string? Find_Return_Column { get; set; } = "";
        public string? Find_Column_Label_E { get; set; } = "";
        public string? Find_Column_Label_A { get; set; } = "";
        public string? Control_Size { get; set; } = "";
        public string? Control_Label_Size { get; set; } = "";
        public string? Default_Value { get; set; } = "";
        public string? Report_Filter_Id { get; set; } = "";
        public bool Show { get; set; }
        public string? Control_Css_Class { get; set; } = "";
        public Nullable<short> Dropdown_Type { get; set; }
        public Nullable<bool> Allow_Clear { get; set; }
        public Nullable<short> Select_Element_Index { get; set; }
        public Nullable<bool> Add_Select_Element_Clause { get; set; }
        public Nullable<bool> Is_Required { get; set; }
        public string? Required_Field_Msg { get; set; } = "";
        public string? Required_Field_Css_Class { get; set; } = "";
        public Nullable<int> Sequence { get; set; }
        public string? Parameter_DB_Type { get; set; } = "";
        public string? Dropdown_Manual_Values { get; set; } = "";
        public int? Error_Label_Id { get; set; }


    }
}

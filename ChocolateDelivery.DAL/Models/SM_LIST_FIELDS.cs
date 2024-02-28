using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateDelivery.DAL
{
    public partial class SM_LIST_FIELDS
    {
        [Key]
        public long Field_Id { get; set; }
        public int List_Id { get; set; }
        public string Field_Name_E { get; set; } = String.Empty;
        public string? Field_Name_A { get; set; } = String.Empty;
        public string? Table_Id { get; set; } = String.Empty;
        public string? Table_Field_Name { get; set; } = String.Empty;
        public Nullable<int> Sequence { get; set; }
        public string? Field_Type { get; set; } = String.Empty;
        public string? Format_String { get; set; } = String.Empty;
        public string? ItemStyle_CssClass { get; set; } = String.Empty;
        public string? HeaderStyle_CssClass { get; set; } = String.Empty;
        public string? FooterStyle_CssClass { get; set; } = String.Empty;
        public bool Show { get; set; }
        public bool Is_BoundField { get; set; }
        public Nullable<int> TemplateField_Type { get; set; }
        public string? TemplateField_Class { get; set; } = String.Empty;
        public Nullable<int> TemplateField_Width { get; set; }
        public Nullable<int> TemplateField_Height { get; set; }
        public string? TemplateField_Id { get; set; } = String.Empty;
        public string? Update_Clause { get; set; } = String.Empty;
        public Nullable<short> Field_Group_By_Type { get; set; }
        public string? Group_By_Field_Name { get; set; } = String.Empty;
        public Nullable<bool> Calculate_Footer { get; set; }
        public string? TemplateField_Hyperlink_Text { get; set; } = String.Empty;
        public string? TemplateField_NavigateURL { get; set; } = String.Empty;
        public bool Primary_Key_Column { get; set; }
        public string Field_Language { get; set; } = String.Empty;
    }
}

using System.ComponentModel.DataAnnotations;

namespace ChocolateDelivery.DAL;

public class SM_LIST_FIELDS
{
    [Key]
    public long Field_Id { get; set; }
    public int List_Id { get; set; }
    public string Field_Name_E { get; set; } = String.Empty;
    public string? Field_Name_A { get; set; } = String.Empty;
    public string? Table_Id { get; set; } = String.Empty;
    public string? Table_Field_Name { get; set; } = String.Empty;
    public int? Sequence { get; set; }
    public string? Field_Type { get; set; } = String.Empty;
    public string? Format_String { get; set; } = String.Empty;
    public string? ItemStyle_CssClass { get; set; } = String.Empty;
    public string? HeaderStyle_CssClass { get; set; } = String.Empty;
    public string? FooterStyle_CssClass { get; set; } = String.Empty;
    public bool Show { get; set; }
    public bool Is_BoundField { get; set; }
    public int? TemplateField_Type { get; set; }
    public string? TemplateField_Class { get; set; } = String.Empty;
    public int? TemplateField_Width { get; set; }
    public int? TemplateField_Height { get; set; }
    public string? TemplateField_Id { get; set; } = String.Empty;
    public string? Update_Clause { get; set; } = String.Empty;
    public short? Field_Group_By_Type { get; set; }
    public string? Group_By_Field_Name { get; set; } = String.Empty;
    public bool? Calculate_Footer { get; set; }
    public string? TemplateField_Hyperlink_Text { get; set; } = String.Empty;
    public string? TemplateField_NavigateURL { get; set; } = String.Empty;
    public bool Primary_Key_Column { get; set; }
    public string Field_Language { get; set; } = String.Empty;
}
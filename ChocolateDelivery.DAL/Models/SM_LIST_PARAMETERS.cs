using System.ComponentModel.DataAnnotations;

namespace ChocolateDelivery.DAL;

public class SM_LIST_PARAMETERS
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
    public long? Find_Id { get; set; }
    public string? Find_Return_Column { get; set; } = "";
    public string? Find_Column_Label_E { get; set; } = "";
    public string? Find_Column_Label_A { get; set; } = "";
    public string? Control_Size { get; set; } = "";
    public string? Control_Label_Size { get; set; } = "";
    public string? Default_Value { get; set; } = "";
    public string? Report_Filter_Id { get; set; } = "";
    public bool Show { get; set; }
    public string? Control_Css_Class { get; set; } = "";
    public short? Dropdown_Type { get; set; }
    public bool? Allow_Clear { get; set; }
    public short? Select_Element_Index { get; set; }
    public bool? Add_Select_Element_Clause { get; set; }
    public bool? Is_Required { get; set; }
    public string? Required_Field_Msg { get; set; } = "";
    public string? Required_Field_Css_Class { get; set; } = "";
    public int? Sequence { get; set; }
    public string? Parameter_DB_Type { get; set; } = "";
    public string? Dropdown_Manual_Values { get; set; } = "";
    public int? Error_Label_Id { get; set; }


}
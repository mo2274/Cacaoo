using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace ChocolateDelivery.DAL;

public class SM_LISTS
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
    public short? Created_By { get; set; }
    public System.DateTime? Created_Datetime { get; set; }
    public short? Updated_By { get; set; }
    public System.DateTime? Updated_Datetime { get; set; }
    public int? Sequence_By_Column { get; set; }
    public bool? Contain_Group_By_Clause { get; set; }
    public bool? Pre_Load_Report { get; set; }
    public string? Color_Columns { get; set; }
    public bool? Show_Footer { get; set; }
    public bool? Show_Horizontal_Scrollbar { get; set; }
    public int? Horizontal_Scrollbar_Width { get; set; }
    public bool? Show_Vertical_Scrollbar { get; set; }
    public int? Vertical_Scrollbar_Height { get; set; }
    public bool? Is_StoredProcedure { get; set; }
    public string? StoredProcedure_Name { get; set; }
    public string? StoredProcedure_Content { get; set; }
    public int? Command_Timeout { get; set; }
    public int? Gridview_Type { get; set; }
    public bool? Write_SQL_Log { get; set; }
    public string? Update_URL { get; set; }
    public bool Contain_Entity_Id { get; set; }
    public string? Find_Return_Columns { get; set; }
    public string? Entity_Id_Alias { get; set; }
    public short Render_Type { get; set; }
    public string? Control_Name { get; set; }
    public string? Control_Css_Class { get; set; }
    public string? Bar_Chart_Label { get; set; }
    public bool Show_Add_New_URL { get; set; }
    
    [NotMapped]
    public DataTable? Report_Data { get; set; }
    [NotMapped]
    public List<SM_LIST_FIELDS> Report_Fields { get; set; } = new();
}
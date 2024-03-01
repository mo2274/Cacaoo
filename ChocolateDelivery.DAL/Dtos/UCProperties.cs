namespace ChocolateDelivery.DAL;

public class UCProperties
{
    public string Id { get; set; } = "";
    public string? Name { get; set; } = "";
    public string Value { get; set; } = "";
    public string CSSClass { get; set; } = "";
    public string Placeholder { get; set; } = "";
    public string Element_Type { get; set; } = Form_Element_Types.Text;
    public long? List_Id { get; set; } = 0;
    public bool Is_Required { get; set; }
    public int? Error_Label_Id { get; set; } = 0;
    public int? Max_Length { get; set; }
    public int? Min_Length { get; set; }
    public string? OnKeyup { get; set; } = "";
    public bool Read_Only { get; set; }
    public string? Additional_Where_Clause { get; set; } = "";
    public bool Multiple { get; set; }
    public List<UCManualDropdownModel> DropDownData { get; set; } = new();
}

public class UCManualDropdownModel
{
    public string Text { get; set; } = "";
    public string Value { get; set; } = "";
    public bool Is_Selected { get; set; } = false;
}
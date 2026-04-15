namespace HUP.Core.Enums;

[AttributeUsage(AttributeTargets.Field)]
public class LocalizedAttribute : Attribute
{
    public string En { get;}
    public string Ar { get;}

    public LocalizedAttribute(string en, string ar)
    {
        En = en;
        Ar = ar;
    }
    
}
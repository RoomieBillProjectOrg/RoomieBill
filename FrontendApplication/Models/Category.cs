namespace FrontendApplication.Models
{
    public enum Category
    {
        Other,
        Electricity,
        Water,
        Gas,
        PropertyTaxes
    }

    public static class CategoryExtensions
    {
        public static string GetName(this Category category)
        {
            return category switch
            {
                Category.Electricity => "Electricity",
                Category.Water => "Water",
                Category.Gas => "Gas",
                Category.PropertyTaxes => "Property Taxes",
                Category.Other => "Other",
                _ => "Unknown"
            };
        }
    }
}

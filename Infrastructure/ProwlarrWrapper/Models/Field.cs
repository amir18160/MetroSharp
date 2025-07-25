namespace Infrastructure.ProwlarrWrapper.Models
{
    public class Field
    {
        public int Order { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Unit { get; set; }
        public string HelpText { get; set; }
        public string HelpTextWarning { get; set; }
        public string HelpLink { get; set; }
        public object Value { get; set; }
        public string Type { get; set; }
        public bool Advanced { get; set; }
        public List<SelectOption> SelectOptions { get; set; }
        public string SelectOptionsProviderAction { get; set; }
        public string Section { get; set; }
        public string Hidden { get; set; }
        public string Privacy { get; set; }
        public string Placeholder { get; set; }
        public bool IsFloat { get; set; }
    }

}
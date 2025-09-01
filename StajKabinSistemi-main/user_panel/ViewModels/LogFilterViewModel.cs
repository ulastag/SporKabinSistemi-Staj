namespace user_panel.ViewModels
{
    public class LogFilterViewModel
    {
        public string? filter {  get; set; }
        public string? actualFilter { get; set; }

        public Dictionary<string, string> filterOptions { get; set; } = new Dictionary<string, string>();
    }
}

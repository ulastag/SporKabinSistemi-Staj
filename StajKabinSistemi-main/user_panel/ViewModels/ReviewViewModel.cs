using System;

namespace user_panel.ViewModels
{
    public class ReviewViewModel
    {
        public string Content { get; set; }
        public double Rating { get; set; }
        public DateTime ReviewDate { get; set; }
        public string UserName { get; set; }
    }
}
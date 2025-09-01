using user_panel.Data;
using System.Collections.Generic;

namespace user_panel.ViewModels
{
    public class CabinDetailViewModel
    {
        public Cabin Cabin { get; set; }
        public List<ReviewViewModel> Reviews { get; set; }
        public bool CanUserReview { get; set; }
    }
}
using WebsiteOrdering.Models;

namespace WebsiteOrdering.ViewModels
{
    public class PersonalPageViewModel
    {
        public UpdateProfileViewModel Profile { get; set; }
        public List<Datban> ListDatBan { get; set; }
        public List<Donhang> ListDonHang { get; set; }
    }
}

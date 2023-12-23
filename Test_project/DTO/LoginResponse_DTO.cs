using Test_project.ViewModel;

namespace Test_project.DTO
{
    public class LoginResponse_DTO
    {
        public string? LoginID { get; set; }
        public int? UserID { get; set; }
        public int? UserInfoID { get; set; }
        public string? UserName { get; set; }
        public int? RoleID { get; set; }
        public string? RoleName { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public ResponseViewModel? response { get; set; }
        public LoginResponse_DTO()
        {
            response = new ResponseViewModel();
        }
    }
}

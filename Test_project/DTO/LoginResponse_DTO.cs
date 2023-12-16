namespace Test_project.DTO
{
    public class LoginResponse_DTO
    {
        public string? LoginID { get; set; }
        public int UserID { get; set; }
        public int UserInfoID { get; set; }
        public string? UserName { get; set; }
        public int RoleID { get; set; }
        public string? RoleName { get; set; }
        public string? Token { get; set; }
        public string? Secret { get; set; }
    }
}

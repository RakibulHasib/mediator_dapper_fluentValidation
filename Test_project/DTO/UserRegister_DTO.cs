namespace Test_project.DTO
{
    public class UserRegister_DTO
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public int Phone { get; set; }
        public string LoginId { get; set; } = null!;

        public string Password { get; set; } = null!;
    }
}

namespace TodoApi.DTO
{
    public class RegisterRequestDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string[] Roles { get; set; }
    }
}

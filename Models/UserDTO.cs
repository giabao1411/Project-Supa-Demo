public class UserDTO
{
        public Guid Id { get; set; } 
    public string password { get; set; }= "";
    public string Email { get; set; } = "";
 
   public DateTime CreatedAt { get; set; }
    public bool IsEmailVerified { get; set; }
    public string? AvatarUrl{get;set;}="";

    public List<RoleDTO> Roles { get; set; } = new List<RoleDTO>();
    public RoleDTO Role { get; set; } = new RoleDTO();

    public List<string> Permissions { get; set; } = new List<string>();
    public string coffirmPassword { get; set; }= "";
    public string  currentPassword { get; set; }="";

    public string userName { get; set; }= "";

    public int TotalCount { get; set; }= 0;
}
namespace PlataCuOra.Server.Models.Domain
{
	public class User
	{
		public int Id{ get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string Role { get; set; }

		public User SetId (int id)
		{
			this.Id = id;
			return this;
		}

		public User() { }
		public User (string name, string email, string password, string role)
		{
			Name = name;
			Email = email;
			Password = password;
			Role = role;
		}
	}
}

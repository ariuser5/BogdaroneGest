using System;

namespace DBEntities
{
	public class User
	{
		public string Name { get; set; }
		public string Password { get; set; }

		public override string ToString()
		{
			return $"username='{this.Name}'";
		}
	}
}

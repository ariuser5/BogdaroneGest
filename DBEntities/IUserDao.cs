using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBEntities
{
	public interface IUserDao
	{
		IEnumerable<User> GetUsers();
		User GetUser(string userName);
		void CreateUser(User user);
		void UpdateUser(User user);
		bool DeleteUser(string userName);
	}
}

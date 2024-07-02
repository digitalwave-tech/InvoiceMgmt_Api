using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface ICustomer
    {
        Task<string> CreateUserAccount(UserAccount userAccount);
        //List<UserAccount> GetUser();
        UserAccount GetUserByGstNo(string gstNo);
        Task<string> CreateCustomer(Customers customers);

        List<string> GetCustomeNameByGstNo(string gstNo);

    }
}

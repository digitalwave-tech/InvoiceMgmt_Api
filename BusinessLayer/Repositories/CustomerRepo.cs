using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Repositories
{
    public class CustomerRepo : ICustomer
    {
        ApplicationDbContext dbContext = new ApplicationDbContext();
        ErrorLogRepo errorLogRepo = new ErrorLogRepo();
        public async Task<string> CreateCustomer(Customers customers)
        {
            try
            {
                var response = dbContext.AddAsync(customers);
                await dbContext.SaveChangesAsync();
                return HttpStatusCode.Created.ToString();
            }
            catch (Exception ex)
            {
                var exc = new ErrorLog
                {
                    errorMsg = "ErrorMsg : " + ex.InnerException + "\r\n" + ex.StackTrace,
                    methodName = nameof(CreateCustomer),
                };
                errorLogRepo.WriteErrorLog(exc);
                int da = (int)HttpStatusCode.InternalServerError;
                return da.ToString();
            }
        }

        public async Task<string> CreateUserAccount(UserAccount customers)
        {
            try
            {
                var user = dbContext.UserAccount.Select(x => x.CompanyName == customers.CompanyName).FirstOrDefault();
                if (!user)
                {
                    var response = dbContext.AddAsync(customers);
                    await dbContext.SaveChangesAsync();
                    return HttpStatusCode.Created.ToString();
                }
                else
                {
                    return "Company Name already registered";
                }
            }
            catch (Exception ex)
            {
                var exc = new ErrorLog
                {
                    errorMsg = "ErrorMsg : " + ex.InnerException + "\r\n" + ex.StackTrace,
                    methodName = nameof(CreateUserAccount),
                };
                errorLogRepo.WriteErrorLog(exc);
                int da = (int)HttpStatusCode.InternalServerError;
                return da.ToString();
            }
        }

        public List<string> GetCustomeNameByGstNo(string gstNo)
        {
            try
            {
                var companyId = dbContext.UserAccount.Where(u => u.GstNo == gstNo).Select(z => z.CompanyId).FirstOrDefault();

                var result = dbContext.Customers.Where(u => u.CompanyId == companyId).Select(y => $"{y.FirstName} {y.LastName}").ToList();
                if (result.Count() > 0)
                {
                    return result;

                }
            }
            catch(Exception ex)
            {
                var exc = new ErrorLog
                {
                    errorMsg = "ErrorMsg : " + ex.InnerException + "\r\n" + ex.StackTrace,
                    methodName = nameof(GetCustomeNameByGstNo),
                };
                errorLogRepo.WriteErrorLog(exc);
            }
            return null;
        }

        public UserAccount GetUserByGstNo(string gstNo)
        {
            try
            {
                var result = dbContext.UserAccount.Where(u => u.GstNo == gstNo);
                if (result.Count() > 0)
                {
                    return result.FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                var exc = new ErrorLog
                {
                    errorMsg = "ErrorMsg : " + ex.InnerException + "\r\n" + ex.StackTrace,
                    methodName = nameof(GetUserByGstNo),
                };
                errorLogRepo.WriteErrorLog(exc);
            }
            return null;
        }
    }
}

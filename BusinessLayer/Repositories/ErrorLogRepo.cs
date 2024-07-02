using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Repositories
{
    public class ErrorLogRepo:IErrorLogging
    {
        ApplicationDbContext dbContext = new ApplicationDbContext();

        public void WriteErrorLog(ErrorLog errorLog)
        {
            var response = dbContext.AddAsync(errorLog);
            dbContext.SaveChangesAsync();
            
        }
    }
}

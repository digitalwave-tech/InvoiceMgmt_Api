using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

using System.Net.Http;

namespace BusinessLayer.Repositories
{
    public class InvoiceRepo : IInvoice
    {
        ApplicationDbContext dbContext = new ApplicationDbContext();
       ErrorLogRepo errorLogRepo = new ErrorLogRepo();
        public async Task<string> CreateInvoice(Invoices invoices)
        {
            try 
            {
                var response = dbContext.Invoices.Add(invoices);
                await dbContext.SaveChangesAsync();

                return "Invoice created successfully.";
            }
            catch (Exception ex)
            {
                var exc = new ErrorLog
                {
                    errorMsg = "ErrorMsg : " + ex.Message + "\r\n" + ex.StackTrace,
                    methodName = nameof(CreateInvoice),

                };

                errorLogRepo.WriteErrorLog(exc);
                return exc.ToString();

            }

            return null;
        }

        public IList<ViewInvoice> GetInvoices(string gstNo)
        {
            try
            {
                var companyAcct = dbContext.UserAccount.Where(u => u.GstNo == gstNo).FirstOrDefault();

                var invoicelist = dbContext.Invoices.Where(u => u.CompanyId == companyAcct.CompanyId).ToList();
                if (invoicelist.Count() > 0)
                {
                    var invoiceNumbers = invoicelist.Select(x => x.InvoiceNo).ToList();
                    var itemDetails = dbContext.ItemsDetails.Where(u => invoiceNumbers.Contains(u.InvoiceNo)).ToList();

                    // Creating the list of ViewInvoice
                    var viewInvoices = invoicelist.Select(invoice => new ViewInvoice
                    {
                        InvoiceDate = invoice.InvoiceDate,
                        CustomerName = invoice.CustomerName,
                        InvoiceNo = invoice.InvoiceNo,
                        NetAmount = itemDetails
                                    .Where(item => item.InvoiceNo == invoice.InvoiceNo)
                                    .Sum(item => item.NetAmt)
                    }).ToList();


                    return viewInvoices;

                }
            }
            catch (Exception ex)
            {
                var exc = new ErrorLog
                {
                    errorMsg = "ErrorMsg : " + ex.Message + "\r\n" + ex.StackTrace,
                    methodName = nameof(GetInvoices),
                };
                errorLogRepo.WriteErrorLog(exc);



            }
            return null;
        }

        public IList<PDFTemplate> GetInvoiceItemDetails(List<int> invoicenos)
        {
            try
            {
                var data = from id in dbContext.ItemsDetails
                           join i in dbContext.Invoices on id.InvoiceNo equals i.InvoiceNo
                           join ua in dbContext.UserAccount on i.CompanyId equals ua.CompanyId
                           join c in dbContext.Customers on ua.CompanyId equals c.CompanyId
                           where invoicenos.Contains(i.InvoiceNo ?? 0) &&
                                 i.CustomerName.Contains(c.FirstName + " " + c.LastName)
                           group new { id, i, ua, c } by new
                           {
                               ua.CompanyId,
                               ua.CompanyName,
                               ua.AddressLine,
                               ua.Phone,
                               ua.City,
                               ua.EmailId,
                               ua.States,
                               ua.Pincode,
                               c.FirstName,
                               c.LastName,
                               customerAddress = c.AddressLine,
                               customerPhone = c.Phone,
                               customerCity = c.City,
                               customerEmail = c.EmailId,
                               customerState = c.States,
                               customerPincode = c.Pincode
                           } into g
                           select new PDFTemplate
                           {
                               companyDetails = new Companydetails
                               {
                                   companyid = g.Key.CompanyId,
                                   companyName = g.Key.CompanyName,
                                   companyaddress = g.Key.AddressLine,
                                   companyphoneNo = g.Key.Phone,
                                   companycity = g.Key.City,
                                   companyemail = g.Key.EmailId,
                                   companystate = g.Key.States,
                                   companypincode = g.Key.Pincode,
                                   customerdetails = new[]
                                   {
                                   new Customerdetail
                                   {
                                       customername = g.Key.FirstName + " " + g.Key.LastName,
                                       custoomeraddress = g.Key.customerAddress,
                                       customerphoneNo = g.Key.customerPhone,
                                       customercity = g.Key.customerCity,
                                       customeremail = g.Key.customerEmail,
                                       customerstate = g.Key.customerState,
                                       customerpincode = g.Key.customerPincode,
                                       itemDetails = g.Select(x => new Itemdetail
                                       {
                                           invoiceNo = x.id.InvoiceNo,
                                           invocieDate = x.i.InvoiceDate.ToString(),
                                           itemName = x.id.ItemName,
                                           quantity = x.id.Quantity,
                                           price = x.id.Price,
                                           totalAmount = x.id.NetAmt
                                       }).ToArray()
                                   }
                               }
                               }
                           };

                var result = data.ToList();

                return result;
            }
            catch (Exception ex)
            {
                var exc = new ErrorLog
                {
                    errorMsg = "ErrorMsg : " + ex.Message + "\r\n" + ex.StackTrace,
                    methodName = nameof(GetInvoiceItemDetails),
                };
                errorLogRepo.WriteErrorLog(exc);
            }
            return null;
        }

       
    }
}

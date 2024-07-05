using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
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
                var customeId = dbContext.Customers
                    .Where(y => (y.FirstName + " " + y.LastName) == invoices.CustomerName)
                    .Select(x =>x.CustomerId).FirstOrDefault();

                
                invoices.CustomerId = customeId;

                var response = dbContext.Invoices.Add(invoices);
                await dbContext.SaveChangesAsync();

                return HttpStatusCode.Created.ToString();
            }
            catch (Exception ex)
            {
                var exc = new ErrorLog
                {
                    errorMsg = "ErrorMsg : " + ex.InnerException + "\r\n" + ex.StackTrace,
                    methodName = nameof(CreateInvoice)
                };

                errorLogRepo.WriteErrorLog(exc);
                int da = (int)HttpStatusCode.InternalServerError;
                return da.ToString();

            }
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
                    var itemDetails = dbContext.ItemsDetails.Where(u => invoiceNumbers.Contains(u.invoiceNo)).ToList();

                    // Creating the list of ViewInvoice
                    var viewInvoices = invoicelist.Select(invoice => new ViewInvoice
                    {
                        InvoiceDate = invoice.InvoiceDate,
                        //CustomerName = invoice.CustomerName,
                        InvoiceNo = invoice.InvoiceNo,
                        NetAmt = itemDetails
                                    .Where(item => item.invoiceNo == invoice.InvoiceNo)
                                    .Sum(item => item.Amount)
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

        //public IList<PDFTemplate> GetInvoiceItemDetails(List<int> invoicenos)
        //{
        //    //try
        //    //{



        //    //    //var data = from id in dbContext.ItemsDetails
        //    //    //           join i in dbContext.Invoices on id.invoiceNo equals i.InvoiceNo
        //    //    //           join ua in dbContext.UserAccount on i.CompanyId equals ua.CompanyId
        //    //    //           join c in dbContext.Customers on ua.CompanyId equals c.CompanyId
        //    //    //           where invoicenos.Contains(i.InvoiceNo ?? 0) &&
        //    //    //                 i.CustomerName.Contains(c.FirstName + " " + c.LastName)
        //    //    //           group new { id, i, ua, c } by new
        //    //    //           {
        //    //    //               ua.CompanyId,
        //    //    //               ua.CompanyName,
        //    //    //               ua.AddressLine,
        //    //    //               ua.Phone,
        //    //    //               ua.City,
        //    //    //               ua.EmailId,
        //    //    //               ua.States,
        //    //    //               ua.Pincode,
        //    //    //               c.FirstName,
        //    //    //               c.LastName,
        //    //    //               customerAddress = c.AddressLine,
        //    //    //               customerPhone = c.Phone,
        //    //    //               customerCity = c.City,
        //    //    //               customerEmail = c.EmailId,
        //    //    //               customerState = c.States,
        //    //    //               customerPincode = c.Pincode
        //    //    //           } into g
        //    //    //           select new PDFTemplate
        //    //    //           {
        //    //    //               companyDetails = new Companydetails
        //    //    //               {
        //    //    //                   companyid = g.Key.CompanyId,
        //    //    //                   companyName = g.Key.CompanyName,
        //    //    //                   companyaddress = g.Key.AddressLine,
        //    //    //                   companyphoneNo = g.Key.Phone,
        //    //    //                   companycity = g.Key.City,
        //    //    //                   companyemail = g.Key.EmailId,
        //    //    //                   companystate = g.Key.States,
        //    //    //                   companypincode = g.Key.Pincode,
        //    //    //                   customerdetails = new[]
        //    //    //                   {
        //    //    //                   new Customerdetail
        //    //    //                   {
        //    //    //                       customername = g.Key.FirstName + " " + g.Key.LastName,
        //    //    //                       custoomeraddress = g.Key.customerAddress,
        //    //    //                       customerphoneNo = g.Key.customerPhone,
        //    //    //                       customercity = g.Key.customerCity,
        //    //    //                       customeremail = g.Key.customerEmail,
        //    //    //                       customerstate = g.Key.customerState,
        //    //    //                       customerpincode = g.Key.customerPincode,
        //    //    //                       itemDetails = g.Select(x => new Itemdetail
        //    //    //                       {
        //    //    //                           invoiceNo = x.id.invoiceNo,
        //    //    //                           invocieDate = x.i.InvoiceDate.ToString(),
        //    //    //                           itemName = x.id.itemName,
        //    //    //                           quantity = x.id.quantity,
        //    //    //                           price = x.id.price,
        //    //    //                           totalAmount = x.id.NetAmt
        //    //    //                       }).ToArray()
        //    //    //                   }
        //    //    //               }
        //    //    //               }
        //    //    //           };

        //    //    //var result = data.ToList();


        //    //    var intermediateData = from id in dbContext.ItemsDetails
        //    //                           join i in dbContext.Invoices on id.invoiceNo equals i.InvoiceNo
        //    //                           join ua in dbContext.UserAccount on i.CompanyId equals ua.CompanyId
        //    //                           join c in dbContext.Customers on ua.CompanyId equals c.CompanyId
        //    //                           where invoicenos.Contains(i.InvoiceNo ?? 0) &&
        //    //                                 (i.CustomerName ?? "").Contains((c.FirstName ?? "") + " " + (c.LastName ?? ""))
        //    //                           select new { id, i, ua, c };

        //    //    var intermediateResult = intermediateData.ToList();

        //    //    var data = from id in dbContext.ItemsDetails
        //    //               join i in dbContext.Invoices on id.invoiceNo equals i.InvoiceNo
        //    //               join ua in dbContext.UserAccount on i.CompanyId equals ua.CompanyId
        //    //               join c in dbContext.Customers on ua.CompanyId equals c.CompanyId
        //    //               where invoicenos.Contains(i.InvoiceNo ?? 0) &&
        //    //                     (i.CustomerName ?? "").Contains((c.FirstName ?? "") + " " + (c.LastName ?? ""))
        //    //               group new { id, i, ua, c } by new
        //    //               {
        //    //                   ua.CompanyId,
        //    //                   ua.CompanyName,
        //    //                   ua.AddressLine,
        //    //                   ua.Phone,
        //    //                   ua.City,
        //    //                   ua.EmailId,
        //    //                   ua.States,
        //    //                   ua.Pincode,
        //    //                   c.FirstName,
        //    //                   c.LastName,
        //    //                   customerAddress = c.AddressLine,
        //    //                   customerPhone = c.Phone,
        //    //                   customerCity = c.City,
        //    //                   customerEmail = c.EmailId,
        //    //                   customerState = c.States,
        //    //                   customerPincode = c.Pincode
        //    //               }
        //    //            into g
        //    //            select g;
                       
        //    //    //        select new PDFTemplate
        //    //    //        {
        //    //    //            companyDetails = new Companydetails
        //    //    //            {
        //    //    //                companyid = g.Key.CompanyId,
        //    //    //                companyName = g.Key.CompanyName,
        //    //    //                companyaddress = g.Key.AddressLine,
        //    //    //                companyphoneNo = g.Key.Phone,
        //    //    //                companycity = g.Key.City,
        //    //    //                companyemail = g.Key.EmailId,
        //    //    //                companystate = g.Key.States,
        //    //    //                companypincode = g.Key.Pincode,
        //    //    //                customerdetails = new[]
        //    //    //                {
        //    //    //    new Customerdetail
        //    //    //    {
        //    //    //        customername = g.Key.FirstName + " " + g.Key.LastName,
        //    //    //        custoomeraddress = g.Key.customerAddress,
        //    //    //        customerphoneNo = g.Key.customerPhone,
        //    //    //        customercity = g.Key.customerCity,
        //    //    //        customeremail = g.Key.customerEmail,
        //    //    //        customerstate = g.Key.customerState,
        //    //    //        customerpincode = g.Key.customerPincode,
        //    //    //        itemDetails = g.Select(x => new Itemdetail
        //    //    //        {
        //    //    //            invoiceNo = x.id.invoiceNo,
        //    //    //            invocieDate = x.i.InvoiceDate.ToString(),
        //    //    //            itemName = x.id.itemName,
        //    //    //            quantity = x.id.quantity,
        //    //    //            price = x.id.price,
        //    //    //            totalAmount = x.id.NetAmt
        //    //    //        }).ToArray()
        //    //    //    }
        //    //    //}
        //    //    //            }
        //    //    //        };
        //    //    var result = data.ToArray();


        //    //    var list_ = result.ToList();
        //    //    return (IList<PDFTemplate>)list_;
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    var exc = new ErrorLog
        //    //    {
        //    //        errorMsg = "ErrorMsg : " + ex.Message + "\r\n" + ex.StackTrace,
        //    //        methodName = nameof(GetInvoiceItemDetails),
        //    //    };
        //    //    errorLogRepo.WriteErrorLog(exc);
        //    //}
        //    return null;
        //}

       
    }
}

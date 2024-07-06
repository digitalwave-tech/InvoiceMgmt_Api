using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http;
using System.Security.Claims;

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

                    var customerIds = invoicelist.Select(x => x.CustomerId).Distinct().ToList();
                    var customers = dbContext.Customers
                        .Where(c => customerIds.Contains(c.CustomerId))
                        .ToList();

                    // Creating the list of ViewInvoice
                    var viewInvoices = invoicelist.Select(invoice => new ViewInvoice
                    {
                        InvoiceDate = invoice.InvoiceDate,
                        CustomerName = customers
                                .Where(c => c.CustomerId == invoice.CustomerId)
                                .Select(c => c.FirstName + " " + c.LastName)
                                .FirstOrDefault() ?? "Unknown",
                        InvoiceNo = invoice.InvoiceNo,
                        NetAmt = invoice.TotalAmt
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

        public Task<string> BindInvoice(InvoiceRequest invoiceRequest)
        {
            var customer_Id = dbContext.Customers
                   .Where(y => (y.FirstName + " " + y.LastName) == invoiceRequest.customerName)
                   .Select(x => x.CustomerId).FirstOrDefault();

            Invoices invoices = new Invoices()
            {
                CompanyId = invoiceRequest.companyId,
                CustomerId = customer_Id,
                SubTotal = invoiceRequest.subtotal,
                GstRate = invoiceRequest.gstRate,
                TotalAmt = invoiceRequest.totalAmt,
                Details = invoiceRequest.Details
            };
               var result= CreateInvoice(invoices);
            return result;

        }
        public IList<PDFTemplate> GetInvoiceItemDetails(List<int?> invoicenos)
        {
            try
            {

                var query = from id in dbContext.ItemsDetails
                            where invoicenos.Contains(id.invoiceNo)  // Filter by invoicenos list
                            join i in dbContext.Invoices on id.invoiceNo equals i.InvoiceNo
                            join c in dbContext.Customers on i.CustomerId equals c.CustomerId
                            join ua in dbContext.UserAccount on c.CompanyId equals ua.CompanyId
                            group new { id, i, c, ua } by new { ua.CompanyId, ua.CompanyName, ua.AddressLine, ua.Phone, ua.City, ua.States, ua.Pincode, ua.EmailId } into companyGroup
                            select new PDFTemplate
                            {
                                companyDetails = new Companydetails
                                {
                                    companyid = companyGroup.Key.CompanyId,
                                    companyName = companyGroup.Key.CompanyName,
                                    companyaddress = companyGroup.Key.AddressLine,
                                    companyphoneNo = companyGroup.Key.Phone,
                                    companycity = companyGroup.Key.City,
                                    companystate = companyGroup.Key.States,
                                    companypincode = companyGroup.Key.Pincode,
                                    companyemail = companyGroup.Key.EmailId,
                                    customerdetails = (
                                        from customerGroup in companyGroup
                                        group customerGroup by new { customerGroup.c.CustomerId, customerGroup.c.FirstName, customerGroup.c.LastName, customerGroup.c.AddressLine, customerGroup.c.Phone, customerGroup.c.City, customerGroup.c.States, customerGroup.c.Pincode, customerGroup.c.EmailId } into groupedCustomers
                                        select new Customerdetail
                                        {
                                            customername = groupedCustomers.Key.FirstName + " " + groupedCustomers.Key.LastName,
                                            custoomeraddress = groupedCustomers.Key.AddressLine,
                                            customerphoneNo = groupedCustomers.Key.Phone,
                                            customercity = groupedCustomers.Key.City,
                                            customerstate = groupedCustomers.Key.States,
                                            customerpincode = groupedCustomers.Key.Pincode,
                                            customeremail = groupedCustomers.Key.EmailId,
                                            itemDetails = groupedCustomers.Select(item => new Itemdetail
                                            {
                                                invoiceNo = item.id.invoiceNo,
                                                invocieDate = item.i.InvoiceDate,
                                                itemName = item.id.itemName,
                                                quantity = item.id.quantity,
                                                price = item.id.price,
                                                Amount = item.id.Amount,
                                                gstRate = item.i.GstRate,
                                                subtotalAmt = item.i.SubTotal,
                                                netAmount = item.i.TotalAmt,
                                            }).ToArray()
                                        }).ToArray()
                                }
                            };

                return query.ToList();
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

using DAL.Entities;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IInvoice
    {
        Task<string> CreateInvoice(Invoices invoices);
        IList<ViewInvoice> GetInvoices(string gstNo);
        IList<PDFTemplate> GetInvoiceItemDetails(List<int> invoiceno);
    }
}

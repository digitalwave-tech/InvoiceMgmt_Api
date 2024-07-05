using DAL.Entities;
using DAL.Interfaces;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
//using PdfSharpCore;
//using PdfSharpCore.Drawing;
//using PdfSharpCore.Pdf;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.IO;
using iTextSharp.text;
using iTextSharp.tool.xml;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using Document = iTextSharp.text.Document;
using iTextSharp;
using System.Net;

namespace InvoiceManagement.Controllers
{

    [ApiController]
    [Route("api/controller")]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration configuration;
        
        private ICustomer customer_ { get; }
        private IInvoice invoice_ { get; }
        private IErrorLogging errorLogging_ { get; }
        public UserController(IConfiguration _config, ICustomer customer, IInvoice invoice_, IErrorLogging errorLogging)
        {
            configuration = _config;
            this.customer_ = customer;
            this.invoice_ = invoice_;
            this.errorLogging_ = errorLogging;
        }

        [HttpPost("login")]
        public IActionResult Login(string gstNo)
        {
            try
            {
                var user = customer_.GetUserByGstNo(gstNo);
                if (user != null)
                {
                    var claims = new[]
                   {
                        new Claim(ClaimTypes.Name,gstNo),
                        new Claim(ClaimTypes.Email,user.EmailId),
                        new Claim(ClaimTypes.Actor,user.CompanyId.ToString())
                    };
                    var token = GenerateToken(claims);
                    var response = new { success = true, Token = token };

                    return Ok(response);
                }
                else
                {
                    var responseData = new { success = false, message = "User Not Found" };
                    return NotFound(responseData);
                }
            }
            catch (Exception ex)
            {
                var exc = new ErrorLog
                {
                    errorMsg = "ErrorMsg : " + ex.InnerException + "\r\n" + ex.StackTrace,
                    methodName = nameof(Login),
                };
                errorLogging_.WriteErrorLog(exc);
                return BadRequest(ex.Message);
            }

        }

        private string GenerateToken(Claim[] claims)
        {
            //try
            //{


                var tokenHandler = new JwtSecurityTokenHandler();
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                        issuer: configuration["Jwt:Issuer"],
                        audience: configuration["Jwt:Audience"],
                        claims: claims,
                        expires: DateTime.Now.AddMinutes(200),
                        signingCredentials: credentials

                    );
                return new JwtSecurityTokenHandler().WriteToken(token);
            //}
            //catch (Exception ex)
            //{
            //    var exc = new ErrorLog
            //    {
            //        errorMsg = ex.Message,
            //        methodName = ex.GetType().Name,
            //    };
            //    errorLogging_.WriteErrorLog(exc);
            //}
        }


        [HttpPost("CreateUserAccount")]
        public IActionResult CreateUserAcct([FromBody] UserAccount user)
        {
            try
            {
                user.CompanyId = Guid.NewGuid();
                var user_ = customer_.CreateUserAccount(user);

                var response = new { success = true, msg = user_.Result };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var exc = new ErrorLog
                {
                    errorMsg = "ErrorMsg : " + ex.InnerException + "\r\n" + ex.StackTrace,
                    methodName = nameof(CreateUserAcct),
                };
                errorLogging_.WriteErrorLog(exc);
                return StatusCode((int)HttpStatusCode.InternalServerError);
                //return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("CreateCustomer")]
        public IActionResult CreateCustomer([FromBody] Customers customers)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                var companyId = identity.FindFirst(ClaimTypes.Actor)?.Value;
                if (string.IsNullOrEmpty(companyId) || !Guid.TryParse(companyId, out Guid company_id))
                {
                    return BadRequest("Invalid or missing companyId claim.");
                }
                customers.CompanyId = company_id != null ? company_id : Guid.Empty;
                var user_ = customer_.CreateCustomer(customers);

                var response = new { success = true, msg = user_.Result };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var exc = new ErrorLog
                {
                    errorMsg = "ErrorMsg : " + ex.InnerException + "\r\n" + ex.StackTrace,
                    methodName = nameof(CreateCustomer),
                };
                errorLogging_.WriteErrorLog(exc);
                return StatusCode((int)HttpStatusCode.InternalServerError);
                //return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("CreateInvoice")]
        public IActionResult CreateInvoice([FromBody] Invoices invoices)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                var companyId = identity.FindFirst(ClaimTypes.Actor)?.Value;
                if (string.IsNullOrEmpty(companyId) || !Guid.TryParse(companyId, out Guid company_id))
                {
                    return BadRequest("Invalid or missing companyId claim.");
                }
                invoices.CompanyId = company_id;

                foreach (var item in invoices.Details)
                {
                    item.invoiceNo = invoices.InvoiceNo;
                }

                var user_ = invoice_.CreateInvoice(invoices);
                if (user_.Result == "500")
                {
                    return BadRequest(user_.Result);
                }
                else
                {
                    var response = new { success = true, msg = user_.Result };
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                var exc = new ErrorLog
                {
                    errorMsg = "ErrorMsg : " + ex.InnerException + "\r\n" + ex.StackTrace,
                    methodName = nameof(CreateInvoice),
                };
                errorLogging_.WriteErrorLog(exc);
                return StatusCode((int)HttpStatusCode.InternalServerError);
                //return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("GetInvoicesByGSTNo")]
        public IActionResult GetInvoicesByGSTNo()
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                var gstNoClaim = identity?.FindFirst(ClaimTypes.Name)?.Value;

                var user_ = invoice_.GetInvoices(gstNoClaim);
                if(user_ == null)
                {
                    return BadRequest(user_);
                }
                else
                {
                    var response = new { success = true, msg = user_ };
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                var exc = new ErrorLog
                {
                    errorMsg = "ErrorMsg : " + ex.InnerException + "\r\n" + ex.StackTrace,
                    methodName = nameof(GetInvoicesByGSTNo),
                };
                errorLogging_.WriteErrorLog(exc);
                return StatusCode((int)HttpStatusCode.InternalServerError);
                //return BadRequest(ex.Message);
            }

        }

        [Authorize]
        [HttpGet("GetCustomerNameByGstNo")]
        public IActionResult GetCustomerNameByGstNo()
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                var gstNoClaim = identity?.FindFirst(ClaimTypes.Name)?.Value;

                var user_ = customer_.GetCustomeNameByGstNo(gstNoClaim);

                var response = new { success = true, msg = user_ };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var exc = new ErrorLog
                {
                    errorMsg = "ErrorMsg : " + ex.InnerException + "\r\n" + ex.StackTrace,
                    methodName = nameof(GetCustomerNameByGstNo),
                };
                errorLogging_.WriteErrorLog(exc);
                return StatusCode((int)HttpStatusCode.InternalServerError);
                //return BadRequest(ex.Message);
            }
        }

        //[Authorize]
        //[HttpPost("GetInvoiceItemDetails")]
        //public async Task<IActionResult> GetInvoiceItemDetails([FromBody] List<int> invoice_No)
        //{
        //    try
        //    {
        //        var pDFTemplate = invoice_.GetInvoiceItemDetails(invoice_No);

        //        string logoPath = "C:\\InvoiceMgmt\\InvoiceManagement\\Images\\logo-png.png";

        //        string invoicedate = pDFTemplate[0].companyDetails.customerdetails[0].itemDetails[0].invocieDate;

        //        string htmlContent = "<div style = 'margin: 20px auto; height:1000px; max-width: 600px; padding: 20px; border: 1px solid #ccc; background-color: #FFFFFF; font-family: Arial, sans-serif;'>";

        //        htmlContent += "<div style = 'text-align: center; margin-bottom: 20px;'>";
        //        htmlContent += "<h1>Invoice</h1>";
        //        htmlContent += "</div>";

        //        htmlContent += "<div style = 'margin-bottom: 20px; text-align: center;'>";
        //        htmlContent += "<img src = " + logoPath + " alt = 'Company Logo' style = 'max-width: 100px; margin-bottom: 10px; height:80px' />";
        //        htmlContent += "</div>";

        //        htmlContent += "<h3 style = 'margin: 0;' >From</h3>";
        //        htmlContent += "<p style = 'margin: 0;' >" + pDFTemplate[0].companyDetails.companyName + "</p>";
        //        htmlContent += "<p style = 'margin: 0;' >" + pDFTemplate[0].companyDetails.companyaddress + "," + pDFTemplate[0].companyDetails.companypincode + "</p>";
        //        htmlContent += "<p style = 'margin: 0;' > " + pDFTemplate[0].companyDetails.companycity + "," + pDFTemplate[0].companyDetails.companystate + " </p>";
        //        htmlContent += "<p style = 'margin: 0;' > " + "Email : " + pDFTemplate[0].companyDetails.companyemail + " </p>";
        //        htmlContent += "<p style = 'margin: 0;' > " + "Phone No : " + pDFTemplate[0].companyDetails.companyphoneNo + " </p>";
        //        htmlContent += "<br/>";

        //        htmlContent += "<div style='display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 20px;'>";
        //        htmlContent += "<div style='width: 48%;'>";
        //        htmlContent += "<h3>Bill To</h3>";
        //        htmlContent += "<p>" + pDFTemplate[0].companyDetails.customerdetails[0].customername + "</p>";
        //        htmlContent += "<p>" + pDFTemplate[0].companyDetails.customerdetails[0].custoomeraddress + ", " + pDFTemplate[0].companyDetails.customerdetails[0].customerpincode + "</p>";
        //        htmlContent += "<p>" + pDFTemplate[0].companyDetails.customerdetails[0].customercity + ", " + pDFTemplate[0].companyDetails.customerdetails[0].customerstate + "</p>";
        //        htmlContent += "<p>Email: " + pDFTemplate[0].companyDetails.customerdetails[0].customeremail + "</p>";
        //        htmlContent += "<p>Phone No: " + pDFTemplate[0].companyDetails.customerdetails[0].customerphoneNo + "</p>";
        //        htmlContent += "</div>";

        //        htmlContent += "<div style='width: 260px; float:right; display:flex;  '>";
        //        htmlContent += "<h3>Invoice : INV_" + pDFTemplate[0].companyDetails.customerdetails[0].itemDetails[0].invoiceNo + "</h3>";
        //        htmlContent += "<p>Invoice Date : " + invoicedate + "</p>";
        //        htmlContent += "</div>";
        //        htmlContent += "</div>";



        //        htmlContent += "<table style = 'cellspacing:0px; cellpadding:2px;'>";
        //        htmlContent += "<thead>";
        //        htmlContent += "<tr>";
        //        htmlContent += "<th style = 'padding: 20px; widht:25%; text-align: center; border-bottom: 1px solid #ddd;' > Item Name </th>";
        //        htmlContent += "<th style = 'padding: 20px; width:10%; text-align: center; border-bottom: 1px solid #ddd;' > Quantity </th>";
        //        htmlContent += "<th style = 'padding: 20px; width:10%; text-align: center; border-bottom: 1px solid #ddd;' > Unit Price </th>";
        //        htmlContent += "<th style = 'padding: 20px;width:10%; text-align: center; border-bottom: 1px solid #ddd;' > Amount </th>";
        //        htmlContent += "</tr><hr/>";
        //        htmlContent += "</thead>";

        //        htmlContent += "<tbody>";
        //        decimal? net_totalAmount = 0;
        //        foreach (var customer in pDFTemplate[0].companyDetails.customerdetails)
        //        {
        //            foreach (var item in customer.itemDetails)
        //            {
        //                htmlContent += "<tr>";
        //                htmlContent += "<td style='padding: 20px; width:25%; text-align: left; border-bottom: 1px solid #ddd;'>" + item.itemName + "</td>";
        //                htmlContent += "<td style='padding: 20px; width:15%; text-align: left; border-bottom: 1px solid #ddd;'>" + item.quantity + "</td>";
        //                htmlContent += "<td style='padding: 20px; width:15%; text-align: left; border-bottom: 1px solid #ddd;'>" + item.price + "</td>";
        //                htmlContent += "<td style='padding: 20px; width:15%; text-align: left; border-bottom: 1px solid #ddd;'>Rs." + " " + item.totalAmount + "</td>";
        //                htmlContent += "</tr>";
        //                net_totalAmount += item.totalAmount;
        //            }
        //        }
        //        htmlContent += "</tbody>";
        //        htmlContent += "<tfoot style='width:100%;'>";
        //        htmlContent += "<tr>";
        //        htmlContent += "<td  style = 'padding: 20px; font-weight: bold;'></td>";
        //        htmlContent += "<td  style = 'padding: 20px; font-weight: bold;'></td>";
        //        htmlContent += "<td  style = 'padding: 20px; font-weight: bold;'>Total Amount :</td>";
        //        htmlContent += "<td colspan='4' style = 'padding: 20px;width:100%; float:right; border-top: 1px solid #ddd;' > Rs" + net_totalAmount + "</td>";
        //        htmlContent += "</tr>";
        //        htmlContent += "</tfoot>";

        //        htmlContent += "</table>";
        //        htmlContent += "</div>";

        //        using (MemoryStream memoryStream = new MemoryStream())
        //        {
        //            Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
        //            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
        //            pdfDoc.Open();

        //            using (StringReader sr = new StringReader(htmlContent))
        //            {
        //                XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
        //            }

        //            pdfDoc.Close();
        //            byte[] bytes = memoryStream.ToArray();
        //            memoryStream.Close();

        //            string fileName = "Invoice.pdf";
        //            return File(bytes, "application/pdf", fileName);

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var exc = new ErrorLog
        //        {
        //            errorMsg = "ErrorMsg : " + ex.InnerException + "\r\n" + ex.StackTrace,
        //            methodName = nameof(GetInvoiceItemDetails),
        //        };
        //        errorLogging_.WriteErrorLog(exc);
        //        return StatusCode((int)HttpStatusCode.InternalServerError);
        //        //return BadRequest(ex.Message);
        //    }
        //}
    }

}

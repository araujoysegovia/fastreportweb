using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FastReport;

using FastReport.Export.Image;
using FastReport.Export.Html;
using FastReport.Export.PdfSimple;
using FastReport.Utils;
using FastReport.Data;
using FRWeb.Models;
using System.Web.Hosting;
using System.Data;
using System.IO;
using System.Net.Http.Headers;

namespace FRWeb.Controllers
{
    // The class of parameters in the query
    public class ReportQuery
    {
        // Format of resulting report: png, pdf, html
        public string Format { get; set; }
        // Value of "Parameter" variable in report
        public string Fuente { get; set; }
        public string Documento { get; set; }
        public string DsnId { get; set; }
        // Enable Inline preview in browser (generates "inline" or "attachment")
        public bool Inline { get; set; }
    }

   

    public class ReportsController : ApiController
    { // Reports list
        Reports[] reportItems = new Reports[]
        {
         new Reports { Id = 1, ReportName = "Box.frx" },
         new Reports { Id = 2, ReportName = "Barcode.frx" },
         new Reports { Id = 3, ReportName = "IMPRIDCTO.frx" },
         new Reports { Id = 4, ReportName = "EGRESO07.frx" }
        };

        Dsn[] dsnItems = new Dsn[]
       {
         new Dsn { Id = 1, DataSource= @"Data Source=192.168.100.5\SEGOVIA;AttachDbFilename=;Initial Catalog=mca;Integrated Security=False;Persist Security Info=True;User ID=sa;Password=75080508360" },
         new Dsn { Id = 2, DataSource= @"Data Source=192.168.100.5\SEGOVIA;AttachDbFilename=;Initial Catalog=Contabilidad;Integrated Security=False;Persist Security Info=True;User ID=sa;Password=75080508360" }

       };


        // Get reports list
        public IEnumerable<Reports> GetAllReports()
        {
            return reportItems;
        }

        // Get report on ID from request
        public HttpResponseMessage GetReportById(int id, [FromUri] ReportQuery query)
        {
            // Find report
            Reports reportItem = reportItems.FirstOrDefault((p) => p.Id == id);
            if (reportItem != null)
            {
                string reportPath = HostingEnvironment.MapPath("~/App_Data/" + reportItem.ReportName);
                //string dataSource = 
                MemoryStream stream = new MemoryStream();
                try
                {
                   
                        //Enable web mode
                        Config.WebMode = true;
                        using (Report report = new Report())
                        {
                            report.Load(reportPath); //Load report

                        // preguntar por el datasource
                        if (query.DsnId != null)
                        {

                            Dsn dsnItem = dsnItems.FirstOrDefault((p) => p.Id == id);
                            if (dsnItem != null)
                            {
                                report.Dictionary.Connections[0].ConnectionString = dsnItem.DataSource;
                            }

                         }

                            

                            if (query.Fuente != null)
                            {
                                report.SetParameterValue("FUENTE", query.Fuente); //  Fuente, The value we take from the URL
                            }
                            if (query.Documento != null)
                            {
                                report.SetParameterValue("DOCUMENTO", query.Documento); // # Documento. The value we take from the URL
                            }
                        // Two phases of preparation to exclude the display of any dialogs
                        report.Prepare();
                           

                            if (query.Format == "pdf")
                            {
                                //Export in PDF
                                PDFSimpleExport pdf = new PDFSimpleExport();
                                // We use the flow to store the report, so as not to produce files
                                report.Export(pdf, stream);
                            }
                            else if (query.Format == "html")
                            {
                                // Export in HTML
                                HTMLExport html = new HTMLExport();
                                html.SinglePage = true;
                                html.Navigator = false;
                                html.EmbedPictures = true;
                                report.Export(html, stream);
                            }
                            else
                            {
                                // Export in picture
                                ImageExport img = new ImageExport();
                                img.ImageFormat = ImageExportFormat.Png;
                                img.SeparateFiles = false;
                                img.ResolutionX = 96;
                                img.ResolutionY = 96;
                                report.Export(img, stream);
                                query.Format = "png";
                            }
                        }
                  
                    // Create result variable
                    HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(stream.ToArray())
                    };
                    stream.Dispose();

                    result.Content.Headers.ContentDisposition =
                    new System.Net.Http.Headers.ContentDispositionHeaderValue(query.Inline ? "inline" : "attachment")
                    {
                        // Specify the file extension depending on the type of export 
                        FileName = String.Concat(Path.GetFileNameWithoutExtension(reportPath), ".", query.Format)
 };
                    // Determine the type of content for the browser
                    result.Content.Headers.ContentType =
                     new MediaTypeHeaderValue("application/" + query.Format);
                    return result;
                }
                // We handle exceptions
                catch
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }
            }
            else
                return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System.Configuration;
using System.Text.RegularExpressions;

namespace SJKP.AzureSearch.PdfIndexer
{
    public class Functions
    {        
        public static void IndexPdfDocument([BlobTrigger("documents/{name}.{ext}")] Stream input, string name, string ext)
        {
            if (ext.ToLower() != "pdf")
            {
                return;
            }
            List<IndexAction> actions = new List<IndexAction>();

            using (PdfReader reader = new PdfReader(input))
            {
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    actions.Add(new IndexAction(IndexActionType.MergeOrUpload, new Document()
                    {
                        {"filename",name+"."+ext } ,
                        {"id",MakeSafeId(name+"_"+i+ "."+ext)},
                        {"content", PdfTextExtractor.GetTextFromPage(reader, i) },
                        {"page", i }
                    }));
                }
            }

            var client = new SearchIndexClient("sjkp", "documents", new SearchCredentials(ConfigurationManager.AppSettings["SearchApiKey"]));

            
            for (int i = 0; i < (int)Math.Ceiling(actions.Count / 1000.0); i++)
            {
                client.Documents.Index(new IndexBatch(actions.Skip(i*1000).Take(actions.Count-(i*1000))));
            }            
        }

        private static string MakeSafeId(string input) {
            Regex rgx = new Regex("[^a-zA-Z0-9_]");
            return rgx.Replace(input, "");           

        }
    }
}

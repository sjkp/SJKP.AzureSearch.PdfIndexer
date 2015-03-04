using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System.Configuration;

namespace SJKP.AzureSearch.PdfIndexer
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            Setup();
            var host = new JobHost();
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }

        private static void Setup()
        {
            var searchClient = new SearchServiceClient("sjkp", new SearchCredentials(ConfigurationManager.AppSettings["SearchApiKey"]));
            var indexName = "documents";
            var indexExists = searchClient.Indexes.ListNames().Any(s => s == indexName);
            if (indexExists)
            {
                Console.WriteLine("Index {0} exists", indexName);
                return;
            }
                    

            var task = searchClient.Indexes.CreateAsync(new Microsoft.Azure.Search.Models.Index()
            {
                Name = indexName,
                Fields = new Field[] {
                        new Field("filename", DataType.String, AnalyzerName.EnLucene) {
                            IsRetrievable = true,
                            IsSearchable = true,
                        },
                        new Field("content", DataType.String, AnalyzerName.EnLucene) {
                            IsSearchable = true,
                        },
                        new Field("id", DataType.String) {
                            IsKey = true,
                        },
                        new Field("page", DataType.Int32) {
                            IsSortable = true,                            
                            IsFilterable = true
                        }
                    }


            });

            Task.WaitAll(task);
            Console.WriteLine("Result of index creation: " + task.Result.StatusCode);
        }
    }
}

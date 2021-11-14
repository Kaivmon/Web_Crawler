using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using HtmlAgilityPack;
using System.Text;
using System.Data.Odbc;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Basic_Web_Crawler {

    /// <summary>
    /// This program needs to have the following
    /// - Input website URL
    /// - Select Next page button and move through all results
    /// - 
    /// </summary>
    class Program {
        static void Main(string[] args) {
            startCrawlerAsync();
            Console.ReadLine();
        }

        private static async Task startCrawlerAsync() {

            SqlConnection myConnection = new SqlConnection("user id=username;" +
                                               "password=password;" +
                                               "server=(localdb)\\MSSQLLocalDB;" +
                                               "Trusted_Connection=yes;" +
                                               "database=Web_Crawler; " +
                                               "connection timeout=30");
            try {
                myConnection.Open();
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
            Console.ReadLine();


            // INDEED Crawler

            var url = "https://ca.indeed.com/jobs?q=software+developer&filter=0";
            var newRL = url;
            var httpClient = new HttpClient();
            var htmlDocument = new HtmlDocument();

            var jobs = new List<Job>();
            int index = 0;
            do {
                var html = await httpClient.GetStringAsync(newRL);
                htmlDocument.LoadHtml(html);

                var links = htmlDocument.DocumentNode.Descendants("a").Where(node => node.GetAttributeValue("class", "").Contains("tapItem fs-unmask result")).ToList();
                foreach (var link in links) {
                    var job = new Job();
                    job.Title = link.Descendants("span").Where(node => node.GetAttributeValue("title", "").Any()).FirstOrDefault().InnerText;
                    job.Company = link.Descendants("span").Where(node => node.GetAttributeValue("class", "").Equals("companyName")).FirstOrDefault().InnerText;
                    job.Location = link.Descendants("div").Where(node => node.GetAttributeValue("class", "").Equals("companyLocation")).FirstOrDefault().InnerText;
                    job.Salary = GetSalary(link);
                    job.Description = GetDescription(link);
                    job.Link = "https://ca.indeed.com" + link.GetAttributeValue("href", "").ToString();

                    jobs.Add(job);


                    //Prepare Insert Query by using variables 
                    string InsertQuery = "";
                    InsertQuery = " Insert into dbo.Table(Title, Company, Location, Salary, Description, Link)";
                    InsertQuery += "VALUES (" + job.Title + "," + job.Company + "," + job.Location + "," + job.Salary + "," + job.Description + "," + job.Link + ")";

                    //Execute Insert Query to Insert Data into SQL Server Table 
                    SqlCommand cmd = myConnection.CreateCommand();
                    cmd.CommandText = InsertQuery;
                    try {
                        myConnection.Open();
                    }
                    catch (Exception e) {
                        Console.WriteLine(e.ToString());
                    }
                    cmd.ExecuteNonQuery();
                    myConnection.Close();



                    Console.WriteLine(job.Title);
                    Console.WriteLine(job.Company);
                    Console.WriteLine(job.Location);
                    Console.WriteLine(job.Salary);
                    Console.WriteLine(job.Description);
                    Console.WriteLine(job.Link + "\n\n");

                }
                index += 10;
                newRL = url + "&start=" + index;
            } while (index < 1000);

            try { 
                myConnection.Close();
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        private static string GetSalary(HtmlNode link) {
            if (link.Descendants("div").Where(node => node.HasClass("salary-snippet")).FirstOrDefault() != null) {
                string salaryM = link.Descendants("div").Where(node => node.HasClass("salary-snippet")).FirstOrDefault().InnerHtml.ToString();
                char[] charsToTrimStart = { '<', 's', 'p', 'a', 'n', '>' };
                char[] charsToTrimEnd = { '<', '/', 's', 'p', 'a', 'n', '>' };
                salaryM = salaryM.TrimStart(charsToTrimStart);
                salaryM = salaryM.TrimEnd(charsToTrimEnd);
                return salaryM;
            }
            else {
                return "Salary N/A";
            }
        }

        private static string GetDescription(HtmlNode link) {
            var descriptions = link.Descendants("li").ToList();
            string description = "";
            foreach (var d in descriptions) {
                description += d.InnerText;
                description += " ~~~ ";
            }
            return description;
        }
    }
}

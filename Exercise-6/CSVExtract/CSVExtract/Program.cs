using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Linq;

namespace CSVExtract
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("Processing input csv files!");
            bool result = false;

            DataTable filesData = ReadFiles();
            if (filesData != null && filesData.Rows.Count > 0)
            {
                DataTable processedData = ProcessData(filesData);
                if (processedData != null && processedData.Rows.Count > 0)
                    result = WriteFiles(processedData);

                if (result)
                    Console.WriteLine("Files are processed successfully");
                else
                    Console.WriteLine("Files are not processed");
            }
            else
            {
                Console.WriteLine("No valid files found");
            }
            Console.ReadLine();
        }

        /// <summary>
        /// Read the CSV files into a datatable
        /// </summary>
        /// <returns></returns>
        static DataTable ReadFiles()
        {
            DataTable dtInsurance = new DataTable("Insurance");

            if (Directory.Exists(Path.GetFullPath("Inputfiles")))
            {
                foreach (String file in Directory.GetFiles(Path.GetFullPath("Inputfiles"), "*.csv", System.IO.SearchOption.AllDirectories))
                {
                    DataTable dtCSV = GetDataTabletFromCSVFile(file);
                    dtInsurance.Merge(dtCSV);
                    dtInsurance.AcceptChanges();
                }
            }
            return dtInsurance;
        }

        /// <summary>
        /// For filtering the user with latest version
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        static DataTable ProcessData(DataTable dataTable)
        {
            dataTable.DefaultView.Sort = "[Insurance Company], [Last Name], [First Name] asc";
            var queryVersionMax =
                       from t in dataTable.DefaultView.ToTable().AsEnumerable()
                       group t by new { C = t.Field<string>("Insurance Company"), LN = t.Field<string>("Last Name"), FN = t.Field<string>("First Name"), U = t.Field<string>("User Id") } into insuranceGroup
                       select new
                       {
                           UserId = insuranceGroup.Key.U,
                           FullName = insuranceGroup.Key.LN + " " + insuranceGroup.Key.FN,
                           Version =
                                   (from c in insuranceGroup
                                    select c.Field<string>("Version")).Max(),
                           Company = insuranceGroup.Key.C
                       };
            return queryVersionMax.CopyToDataTable();
        }

        /// <summary>
        /// For writing individual insurance files
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        static bool WriteFiles(DataTable dataTable)
        {
            StringBuilder sb = new StringBuilder();
            IEnumerable<string> columnNames = dataTable.Columns.Cast<DataColumn>().
                                                Select(column => column.ColumnName);

            sb.AppendLine(string.Join(",", columnNames));
            string sCompany = string.Empty;
            foreach (DataRow row in dataTable.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                if (sCompany.Equals(row["Company"]) || string.IsNullOrEmpty(sCompany))
                {
                    sb.AppendLine(string.Join(",", fields));
                }
                else
                {
                    File.WriteAllText(Path.GetFullPath("Outputfiles") + "/" + sCompany + ".csv", sb.ToString());
                    sb.Clear();
                    sb.AppendLine(string.Join(",", columnNames));
                    sb.AppendLine(string.Join(",", fields));
                }
                sCompany = row["Company"].ToString();
            }

            if (!String.IsNullOrEmpty(sb.ToString()))
                File.WriteAllText(Path.GetFullPath("Outputfiles") + "/" + sCompany + ".csv", sb.ToString());

            return true;
        }

        /// <summary>
        /// For reading the CSV file
        /// </summary>
        /// <param name="csv_file_path"></param>
        /// <returns></returns>
        private static DataTable GetDataTabletFromCSVFile(string csv_file_path)
        {

            DataTable dtCSV = new DataTable();
            int iNameIndex = -1;
            try
            {
                using (StreamReader sr = new StreamReader(csv_file_path))
                {
                    dtCSV = new DataTable();
                    string header = sr.ReadLine().Trim();
                    string[] headers = header.Substring(1, header.Length-2).Split(',');

                    for (int i = 0; i < headers.Count(); i++)
                    {
                        //break the full name column into two name columns 
                        if (headers[i] == "First and Last Name")
                        {
                            iNameIndex = i;
                            dtCSV.Columns.Add("Last Name");
                            dtCSV.Columns.Add("First Name");
                        }
                        else
                            dtCSV.Columns.Add(headers[i]);
                    }
                    
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine().Trim();
                        //skip the escape chars and split the line by comma
                        string[] cols = line.Substring(1,line.Length-2).Split(','); 
                        DataRow dr = dtCSV.NewRow();

                        for (int i = 0; i < cols.Count(); i++)
                        {
                            //split the First and Last Name column by space
                            if ( i == iNameIndex)
                            {
                                string[] name = cols[i].Split(" ");
                                dr[i] = name[1];
                                dr[i+1] = name[0];
                            }
                            else if (i > iNameIndex)
                            {
                                dr[i+1] = cols[i];
                            }
                            else
                                dr[i] = cols[i];

                        }

                        dtCSV.Rows.Add(dr);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Not able to process the file " + csv_file_path + " " + ex.Message);
            }
            return dtCSV;
        }
    }

    /// <summary>
    /// For extension method
    /// </summary>
    public static class CustomLINQtoDataSetMethods
    {
        public static DataTable CopyToDataTable<T>(this IEnumerable<T> source)
        {
            return new ObjectShredder<T>().Shred(source, null, null);
        }

        public static DataTable CopyToDataTable<T>(this IEnumerable<T> source,
                                                    DataTable table, LoadOption? options)
        {
            return new ObjectShredder<T>().Shred(source, table, options);
        }
    }
}

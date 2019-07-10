using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using CSVtoDB.Models;
using Microsoft.EntityFrameworkCore;


namespace CSVtoDB
{
    class Program
    {
        static void Main(string[] args)
        {
            var _context = new dbContext();
            
            //THIS WILL EMPTY THE DB 
            // List<Employee> allFromDb = _context.employees.ToList();
            // _context.employees.RemoveRange(allFromDb);
            // _context.SaveChanges();

            List<string> filenames = new List<string>{"Sample1_Stable.csv", "Sample2_Missing_Data.csv", "Sample3_Incorrect_Types.csv", "Sample4_Delimeter_Change.csv"};
            
            //loop through file names, adding them to db if they don't fail to read
            foreach(string file in filenames)
            {
                List<Employee> employeesToAdd = new List<Employee>();
                //get emloyees from csv
                try
                {  
                    employeesToAdd = csvParser(file);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                    //I tried to get it to log the location in the csv (char#) as well, but failed to navigate the exception object successfully
                }

                //this will add the employees if the CsvReader didn't throw execption (list would remain empty if so)
                foreach(Employee e in employeesToAdd)
                {
                    _context.employees.Add(e);
                }
                _context.SaveChanges();
            }
            
            
             
            List<Employee> allFromDb = _context.employees.ToList();
            System.Console.WriteLine(allFromDb.Count());

            System.Console.WriteLine("done");

        }

        public static List<Employee> csvParser(string csvName)
        {
            using (var reader = new StreamReader(csvName))
            using (var csv = new CsvReader(reader))
            {   
                //use helper method to identify delimeter being used by csv
                List<char> delimters = new List<char>{',', '|', ';'};
                string currentDelimeter = Detect(reader, 1, delimters).ToString();
                
                //helper method used the first line to identify the deleter
                //this causes the reader to start from line 2 (not the header)
                //this will reset the stream back to this beginning
                reader.DiscardBufferedData();
                reader.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                
                //set delimeter
                csv.Configuration.Delimiter = currentDelimeter;
                
                //makes sure we map the correct csv headers to model 
                csv.Configuration.RegisterClassMap<EmployeeMap>();

                //read csv and map records onto list of objects
                List<Employee> records = new List<Employee>();
                try
                {
                    records = csv.GetRecords<Employee>().ToList();
                }
                //ideally, we would log the exception and skip the erroneos row so we could continue mapping the rest of the file
                //reading forums like this : https://github.com/JoshClose/CsvHelper/issues/137
                //indicate this should be possible, but i was only able to get it partially working with setting MissingFieldException = null, which i decided was not what i wanted to do
                //I was unable to figure out how to handle the bad row and keep parsing the remaining rows
                //The end result is that it rejects the entire file read
                catch(CsvHelper.MissingFieldException ex)
                {
                    throw ex;
                }
                catch (CsvHelper.TypeConversion.TypeConverterException ex)
                {
                    throw ex;
                }
                

                //print how many objects
                System.Console.WriteLine(records.ToList().Count());

                return records;
                
            }
        }

        //maps the headers of the csv file to the model of employee
        public class EmployeeMap : ClassMap<Employee>
        {
            public EmployeeMap()
            {
                //removed id, as it was flagged as database generated, so multiple entries of same file can be used
                Map(m => m.FirstName).Name("first_name");
                Map(m => m.LastName).Name("last_name");
                Map(m => m.Email).Name("email");
                Map(m => m.Address).Name("address_line_1");
                Map(m => m.City).Name("city");
                Map(m => m.Zipcode).Name("zip");
                Map(m => m.Country).Name("country");
                Map(m => m.EmploymentAge).Name("employment_age");
            }
        }

        //parsess the csv file, given the stream reader and how many rows to parce, and what delimetors to look for
        //THIS IS NOT MY CODE, i found it here: https://www.codeproject.com/Articles/231582/Auto-detect-CSV-separator
        public static char Detect(TextReader reader, int rowCount, IList<char> separators)
        {
            IList<int> separatorsCount = new int[separators.Count];
            int character;
            int row = 0;
            bool quoted = false;
            bool firstChar = true;
            while (row < rowCount)
            {
                character = reader.Read();
                switch (character)
                {
                    case '"':
                        if (quoted)
                        {
                            if (reader.Peek() != '"') // Value is quoted and 
                            // current character is " and next character is not ".
                                quoted = false;
                            else
                                reader.Read(); // Value is quoted and current and 
                            // next characters are "" - read (skip) peeked qoute.
                        }
                        else
                        {
                            if (firstChar)  // Set value as quoted only if this quote is the 
                            // first char in the value.
                                quoted = true;
                        }
                        break;
                    case '\n':
                        if (!quoted)
                        {
                            ++row;
                            firstChar = true;
                            continue;
                        }
                        break;
                    case -1:
                        row = rowCount;
                        break;
                    default:
                        if (!quoted)
                        {
                            int index = separators.IndexOf((char)character);
                            if (index != -1)
                            {
                                ++separatorsCount[index];
                                firstChar = true;
                                continue;
                            }
                        }
                        break;
                }
                if (firstChar)
                    firstChar = false;
            }
            int maxCount = separatorsCount.Max();
            return maxCount == 0 ? '\0' : separators[separatorsCount.IndexOf(maxCount)];
        }
    }
}

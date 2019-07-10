using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSVtoDB.Models
{
    public class Employee
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id {get; set;}

        public string FirstName {get; set;}
        
        public string LastName {get; set;}

        public string Email  {get; set;}

        public string Address  {get; set;}

        public string City  {get; set;}

        public string Zipcode  {get; set;}

        public string Country  {get; set;}

        public int EmploymentAge  {get; set;}

        public Employee()
        {

        }
    }
}
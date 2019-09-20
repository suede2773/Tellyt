using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tellyt
{
  public class InputUserAccount
  {
    public string Email { get; set; }
    public string Password { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string DisplayName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PostalCode { get; set; }
    public string State { get; set; }
    public string StreetAddress { get; set; }
    public string ObjectId { get; set; }
    public string Error { get; set; }
    public string CompanyName { get; set; }
    public string AccountKey { get; set; }
    public bool IsValid { get; set; }
  }
}
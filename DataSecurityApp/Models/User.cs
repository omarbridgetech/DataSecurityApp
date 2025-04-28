namespace DataSecurityApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // "Admin" or "User"

        // New fields for credit card info
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PersonalId { get; set; } // Israeli ID
        public string CreditCardNumber { get; set; }
        public string ValidDate { get; set; } // MM/YY or MM/YYYY
        public string CVC { get; set; }
    }
}

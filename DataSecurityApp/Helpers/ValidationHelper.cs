// Helpers/ValidationHelper.cs
using System.Text.RegularExpressions;

namespace DataSecurityApp.Helpers
{
    public static class ValidationHelper
    {
        // Israeli ID validation (9 digits)
        public static bool IsValidIsraeliId(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            return Regex.IsMatch(id, @"^\d{9}$");
        }

        // Credit card number validation (16 digits, can have spaces every 4 digits)
        public static bool IsValidCreditCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber)) return false;
            // Remove spaces for validation
            string sanitized = Regex.Replace(cardNumber, @"\s", "");
            // Check if it's 16 digits
            return Regex.IsMatch(sanitized, @"^\d{16}$");
        }

        public static bool IsValidExpirationDate(string date)
        {
            if (string.IsNullOrEmpty(date)) return false;

            // First check the format MM/YY or MM/YYYY
            if (!Regex.IsMatch(date, @"^(0[1-9]|1[0-2])\/([0-9]{2}|[0-9]{4})$"))
                return false;

            string[] parts = date.Split('/');
            int month = int.Parse(parts[0]);
            int year = int.Parse(parts[1]);

            // If it's a 2-digit year, convert to 4-digit by adding 2000
            if (parts[1].Length == 2)
                year += 2000;

            // Check month between 1-12
            if (month < 1 || month > 12)
                return false;

            // Check year between 2025-2099
            if (year < 2025 || year > 2099)
                return false;

            return true;
        }

        // CVC validation (3 or 4 digits)
        public static bool IsValidCVC(string cvc)
        {
            if (string.IsNullOrEmpty(cvc)) return false;
            return Regex.IsMatch(cvc, @"^\d{3,4}$");
        }

        public static bool IsValidName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            // Allow letters, spaces, and hyphens (for compound names like "Anne-Marie")
            return Regex.IsMatch(name, @"^[a-zA-Z\s\-]+$");
        }
    }
}
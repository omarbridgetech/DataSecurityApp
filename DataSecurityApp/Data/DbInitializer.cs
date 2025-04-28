// Data/DbInitializer.cs

using DataSecurityApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DataSecurityApp.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.Migrate();

            // Check if we already have at least 10 users
            if (context.Users.Count() >= 10)
            {
                // We already have enough users, but we need to make sure they have credit card info
                UpdateExistingUsersWithCreditCardInfo(context);
                return;
            }

            // We need to add more users to reach 10
            // First, let's check if the admin user exists
            var adminExists = context.Users.Any(u => u.Role == "Admin");

            // Create a list of users to add
            var usersToAdd = new List<User>();

            // If admin doesn't exist, add one
            if (!adminExists)
            {
                usersToAdd.Add(new User
                {
                    Username = "admin",
                    Email = "admin@example.com",
                    PasswordHash = HashPassword("Admin123!"),
                    Role = "Admin",
                    FirstName = "Admin",
                    LastName = "User",
                    PersonalId = "123456789",
                    CreditCardNumber = "1234 5567 8901 2345",
                    ValidDate = "12/32",
                    CVC = "123"
                });
            }

            // Add regular users until we reach 10 total users
            int existingUserCount = context.Users.Count();
            int usersNeeded = 10 - existingUserCount;

            for (int i = 1; i <= usersNeeded; i++)
            {
                // Create a unique username based on existing count
                string userName = $"user{existingUserCount + i}";

                usersToAdd.Add(new User
                {
                    Username = userName,
                    Email = $"{userName}@example.com",
                    PasswordHash = HashPassword("User1234!"),
                    Role = "User",
                    FirstName = GetRandomFirstName(),
                    LastName = GetRandomLastName(),
                    PersonalId = GenerateRandomIsraeliId(),
                    CreditCardNumber = GenerateRandomCreditCard(),
                    ValidDate = GenerateRandomValidDate(),
                    CVC = GenerateRandomCVC()
                });
            }

            // Add the new users to the database
            if (usersToAdd.Any())
            {
                context.Users.AddRange(usersToAdd);
                context.SaveChanges();
            }

            // Update existing users with credit card info if they don't have it
            UpdateExistingUsersWithCreditCardInfo(context);
        }

        private static void UpdateExistingUsersWithCreditCardInfo(AppDbContext context)
        {
            // Get all users that don't have credit card info
            var usersWithoutCreditInfo = context.Users
                .Where(u => string.IsNullOrEmpty(u.CreditCardNumber))
                .ToList();

            foreach (var user in usersWithoutCreditInfo)
            {
                // Only update the credit card fields, leave other fields as is
                user.FirstName = user.FirstName ?? GetRandomFirstName();
                user.LastName = user.LastName ?? GetRandomLastName();
                user.PersonalId = user.PersonalId ?? GenerateRandomIsraeliId();
                user.CreditCardNumber = GenerateRandomCreditCard();
                user.ValidDate = GenerateRandomValidDate();
                user.CVC = GenerateRandomCVC();
            }

            if (usersWithoutCreditInfo.Any())
            {
                context.SaveChanges();
            }
        }

        private static string HashPassword(string password)
        {
            // Using SHA-256 as required in the specifications
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Helper methods to generate random data
        private static string GetRandomFirstName()
        {
            string[] firstNames = { "John", "Jane", "Michael", "Emily", "David", "Sarah", "Robert", "Jennifer", "Thomas", "Lisa", "Daniel", "Rachel", "Matthew", "Michelle" };
            return firstNames[new Random().Next(firstNames.Length)];
        }

        private static string GetRandomLastName()
        {
            string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Miller", "Davis", "Wilson", "Taylor", "Anderson", "Cohen", "Levy", "Rosenberg", "Goldberg" };
            return lastNames[new Random().Next(lastNames.Length)];
        }

        private static string GenerateRandomIsraeliId()
        {
            // Israeli IDs are 9 digits
            var random = new Random();
            return random.Next(100000000, 999999999).ToString();
        }

        private static string GenerateRandomCreditCard()
        {
            var random = new Random();
            return $"{random.Next(1000, 9999)} {random.Next(1000, 9999)} {random.Next(1000, 9999)} {random.Next(1000, 9999)}";
        }

        private static string GenerateRandomValidDate()
        {
            var random = new Random();
            int month = random.Next(1, 13);
            int year = random.Next(25, 35); // 2025-2035
            return $"{month:D2}/{year}";
        }

        private static string GenerateRandomCVC()
        {
            var random = new Random();
            return random.Next(100, 1000).ToString();
        }
    }
}
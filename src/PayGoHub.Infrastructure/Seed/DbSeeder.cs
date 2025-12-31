using PayGoHub.Domain.Entities;
using PayGoHub.Domain.Enums;
using PayGoHub.Infrastructure.Data;

namespace PayGoHub.Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(PayGoHubDbContext context)
    {
        if (context.Customers.Any())
            return;

        var customers = GetCustomers();
        await context.Customers.AddRangeAsync(customers);
        await context.SaveChangesAsync();

        var devices = GetDevices();
        await context.Devices.AddRangeAsync(devices);
        await context.SaveChangesAsync();

        var installations = GetInstallations(customers, devices);
        await context.Installations.AddRangeAsync(installations);
        await context.SaveChangesAsync();

        var loans = GetLoans(customers);
        await context.Loans.AddRangeAsync(loans);
        await context.SaveChangesAsync();

        var payments = GetPayments(customers);
        await context.Payments.AddRangeAsync(payments);
        await context.SaveChangesAsync();
    }

    private static List<Customer> GetCustomers()
    {
        return new List<Customer>
        {
            new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Kamau",
                Email = "jane.kamau@email.com",
                PhoneNumber = "+254712345678",
                Region = "Nairobi",
                District = "Westlands",
                Address = "123 Waiyaki Way",
                Status = CustomerStatus.Active
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "Peter",
                LastName = "Otieno",
                Email = "peter.otieno@email.com",
                PhoneNumber = "+254723456789",
                Region = "Kisumu",
                District = "Kisumu Central",
                Address = "456 Oginga Odinga Street",
                Status = CustomerStatus.Active
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "Mary",
                LastName = "Wanjiku",
                Email = "mary.wanjiku@email.com",
                PhoneNumber = "+254734567890",
                Region = "Nakuru",
                District = "Nakuru East",
                Address = "789 Kenyatta Avenue",
                Status = CustomerStatus.Active
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Mwangi",
                Email = "john.mwangi@email.com",
                PhoneNumber = "+254745678901",
                Region = "Mombasa",
                District = "Mvita",
                Address = "321 Moi Avenue",
                Status = CustomerStatus.Active
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "Grace",
                LastName = "Njeri",
                Email = "grace.njeri@email.com",
                PhoneNumber = "+254756789012",
                Region = "Eldoret",
                District = "Eldoret East",
                Address = "654 Uganda Road",
                Status = CustomerStatus.Active
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "David",
                LastName = "Kipchoge",
                Email = "david.kipchoge@email.com",
                PhoneNumber = "+254767890123",
                Region = "Nairobi",
                District = "Karen",
                Address = "987 Karen Road",
                Status = CustomerStatus.Active
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "Sarah",
                LastName = "Achieng",
                Email = "sarah.achieng@email.com",
                PhoneNumber = "+254778901234",
                Region = "Kisumu",
                District = "Kisumu West",
                Address = "147 Jomo Kenyatta Highway",
                Status = CustomerStatus.Active
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "Michael",
                LastName = "Omondi",
                Email = "michael.omondi@email.com",
                PhoneNumber = "+254789012345",
                Region = "Nakuru",
                District = "Nakuru West",
                Address = "258 Biashara Street",
                Status = CustomerStatus.Active
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "Ann",
                LastName = "Chebet",
                Email = "ann.chebet@email.com",
                PhoneNumber = "+254790123456",
                Region = "Eldoret",
                District = "Eldoret Central",
                Address = "369 Nandi Road",
                Status = CustomerStatus.Active
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "James",
                LastName = "Mutua",
                Email = "james.mutua@email.com",
                PhoneNumber = "+254701234567",
                Region = "Mombasa",
                District = "Nyali",
                Address = "741 Links Road",
                Status = CustomerStatus.Active
            }
        };
    }

    private static List<Device> GetDevices()
    {
        return new List<Device>
        {
            new Device { Id = Guid.NewGuid(), SerialNumber = "SHS-001-2024", Model = "SHS-80W", Status = DeviceStatus.Active, BatteryHealth = 95 },
            new Device { Id = Guid.NewGuid(), SerialNumber = "SHS-002-2024", Model = "SHS-120W", Status = DeviceStatus.Active, BatteryHealth = 92 },
            new Device { Id = Guid.NewGuid(), SerialNumber = "SHS-003-2024", Model = "SHS-150W", Status = DeviceStatus.Active, BatteryHealth = 88 },
            new Device { Id = Guid.NewGuid(), SerialNumber = "SHS-004-2024", Model = "SHS-200W", Status = DeviceStatus.Active, BatteryHealth = 100 },
            new Device { Id = Guid.NewGuid(), SerialNumber = "SHS-005-2024", Model = "SHS-80W", Status = DeviceStatus.Active, BatteryHealth = 97 },
            new Device { Id = Guid.NewGuid(), SerialNumber = "SHS-006-2024", Model = "SHS-120W", Status = DeviceStatus.Active, BatteryHealth = 85 },
            new Device { Id = Guid.NewGuid(), SerialNumber = "SHS-007-2024", Model = "SHS-150W", Status = DeviceStatus.Inactive, BatteryHealth = 78 },
            new Device { Id = Guid.NewGuid(), SerialNumber = "SHS-008-2024", Model = "SHS-200W", Status = DeviceStatus.Active, BatteryHealth = 91 },
            new Device { Id = Guid.NewGuid(), SerialNumber = "SHS-009-2024", Model = "SHS-80W", Status = DeviceStatus.Active, BatteryHealth = 99 },
            new Device { Id = Guid.NewGuid(), SerialNumber = "SHS-010-2024", Model = "SHS-120W", Status = DeviceStatus.Faulty, BatteryHealth = 45 }
        };
    }

    private static List<Installation> GetInstallations(List<Customer> customers, List<Device> devices)
    {
        var today = DateTime.UtcNow.Date;
        return new List<Installation>
        {
            new Installation
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[0].Id,
                DeviceId = devices[0].Id,
                SystemType = "SHS-80W",
                Status = InstallationStatus.Completed,
                ScheduledDate = today.AddDays(-30),
                CompletedDate = today.AddDays(-30),
                Location = "Westlands, Nairobi",
                TechnicianName = "John Technician"
            },
            new Installation
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[1].Id,
                DeviceId = devices[1].Id,
                SystemType = "SHS-120W",
                Status = InstallationStatus.Completed,
                ScheduledDate = today.AddDays(-25),
                CompletedDate = today.AddDays(-25),
                Location = "Kisumu Central",
                TechnicianName = "Peter Installer"
            },
            new Installation
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[2].Id,
                DeviceId = devices[2].Id,
                SystemType = "SHS-150W",
                Status = InstallationStatus.Scheduled,
                ScheduledDate = today,
                Location = "Nakuru East",
                TechnicianName = "James Tech"
            },
            new Installation
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[3].Id,
                DeviceId = devices[3].Id,
                SystemType = "SHS-200W",
                Status = InstallationStatus.Scheduled,
                ScheduledDate = today.AddDays(1),
                Location = "Mvita, Mombasa",
                TechnicianName = "Grace Engineer"
            },
            new Installation
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[4].Id,
                DeviceId = devices[4].Id,
                SystemType = "SHS-80W",
                Status = InstallationStatus.Pending,
                ScheduledDate = today.AddDays(3),
                Location = "Eldoret East",
                TechnicianName = "David Solar"
            },
            new Installation
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[5].Id,
                DeviceId = devices[5].Id,
                SystemType = "SHS-120W",
                Status = InstallationStatus.InProgress,
                ScheduledDate = today,
                Location = "Karen, Nairobi",
                TechnicianName = "Ann Technician"
            }
        };
    }

    private static List<Loan> GetLoans(List<Customer> customers)
    {
        var today = DateTime.UtcNow.Date;
        return new List<Loan>
        {
            new Loan
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[0].Id,
                Amount = 25000,
                InterestRate = 12.5m,
                Status = LoanStatus.Active,
                IssuedDate = today.AddMonths(-6),
                DueDate = today.AddMonths(6),
                RemainingBalance = 15000
            },
            new Loan
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[1].Id,
                Amount = 35000,
                InterestRate = 12.5m,
                Status = LoanStatus.Active,
                IssuedDate = today.AddMonths(-3),
                DueDate = today.AddMonths(9),
                RemainingBalance = 28000
            },
            new Loan
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[2].Id,
                Amount = 45000,
                InterestRate = 10.0m,
                Status = LoanStatus.Active,
                IssuedDate = today.AddMonths(-1),
                DueDate = today.AddMonths(11),
                RemainingBalance = 42000
            },
            new Loan
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[3].Id,
                Amount = 55000,
                InterestRate = 10.0m,
                Status = LoanStatus.Pending,
                IssuedDate = today,
                DueDate = today.AddMonths(12),
                RemainingBalance = 55000
            },
            new Loan
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[4].Id,
                Amount = 20000,
                InterestRate = 15.0m,
                Status = LoanStatus.PaidOff,
                IssuedDate = today.AddMonths(-12),
                DueDate = today.AddMonths(-1),
                RemainingBalance = 0
            },
            new Loan
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[5].Id,
                Amount = 30000,
                InterestRate = 12.5m,
                Status = LoanStatus.Active,
                IssuedDate = today.AddMonths(-4),
                DueDate = today.AddMonths(8),
                RemainingBalance = 22000
            },
            new Loan
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[6].Id,
                Amount = 40000,
                InterestRate = 12.5m,
                Status = LoanStatus.Active,
                IssuedDate = today.AddMonths(-2),
                DueDate = today.AddMonths(10),
                RemainingBalance = 36000
            },
            new Loan
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[7].Id,
                Amount = 18000,
                InterestRate = 15.0m,
                Status = LoanStatus.Defaulted,
                IssuedDate = today.AddMonths(-18),
                DueDate = today.AddMonths(-6),
                RemainingBalance = 12000
            }
        };
    }

    private static List<Payment> GetPayments(List<Customer> customers)
    {
        var now = DateTime.UtcNow;
        return new List<Payment>
        {
            new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[0].Id,
                Amount = 2500,
                Currency = "KES",
                Status = PaymentStatus.Completed,
                Method = PaymentMethod.Mpesa,
                TransactionReference = "TXN001",
                MpesaReceiptNumber = "QJK7R2BXYZ",
                PaidAt = now.AddMinutes(-5)
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[1].Id,
                Amount = 3500,
                Currency = "KES",
                Status = PaymentStatus.Completed,
                Method = PaymentMethod.Mpesa,
                TransactionReference = "TXN002",
                MpesaReceiptNumber = "QJK8S3CABC",
                PaidAt = now.AddMinutes(-30)
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[2].Id,
                Amount = 5000,
                Currency = "KES",
                Status = PaymentStatus.Completed,
                Method = PaymentMethod.Bank,
                TransactionReference = "TXN003",
                PaidAt = now.AddHours(-1)
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[3].Id,
                Amount = 4200,
                Currency = "KES",
                Status = PaymentStatus.Pending,
                Method = PaymentMethod.Mpesa,
                TransactionReference = "TXN004"
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[4].Id,
                Amount = 1800,
                Currency = "KES",
                Status = PaymentStatus.Completed,
                Method = PaymentMethod.Mpesa,
                TransactionReference = "TXN005",
                MpesaReceiptNumber = "QJK9T4DDEF",
                PaidAt = now.AddHours(-2)
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[5].Id,
                Amount = 6500,
                Currency = "KES",
                Status = PaymentStatus.Completed,
                Method = PaymentMethod.Cash,
                TransactionReference = "TXN006",
                PaidAt = now.AddHours(-3)
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[6].Id,
                Amount = 3000,
                Currency = "KES",
                Status = PaymentStatus.Failed,
                Method = PaymentMethod.Mpesa,
                TransactionReference = "TXN007"
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[7].Id,
                Amount = 2200,
                Currency = "KES",
                Status = PaymentStatus.Completed,
                Method = PaymentMethod.Mpesa,
                TransactionReference = "TXN008",
                MpesaReceiptNumber = "QJK1U5EGHI",
                PaidAt = now.AddHours(-4)
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[0].Id,
                Amount = 2500,
                Currency = "KES",
                Status = PaymentStatus.Completed,
                Method = PaymentMethod.Mpesa,
                TransactionReference = "TXN009",
                MpesaReceiptNumber = "QJK2V6FJKL",
                PaidAt = now.AddDays(-1)
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[1].Id,
                Amount = 3500,
                Currency = "KES",
                Status = PaymentStatus.Completed,
                Method = PaymentMethod.Mpesa,
                TransactionReference = "TXN010",
                MpesaReceiptNumber = "QJK3W7GMNO",
                PaidAt = now.AddDays(-1)
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[2].Id,
                Amount = 4500,
                Currency = "KES",
                Status = PaymentStatus.Completed,
                Method = PaymentMethod.Mpesa,
                TransactionReference = "TXN011",
                MpesaReceiptNumber = "QJK4X8HPQR",
                PaidAt = now.AddDays(-2)
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[3].Id,
                Amount = 5500,
                Currency = "KES",
                Status = PaymentStatus.Completed,
                Method = PaymentMethod.Bank,
                TransactionReference = "TXN012",
                PaidAt = now.AddDays(-2)
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[8].Id,
                Amount = 2800,
                Currency = "KES",
                Status = PaymentStatus.Completed,
                Method = PaymentMethod.Mpesa,
                TransactionReference = "TXN013",
                MpesaReceiptNumber = "QJK5Y9ISTU",
                PaidAt = now.AddDays(-3)
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[9].Id,
                Amount = 3200,
                Currency = "KES",
                Status = PaymentStatus.Completed,
                Method = PaymentMethod.Mpesa,
                TransactionReference = "TXN014",
                MpesaReceiptNumber = "QJK6Z0JVWX",
                PaidAt = now.AddDays(-3)
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[4].Id,
                Amount = 1500,
                Currency = "KES",
                Status = PaymentStatus.Completed,
                Method = PaymentMethod.Cash,
                TransactionReference = "TXN015",
                PaidAt = now.AddDays(-4)
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[5].Id,
                Amount = 4000,
                Currency = "KES",
                Status = PaymentStatus.Completed,
                Method = PaymentMethod.Mpesa,
                TransactionReference = "TXN016",
                MpesaReceiptNumber = "QJK7A1KYZA",
                PaidAt = now.AddDays(-5)
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[6].Id,
                Amount = 2000,
                Currency = "KES",
                Status = PaymentStatus.Completed,
                Method = PaymentMethod.Mpesa,
                TransactionReference = "TXN017",
                MpesaReceiptNumber = "QJK8B2LZAB",
                PaidAt = now.AddDays(-6)
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[7].Id,
                Amount = 3800,
                Currency = "KES",
                Status = PaymentStatus.Completed,
                Method = PaymentMethod.Bank,
                TransactionReference = "TXN018",
                PaidAt = now.AddDays(-7)
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[8].Id,
                Amount = 2100,
                Currency = "KES",
                Status = PaymentStatus.Completed,
                Method = PaymentMethod.Mpesa,
                TransactionReference = "TXN019",
                MpesaReceiptNumber = "QJK9C3MABC",
                PaidAt = now.AddDays(-8)
            },
            new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = customers[9].Id,
                Amount = 4800,
                Currency = "KES",
                Status = PaymentStatus.Completed,
                Method = PaymentMethod.Mpesa,
                TransactionReference = "TXN020",
                MpesaReceiptNumber = "QJK0D4NBCD",
                PaidAt = now.AddDays(-10)
            }
        };
    }
}

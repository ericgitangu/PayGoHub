using PayGoHub.Domain.Entities;
using PayGoHub.Domain.Enums;
using PayGoHub.Infrastructure.Data;

namespace PayGoHub.Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(PayGoHubDbContext context)
    {
        // Seed providers first
        if (!context.Providers.Any())
        {
            var providers = GetProviders();
            await context.Providers.AddRangeAsync(providers);
            await context.SaveChangesAsync();
        }

        if (context.Customers.Any())
            return;

        var customers = GetMultiMarketCustomers();
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

        var payments = GetMultiMarketPayments(customers);
        await context.Payments.AddRangeAsync(payments);
        await context.SaveChangesAsync();
    }

    private static List<Provider> GetProviders()
    {
        return new List<Provider>
        {
            // Uganda
            new Provider { ProviderKey = "ug_mtn_mobilemoney", DisplayName = "MTN Mobile Money", Country = "UG", Currency = "UGX", IsActive = true, MinAmountSubunit = 50000, MaxAmountSubunit = 500000000 },
            new Provider { ProviderKey = "ug_airtel_money", DisplayName = "Airtel Money", Country = "UG", Currency = "UGX", IsActive = true, MinAmountSubunit = 50000, MaxAmountSubunit = 300000000 },
            // Kenya
            new Provider { ProviderKey = "ke_safaricom_mpesa", DisplayName = "M-Pesa Kenya", Country = "KE", Currency = "KES", IsActive = true, MinAmountSubunit = 1000, MaxAmountSubunit = 15000000 },
            // Tanzania
            new Provider { ProviderKey = "tz_vodacom_mpesa", DisplayName = "M-Pesa Tanzania", Country = "TZ", Currency = "TZS", IsActive = true, MinAmountSubunit = 100000, MaxAmountSubunit = 500000000 },
            // Nigeria
            new Provider { ProviderKey = "ng_mtn_momo", DisplayName = "MTN MoMo Nigeria", Country = "NG", Currency = "NGN", IsActive = true, MinAmountSubunit = 10000, MaxAmountSubunit = 100000000 },
            new Provider { ProviderKey = "ng_opay", DisplayName = "OPay", Country = "NG", Currency = "NGN", IsActive = true, MinAmountSubunit = 10000, MaxAmountSubunit = 500000000 },
            // Benin
            new Provider { ProviderKey = "bj_mtn_mobilemoney", DisplayName = "MTN Mobile Money Benin", Country = "BJ", Currency = "XOF", IsActive = true, MinAmountSubunit = 50000, MaxAmountSubunit = 200000000 },
            // Ivory Coast
            new Provider { ProviderKey = "ci_orange_money", DisplayName = "Orange Money", Country = "CI", Currency = "XOF", IsActive = true, MinAmountSubunit = 50000, MaxAmountSubunit = 200000000 },
            new Provider { ProviderKey = "ci_mtn_mobilemoney", DisplayName = "MTN Mobile Money CI", Country = "CI", Currency = "XOF", IsActive = true, MinAmountSubunit = 50000, MaxAmountSubunit = 200000000 },
            // Mozambique
            new Provider { ProviderKey = "mz_mpesa", DisplayName = "M-Pesa Mozambique", Country = "MZ", Currency = "MZN", IsActive = true, MinAmountSubunit = 100000, MaxAmountSubunit = 500000000 },
            new Provider { ProviderKey = "mz_emola", DisplayName = "e-Mola", Country = "MZ", Currency = "MZN", IsActive = true, MinAmountSubunit = 50000, MaxAmountSubunit = 300000000 },
            // Zambia
            new Provider { ProviderKey = "zm_mtn_mobilemoney", DisplayName = "MTN Mobile Money Zambia", Country = "ZM", Currency = "ZMW", IsActive = true, MinAmountSubunit = 1000, MaxAmountSubunit = 10000000 },
            new Provider { ProviderKey = "zm_airtel_money", DisplayName = "Airtel Money Zambia", Country = "ZM", Currency = "ZMW", IsActive = true, MinAmountSubunit = 1000, MaxAmountSubunit = 10000000 }
        };
    }

    private static List<Customer> GetMultiMarketCustomers()
    {
        return new List<Customer>
        {
            // Uganda Customers
            new Customer { Id = Guid.NewGuid(), FirstName = "Stephen", LastName = "KAZIBWE", Email = "stephen.kazibwe@email.ug", PhoneNumber = "+256772345678", Region = "Central", District = "Kampala", Address = "Plot 45 Bombo Road", Status = CustomerStatus.Active, Country = "UG", Currency = "UGX" },
            new Customer { Id = Guid.NewGuid(), FirstName = "Prossy", LastName = "NAKAMYA", Email = "prossy.nakamya@email.ug", PhoneNumber = "+256753456789", Region = "Western", District = "Mbarara", Address = "23 High Street", Status = CustomerStatus.Active, Country = "UG", Currency = "UGX" },
            new Customer { Id = Guid.NewGuid(), FirstName = "Ronald", LastName = "SSENYONJO", Email = "ronald.ssenyonjo@email.ug", PhoneNumber = "+256784567890", Region = "Eastern", District = "Jinja", Address = "17 Nile Avenue", Status = CustomerStatus.Active, Country = "UG", Currency = "UGX" },
            new Customer { Id = Guid.NewGuid(), FirstName = "Harriet", LastName = "NAMUGANZA", Email = "harriet.namuganza@email.ug", PhoneNumber = "+256775678901", Region = "Northern", District = "Gulu", Address = "Plot 8 Acholi Road", Status = CustomerStatus.Active, Country = "UG", Currency = "UGX" },

            // Kenya Customers (Mobisol)
            new Customer { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "KAMAU", Email = "jane.kamau@email.ke", PhoneNumber = "+254712345678", Region = "Nairobi", District = "Westlands", Address = "123 Waiyaki Way", Status = CustomerStatus.Active, Country = "KE", Currency = "KES" },
            new Customer { Id = Guid.NewGuid(), FirstName = "Peter", LastName = "OTIENO", Email = "peter.otieno@email.ke", PhoneNumber = "+254723456789", Region = "Kisumu", District = "Kisumu Central", Address = "456 Oginga Odinga Street", Status = CustomerStatus.Active, Country = "KE", Currency = "KES" },

            // Tanzania Customers (Mobisol)
            new Customer { Id = Guid.NewGuid(), FirstName = "Fatuma", LastName = "MSANGI", Email = "fatuma.msangi@email.tz", PhoneNumber = "+255754321098", Region = "Dar es Salaam", District = "Kinondoni", Address = "45 Bagamoyo Road", Status = CustomerStatus.Active, Country = "TZ", Currency = "TZS" },
            new Customer { Id = Guid.NewGuid(), FirstName = "Juma", LastName = "MWAKASEGE", Email = "juma.mwakasege@email.tz", PhoneNumber = "+255765432109", Region = "Arusha", District = "Arusha Urban", Address = "78 Sokoine Drive", Status = CustomerStatus.Active, Country = "TZ", Currency = "TZS" },

            // Nigeria Customers
            new Customer { Id = Guid.NewGuid(), FirstName = "Chukwuemeka", LastName = "OKONKWO", Email = "chukwuemeka.okonkwo@email.ng", PhoneNumber = "+2348012345678", Region = "South East", District = "Enugu", Address = "15 Ogui Road", Status = CustomerStatus.Active, Country = "NG", Currency = "NGN" },
            new Customer { Id = Guid.NewGuid(), FirstName = "Aisha", LastName = "ABDULLAHI", Email = "aisha.abdullahi@email.ng", PhoneNumber = "+2348023456789", Region = "North Central", District = "Kaduna", Address = "22 Ahmadu Bello Way", Status = CustomerStatus.Active, Country = "NG", Currency = "NGN" },
            new Customer { Id = Guid.NewGuid(), FirstName = "Oluwaseun", LastName = "ADEYEMI", Email = "oluwaseun.adeyemi@email.ng", PhoneNumber = "+2348034567890", Region = "South West", District = "Lagos Island", Address = "56 Marina Street", Status = CustomerStatus.Active, Country = "NG", Currency = "NGN" },

            // Benin Customers
            new Customer { Id = Guid.NewGuid(), FirstName = "Aristide", LastName = "AGOSSOU", Email = "aristide.agossou@email.bj", PhoneNumber = "+22997123456", Region = "Littoral", District = "Cotonou", Address = "Boulevard de la Marina", Status = CustomerStatus.Active, Country = "BJ", Currency = "XOF" },
            new Customer { Id = Guid.NewGuid(), FirstName = "Celestine", LastName = "HOUNKPATIN", Email = "celestine.hounkpatin@email.bj", PhoneNumber = "+22996234567", Region = "Atlantique", District = "Abomey-Calavi", Address = "Route de l'Aeroport", Status = CustomerStatus.Active, Country = "BJ", Currency = "XOF" },

            // Ivory Coast Customers
            new Customer { Id = Guid.NewGuid(), FirstName = "Kouame", LastName = "KOFFI", Email = "kouame.koffi@email.ci", PhoneNumber = "+22507123456", Region = "Abidjan", District = "Cocody", Address = "Rue des Jardins", Status = CustomerStatus.Active, Country = "CI", Currency = "XOF" },
            new Customer { Id = Guid.NewGuid(), FirstName = "Aminata", LastName = "TOURE", Email = "aminata.toure@email.ci", PhoneNumber = "+22508234567", Region = "Yamoussoukro", District = "Centre", Address = "Avenue Houphouet-Boigny", Status = CustomerStatus.Active, Country = "CI", Currency = "XOF" },

            // Mozambique Customers
            new Customer { Id = Guid.NewGuid(), FirstName = "Fernando", LastName = "MACHAVA", Email = "fernando.machava@email.mz", PhoneNumber = "+258841234567", Region = "Maputo", District = "KaMpfumo", Address = "Avenida Julius Nyerere 123", Status = CustomerStatus.Active, Country = "MZ", Currency = "MZN" },
            new Customer { Id = Guid.NewGuid(), FirstName = "Graciela", LastName = "MONDLANE", Email = "graciela.mondlane@email.mz", PhoneNumber = "+258852345678", Region = "Gaza", District = "Xai-Xai", Address = "Rua Eduardo Mondlane", Status = CustomerStatus.Active, Country = "MZ", Currency = "MZN" },

            // Zambia Customers
            new Customer { Id = Guid.NewGuid(), FirstName = "Chanda", LastName = "MWAPE", Email = "chanda.mwape@email.zm", PhoneNumber = "+260971234567", Region = "Lusaka", District = "Lusaka Central", Address = "Cairo Road 456", Status = CustomerStatus.Active, Country = "ZM", Currency = "ZMW" },
            new Customer { Id = Guid.NewGuid(), FirstName = "Bwalya", LastName = "MUMBA", Email = "bwalya.mumba@email.zm", PhoneNumber = "+260962345678", Region = "Copperbelt", District = "Ndola", Address = "Broadway Avenue", Status = CustomerStatus.Active, Country = "ZM", Currency = "ZMW" }
        };
    }

    private static List<Device> GetDevices()
    {
        return new List<Device>
        {
            new Device { Id = Guid.NewGuid(), SerialNumber = "SHS12L/A/BT/243203457", Model = "SHS-80W", Status = DeviceStatus.Active, BatteryHealth = 95, Country = "UG" },
            new Device { Id = Guid.NewGuid(), SerialNumber = "SCBLNX/A/BT/240300126005", Model = "SHS-120W", Status = DeviceStatus.Active, BatteryHealth = 92, Country = "UG" },
            new Device { Id = Guid.NewGuid(), SerialNumber = "SHS-KE-003-2024", Model = "SHS-150W", Status = DeviceStatus.Active, BatteryHealth = 88, Country = "KE" },
            new Device { Id = Guid.NewGuid(), SerialNumber = "SHS-TZ-004-2024", Model = "SHS-200W", Status = DeviceStatus.Active, BatteryHealth = 100, Country = "TZ" },
            new Device { Id = Guid.NewGuid(), SerialNumber = "SHS-NG-005-2024", Model = "SHS-80W", Status = DeviceStatus.Active, BatteryHealth = 97, Country = "NG" },
            new Device { Id = Guid.NewGuid(), SerialNumber = "SHS-BJ-006-2024", Model = "SHS-120W", Status = DeviceStatus.Active, BatteryHealth = 85, Country = "BJ" },
            new Device { Id = Guid.NewGuid(), SerialNumber = "SHS-CI-007-2024", Model = "SHS-150W", Status = DeviceStatus.Inactive, BatteryHealth = 78, Country = "CI" },
            new Device { Id = Guid.NewGuid(), SerialNumber = "SHS-MZ-008-2024", Model = "SHS-200W", Status = DeviceStatus.Active, BatteryHealth = 91, Country = "MZ" },
            new Device { Id = Guid.NewGuid(), SerialNumber = "SHS-ZM-009-2024", Model = "SHS-80W", Status = DeviceStatus.Active, BatteryHealth = 99, Country = "ZM" },
            new Device { Id = Guid.NewGuid(), SerialNumber = "SHS-UG-010-2024", Model = "SHS-120W", Status = DeviceStatus.Faulty, BatteryHealth = 45, Country = "UG" }
        };
    }

    private static List<Installation> GetInstallations(List<Customer> customers, List<Device> devices)
    {
        var today = DateTime.UtcNow.Date;
        return new List<Installation>
        {
            new Installation { Id = Guid.NewGuid(), CustomerId = customers[0].Id, DeviceId = devices[0].Id, SystemType = "SHS-80W", Status = InstallationStatus.Completed, ScheduledDate = today.AddDays(-30), CompletedDate = today.AddDays(-30), Location = "Kampala, Uganda", TechnicianName = "Daniel KIMASSAI" },
            new Installation { Id = Guid.NewGuid(), CustomerId = customers[1].Id, DeviceId = devices[1].Id, SystemType = "SHS-120W", Status = InstallationStatus.Completed, ScheduledDate = today.AddDays(-25), CompletedDate = today.AddDays(-25), Location = "Mbarara, Uganda", TechnicianName = "Albert LUMU" },
            new Installation { Id = Guid.NewGuid(), CustomerId = customers[4].Id, DeviceId = devices[2].Id, SystemType = "SHS-150W", Status = InstallationStatus.Scheduled, ScheduledDate = today, Location = "Nairobi, Kenya", TechnicianName = "Boniface NTARANGWI" },
            new Installation { Id = Guid.NewGuid(), CustomerId = customers[6].Id, DeviceId = devices[3].Id, SystemType = "SHS-200W", Status = InstallationStatus.Scheduled, ScheduledDate = today.AddDays(1), Location = "Dar es Salaam, Tanzania", TechnicianName = "Joshua OCERO" },
            new Installation { Id = Guid.NewGuid(), CustomerId = customers[8].Id, DeviceId = devices[4].Id, SystemType = "SHS-80W", Status = InstallationStatus.Pending, ScheduledDate = today.AddDays(3), Location = "Enugu, Nigeria", TechnicianName = "Aureliu BRINZEANU" },
            new Installation { Id = Guid.NewGuid(), CustomerId = customers[11].Id, DeviceId = devices[5].Id, SystemType = "SHS-120W", Status = InstallationStatus.InProgress, ScheduledDate = today, Location = "Cotonou, Benin", TechnicianName = "Eric GITANGU" }
        };
    }

    private static List<Loan> GetLoans(List<Customer> customers)
    {
        var today = DateTime.UtcNow.Date;
        return new List<Loan>
        {
            // Uganda loans (UGX)
            new Loan { Id = Guid.NewGuid(), CustomerId = customers[0].Id, Amount = 2500000, InterestRate = 12.5m, Status = LoanStatus.Active, IssuedDate = today.AddMonths(-6), DueDate = today.AddMonths(6), RemainingBalance = 1500000, Currency = "UGX" },
            new Loan { Id = Guid.NewGuid(), CustomerId = customers[1].Id, Amount = 3500000, InterestRate = 12.5m, Status = LoanStatus.Active, IssuedDate = today.AddMonths(-3), DueDate = today.AddMonths(9), RemainingBalance = 2800000, Currency = "UGX" },
            // Kenya loans (KES)
            new Loan { Id = Guid.NewGuid(), CustomerId = customers[4].Id, Amount = 45000, InterestRate = 10.0m, Status = LoanStatus.Active, IssuedDate = today.AddMonths(-1), DueDate = today.AddMonths(11), RemainingBalance = 42000, Currency = "KES" },
            // Nigeria loans (NGN)
            new Loan { Id = Guid.NewGuid(), CustomerId = customers[8].Id, Amount = 550000, InterestRate = 10.0m, Status = LoanStatus.Pending, IssuedDate = today, DueDate = today.AddMonths(12), RemainingBalance = 550000, Currency = "NGN" },
            // Benin loans (XOF)
            new Loan { Id = Guid.NewGuid(), CustomerId = customers[11].Id, Amount = 200000, InterestRate = 15.0m, Status = LoanStatus.PaidOff, IssuedDate = today.AddMonths(-12), DueDate = today.AddMonths(-1), RemainingBalance = 0, Currency = "XOF" },
            // Mozambique loans (MZN)
            new Loan { Id = Guid.NewGuid(), CustomerId = customers[16].Id, Amount = 300000, InterestRate = 12.5m, Status = LoanStatus.Active, IssuedDate = today.AddMonths(-4), DueDate = today.AddMonths(8), RemainingBalance = 220000, Currency = "MZN" },
            // Zambia loans (ZMW)
            new Loan { Id = Guid.NewGuid(), CustomerId = customers[18].Id, Amount = 4000, InterestRate = 12.5m, Status = LoanStatus.Active, IssuedDate = today.AddMonths(-2), DueDate = today.AddMonths(10), RemainingBalance = 3600, Currency = "ZMW" }
        };
    }

    private static List<Payment> GetMultiMarketPayments(List<Customer> customers)
    {
        var now = DateTime.UtcNow;
        return new List<Payment>
        {
            // Uganda payments (UGX - MTN Mobile Money)
            new Payment { Id = Guid.NewGuid(), CustomerId = customers[0].Id, Amount = 250000, Currency = "UGX", Status = PaymentStatus.Completed, Method = PaymentMethod.Mpesa, TransactionReference = "UG-TXN001", MpesaReceiptNumber = "MTN7R2BXYZ", PaidAt = now.AddMinutes(-5), ProviderKey = "ug_mtn_mobilemoney" },
            new Payment { Id = Guid.NewGuid(), CustomerId = customers[1].Id, Amount = 350000, Currency = "UGX", Status = PaymentStatus.Completed, Method = PaymentMethod.Mpesa, TransactionReference = "UG-TXN002", MpesaReceiptNumber = "MTN8S3CABC", PaidAt = now.AddMinutes(-30), ProviderKey = "ug_airtel_money" },

            // Kenya payments (KES - M-Pesa)
            new Payment { Id = Guid.NewGuid(), CustomerId = customers[4].Id, Amount = 5000, Currency = "KES", Status = PaymentStatus.Completed, Method = PaymentMethod.Mpesa, TransactionReference = "KE-TXN001", MpesaReceiptNumber = "QJK7R2BXYZ", PaidAt = now.AddHours(-1), ProviderKey = "ke_safaricom_mpesa" },

            // Tanzania payments (TZS - Vodacom M-Pesa)
            new Payment { Id = Guid.NewGuid(), CustomerId = customers[6].Id, Amount = 75000, Currency = "TZS", Status = PaymentStatus.Completed, Method = PaymentMethod.Mpesa, TransactionReference = "TZ-TXN001", MpesaReceiptNumber = "VOD4R2XYZ", PaidAt = now.AddHours(-2), ProviderKey = "tz_vodacom_mpesa" },

            // Nigeria payments (NGN - MTN MoMo)
            new Payment { Id = Guid.NewGuid(), CustomerId = customers[8].Id, Amount = 42000, Currency = "NGN", Status = PaymentStatus.Pending, Method = PaymentMethod.Mpesa, TransactionReference = "NG-TXN001", ProviderKey = "ng_mtn_momo" },
            new Payment { Id = Guid.NewGuid(), CustomerId = customers[9].Id, Amount = 55000, Currency = "NGN", Status = PaymentStatus.Completed, Method = PaymentMethod.Mpesa, TransactionReference = "NG-TXN002", MpesaReceiptNumber = "OPY5X7ABC", PaidAt = now.AddHours(-3), ProviderKey = "ng_opay" },

            // Benin payments (XOF - MTN Mobile Money)
            new Payment { Id = Guid.NewGuid(), CustomerId = customers[11].Id, Amount = 35000, Currency = "XOF", Status = PaymentStatus.Completed, Method = PaymentMethod.Mpesa, TransactionReference = "BJ-TXN001", MpesaReceiptNumber = "MTN9T4DDEF", PaidAt = now.AddHours(-4), ProviderKey = "bj_mtn_mobilemoney" },

            // Ivory Coast payments (XOF - Orange Money)
            new Payment { Id = Guid.NewGuid(), CustomerId = customers[13].Id, Amount = 65000, Currency = "XOF", Status = PaymentStatus.Completed, Method = PaymentMethod.Mpesa, TransactionReference = "CI-TXN001", MpesaReceiptNumber = "ORG6U5EGHI", PaidAt = now.AddHours(-5), ProviderKey = "ci_orange_money" },

            // Mozambique payments (MZN - M-Pesa)
            new Payment { Id = Guid.NewGuid(), CustomerId = customers[16].Id, Amount = 30000, Currency = "MZN", Status = PaymentStatus.Failed, Method = PaymentMethod.Mpesa, TransactionReference = "MZ-TXN001", ProviderKey = "mz_mpesa" },
            new Payment { Id = Guid.NewGuid(), CustomerId = customers[17].Id, Amount = 22000, Currency = "MZN", Status = PaymentStatus.Completed, Method = PaymentMethod.Mpesa, TransactionReference = "MZ-TXN002", MpesaReceiptNumber = "EMO7V6FJKL", PaidAt = now.AddHours(-6), ProviderKey = "mz_emola" },

            // Zambia payments (ZMW - MTN Mobile Money)
            new Payment { Id = Guid.NewGuid(), CustomerId = customers[18].Id, Amount = 250, Currency = "ZMW", Status = PaymentStatus.Completed, Method = PaymentMethod.Mpesa, TransactionReference = "ZM-TXN001", MpesaReceiptNumber = "MTN2W7GMNO", PaidAt = now.AddDays(-1), ProviderKey = "zm_mtn_mobilemoney" },
            new Payment { Id = Guid.NewGuid(), CustomerId = customers[19].Id, Amount = 350, Currency = "ZMW", Status = PaymentStatus.Completed, Method = PaymentMethod.Mpesa, TransactionReference = "ZM-TXN002", MpesaReceiptNumber = "AIR3X8HPQR", PaidAt = now.AddDays(-1), ProviderKey = "zm_airtel_money" }
        };
    }
}

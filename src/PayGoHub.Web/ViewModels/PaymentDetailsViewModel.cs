using PayGoHub.Application.DTOs;

namespace PayGoHub.Web.ViewModels;

public class PaymentDetailsViewModel
{
    public PaymentDto Payment { get; set; } = null!;
    public CustomerDto? Customer { get; set; }
    public bool CanValidate { get; set; }
    public bool CanConfirm { get; set; }

    // MoMo provider options
    public static readonly Dictionary<string, string> ProviderKeys = new()
    {
        { "ke_safaricom_mpesa", "Safaricom M-Pesa (Kenya)" },
        { "ug_mtn_mobilemoney", "MTN Mobile Money (Uganda)" },
        { "tz_vodacom_mpesa", "Vodacom M-Pesa (Tanzania)" },
        { "rw_mtn_mobilemoney", "MTN Mobile Money (Rwanda)" },
        { "zm_mtn_mobilemoney", "MTN Mobile Money (Zambia)" },
        { "mz_vodacom_mpesa", "Vodacom M-Pesa (Mozambique)" },
        { "ng_mtn_mobilemoney", "MTN Mobile Money (Nigeria)" }
    };

    // Currency options
    public static readonly Dictionary<string, string> Currencies = new()
    {
        { "KES", "Kenyan Shilling (KES)" },
        { "UGX", "Ugandan Shilling (UGX)" },
        { "TZS", "Tanzanian Shilling (TZS)" },
        { "RWF", "Rwandan Franc (RWF)" },
        { "ZMW", "Zambian Kwacha (ZMW)" },
        { "MZN", "Mozambican Metical (MZN)" },
        { "NGN", "Nigerian Naira (NGN)" }
    };
}

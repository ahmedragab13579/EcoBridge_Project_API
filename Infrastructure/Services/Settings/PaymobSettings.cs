namespace EcoBridge.Infrastructure.Services.Settings;

public class PaymobSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string CardIntegrationId { get; set; } = string.Empty;
    public string WalletIntegrationId { get; set; } = string.Empty;
    public string IframeId { get; set; } = string.Empty;
    public string HmacSecret { get; set; } = string.Empty;
}

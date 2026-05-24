namespace SafeCheck.Models;

public enum RiskLevel
{
    Unknown,
    Safe,       // 0-20% risk
    Caution,    // 21-60% risk
    Warning,    // 61-85% risk
    Danger      // 86-100% risk
}

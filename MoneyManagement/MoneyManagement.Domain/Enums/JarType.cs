namespace MoneyManagement.Domain.Enums;

/// <summary>
///     Jar type enumeration based on 6 Jars method (EN)<br />
///     Enum loại lọ dựa trên phương pháp 6 lọ (VI)
/// </summary>
public enum JarType
{
    /// <summary>
    ///     Necessities jar - 55% (EN)<br />
    ///     Lọ chi tiêu thiết yếu - 55% (VI)
    /// </summary>
    Necessities = 1,

    /// <summary>
    ///     Financial Freedom Account - 10% (EN)<br />
    ///     Tài khoản tự do tài chính - 10% (VI)
    /// </summary>
    FinancialFreedom = 2,

    /// <summary>
    ///     Long Term Savings for Spending - 10% (EN)<br />
    ///     Tiết kiệm dài hạn để chi tiêu - 10% (VI)
    /// </summary>
    LongTermSavings = 3,

    /// <summary>
    ///     Education jar - 10% (EN)<br />
    ///     Lọ giáo dục - 10% (VI)
    /// </summary>
    Education = 4,

    /// <summary>
    ///     Play jar - 10% (EN)<br />
    ///     Lọ vui chơi - 10% (VI)
    /// </summary>
    Play = 5,

    /// <summary>
    ///     Give jar - 5% (EN)<br />
    ///     Lọ cho đi - 5% (VI)
    /// </summary>
    Give = 6,

    /// <summary>
    ///     Custom jar type (EN)<br />
    ///     Loại lọ tùy chỉnh (VI)
    /// </summary>
    Custom = 7
}
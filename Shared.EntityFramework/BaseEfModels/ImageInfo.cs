namespace Shared.EntityFramework.BaseEfModels;

/// <summary>
///     Represents information about an image. (EN)<br />
///     Đại diện cho thông tin về một hình ảnh. (VI)
/// </summary>
public class ImageInfo : BaseEntity<Guid>
{
    /// <summary>
    ///     Gets or sets the name of the image. (EN)<br />
    ///     Lấy hoặc đặt tên của hình ảnh. (VI)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the original client name of the image file. (EN)<br />
    ///     Lấy hoặc đặt tên gốc của tệp hình ảnh từ client. (VI)
    /// </summary>
    public string? ClientName { get; set; }

    /// <summary>
    ///     Gets or sets the URL where the image is stored. (EN)<br />
    ///     Lấy hoặc đặt URL nơi hình ảnh được lưu trữ. (VI)
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    ///     Gets or sets the size of the image in bytes. (EN)<br />
    ///     Lấy hoặc đặt kích thước của hình ảnh theo byte. (VI)
    /// </summary>
    public int? Size { get; set; }
}
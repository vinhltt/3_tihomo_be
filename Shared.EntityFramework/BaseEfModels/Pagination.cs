namespace Shared.EntityFramework.BaseEfModels;

/// <summary>
///     Represents pagination information for data retrieval. (EN)<br />
///     Đại diện cho thông tin phân trang để truy xuất dữ liệu. (VI)
/// </summary>
public class Pagination
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Pagination" /> class with default values. (EN)<br />
    ///     Khởi tạo một phiên bản mới của lớp <see cref="Pagination" /> với các giá trị mặc định. (VI)
    /// </summary>
    public Pagination()
    {
        PageIndex = 1;
        PageSize = 10;
        TotalRow = 0;
        PageCount = 0;
    }

    /// <summary>
    ///     Gets or sets the total number of rows in the dataset. (EN)<br />
    ///     Lấy hoặc đặt tổng số hàng trong tập dữ liệu. (VI)
    /// </summary>
    public int TotalRow { get; set; }

    /// <summary>
    ///     Gets or sets the total number of pages. (EN)<br />
    ///     Lấy hoặc đặt tổng số trang. (VI)
    /// </summary>
    public int PageCount { get; set; }

    /// <summary>
    ///     Gets or sets the current page index (1-based). (EN)<br />
    ///     Lấy hoặc đặt chỉ mục trang hiện tại (bắt đầu từ 1). (VI)
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    ///     Gets or sets the number of items per page. (EN)<br />
    ///     Lấy hoặc đặt số lượng mục trên mỗi trang. (VI)
    /// </summary>
    public int PageSize { get; set; }
}
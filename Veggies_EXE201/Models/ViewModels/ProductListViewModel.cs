using System.Collections.Generic;

namespace Veggies_EXE201.Models.ViewModels
{
    public class ProductListViewModel
    {
        // Lớp con đại diện cho một sản phẩm trong danh sách hiển thị
        public class ProductItem
        {
            public int ProductId { get; set; }
            public string? ProductName { get; set; }
            public string? Description { get; set; }
            public decimal Price { get; set; }
            public string? ProductImage { get; set; } // Thuộc tính đúng là ImageUrl
            public string? CategoryName { get; set; }
        }

        // Lớp con đại diện cho một danh mục trong bộ lọc
        public class CategoryItem
        {
            public int CategoryId { get; set; }
            public string? CategoryName { get; set; }
        }

        // --- Các thuộc tính chính của ViewModel ---

        public List<ProductItem> Products { get; set; } = new List<ProductItem>();
        public List<CategoryItem> Categories { get; set; } = new List<CategoryItem>();

        // Thông tin Lọc
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? CurrentSort { get; set; }

        // Thông tin Phân trang
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        // === THÊM 2 THUỘC TÍNH MỚI ĐỂ SỬA LỖI ===
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
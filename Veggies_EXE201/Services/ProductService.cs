using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Veggies_EXE201.Models;
using Veggies_EXE201.Models.ViewModels;
using Veggies_EXE201.Repositories;

namespace Veggies_EXE201.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        // Tiêm IProductRepository vào Service thông qua constructor
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        /// <summary>
        /// Lấy ViewModel cho trang danh sách sản phẩm, xử lý logic lọc, sắp xếp, phân trang.
        /// </summary>
        public async Task<ProductListViewModel> GetProductsViewModelAsync(string? searchTerm, int? categoryId, decimal? minPrice, decimal? maxPrice, string sortBy, int pageNumber)
        {
            // 1. Lấy dữ liệu gốc từ Repository
            var allProducts = await _productRepository.GetAllAsync();
            var allCategories = await _productRepository.GetCategoriesAsync();

            var productsQuery = allProducts.AsQueryable();

            // 2. Lọc (Filtering)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                productsQuery = productsQuery.Where(p =>
                    p.ProductName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (p.Description != null && p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
            }
            if (categoryId.HasValue && categoryId > 0)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }
            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);
            }

            // 3. Sắp xếp (Sorting)
            productsQuery = sortBy switch
            {
                "price_desc" => productsQuery.OrderByDescending(p => p.Price),
                "price_asc" => productsQuery.OrderBy(p => p.Price),
                "name_desc" => productsQuery.OrderByDescending(p => p.ProductName),
                _ => productsQuery.OrderBy(p => p.ProductName),
            };

            // 4. Phân trang (Pagination)
            const int pageSize = 9;
            var totalItems = productsQuery.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages == 0 ? 1 : totalPages));
            var pagedProducts = productsQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            // 5. Tạo ViewModel để trả về
            var viewModel = new ProductListViewModel
            {
                Products = pagedProducts.Select(p => new ProductListViewModel.ProductItem
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    Description = p.Description,
                    ProductImage = p.ProductImage ?? "/images/default-product.png",
                    CategoryName = p.Category?.CategoryName ?? "Không xác định"
                }).ToList(),
                Categories = allCategories.Select(c => new ProductListViewModel.CategoryItem
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                }).ToList(),
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                CurrentSort = sortBy,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };

            return viewModel;
        }

        /// <summary>
        /// Lấy thông tin một sản phẩm theo ID.
        /// </summary>
        /// <param name="productId">ID của sản phẩm cần tìm.</param>
        /// <returns>Đối tượng Product hoặc null nếu không tìm thấy.</returns>
        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _productRepository.GetByIdAsync(productId);
        }

        /// <summary>
        /// Lấy tất cả các danh mục sản phẩm.
        /// </summary>
        /// <returns>Danh sách các Category.</returns>
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _productRepository.GetCategoriesAsync();
        }

        /// <summary>
        /// Tạo một sản phẩm mới.
        /// </summary>
        /// <param name="product">Đối tượng Product chứa thông tin sản phẩm mới.</param>
        public async Task CreateProductAsync(Product product)
        {
            await _productRepository.AddAsync(product);
        }

        /// <summary>
        /// Cập nhật thông tin một sản phẩm đã có.
        /// </summary>
        /// <param name="product">Đối tượng Product chứa thông tin đã được chỉnh sửa.</param>
        public async Task UpdateProductAsync(Product product)
        {
            await _productRepository.UpdateAsync(product);
        }

        /// <summary>
        /// Xóa một sản phẩm khỏi database theo ID.
        /// </summary>
        /// <param name="productId">ID của sản phẩm cần xóa.</param>
        public async Task DeleteProductAsync(int productId)
        {
            // Tìm sản phẩm trước khi xóa
            var productToDelete = await _productRepository.GetByIdAsync(productId);
            if (productToDelete != null)
            {
                // Nếu tìm thấy, tiến hành xóa
                await _productRepository.DeleteAsync(productToDelete);
            }
            // Nếu không tìm thấy, không làm gì cả hoặc có thể throw exception
        }
    }
}
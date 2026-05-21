using Microsoft.EntityFrameworkCore;

public class ProductService : IProductServices
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    // ================= GET PAGED =================
    public async Task<PageResult<ProductDTO>> GetPaged(int page, int pageSize)
    {
        var query = _context.Products
            .Include(x => x.Category)
            .Include(x => x.Images);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProductDTO
            {
                Id = x.Id,
                Name = x.Name,
                Price = x.Price,
                Stock = x.Stock,
                CategoryName = x.Category.Name,
                CategoryId=x.CategoryId.ToString(),
                ImageUrl = x.Images
                    .Where(i => i.IsMain)
                    .Select(i => i.Url)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return new PageResult<ProductDTO>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };
    }
     public async Task<PageResult<ProductDTO>> GetAll(ProductQueryDTO query)
    {
        var products = _context.Products
            .Include(x => x.Category)
            .Include(x => x.Images)
            .AsQueryable();

        // Filter
        if (!string.IsNullOrEmpty(query.Keyword))
        {
            products = products.Where(x => x.Name.Contains(query.Keyword));
        }

        if (!string.IsNullOrEmpty(query.CategoryId))
        {
            products = products.Where(x => x.CategoryId == Guid.Parse(query.CategoryId));
        }

        var total = await products.CountAsync();

        var data = await products
            .OrderByDescending(x => x.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new ProductDTO
            {
                Id = x.Id,
                Name = x.Name,
                Price = x.Price,
                Stock = x.Stock,
                CategoryId = x.CategoryId.ToString(),
                CategoryName = x.Category.Name,
                ImageUrl = x.Images
                    .Where(i => i.IsMain)
                    .Select(i => i.Url)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return new PageResult<ProductDTO>
        {
            Items = data,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = total
        };
    }

    // ================= CREATE =================
    public async Task Create(ProductCreateDTO dto)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Price = dto.Price,
            Stock = dto.Stock,
            CategoryId = dto.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);

        var images = dto.Images.Select((url, index) => new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Url = url,
            IsMain = index == 0
        });

        _context.ProductImages.AddRange(images);

        await _context.SaveChangesAsync();
    }

    // ================= UPDATE =================
    public async Task Update(Guid id, ProductCreateDTO dto)
    {
        var product = await _context.Products
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product == null) throw new Exception("Not found");

        product.Name = dto.Name;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.CategoryId = dto.CategoryId;
        if (dto.Images != null)
        {
         var toDelete = product.Images
                        .Where(x => !dto.Images.Contains(x.Url))
                        .ToList();

            foreach (var img in toDelete)
            {
                product.Images.Remove(img);

                FileHelper.DeleteFile(img.Url);

            }
            // 🔥 2. Thêm ảnh mới (chưa có trong DB)
            var existingUrls = product.Images.Select(x => x.Url).ToList();

            var toAdd = dto.Images
                .Where(url => !existingUrls.Contains(url))
                .ToList();

            // ❗ ADD ẢNH MỚI
            var images = toAdd.Select((url, index) => new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Url = url,
                IsMain = index == 0
            });

            _context.ProductImages.AddRange(images);
        }


        await _context.SaveChangesAsync();
    }

    // ================= DELETE =================
    public async Task Delete(Guid id)
    {
        var product = await _context.Products
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product == null) return;

        foreach (var img in product.Images)
        {
            FileHelper.DeleteFile(img.Url);
        }

        _context.ProductImages.RemoveRange(product.Images);
        _context.Products.Remove(product);

        await _context.SaveChangesAsync();
    }

   public async Task<ProductCreateDTO> Get(Guid id)
    {
        var product= await _context.Products.Where(x=> x.Id==id)
        .Select(x=> new ProductCreateDTO
        {
         Name = x.Name,
         Stock= x.Stock,
         Price = x.Price,
         CategoryId = x.Category.Id,
         Images=x.Images.OrderByDescending(i => i.IsMain).Select( i => i.Url).ToList()
        }).FirstOrDefaultAsync();
        return product;
    }
}
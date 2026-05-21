using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/admin/products")]
public class ProductController : ControllerBase
{
    private readonly IProductServices _service;

    public ProductController(IProductServices service)
    {
        _service = service;
    }
    
    
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        return Ok(await _service.Get(id));
    }
    
    [HttpGet("products")]
    public async Task<IActionResult> Get(int page = 1, int pageSize = 10)
    {
        return Ok(await _service.GetPaged(page, pageSize));
    }
    
    [HttpPost("products-by-keyword")]
    public async Task<IActionResult> GetByKeyWord([FromBody] ProductQueryDTO dto)
    {
        return Ok(await _service.GetAll(dto));
    }

    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> Create(ProductCreateDTO dto)
    {
        await _service.Create(dto);
        return Ok();
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, ProductCreateDTO dto)
    {
        await _service.Update(id, dto);
        return Ok();
    }
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.Delete(id);
        return Ok();
    }

    // UPLOAD
    [Authorize]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(List<IFormFile> files)
    {
        var urls = new List<string>();

        foreach (var file in files)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine("wwwroot/uploads/products", fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            urls.Add($"/uploads/products/{fileName}");
        }

        return Ok(urls);
    }
}
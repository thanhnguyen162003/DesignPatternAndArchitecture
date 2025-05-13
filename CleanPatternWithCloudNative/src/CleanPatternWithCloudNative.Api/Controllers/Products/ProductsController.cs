using CleanPatternWithCloudNative.Application.Products.Commands.CreateProduct;
using CleanPatternWithCloudNative.Application.Products.Queries.ReadProducts;
using CleanPatternWithCloudNative.Domain.Entities;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace CleanPatternWithCloudNative.Api.Controllers.Products
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController(
        ISender sender,
        ILogger<ProductsController> logger) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateProduct(
            [FromBody] CreateProductRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return BadRequest();
            }

            Guid? productId = await sender.Send(
                new CreateProductCommand(request.Name, request.Description),
                cancellationToken);

            return productId is null ? BadRequest() : Ok(productId);
        }

        [HttpGet]
        public async Task<ActionResult<ReadProductsResponse>> GetProducts(
            CancellationToken cancellationToken = default)
        {
            Product[]? products = await sender.Send(
                new ReadProductsQuery(),
                cancellationToken);

            ProductResponse[] productsResponse = products!
                .Select(x => new ProductResponse(x.Id, x.Name, x.Description))
                .ToArray();

            logger.LogInformation("Found {Count} products: {@Products}", products!.Length, products!);

            ReadProductsResponse response = new(new(productsResponse));

            return Ok(response);
        }
    }
}
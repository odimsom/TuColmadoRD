using MediatR;
using TuColmadoRD.Core.Application.Inventory.Commands;
using TuColmadoRD.Core.Application.Inventory.Queries;
using TuColmadoRD.Presentation.API.Extensions;

namespace TuColmadoRD.Presentation.API.Endpoints.Inventory;

/// <summary>
/// Minimal API endpoints for inventory module.
/// </summary>
public static class InventoryEndpoints
{
    /// <summary>
    /// Maps inventory endpoint group.
    /// </summary>
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory")
            .WithTags("Inventory")
            .RequireAuthorization();

        group.MapPost("/products", async (CreateProductRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new CreateProductCommand(
                request.Name,
                request.CategoryId,
                request.CostPrice,
                request.SalePrice,
                request.ItbisRate,
                request.UnitType);

            var result = await mediator.Send(command, ct);
            if (!result.TryGetResult(out var productId))
            {
                return result.Error.MapDomainError();
            }

            return TypedResults.Created($"/api/inventory/products/{productId}", new CreatedProductResponse(productId));
        });

        group.MapPut("/products/{id:guid}/price", async (Guid id, UpdatePriceRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new UpdateProductPriceCommand(id, request.NewCostPrice, request.NewSalePrice);
            var result = await mediator.Send(command, ct);
            if (!result.IsGood)
            {
                return result.Error.MapDomainError();
            }

            return TypedResults.Ok(new { });
        });

        group.MapPost("/products/{id:guid}/stock/adjust", async (Guid id, AdjustStockRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new AdjustStockCommand(id, request.Delta, request.Reason);
            var result = await mediator.Send(command, ct);
            if (!result.IsGood)
            {
                return result.Error.MapDomainError();
            }

            var productResult = await mediator.Send(new GetProductByIdQuery(id), ct);
            if (!productResult.TryGetResult(out var product))
            {
                return productResult.Error.MapDomainError();
            }

            return TypedResults.Ok(new { newStockQuantity = product!.StockQuantity });
        });

        group.MapDelete("/products/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new DeactivateProductCommand(id), ct);
            if (!result.IsGood)
            {
                return result.Error.MapDomainError();
            }

            return TypedResults.NoContent();
        });

        group.MapGet("/products/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetProductByIdQuery(id), ct);
            if (!result.TryGetResult(out var dto))
            {
                return result.Error.MapDomainError();
            }

            return TypedResults.Ok(dto);
        });

        group.MapGet("/products", async (
            IMediator mediator,
            int page = 1,
            int pageSize = 20,
            string? nameFilter = null,
            Guid? categoryId = null,
            bool includeInactive = false,
            CancellationToken ct = default) =>
        {
            var query = new GetProductsPagedQuery(page, pageSize, nameFilter, categoryId, includeInactive);
            var result = await mediator.Send(query, ct);
            if (!result.TryGetResult(out var dto))
            {
                return result.Error.MapDomainError();
            }

            return TypedResults.Ok(dto);
        });

        return app;
    }
}

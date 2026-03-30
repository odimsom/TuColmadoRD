using MediatR;
using TuColmadoRD.Core.Application.Customers.Queries;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Customers;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Customers;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Customers.Handlers;

public sealed class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, OperationResult<CustomerDetailResult, DomainError>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ITenantProvider _tenantProvider;

    public GetCustomerByIdQueryHandler(
        ICustomerRepository customerRepository,
        ITenantProvider tenantProvider)
    {
        _customerRepository = customerRepository;
        _tenantProvider = tenantProvider;
    }

    public async Task<OperationResult<CustomerDetailResult, DomainError>> Handle(
        GetCustomerByIdQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(
            request.CustomerId,
            [c => c.Account],
            cancellationToken);

        if (customer is null || customer.TenantId != _tenantProvider.TenantId)
        {
            return OperationResult<CustomerDetailResult, DomainError>.Bad(
                DomainError.NotFound("customer.not_found", "Cliente no encontrado."));
        }

        var detail = new CustomerDetailResult(
            customer.Id,
            customer.FullName,
            customer.DocumentId.Value,
            customer.ContactPhone?.Value,
            customer.IsActive,
            customer.CreatedAt,
            customer.Account.Id,
            customer.Account.Balance.Amount,
            customer.Account.CreditLimit.Amount,
            customer.Account.LastActivity,
            customer.Account.Status.ToString());

        return OperationResult<CustomerDetailResult, DomainError>.Good(detail);
    }
}

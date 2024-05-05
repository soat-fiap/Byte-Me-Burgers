using FIAP.TechChallenge.ByteMeBurger.Domain.Entities;

namespace FIAP.TechChallenge.ByteMeBurger.Application.UseCases.Orders;

public interface IGetOrderDetailsUseCase
{
    Task<Order?> Execute(Guid id);
}
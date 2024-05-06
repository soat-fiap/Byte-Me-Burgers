using System.Data;
using Dapper;
using FIAP.TechChallenge.ByteMeBurger.Domain.Entities;
using FIAP.TechChallenge.ByteMeBurger.Domain.Ports.Outgoing;
using FIAP.TechChallenge.ByteMeBurger.Infrastructure.Dto;
using Microsoft.Extensions.Logging;

namespace FIAP.TechChallenge.ByteMeBurger.Infrastructure.Repository;

public class CustomerRepositoryDapper(IDbConnection dbConnection, ILogger<CustomerRepositoryDapper> logger)
    : ICustomerRepository
{
    public async Task<Customer?> FindByCpfAsync(string cpf)
    {
        logger.LogInformation("Finding customer with CPF: {Cpf}", cpf);
        var customerDto = await dbConnection.QuerySingleOrDefaultAsync<CustomerDto>(
            "SELECT * FROM Customers WHERE Cpf=@Cpf",
            new { Cpf = cpf });

        if (customerDto == null)
        {
            logger.LogWarning("Customer with CPF: {Cpf} not found", cpf);
        }
        else
        {
            logger.LogInformation("Customer with CPF: {Cpf} found", cpf);
        }

        return (Customer)customerDto;
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        logger.LogInformation("Creating customer with CPF: {Cpf}", customer.Cpf);
        var param = (CustomerDto)customer;
        var rowsAffected = await dbConnection.ExecuteAsync(
            "insert into Customers (Id, Cpf, Name, Email) values (@Id, @Cpf, @Name, @Email);",
            param);

        if (rowsAffected > 0)
        {
            logger.LogInformation("Customer with CPF: {Cpf} created", customer.Cpf);
        }
        else
        {
            logger.LogError("Failed to create customer with CPF: {Cpf}", customer.Cpf);
        }

        return customer;
    }
}
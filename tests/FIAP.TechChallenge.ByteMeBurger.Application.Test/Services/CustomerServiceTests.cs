using FIAP.TechChallenge.ByteMeBurger.Application.Services;
using FIAP.TechChallenge.ByteMeBurger.Domain.Entities;
using FIAP.TechChallenge.ByteMeBurger.Domain.Ports.Ingoing;
using FIAP.TechChallenge.ByteMeBurger.Domain.Ports.Outgoing;
using FIAP.TechChallenge.ByteMeBurger.Domain.ValueObjects;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

namespace FIAP.TechChallenge.ByteMeBurger.Application.Test.Services;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _repository;
    private readonly ICustomerService _service;
    private readonly Cpf ValidCpf = new Cpf("863.917.790-23");

    public CustomerServiceTests()
    {
        _repository = new Mock<ICustomerRepository>();
        _service = new CustomerService(_repository.Object);

        _repository.Setup(r => r.FindByCpfAsync(It.Is<string>(s => s != ValidCpf)))
            .ReturnsAsync(default(Customer));
    }

    [Fact]
    public async Task FindBy_Cpf_Success()
    {
        // Arrange
        var expectedCustomer = new Customer(ValidCpf, "name", "email@email.com");
        _repository.Setup(r => r.FindByCpfAsync(ValidCpf))
            .ReturnsAsync(expectedCustomer);

        // Act
        var customer = await _service.FindByCpfAsync(ValidCpf);

        // Assert
        using (new AssertionScope())
        {
            customer.Should().NotBeNull();
            customer.Should().BeEquivalentTo(expectedCustomer, options => options.ComparingByMembers<Customer>());
        }
    }

    [Fact]
    public async Task FindBy_Cpf_NotFound()
    {
        // Arrange
        // Act
        var customer = await _service.FindByCpfAsync("628.040.140-53");

        // Assert
        customer.Should().BeNull();
    }

    [Fact(Skip = "changing business rules")]
    public async Task Create_Customer_Anonymous_Success()
    {
        // Arrange
        _repository.Setup(r => r.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync(new Customer());

        // Act
        var customer = await _service.CreateAnonymousAsync();

        // Assert
        using (new AssertionScope())
        {
            customer.Should().NotBeNull();
            customer.Name.Should().Be("Anonymous");
            customer.Email.Should().BeNull();
        }
    }

    [Fact]
    public async Task Create_Customer_CpfOnly_Success()
    {
        // Arrange
        var expectedCustomer = new Customer(ValidCpf);

        _repository.Setup(r => r.CreateAsync(It.Is<Customer>(c => c.Cpf.Equals(ValidCpf))))
            .ReturnsAsync(expectedCustomer);

        // Act
        var result = await _service.CreateAsync(ValidCpf);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedCustomer, options => options.ComparingByMembers<Customer>());
            result.Id.Should().NotBeEmpty();
        }
    }

    [Theory]
    [InlineData("310.686.640-37", "customer", "email@email.com")]
    public async Task Create_Customer_NameEmail_Success(string cpf, string name, string email)
    {
        // Arrange
        var customer = new Customer(cpf, name, email);
        _repository.Setup(r => r.CreateAsync(It.Is<Customer>(c => c.Cpf.Equals(customer.Cpf))))
            .ReturnsAsync(customer);

        // Act
        var result = await _service.CreateAsync(cpf, name, email);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Be(customer);
        }
    }
}
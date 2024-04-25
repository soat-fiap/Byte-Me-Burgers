using System.Text.RegularExpressions;
using FIAP.TechChallenge.ByteMeBurger.Domain.Base;

namespace FIAP.TechChallenge.ByteMeBurger.Domain.Entities;

public class Customer : Entity<string>
{
    public string? Name { get; private set; }

    public string? Email { get; private set; }

    public Customer()
        : base(ValidateId(Guid.NewGuid().ToString()))
    {
        Name = "Anonymous";
    }

    public Customer(string cpf)
        : base(ValidateId(cpf))
    {
    }

    public Customer(string cpf, string name, string email)
        : base(ValidateId(cpf))
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Email = ValidateEmail(email);
        Id = ValidateId(cpf);
    }

    private static string ValidateEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        if (!Regex.IsMatch(email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
            throw new ArgumentException($"Invalid email format '{email}'");

        return email;
    }

    private static string SanityseCpf(string cpf) => cpf
        .Replace(".", "")
        .Replace("-", "")
        .Trim();

    public static string ValidateId(string cpf)
    {
        if (Guid.TryParse(cpf, out var validGuid) && Guid.Empty != validGuid)
            return cpf;

        ArgumentException.ThrowIfNullOrWhiteSpace(cpf);
        var isValidCpf = false;

        var multiplicador1 = new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        var multiplicador2 = new[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        var workingWpf = SanityseCpf(cpf);

        if (workingWpf.Length == 11 && workingWpf.Distinct().Count() > 1)
        {
            var tempCpf = workingWpf.Substring(0, 9);
            var soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            var resto = soma % 11;

            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            var digito = resto.ToString();

            tempCpf += digito;

            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;

            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito += resto.ToString();

            isValidCpf = workingWpf.EndsWith(digito);
        }

        if (!isValidCpf)
        {
            throw new ArgumentException($"Invalid CPF value '{cpf}'");
        }

        return workingWpf;
    }
}
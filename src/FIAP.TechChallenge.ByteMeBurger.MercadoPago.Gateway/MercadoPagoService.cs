// Copyright (c) 2024, Italo Pessoa (https://github.com/italopessoa)
// All rights reserved.
//
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Diagnostics.CodeAnalysis;
using FIAP.TechChallenge.ByteMeBurger.Domain.Entities;
using FIAP.TechChallenge.ByteMeBurger.Domain.Interfaces;
using FIAP.TechChallenge.ByteMeBurger.Domain.ValueObjects;
using FIAP.TechChallenge.ByteMeBurger.MercadoPago.Gateway.Configuration;
using MercadoPago.Client;
using MercadoPago.Client.Common;
using MercadoPago.Client.Payment;
using Microsoft.Extensions.Options;
using DomainPayment = FIAP.TechChallenge.ByteMeBurger.Domain.Entities.Payment;

namespace FIAP.TechChallenge.ByteMeBurger.MercadoPago.Gateway;

[ExcludeFromCodeCoverage]
public class MercadoPagoService : IPaymentGateway
{
    private readonly MercadoPagoOptions _mercadoPagoOptions;

    public MercadoPagoService(IOptions<MercadoPagoOptions> mercadoPagoOptions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mercadoPagoOptions.Value.WebhookSecret,
            nameof(mercadoPagoOptions.Value.WebhookSecret));
        _mercadoPagoOptions = mercadoPagoOptions.Value;
    }

    public async Task<DomainPayment> CreatePaymentAsync(Order order)
    {
        var requestOptions = new RequestOptions()
        {
            AccessToken = _mercadoPagoOptions.AccessToken,
            CustomHeaders =
            {
                { "x-idempotency-key", order.Id.ToString() }
            }
        };

        PaymentCreateRequest MapPaymentCreateRequest(PaymentPayerRequest paymentPayerRequest2,
            PaymentAdditionalInfoRequest paymentAdditionalInfoRequest)
        {
            return new PaymentCreateRequest
            {
                Description = $"Payment for Order {order.TrackingCode.Value}",
                ExternalReference = order.TrackingCode.Value,
                Installments = 1,
                NotificationUrl = _mercadoPagoOptions.NotificationUrl,
                Payer = paymentPayerRequest2,
                PaymentMethodId = "pix",
                StatementDescriptor = "tech challenge restaurant",
                TransactionAmount = (decimal?)0.01,
                AdditionalInfo = paymentAdditionalInfoRequest,
                DateOfExpiration = DateTime.UtcNow.AddMinutes(5)
            };
        }

        PaymentPayerRequest MapPaymentPayerRequest()
            => new()
            {
                Type = "individual",
                EntityType = "customer",
                Email = order.Customer.Email,
                FirstName = order.Customer.Name,
                LastName = "User",
                Identification = new IdentificationRequest
                {
                    Type = "CPF",
                    Number = order.Customer.Cpf
                },
            };

        IEnumerable<PaymentItemRequest> MapPaymentItemRequests() => order.OrderItems.Select(item =>
            new PaymentItemRequest
            {
                Id = item.Id.ToString(),
                Title = item.ProductName,
                Description = item.ProductName,
                CategoryId = "food",
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Warranty = false,
                CategoryDescriptor = new PaymentCategoryDescriptorRequest
                {
                    Passenger = { },
                    Route = { }
                }
            }
        );


        var paymentPayerRequest = new PaymentPayerRequest()
        {
            Email = "guest@mercadofiado.com",
        };
        if (order.Customer is not null)
        {
            paymentPayerRequest = MapPaymentPayerRequest();
        }

        var items = MapPaymentItemRequests();

        var additionalInfo = new PaymentAdditionalInfoRequest
        {
            Items = items.ToList(),
        };

        var request = MapPaymentCreateRequest(paymentPayerRequest, additionalInfo);
        var client = new PaymentClient();


        var mercadoPagoPayment = await client.CreateAsync(request, requestOptions);

        var status = Enum.TryParse(typeof(PaymentStatus), mercadoPagoPayment.Status, true, out var paymentStatus)
            ? (PaymentStatus)paymentStatus
            : PaymentStatus.Pending;
        return new DomainPayment
        {
            Status = status,
            Id = new PaymentId(mercadoPagoPayment.Id.ToString()!, order.Id),
            PaymentType = 1,
            QrCode = mercadoPagoPayment.PointOfInteraction.TransactionData.QrCode
        };
    }
}
// Copyright (c) 2024, Italo Pessoa (https://github.com/italopessoa)
// All rights reserved.
//
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using FIAP.TechChallenge.ByteMeBurger.Domain.Entities;
using FIAP.TechChallenge.ByteMeBurger.Domain.Events;
using FIAP.TechChallenge.ByteMeBurger.Domain.Interfaces;
using FIAP.TechChallenge.ByteMeBurger.Domain.ValueObjects;

namespace FIAP.TechChallenge.ByteMeBurger.Application.UseCases.Products;

public class CreateProductUseCase(IProductRepository repository) : ICreateProductUseCase
{
    public async Task<Product> Execute(string name, string description, ProductCategory category, decimal price,
        string[] images)
    {
        var product = new Product(name, description, category, price, images);
        product.Create();

        var newProduct = await repository.CreateAsync(product);
        DomainEventTrigger.RaiseProductCreated(new ProductCreated(product));
        return newProduct;
    }
}

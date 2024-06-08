// Copyright (c) 2024, Italo Pessoa (https://github.com/italopessoa)
// All rights reserved.
//
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using FIAP.TechChallenge.ByteMeBurger.Application;
using FIAP.TechChallenge.ByteMeBurger.Persistence;

namespace FIAP.TechChallenge.ByteMeBurger.Api;

internal static class ServiceCollectionExtensions
{
    public static void DependencyInversion(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigurePersistenceApp(configuration);
        services.ConfigureApplicationApp();
    }
}

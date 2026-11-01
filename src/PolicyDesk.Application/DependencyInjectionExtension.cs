using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using PolicyDesk.Application.Policies.Cancel;
using PolicyDesk.Application.Policies.Listing;
using PolicyDesk.Application.Policies.Register;
using PolicyDesk.Application.Policyholders.Deactivate;
using PolicyDesk.Application.Policyholders.Listing;
using PolicyDesk.Application.Policyholders.Register;
using PolicyDesk.Application.Policyholders.Update;

[assembly: InternalsVisibleTo("PolicyDesk.Application.Tests")]

namespace PolicyDesk.Application;

public static class DependencyInjectionExtension
{
    public static void AddApplication(this IServiceCollection services)
    {
        // Policyholders
        services.AddScoped<IRegisterPolicyholderUseCase, RegisterPolicyholderUseCase>();
        services.AddScoped<IUpdatePolicyholderCommand, UpdatePolicyholderCommand>();
        services.AddScoped<IDeactivatePolicyholderUseCase, DeactivatePolicyholderUseCase>();
        services.AddScoped<IListPolicyholdersUseCase, ListPolicyholdersUseCase>();
        
        // Policies
        services.AddScoped<IListPoliciesUseCase, ListPoliciesUseCase>();
        services.AddScoped<IRegisterPolicyUseCase, RegisterPolicyUseCase>();
        services.AddScoped<ICancelPolicyUseCase, CancelPolicyUseCase>();
    }
}
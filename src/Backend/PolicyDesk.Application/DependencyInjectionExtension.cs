using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using PolicyDesk.Application.Claims.Analysis;
using PolicyDesk.Application.Claims.Approve;
using PolicyDesk.Application.Claims.Deny;
using PolicyDesk.Application.Claims.Listing;
using PolicyDesk.Application.Claims.Payment;
using PolicyDesk.Application.Claims.Register;
using PolicyDesk.Application.Policies.Cancel;
using PolicyDesk.Application.Policies.Listing;
using PolicyDesk.Application.Policies.Register;
using PolicyDesk.Application.Policyholders.Deactivate;
using PolicyDesk.Application.Policyholders.Listing;
using PolicyDesk.Application.Policyholders.Register;
using PolicyDesk.Application.Policyholders.Update;
using PolicyDesk.Application.Users.Register;

[assembly: InternalsVisibleTo("PolicyDesk.Application.Tests")]

namespace PolicyDesk.Application;

public static class DependencyInjectionExtension
{
    public static void AddApplication(this IServiceCollection services)
    {
        // Policyholders
        services.AddScoped<IRegisterPolicyholderUseCase, RegisterPolicyholderUseCase>();
        services.AddScoped<IUpdatePolicyholderUseCase, UpdatePolicyholderUseCase>();
        services.AddScoped<IDeactivatePolicyholderUseCase, DeactivatePolicyholderUseCase>();
        services.AddScoped<IListPolicyholdersUseCase, ListPolicyholdersUseCase>();
        
        // Policies
        services.AddScoped<IListPoliciesUseCase, ListPoliciesUseCase>();
        services.AddScoped<IRegisterPolicyUseCase, RegisterPolicyUseCase>();
        services.AddScoped<ICancelPolicyUseCase, CancelPolicyUseCase>();

        // Claims
        services.AddScoped<IRegisterClaimUseCase, RegisterClaimUseCase>();
        services.AddScoped<IListClaimsUseCase, ListClaimsUseCase>();
        services.AddScoped<IAnalyzeClaimUseCase, AnalyzeClaimUseCase>();
        services.AddScoped<IApproveClaimUseCase, ApproveClaimUseCase>();
        services.AddScoped<IDenyClaimUseCase, DenyClaimUseCase>();
        services.AddScoped<IPayClaimUseCase, PayClaimUseCase>();

        // Users
        services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
    }
}
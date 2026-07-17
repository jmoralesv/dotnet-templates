using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Modules.Common.Infrastructure.Policies;

public class AuthorizationConfigureOptions(
    IEnumerable<IPolicyFactory> policyFactories,
    ILogger<AuthorizationConfigureOptions> logger)
    : IConfigureOptions<AuthorizationOptions>
{
    private static readonly Action<ILogger, string, Exception?> ConfigurePoliciesMessage =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1, nameof(Configure)),
            "Configuring authorization policies for module: {ModuleName}");

    private static readonly Action<ILogger, string, Exception?> AddedPolicyMessage =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(2, nameof(Configure)),
            "Added policy: {PolicyName}");

    public void Configure(AuthorizationOptions options)
    {
        foreach (var factory in policyFactories)
        {
            ConfigurePoliciesMessage(logger, factory.ModuleName, null);
                
            var policies = factory.GetPolicies();
            
            foreach (var (policyName, policyBuilder) in policies)
            {
                options.AddPolicy(policyName, policyBuilder);
                AddedPolicyMessage(logger, policyName, null);
            }
        }
    }
}

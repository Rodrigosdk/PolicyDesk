using System.Globalization;
using CommonTestUtilities.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Domain.Tests.Validations;

public class FieldLimitsTests
{
    [Fact(DisplayName = nameof(Deve_AceitarNome_Quando_TiverExatamenteOLimiteDepoisDoTrim))]
    [Trait("Domain", "FieldLimits - Segurado")]
    public void Deve_AceitarNome_Quando_TiverExatamenteOLimiteDepoisDoTrim()
    {
        var name = new string('a', FieldLimits.PolicyholderName);

        var policyholder = PolicyholderBuilder.Build(name: $"  {name}  ");

        policyholder.Name.ShouldBe(name);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecao_Quando_NomeExcederOLimiteNoCadastro))]
    [Trait("Domain", "FieldLimits - Segurado")]
    public void Deve_LancarExcecao_Quando_NomeExcederOLimiteNoCadastro()
    {
        var exception = Should.Throw<DomainRuleException>(
            () => PolicyholderBuilder.Build(name: new string('a', FieldLimits.PolicyholderName + 1)));

        exception.Message.ShouldBe(FieldLimits.PolicyholderNameTooLong);
        exception.Message.ShouldContain(FieldLimits.PolicyholderName.ToString(CultureInfo.InvariantCulture));
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoSemAlterar_Quando_NomeExcederOLimiteNaAtualizacao))]
    [Trait("Domain", "FieldLimits - Segurado")]
    public void Deve_LancarExcecaoSemAlterar_Quando_NomeExcederOLimiteNaAtualizacao()
    {
        var policyholder = PolicyholderBuilder.Build();
        var originalName = policyholder.Name;

        var exception = Should.Throw<DomainRuleException>(() => policyholder.Update(
            new string('a', FieldLimits.PolicyholderName + 1),
            policyholder.BirthDate,
            policyholder.Email,
            policyholder.Phone));

        exception.Message.ShouldBe(FieldLimits.PolicyholderNameTooLong);
        policyholder.Name.ShouldBe(originalName);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecao_Quando_DescricaoDaApoliceExcederOLimite))]
    [Trait("Domain", "FieldLimits - Apólice")]
    public void Deve_LancarExcecao_Quando_DescricaoDaApoliceExcederOLimite()
    {
        PolicyBuilder.Build(description: new string('a', FieldLimits.Description)).ShouldNotBeNull();

        var exception = Should.Throw<DomainRuleException>(
            () => PolicyBuilder.Build(description: new string('a', FieldLimits.Description + 1)));

        exception.Message.ShouldBe(FieldLimits.DescriptionTooLong);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecao_Quando_DescricaoDoSinistroExcederOLimite))]
    [Trait("Domain", "FieldLimits - Sinistro")]
    public void Deve_LancarExcecao_Quando_DescricaoDoSinistroExcederOLimite()
    {
        ClaimBuilder.Build(description: new string('a', FieldLimits.Description)).ShouldNotBeNull();

        var exception = Should.Throw<DomainRuleException>(
            () => ClaimBuilder.Build(description: new string('a', FieldLimits.Description + 1)));

        exception.Message.ShouldBe(FieldLimits.DescriptionTooLong);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoSemNegar_Quando_MotivoDaNegacaoExcederOLimite))]
    [Trait("Domain", "FieldLimits - Sinistro")]
    public void Deve_LancarExcecaoSemNegar_Quando_MotivoDaNegacaoExcederOLimite()
    {
        var claim = ClaimBuilder.BuildWithStatus(ClaimStatus.InAnalysis);

        var exception = Should.Throw<DomainRuleException>(
            () => claim.Deny(new string('a', FieldLimits.DenialReason + 1)));

        exception.Message.ShouldBe(FieldLimits.DenialReasonTooLong);
        claim.Status.ShouldBe(ClaimStatus.InAnalysis);
        claim.DenialReason.ShouldBeNull();
    }

    [Fact(DisplayName = nameof(Deve_Negar_Quando_MotivoTiverExatamenteOLimite))]
    [Trait("Domain", "FieldLimits - Sinistro")]
    public void Deve_Negar_Quando_MotivoTiverExatamenteOLimite()
    {
        var claim = ClaimBuilder.BuildWithStatus(ClaimStatus.InAnalysis);
        var reason = new string('a', FieldLimits.DenialReason);

        claim.Deny(reason);

        claim.DenialReason.ShouldBe(reason);
    }
}

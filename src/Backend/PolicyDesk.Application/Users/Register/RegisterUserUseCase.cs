using Microsoft.Extensions.Logging;
using PolicyDesk.Application.Logging;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Users;
using PolicyDesk.Domain.Security.PasswordHashing;
using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Users.Register;

internal class RegisterUserUseCase(
    IUsersWriteOnlyRepository usersWriteOnlyRepository,
    IUsersReadOnlyRepository usersReadOnlyRepository,
    IUsersUpdateOnlyRepository usersUpdateOnlyRepository,
    IPasswordHashing passwordHashing,
    IUnitOfWork unitOfWork,
    BusinessClock clock,
    ILogger<RegisterUserUseCase> logger) : IRegisterUserUseCase
{
    public async Task<ResponseCreatedJson> ExecuteAsync(RequestUserJson request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var email = EmailValidator.Normalize(request.Email);
        if (await usersReadOnlyRepository.ExistActiveUserWithEmailAsync(email, cancellationToken: cancellationToken))
        {
            throw new ErrorOnValidationException(ResourcesMessageException.USER_ACTIVE_EMAIL_EXISTS);
        }

        var inactiveUser = await usersUpdateOnlyRepository.GetLatestInactiveByEmailForUpdateAsync(email, cancellationToken);
        if (inactiveUser is not null && inactiveUser.CanBeReactivated(clock.UtcNow))
        {
            return await ReactivateAsync(inactiveUser, request.Senha, cancellationToken);
        }

        var user = User.Create(request.Nome, email, passwordHashing.HashPassword(request.Senha));

        await usersWriteOnlyRepository.AddAsync(user);
        await unitOfWork.CommitAsync(cancellationToken);
        logger.UserRegistered(user.Id);

        return new ResponseCreatedJson { Id = user.Id };
    }

    private async Task<ResponseCreatedJson> ReactivateAsync(User user, string password, CancellationToken cancellationToken)
    {
        if (!passwordHashing.VerifyPassword(password, user.Password))
        {
            throw new ErrorOnValidationException(ResourcesMessageException.USER_INVALID_CREDENTIALS);
        }

        user.Reactivate();
        await unitOfWork.CommitAsync(cancellationToken);
        logger.UserReactivated(user.Id);

        return new ResponseCreatedJson { Id = user.Id };
    }

    private static void Validate(RequestUserJson request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Nome))
            errors.Add(ResourcesMessageException.USER_NAME_REQUIRED);
        else if (FieldLimits.Exceeds(request.Nome, FieldLimits.UserName))
            errors.Add(FieldLimits.UserNameTooLong);

        if (!EmailValidator.IsValid(request.Email))
            errors.Add(ResourcesMessageException.INVALID_EMAIL);

        if (string.IsNullOrWhiteSpace(request.Senha))
            errors.Add(ResourcesMessageException.USER_PASSWORD_REQUIRED);
        else if (request.Senha.Length < FieldLimits.UserPasswordMinLength)
            errors.Add(FieldLimits.UserPasswordTooShort);

        if (errors.Count > 0)
            throw new ErrorOnValidationException(errors);
    }
}

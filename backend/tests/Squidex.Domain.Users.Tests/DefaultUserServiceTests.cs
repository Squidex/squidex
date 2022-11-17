// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Squidex.Infrastructure;
using Squidex.Shared;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Domain.Users;

public class DefaultUserServiceTests
{
    private readonly UserManager<IdentityUser> userManager = A.Fake<UserManager<IdentityUser>>();
    private readonly IUserFactory userFactory = A.Fake<IUserFactory>();
    private readonly IUserEvents userEvents = A.Fake<IUserEvents>();
    private readonly DefaultUserService sut;

    public DefaultUserServiceTests()
    {
        A.CallTo(() => userFactory.IsId(A<string>._))
            .Returns(true);

        A.CallTo(userManager).WithReturnType<Task<IdentityResult>>()
            .Returns(IdentityResult.Success);

        var log = A.Fake<ILogger<DefaultUserService>>();

        sut = new DefaultUserService(userManager, userFactory, Enumerable.Repeat(userEvents, 1), log);
    }

    [Fact]
    public async Task Should_not_resolve_identity_if_id_not_valid()
    {
        var invalidId = "__";

        A.CallTo(() => userFactory.IsId(invalidId))
            .Returns(false);

        var actual = await sut.FindByIdAsync(invalidId);

        Assert.Null(actual);

        A.CallTo(() => userManager.FindByIdAsync(invalidId))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_return_identity_by_id_if_found()
    {
        var identity = CreateIdentity(found: true);

        var actual = await sut.FindByIdAsync(identity.Id);

        Assert.Same(identity, actual?.Identity);
    }

    [Fact]
    public async Task Should_return_null_if_identity_by_id_not_found()
    {
        var identity = CreateIdentity(found: false);

        var actual = await sut.FindByIdAsync(identity.Id);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_return_identity_by_email_if_found()
    {
        var identity = CreateIdentity(found: true);

        var actual = await sut.FindByEmailAsync(identity.Email!);

        Assert.Same(identity, actual?.Identity);
    }

    [Fact]
    public async Task Should_return_null_if_identity_by_email_not_found()
    {
        var identity = CreateIdentity(found: false);

        var actual = await sut.FindByEmailAsync(identity.Email!);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_return_identity_by_login_if_found()
    {
        var provider = "my-provider";
        var providerKey = "key";

        var identity = CreateIdentity(found: true);

        A.CallTo(() => userManager.FindByLoginAsync(provider, providerKey))
            .Returns(identity);

        var actual = await sut.FindByLoginAsync(provider, providerKey);

        Assert.Same(identity, actual?.Identity);
    }

    [Fact]
    public async Task Should_return_null_if_identity_by_login_not_found()
    {
        var provider = "my-provider";
        var providerKey = "key";

        CreateIdentity(found: false);

        A.CallTo(() => userManager.FindByLoginAsync(provider, providerKey))
            .Returns(Task.FromResult<IdentityUser?>(null));

        var actual = await sut.FindByLoginAsync(provider, providerKey);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_provide_password_existence()
    {
        var identity = CreateIdentity(found: true);

        var user = A.Fake<IUser>();

        A.CallTo(() => user.Identity)
            .Returns(identity);

        A.CallTo(() => userManager.HasPasswordAsync(identity))
            .Returns(true);

        var actual = await sut.HasPasswordAsync(user);

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_provide_logins()
    {
        var logins = new List<UserLoginInfo>();

        var identity = CreateIdentity(found: true);

        var user = A.Fake<IUser>();

        A.CallTo(() => user.Identity)
            .Returns(identity);

        A.CallTo(() => userManager.GetLoginsAsync(identity))
            .Returns(logins);

        var actual = await sut.GetLoginsAsync(user);

        Assert.Same(logins, actual);
    }

    [Fact]
    public async Task Create_should_add_user()
    {
        var identity = CreateIdentity(found: false);

        var values = new UserValues
        {
            Email = identity.Email!
        };

        SetupCreation(identity, 1);

        await sut.CreateAsync(values.Email!, values);

        A.CallTo(() => userEvents.OnUserRegisteredAsync(A<IUser>.That.Matches(x => x.Identity == identity)))
            .MustHaveHappened();

        A.CallTo(() => userEvents.OnConsentGivenAsync(A<IUser>.That.Matches(x => x.Identity == identity)))
            .MustNotHaveHappened();

        A.CallTo(() => userManager.AddClaimsAsync(identity, HasClaim(SquidexClaimTypes.Permissions)))
            .MustNotHaveHappened();

        A.CallTo(() => userManager.AddPasswordAsync(identity, A<string>._))
            .MustNotHaveHappened();

        A.CallTo(() => userManager.SetLockoutEndDateAsync(identity, A<DateTimeOffset>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Create_should_raise_event_if_consent_given()
    {
        var identity = CreateIdentity(found: false);

        var values = new UserValues
        {
            Consent = true
        };

        SetupCreation(identity, 1);

        await sut.CreateAsync(identity.Email!, values);

        A.CallTo(() => userEvents.OnConsentGivenAsync(A<IUser>.That.Matches(x => x.Identity == identity)))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Create_should_set_admin_if_first_user()
    {
        var identity = CreateIdentity(found: false);

        var values = new UserValues
        {
            Consent = true
        };

        SetupCreation(identity, 0);

        await sut.CreateAsync(identity.Email!, values);

        A.CallTo(() => userManager.AddClaimsAsync(identity, HasClaim(SquidexClaimTypes.Permissions, PermissionIds.Admin)))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Create_should_not_lock_first_user()
    {
        var identity = CreateIdentity(found: false);

        var values = new UserValues
        {
            Consent = true
        };

        SetupCreation(identity, 0);

        await sut.CreateAsync(identity.Email!, values, true);

        A.CallTo(() => userManager.SetLockoutEndDateAsync(identity, A<DateTimeOffset>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Create_should_lock_second_user()
    {
        var identity = CreateIdentity(found: false);

        var values = new UserValues
        {
            Consent = true
        };

        SetupCreation(identity, 1);

        await sut.CreateAsync(identity.Email!, values, true);

        A.CallTo(() => userManager.SetLockoutEndDateAsync(identity, InFuture()))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Create_should_add_password()
    {
        var identity = CreateIdentity(found: false);

        var values = new UserValues
        {
            Password = "password"
        };

        SetupCreation(identity, 1);

        await sut.CreateAsync(identity.Email!, values, false);

        A.CallTo(() => userManager.AddPasswordAsync(identity, values.Password))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Update_should_throw_exception_if_not_found()
    {
        var update = new UserValues
        {
            Email = "new@email.com"
        };

        var identity = CreateIdentity(found: false);

        await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.UpdateAsync(identity.Id, update));
    }

    [Fact]
    public async Task Update_should_not_invoke_events_if_silent()
    {
        var update = new UserValues();

        var identity = CreateIdentity(found: true);

        await sut.UpdateAsync(identity.Id, update, true);

        A.CallTo(() => userEvents.OnUserUpdatedAsync(A<IUser>.That.Matches(x => x.Identity == identity), A<IUser>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Update_should_do_nothing_for_new_update()
    {
        var update = new UserValues();

        var identity = CreateIdentity(found: true);

        await sut.UpdateAsync(identity.Id, update);

        A.CallTo(() => userEvents.OnUserUpdatedAsync(A<IUser>.That.Matches(x => x.Identity == identity), A<IUser>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Update_should_change_password_if_changed()
    {
        var update = new UserValues
        {
            Password = "password"
        };

        var identity = CreateIdentity(found: true);

        A.CallTo(() => userManager.HasPasswordAsync(identity))
            .Returns(true);

        await sut.UpdateAsync(identity.Id, update);

        A.CallTo(() => userManager.RemovePasswordAsync(identity))
            .MustHaveHappened();

        A.CallTo(() => userManager.AddPasswordAsync(identity, update.Password))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Update_should_change_email_if_changed()
    {
        var update = new UserValues
        {
            Email = "new@email.com"
        };

        var identity = CreateIdentity(found: true);

        await sut.UpdateAsync(identity.Id, update);

        A.CallTo(() => userManager.SetEmailAsync(identity, update.Email))
            .MustHaveHappened();

        A.CallTo(() => userManager.SetUserNameAsync(identity, update.Email))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Update_should_set_claim_if_consent_given()
    {
        var update = new UserValues
        {
            Consent = true
        };

        var identity = CreateIdentity(found: true);

        await sut.UpdateAsync(identity.Id, update);

        A.CallTo<Task<IdentityResult>>(() => userManager.AddClaimsAsync(identity, HasClaim(SquidexClaimTypes.Consent)))
            .MustHaveHappened();

        A.CallTo(() => userEvents.OnConsentGivenAsync(A<IUser>.That.Matches(x => x.Identity == identity)))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Update_should_set_claim_if_email_consent_given()
    {
        var update = new UserValues
        {
            ConsentForEmails = true
        };

        var identity = CreateIdentity(found: true);

        await sut.UpdateAsync(identity.Id, update);

        A.CallTo<Task<IdentityResult>>(() => userManager.AddClaimsAsync(identity, HasClaim(SquidexClaimTypes.ConsentForEmails)))
            .MustHaveHappened();

        A.CallTo(() => userEvents.OnConsentGivenAsync(A<IUser>.That.Matches(x => x.Identity == identity)))
            .MustHaveHappened();
    }

    [Fact]
    public async Task SetPassword_should_throw_exception_if_not_found()
    {
        var password = "password";

        var identity = CreateIdentity(found: false);

        await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.SetPasswordAsync(identity.Id, password, null));
    }

    [Fact]
    public async Task SetPassword_should_succeed_if_found()
    {
        var password = "password";

        var identity = CreateIdentity(found: true);

        await sut.SetPasswordAsync(identity.Id, password, null);

        A.CallTo(() => userManager.AddPasswordAsync(identity, password))
            .MustHaveHappened();
    }

    [Fact]
    public async Task SetPassword_should_change_password_if_identity_has_password()
    {
        var password = "password";

        var identity = CreateIdentity(found: true);

        A.CallTo(() => userManager.HasPasswordAsync(identity))
            .Returns(true);

        await sut.SetPasswordAsync(identity.Id, password, "old");

        A.CallTo(() => userManager.ChangePasswordAsync(identity, "old", password))
            .MustHaveHappened();
    }

    [Fact]
    public async Task AddLogin_should_throw_exception_if_not_found()
    {
        var login = A.Fake<ExternalLoginInfo>();

        var identity = CreateIdentity(found: false);

        await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.AddLoginAsync(identity.Id, login));
    }

    [Fact]
    public async Task AddLogin_should_succeed_if_found()
    {
        var login = A.Fake<ExternalLoginInfo>();

        var identity = CreateIdentity(found: true);

        await sut.AddLoginAsync(identity.Id, login);

        A.CallTo(() => userManager.AddLoginAsync(identity, login))
            .MustHaveHappened();
    }

    [Fact]
    public async Task RemoveLogin_should_throw_exception_if_not_found()
    {
        var provider = "my-provider";
        var providerKey = "key";

        var identity = CreateIdentity(found: false);

        await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.RemoveLoginAsync(identity.Id, provider, providerKey));
    }

    [Fact]
    public async Task RemoveLogin_should_succeed_if_found()
    {
        var provider = "my-provider";
        var providerKey = "key";

        var identity = CreateIdentity(found: true);

        await sut.RemoveLoginAsync(identity.Id, provider, providerKey);

        A.CallTo(() => userManager.RemoveLoginAsync(identity, provider, providerKey))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Lock_should_throw_exception_if_not_found()
    {
        var identity = CreateIdentity(found: false);

        await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.LockAsync(identity.Id));
    }

    [Fact]
    public async Task Lock_should_succeed_if_found()
    {
        var identity = CreateIdentity(found: true);

        await sut.LockAsync(identity.Id);

        A.CallTo<Task<IdentityResult>>(() => userManager.SetLockoutEndDateAsync(identity, InFuture()))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Unlock_should_throw_exception_if_not_found()
    {
        var identity = CreateIdentity(found: false);

        await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.UnlockAsync(identity.Id));
    }

    [Fact]
    public async Task Unlock_should_succeeed_if_found()
    {
        var identity = CreateIdentity(found: true);

        await sut.UnlockAsync(identity.Id);

        A.CallTo(() => userManager.SetLockoutEndDateAsync(identity, null))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Delete_should_throw_exception_if_not_found()
    {
        var identity = CreateIdentity(found: false);

        await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.DeleteAsync(identity.Id));

        A.CallTo(() => userEvents.OnUserDeletedAsync(A<IUser>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Delete_should_succeed_if_found()
    {
        var identity = CreateIdentity(found: true);

        await sut.DeleteAsync(identity.Id);

        A.CallTo(() => userManager.DeleteAsync(identity))
            .MustHaveHappened();

        A.CallTo(() => userEvents.OnUserDeletedAsync(A<IUser>.That.Matches(x => x.Identity == identity)))
            .MustHaveHappened();
    }

    private IdentityUser CreateIdentity(bool found, string id = "123")
    {
        var identity = CreatePendingUser(id);

        if (found)
        {
            A.CallTo(() => userManager.FindByIdAsync(identity.Id))
                .Returns(identity);

            A.CallTo(() => userManager.FindByEmailAsync(identity.Email!))
                .Returns(identity);
        }
        else
        {
            A.CallTo(() => userManager.FindByIdAsync(identity.Id))
                .Returns(Task.FromResult<IdentityUser?>(null));

            A.CallTo(() => userManager.FindByEmailAsync(identity.Email!))
                .Returns(Task.FromResult<IdentityUser?>(null));
        }

        return identity;
    }

    private void SetupCreation(IdentityUser identity, int numCurrentUsers)
    {
        var users = new List<IdentityUser>();

        for (var i = 0; i < numCurrentUsers; i++)
        {
            users.Add(CreatePendingUser(i.ToString(CultureInfo.InvariantCulture)));
        }

        A.CallTo(() => userManager.Users)
            .Returns(users.AsQueryable());

        A.CallTo(() => userFactory.Create(identity.Email!))
            .Returns(identity);
    }

    private static IEnumerable<Claim> HasClaim(string claim)
    {
        return A<IEnumerable<Claim>>.That.Matches(x => x.Any(y => y.Type == claim));
    }

    private static IEnumerable<Claim> HasClaim(string claim, string value)
    {
        return A<IEnumerable<Claim>>.That.Matches(x => x.Any(y => y.Type == claim && y.Value == value));
    }

    private static DateTimeOffset InFuture()
    {
        return A<DateTimeOffset>.That.Matches(x => x >= DateTimeOffset.UtcNow.AddYears(1));
    }

    private static IdentityUser CreatePendingUser(string id = "123")
    {
        return new IdentityUser
        {
            Id = id,
            Email = $"{id}@email.com"
        };
    }
}

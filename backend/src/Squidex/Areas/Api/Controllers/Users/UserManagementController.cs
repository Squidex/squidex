// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.Users.Models;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Translations;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Users;

/// <summary>
/// Retrieve and manage users.
/// </summary>
[ApiModelValidation(true)]
[ApiExplorerSettings(GroupName = "UserManagement")]
public sealed class UserManagementController : ApiController
{
    private readonly IUserService userService;

    public UserManagementController(ICommandBus commandBus, IUserService userService)
        : base(commandBus)
    {
        this.userService = userService;
    }

    /// <summary>
    /// Get users by query.
    /// </summary>
    /// <param name="query">Optional query to search by email address or username.</param>
    /// <param name="skip">The number of users to skip.</param>
    /// <param name="take">The number of users to return.</param>
    /// <response code="200">Users returned.</response>.
    [HttpGet]
    [Route("user-management/")]
    [ProducesResponseType(typeof(UsersDto), StatusCodes.Status200OK)]
    [ApiPermission(PermissionIds.AdminUsersRead)]
    public async Task<IActionResult> GetUsers([FromQuery] string? query = null, [FromQuery] int skip = 0, [FromQuery] int take = 10)
    {
        var users = await userService.QueryAsync(query, take, skip, HttpContext.RequestAborted);

        var response = UsersDto.FromDomain(users, users.Total, Resources);

        return Ok(response);
    }

    /// <summary>
    /// Get a user by ID.
    /// </summary>
    /// <param name="id">The ID of the user.</param>
    /// <response code="200">User returned.</response>.
    /// <response code="404">User not found.</response>.
    [HttpGet]
    [Route("user-management/{id}/")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ApiPermission(PermissionIds.AdminUsersRead)]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await userService.FindByIdAsync(id, HttpContext.RequestAborted);

        if (user == null)
        {
            return NotFound();
        }

        var response = UserDto.FromDomain(user, Resources);

        return Ok(response);
    }

    /// <summary>
    /// Create a new user.
    /// </summary>
    /// <param name="request">The user object that needs to be added.</param>
    /// <response code="201">User created.</response>.
    /// <response code="400">User request not valid.</response>.
    [HttpPost]
    [Route("user-management/")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ApiPermission(PermissionIds.AdminUsersCreate)]
    public async Task<IActionResult> PostUser([FromBody] CreateUserDto request)
    {
        var user = await userService.CreateAsync(request.Email, request.ToValues(), ct: HttpContext.RequestAborted);

        var response = UserDto.FromDomain(user, Resources);

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, response);
    }

    /// <summary>
    /// Update a user.
    /// </summary>
    /// <param name="id">The ID of the user.</param>
    /// <param name="request">The user object that needs to be updated.</param>
    /// <response code="200">User created.</response>.
    /// <response code="400">User request not valid.</response>.
    /// <response code="404">User not found.</response>.
    [HttpPut]
    [Route("user-management/{id}/")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ApiPermission(PermissionIds.AdminUsersUpdate)]
    public async Task<IActionResult> PutUser(string id, [FromBody] UpdateUserDto request)
    {
        var user = await userService.UpdateAsync(id, request.ToValues(), ct: HttpContext.RequestAborted);

        var response = UserDto.FromDomain(user, Resources);

        return Ok(response);
    }

    /// <summary>
    /// Lock a user.
    /// </summary>
    /// <param name="id">The ID of the user to lock.</param>
    /// <response code="200">User locked.</response>.
    /// <response code="403">User is the current user.</response>.
    /// <response code="404">User not found.</response>.
    [HttpPut]
    [Route("user-management/{id}/lock/")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ApiPermission(PermissionIds.AdminUsersLock)]
    public async Task<IActionResult> LockUser(string id)
    {
        if (this.IsUser(id))
        {
            throw new DomainForbiddenException(T.Get("users.lockYourselfError"));
        }

        var user = await userService.LockAsync(id, HttpContext.RequestAborted);

        var response = UserDto.FromDomain(user, Resources);

        return Ok(response);
    }

    /// <summary>
    /// Unlock a user.
    /// </summary>
    /// <param name="id">The ID of the user to unlock.</param>
    /// <response code="200">User unlocked.</response>.
    /// <response code="403">User is the current user.</response>.
    /// <response code="404">User not found.</response>.
    [HttpPut]
    [Route("user-management/{id}/unlock/")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ApiPermission(PermissionIds.AdminUsersUnlock)]
    public async Task<IActionResult> UnlockUser(string id)
    {
        if (this.IsUser(id))
        {
            throw new DomainForbiddenException(T.Get("users.unlockYourselfError"));
        }

        var user = await userService.UnlockAsync(id, HttpContext.RequestAborted);

        var response = UserDto.FromDomain(user, Resources);

        return Ok(response);
    }

    /// <summary>
    /// Delete a User.
    /// </summary>
    /// <param name="id">The ID of the user to delete.</param>
    /// <response code="204">User deleted.</response>.
    /// <response code="403">User is the current user.</response>.
    /// <response code="404">User not found.</response>.
    [HttpDelete]
    [Route("user-management/{id}/")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermission(PermissionIds.AdminUsersUnlock)]
    public async Task<IActionResult> DeleteUser(string id)
    {
        if (this.IsUser(id))
        {
            throw new DomainForbiddenException(T.Get("users.deleteYourselfError"));
        }

        await userService.DeleteAsync(id, HttpContext.RequestAborted);

        return NoContent();
    }
}

// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities;

internal static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to register user in notifo: {details}.")]
    public static partial void LogFailedToRegisterUserInNotifoWithDetails(ILogger logger, string details, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to register user in notifo.")]
    public static partial void LogFailedToRegisterUserInNotifo(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to push user to notifo: {details}.")]
    public static partial void LogFailedToPushUserToNotifoWithDetails(ILogger logger, string details, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to push user to notifo.")]
    public static partial void LogFailedToPushUserToNotifo(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to delete asset recursively.")]
    public static partial void LogFailedToDeleteAssetRecursively(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Backup with job id {backupId} with from URL '{url}' started.")]
    public static partial void LogRestoreJobStarted(ILogger logger, DomainId backupId, Uri url);

    [LoggerMessage(Level = LogLevel.Information, Message = "Backup with job id {backupId} from URL '{url}' completed.")]
    public static partial void LogRestoreJobCompleted(ILogger logger, DomainId backupId, Uri url);

    [LoggerMessage(Level = LogLevel.Error, Message = "Backup with job id {backupId} from URL '{url}' failed.")]
    public static partial void LogRestoreJobFailed(ILogger logger, DomainId backupId, Uri url, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to clean up restore.")]
    public static partial void LogFailedToCleanUpRestore(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to handle yjs event.")]
    public static partial void LogFailedToHandleYjsEvent(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Cannot send email to {email}: No email subject configured for template {template}.")]
    public static partial void LogNoEmailSubjectConfigured(ILogger logger, string email, string template);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Cannot send email to {email}: No email body configured for template {template}.")]
    public static partial void LogNoEmailBodyConfigured(ILogger logger, string email, string template);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to send notification to {email}.")]
    public static partial void LogFailedToSendNotification(ILogger logger, string email, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to query scheduled status changes-")]
    public static partial void LogFailedToQueryScheduledStatusChanges(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to execute scheduled status change for content '{contentId}'.")]
    public static partial void LogFailedToExecuteScheduledStatusChange(ILogger logger, DomainId contentId, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to resolve field {field}.")]
    public static partial void LogFailedToResolveField(ILogger logger, string field, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to resolve execute query.")]
    public static partial void LogFailedToResolveQuery(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to invite user: Assigner {assignerId} not found.")]
    public static partial void LogInvitationAssignerNotFound(ILogger logger, RefToken assignerId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to invite user: Assignee {assigneeId} not found.")]
    public static partial void LogInvitationAssigneeNotFound(ILogger logger, string assigneeId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Removed unfinished jobs for owner {ownerId} after start.")]
    public static partial void LogRemovedUnfinishedJobs(ILogger logger, DomainId ownerId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Clearing jobs for owner {ownerId}.")]
    public static partial void LogClearingJobs(ILogger logger, DomainId ownerId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting new backup with backup id '{backupId}' for owner {ownerId}.")]
    public static partial void LogStartingJob(ILogger logger, DomainId backupId, DomainId ownerId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to run job with ID {jobId}.")]
    public static partial void LogFailedToRunJob(ILogger logger, DomainId jobId, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Adding rule job for Rule(trigger={ruleTrigger})")]
    public static partial void LogAddingRuleJob(ILogger logger, string ruleTrigger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to run rule with ID {ruleId}, continue with next job.")]
    public static partial void LogFailedToRunRule(ILogger logger, DomainId? ruleId, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to execute search from source {source} with query '{query}'.")]
    public static partial void LogFailedToExecuteSearch(ILogger logger, string source, string query, Exception exception);
}

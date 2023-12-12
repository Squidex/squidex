// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject;

public partial class AppDomainObject
{
    protected override App Apply(App snapshot, Envelope<IEvent> @event)
    {
        var newSnapshot = snapshot;

        switch (@event.Payload)
        {
            case AppCreated e:
                newSnapshot = snapshot with { Id = e.AppId.Id };
                SimpleMapper.Map(e, newSnapshot);
                break;

            case AppUpdated e:
                newSnapshot = snapshot.Annotate(e.Label, e.Description);
                break;

            case AppTransfered e:
                newSnapshot = snapshot.Transfer(e.TeamId);
                break;

            case AppSettingsUpdated e:
                newSnapshot = snapshot.UpdateSettings(e.Settings);
                break;

            case AppAssetsScriptsConfigured e:
                newSnapshot = snapshot.UpdateAssetScripts(e.Scripts);
                break;

            case AppPlanChanged e:
                newSnapshot = snapshot.ChangePlan(new AssignedPlan(e.Actor, e.PlanId));
                break;

            case AppPlanReset e:
                newSnapshot = snapshot.ChangePlan(null);
                break;

            case AppImageUploaded e:
                newSnapshot = snapshot.UpdateImage(e.Image);
                break;

            case AppImageRemoved e:
                newSnapshot = snapshot.UpdateImage(null);
                break;

            case AppContributorAssigned e:
                newSnapshot = snapshot.UpdateContributors(e, (e, c) => c.Assign(e.ContributorId, e.Role));
                break;

            case AppContributorRemoved e:
                newSnapshot = snapshot.UpdateContributors(e, (e, c) => c.Remove(e.ContributorId));
                break;

            case AppClientAttached e:
                newSnapshot = snapshot.UpdateClients(e, (e, c) => c.Add(e.Id, e.Secret, e.Role));
                break;

            case AppClientUpdated e:
                newSnapshot = snapshot.UpdateClients(e, (e, c) => c.Update(e.Id, e.Name, e.Role, e.ApiCallsLimit, e.ApiTrafficLimit, e.AllowAnonymous));
                break;

            case AppClientRevoked e:
                newSnapshot = snapshot.UpdateClients(e, (e, c) => c.Revoke(e.Id));
                break;

            case AppWorkflowAdded e:
                newSnapshot = snapshot.UpdateWorkflows(e, (e, w) => w.Add(e.WorkflowId, e.Name));
                break;

            case AppWorkflowUpdated e:
                newSnapshot = snapshot.UpdateWorkflows(e, (e, w) => w.Update(e.WorkflowId, e.Workflow));
                break;

            case AppWorkflowDeleted e:
                newSnapshot = snapshot.UpdateWorkflows(e, (e, w) => w.Remove(e.WorkflowId));
                break;

            case AppRoleAdded e:
                newSnapshot = snapshot.UpdateRoles(e, (e, r) => r.Add(e.Name));
                break;

            case AppRoleUpdated e:
                newSnapshot = snapshot.UpdateRoles(e, (e, r) => r.Update(e.Name, e.ToPermissions(), e.Properties));
                break;

            case AppRoleDeleted e:
                newSnapshot = snapshot.UpdateRoles(e, (e, r) => r.Remove(e.Name));
                break;

            case AppLanguageAdded e:
                newSnapshot = snapshot.UpdateLanguages(e, (e, l) => l.Set(e.Language));
                break;

            case AppLanguageRemoved e:
                newSnapshot = snapshot.UpdateLanguages(e, (e, l) => l.Remove(e.Language));
                break;

            case AppLanguageUpdated e:
                return newSnapshot = snapshot.UpdateLanguages(e, (e, l) =>
                {
                    l = l.Set(e.Language, e.IsOptional, e.Fallback);

                    if (e.IsMaster)
                    {
                        l = l.MakeMaster(e.Language);
                    }

                    return l;
                });

            case AppDeleted:
                newSnapshot = snapshot with { Plan = null, IsDeleted = true };
                break;
        }

        if (ReferenceEquals(newSnapshot, snapshot))
        {
            return snapshot;
        }

        return newSnapshot.Apply(@event.To<SquidexEvent>());
    }
}

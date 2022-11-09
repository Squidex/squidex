// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject;

public partial class AppDomainObject
{
    [CollectionName("Apps")]
    public sealed class State : DomainObjectState<State>, IAppEntity
    {
        public string Name { get; set; }

        public string Label { get; set; }

        public string Description { get; set; }

        public DomainId? TeamId { get; set; }

        public Contributors Contributors { get; set; } = Contributors.Empty;

        public Roles Roles { get; set; } = Roles.Empty;

        public AssignedPlan? Plan { get; set; }

        public AppClients Clients { get; set; } = AppClients.Empty;

        public AppImage? Image { get; set; }

        public AppSettings Settings { get; set; } = AppSettings.Empty;

        public AssetScripts AssetScripts { get; set; } = new AssetScripts();

        public LanguagesConfig Languages { get; set; } = LanguagesConfig.English;

        public Workflows Workflows { get; set; } = Workflows.Empty;

        public bool IsDeleted { get; set; }

        [JsonIgnore]
        public DomainId UniqueId
        {
            get => Id;
        }

        public override bool ApplyEvent(IEvent @event)
        {
            switch (@event)
            {
                case AppCreated e:
                    {
                        Id = e.AppId.Id;

                        SimpleMapper.Map(e, this);
                        return true;
                    }

                case AppUpdated e when Is.Change(Label, e.Label) || Is.Change(Description, e.Description):
                    {
                        SimpleMapper.Map(e, this);
                        return true;
                    }

                case AppTransfered e when Is.Change(TeamId, e.TeamId):
                    {
                        SimpleMapper.Map(e, this);
                        return true;
                    }

                case AppSettingsUpdated e when Is.Change(Settings, e.Settings):
                    return UpdateSettings(e.Settings);

                case AppAssetsScriptsConfigured e when Is.Change(e.Scripts, AssetScripts):
                    return UpdateAssetScripts(e.Scripts);

                case AppPlanChanged e when Is.Change(Plan?.PlanId, e.PlanId):
                    return UpdatePlan(e.ToPlan());

                case AppPlanReset e when Plan != null:
                    return UpdatePlan(null);

                case AppImageUploaded e:
                    return UpdateImage(e.Image);

                case AppImageRemoved e when Image != null:
                    return UpdateImage(null);

                case AppContributorAssigned e:
                    return UpdateContributors(e, (e, c) => c.Assign(e.ContributorId, e.Role));

                case AppContributorRemoved e:
                    return UpdateContributors(e, (e, c) => c.Remove(e.ContributorId));

                case AppClientAttached e:
                    return UpdateClients(e, (e, c) => c.Add(e.Id, e.Secret, e.Role));

                case AppClientUpdated e:
                    return UpdateClients(e, (e, c) => c.Update(e.Id, e.Name, e.Role, e.ApiCallsLimit, e.ApiTrafficLimit, e.AllowAnonymous));

                case AppClientRevoked e:
                    return UpdateClients(e, (e, c) => c.Revoke(e.Id));

                case AppWorkflowAdded e:
                    return UpdateWorkflows(e, (e, w) => w.Add(e.WorkflowId, e.Name));

                case AppWorkflowUpdated e:
                    return UpdateWorkflows(e, (e, w) => w.Update(e.WorkflowId, e.Workflow));

                case AppWorkflowDeleted e:
                    return UpdateWorkflows(e, (e, w) => w.Remove(e.WorkflowId));

                case AppRoleAdded e:
                    return UpdateRoles(e, (e, r) => r.Add(e.Name));

                case AppRoleUpdated e:
                    return UpdateRoles(e, (e, r) => r.Update(e.Name, e.ToPermissions(), e.Properties));

                case AppRoleDeleted e:
                    return UpdateRoles(e, (e, r) => r.Remove(e.Name));

                case AppLanguageAdded e:
                    return UpdateLanguages(e, (e, l) => l.Set(e.Language));

                case AppLanguageRemoved e:
                    return UpdateLanguages(e, (e, l) => l.Remove(e.Language));

                case AppLanguageUpdated e:
                    return UpdateLanguages(e, (e, l) =>
                    {
                        l = l.Set(e.Language, e.IsOptional, e.Fallback);

                        if (e.IsMaster)
                        {
                            l = Languages.MakeMaster(e.Language);
                        }

                        return l;
                    });

                case AppDeleted:
                    {
                        Plan = null;

                        IsDeleted = true;
                        return true;
                    }
            }

            return false;
        }

        private bool UpdateContributors<T>(T @event, Func<T, Contributors, Contributors> update)
        {
            var previous = Contributors;

            Contributors = update(@event, previous);

            return !ReferenceEquals(previous, Contributors);
        }

        private bool UpdateClients<T>(T @event, Func<T, AppClients, AppClients> update)
        {
            var previous = Clients;

            Clients = update(@event, previous);

            return !ReferenceEquals(previous, Clients);
        }

        private bool UpdateLanguages<T>(T @event, Func<T, LanguagesConfig, LanguagesConfig> update)
        {
            var previous = Languages;

            Languages = update(@event, previous);

            return !ReferenceEquals(previous, Languages);
        }

        private bool UpdateRoles<T>(T @event, Func<T, Roles, Roles> update)
        {
            var previous = Roles;

            Roles = update(@event, previous);

            return !ReferenceEquals(previous, Roles);
        }

        private bool UpdateWorkflows<T>(T @event, Func<T, Workflows, Workflows> update)
        {
            var previous = Workflows;

            Workflows = update(@event, previous);

            return !ReferenceEquals(previous, Workflows);
        }

        private bool UpdateImage(AppImage? image)
        {
            Image = image;

            return true;
        }

        private bool UpdateAssetScripts(AssetScripts? scripts)
        {
            AssetScripts = scripts ?? new AssetScripts();

            return true;
        }

        private bool UpdateSettings(AppSettings settings)
        {
            Settings = settings;

            return true;
        }

        private bool UpdatePlan(AssignedPlan? plan)
        {
            Plan = plan;

            return true;
        }
    }
}

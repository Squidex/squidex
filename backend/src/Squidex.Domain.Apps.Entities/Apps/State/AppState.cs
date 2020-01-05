// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Squidex.Domain.Apps.Entities.Apps.State
{
    [CollectionName("Apps")]
    public class AppState : DomainObjectState<AppState>, IAppEntity
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Label { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public Roles Roles { get; set; } = Roles.Empty;

        [DataMember]
        public AppPlan? Plan { get; set; }

        [DataMember]
        public AppImage? Image { get; set; }

        [DataMember]
        public AppClients Clients { get; set; } = AppClients.Empty;

        [DataMember]
        public AppPatterns Patterns { get; set; } = AppPatterns.Empty;

        [DataMember]
        public AppContributors Contributors { get; set; } = AppContributors.Empty;

        [DataMember]
        public LanguagesConfig LanguagesConfig { get; set; } = LanguagesConfig.English;

        [DataMember]
        public Workflows Workflows { get; set; } = Workflows.Empty;

        [DataMember]
        public bool IsArchived { get; set; }

        public override bool ApplyEvent(IEvent @event)
        {
            switch (@event)
            {
                case AppCreated e:
                    {
                        SimpleMapper.Map(e, this);

                        return true;
                    }

                case AppUpdated e when !string.Equals(e.Label, Label) || !string.Equals(e.Description, Description):
                    {
                        SimpleMapper.Map(e, this);

                        return true;
                    }

                case AppImageUploaded e:
                    {
                        Image = e.Image;

                        return true;
                    }

                case AppImageRemoved _ when Image != null:
                    {
                        Image = null;

                        break;
                    }

                case AppPlanChanged e when !string.Equals(Plan?.PlanId, e.PlanId):
                    {
                        Plan = AppPlan.Build(e.Actor, e.PlanId);

                        break;
                    }

                case AppPlanReset _ when Plan != null:
                    {
                        Plan = null;

                        break;
                    }

                case AppContributorAssigned e:
                    {
                        Contributors = Contributors.Assign(e.ContributorId, e.Role);

                        break;
                    }

                case AppContributorRemoved e:
                    {
                        Contributors = Contributors.Remove(e.ContributorId);

                        break;
                    }

                case AppClientAttached e:
                    {
                        Clients = Clients.Add(e.Id, e.Secret);

                        break;
                    }

                case AppClientUpdated e:
                    {
                        Clients = Clients.Update(e.Id, e.Role);

                        break;
                    }

                case AppClientRenamed e:
                    {
                        Clients = Clients.Rename(e.Id, e.Name);

                        break;
                    }

                case AppClientRevoked e:
                    {
                        Clients = Clients.Revoke(e.Id);

                        break;
                    }

                case AppWorkflowAdded e:
                    {
                        Workflows = Workflows.Add(e.WorkflowId, e.Name);

                        break;
                    }

                case AppWorkflowUpdated e:
                    {
                        Workflows = Workflows.Update(e.WorkflowId, e.Workflow);

                        break;
                    }

                case AppWorkflowDeleted e:
                    {
                        Workflows = Workflows.Remove(e.WorkflowId);

                        break;
                    }

                case AppPatternAdded e:
                    {
                        Patterns = Patterns.Add(e.PatternId, e.Name, e.Pattern, e.Message);

                        break;
                    }

                case AppPatternDeleted e:
                    {
                        Patterns = Patterns.Remove(e.PatternId);

                        break;
                    }

                case AppPatternUpdated e:
                    {
                        Patterns = Patterns.Update(e.PatternId, e.Name, e.Pattern, e.Message);

                        break;
                    }

                case AppRoleAdded e:
                    {
                        Roles = Roles.Add(e.Name);

                        break;
                    }

                case AppRoleDeleted e:
                    {
                        Roles = Roles.Remove(e.Name);

                        break;
                    }

                case AppRoleUpdated e:
                    {
                        Roles = Roles.Update(e.Name, e.Permissions);

                        break;
                    }

                case AppLanguageAdded e:
                    {
                        LanguagesConfig = LanguagesConfig.Set(e.Language);

                        break;
                    }

                case AppLanguageRemoved e:
                    {
                        LanguagesConfig = LanguagesConfig.Remove(e.Language);

                        break;
                    }

                case AppLanguageUpdated e:
                    {
                        LanguagesConfig = LanguagesConfig.Set(e.Language, e.IsOptional, e.Fallback);

                        if (e.IsMaster)
                        {
                            LanguagesConfig = LanguagesConfig.MakeMaster(e.Language);
                        }

                        break;
                    }

                case AppArchived _:
                    {
                        Plan = null;

                        IsArchived = true;

                        break;
                    }
            }

            return false;
        }
    }
}

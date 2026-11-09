/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { form, FormField } from '@angular/forms/signals';
import { AllAppsState, AppDto, AppFormComponent, AppsState, AuthService, DialogModel, FeatureDto, LocalStoreService, ModalDirective, NewsService, PagerComponent, Settings, TeamDto, TeamsState, TemplateDto, TemplatesState, TitleComponent, TourState, TourStepDirective, TranslatePipe, UIOptions, UIState } from '@app/shared';
import { AppComponent } from './app.component';
import { NewsDialogComponent } from './news-dialog.component';
import { OnboardingDialogComponent } from './onboarding-dialog.component';
import { TeamComponent } from './team.component';

type GroupedApps = { team?: TeamDto; apps: AppDto[] };

@Component({
    selector: 'sqx-apps-page',
    styleUrls: ['./apps-page.component.scss'],
    templateUrl: './apps-page.component.html',
    imports: [
        AppComponent,
        AppFormComponent,
        FormField,
        FormsModule,
        ModalDirective,
        NewsDialogComponent,
        OnboardingDialogComponent,
        PagerComponent,
        TeamComponent,
        TitleComponent,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class AppsPageComponent {
    private readonly appsState = inject(AppsState);
    private readonly localStore = inject(LocalStoreService);
    private readonly newsService = inject(NewsService);
    private readonly teamsState = inject(TeamsState);
    private readonly tourState = inject(TourState);
    private readonly uiOptions = inject(UIOptions);

    protected readonly templatesState = inject(TemplatesState);

    protected readonly allAppsState = inject(AllAppsState);

    protected readonly authState = inject(AuthService);

    protected readonly uiState = inject(UIState);

    protected readonly addAppTemplate = signal<TemplateDto | undefined>(undefined);

    protected readonly newsFeatures = signal<ReadonlyArray<FeatureDto> | undefined>(undefined);

    protected readonly showAllApps = computed(() => {
        const query = this.allAppsState.query();

        // Keep the section visible while searching, even if the search returns fewer apps.
        return this.allAppsState.containsAllApps() && (!!query || this.allAppsState.total() > this.appsState.apps().length);
    });

    protected readonly groupedApps = computed(() => {
        const apps = this.appsState.apps();
        const teams = this.teamsState.teams();
        const grouped: GroupedApps[] = [{ apps: [] }];

        for (const team of teams) {
            grouped.push({ team, apps: [] });
        }

        for (const app of apps) {
            const group = grouped.find(x => x.team?.id === app.teamId) || grouped[0];

            group.apps.push(app);
        }

        if (grouped[0].apps.length === 0) {
            grouped.shift();
        }

        return grouped;
    });

    protected readonly addAppDialog = new DialogModel();

    protected readonly onboardingDialog = new DialogModel();

    protected readonly newsDialog = new DialogModel();

    protected readonly generalInfo: string = this.uiOptions.value.showInfo ? this.uiOptions.value.info : '';

    protected readonly allAppsFilter = form(signal(''));

    constructor() {
        const apps = this.appsState.apps();
        if (this.shouldShowOnboarding(apps)) {
            this.onboardingDialog.show();
        } else {
            if (this.tourState.snapshot.status !== 'Started') {
                this.tourState.complete();
            }

            if (!this.uiOptions.value.hideNews) {
                const newsVersion = this.localStore.getInt(Settings.Local.NEWS_VERSION);

                this.newsService.getFeatures(newsVersion)
                    .subscribe(result => {
                        if (result.version !== newsVersion) {
                            if (result.features.length > 0) {
                                this.newsFeatures.set(result.features);
                                this.newsDialog.show();
                            }

                            this.localStore.setInt(Settings.Local.NEWS_VERSION, result.version);
                        }
                    });
            }
        }

        this.templatesState.load(false, true);

        this.allAppsState.load();
    }

    protected searchAllApps() {
        this.allAppsState.search(this.allAppsFilter().value());
    }

    public createNewApp(template?: TemplateDto) {
        this.addAppTemplate.set(template);
        this.addAppDialog.show();
    }

    protected leaveApp(app: AppDto) {
        this.appsState.leave(app);
    }

    protected leaveTeam(team: TeamDto) {
        this.teamsState.leave(team);
    }

    protected trackByGroup(_index: number, group: GroupedApps) {
        return group.team?.id || '0';
    }

    private shouldShowOnboarding(apps: ReadonlyArray<AppDto>) {
        if (apps.length > 0) {
            return false;
        }

        if (this.uiOptions.value.hideOnboarding === true) {
            return false;
        }

        const tourStatus = this.tourState.snapshot.status;

        return tourStatus !== 'Completed' && tourStatus !== 'Started';
    }
}

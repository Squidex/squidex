/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { combineLatest } from 'rxjs';
import { map, take } from 'rxjs/operators';
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
        AsyncPipe,
        FormsModule,
        ModalDirective,
        NewsDialogComponent,
        OnboardingDialogComponent,
        PagerComponent,
        ReactiveFormsModule,
        TeamComponent,
        TitleComponent,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class AppsPageComponent {
    public readonly addAppDialog = new DialogModel();
    public readonly addAppTemplate = signal<TemplateDto | undefined>(undefined);

    public readonly onboardingDialog = new DialogModel();

    public readonly newsFeatures = signal<ReadonlyArray<FeatureDto> | undefined>(undefined);
    public readonly newsDialog = new DialogModel();

    public readonly generalInfo: string = this.uiOptions.value.showInfo ? this.uiOptions.value.info : '';

    public readonly starters = this.templatesState.starters;

    public readonly allAppsFilter = new UntypedFormControl();

    public readonly showAllApps = toSignal(
        combineLatest([
            this.allAppsState.containsAllApps,
            this.allAppsState.total,
            this.allAppsState.query,
            this.appsState.apps,
        ]).pipe(map(([containsAllApps, total, query, apps]) => {
            // Keep the section visible while searching, even if the search returns fewer apps.
            return containsAllApps && (!!query || total > apps.length);
        })));

    public readonly groupedApps = toSignal(
        combineLatest([
            this.appsState.apps,
            this.teamsState.teams,
        ]).pipe(map(([apps, teams]) => {
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
        })));

    constructor(
        public readonly allAppsState: AllAppsState,
        public readonly authState: AuthService,
        public readonly uiState: UIState,
        private readonly appsState: AppsState,
        private readonly localStore: LocalStoreService,
        private readonly newsService: NewsService,
        private readonly teamsState: TeamsState,
        private readonly templatesState: TemplatesState,
        private readonly tourState: TourState,
        private readonly uiOptions: UIOptions,
    ) {
        this.appsState.apps.pipe(take(1))
            .subscribe(apps => {
                if (this.shouldShowOnboarding(apps)) {
                    this.onboardingDialog.show();
                    return;
                }

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
            });

        this.templatesState.load(false, true);

        this.allAppsState.load();
    }

    public searchAllApps() {
        this.allAppsState.search(this.allAppsFilter.value);
    }

    public createNewApp(template?: TemplateDto) {
        this.addAppTemplate.set(template);
        this.addAppDialog.show();
    }

    public leaveApp(app: AppDto) {
        this.appsState.leave(app);
    }

    public leaveTeam(team: TeamDto) {
        this.teamsState.leave(team);
    }

    public trackByGroup(_index: number, group: GroupedApps) {
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

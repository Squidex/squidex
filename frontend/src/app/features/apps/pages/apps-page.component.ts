/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
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
export class AppsPageComponent implements OnInit {
    public addAppDialog = new DialogModel();
    public addAppTemplate?: TemplateDto;

    public onboardingDialog = new DialogModel();

    public newsFeatures?: ReadonlyArray<FeatureDto>;
    public newsDialog = new DialogModel();

    public generalInfo = '';

    public starters = this.templatesState.starters;

    public allAppsFilter = new UntypedFormControl();

    public showAllApps =
        combineLatest([
            this.allAppsState.containsAllApps,
            this.allAppsState.total,
            this.allAppsState.query,
            this.appsState.apps,
        ]).pipe(map(([containsAllApps, total, query, apps]) => {
            // Keep the section visible while searching, even if the search returns fewer apps.
            return containsAllApps && (!!query || total > apps.length);
        }));

    public groupedApps =
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
        }));

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
        if (uiOptions.value.showInfo) {
            this.generalInfo = uiOptions.value.info;
        }
    }

    public ngOnInit() {
        this.appsState.apps.pipe(take(1))
            .subscribe(apps => {
                if (apps.length === 0 &&
                    this.uiOptions.value.hideOnboarding !== true &&
                    this.tourState.snapshot.status !== 'Completed' &&
                    this.tourState.snapshot.status !== 'Started') {
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
                                    this.newsFeatures = result.features;
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
        this.addAppTemplate = template;
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
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { combineLatest } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { AppDto, AppsState, AuthService, DialogModel, FeatureDto, LocalStoreService, NewsService, OnboardingService, TeamDto, TeamsState, TemplateDto, TemplatesState, UIOptions, UIState } from '@app/shared';
import { Settings } from '@app/shared/state/settings';

type GroupedApps = { team?: TeamDto; apps: AppDto[] };

@Component({
    selector: 'sqx-apps-page',
    styleUrls: ['./apps-page.component.scss'],
    templateUrl: './apps-page.component.html',
})
export class AppsPageComponent implements OnInit {
    public addAppDialog = new DialogModel();
    public addAppTemplate?: TemplateDto;

    public onboardingDialog = new DialogModel();

    public newsFeatures?: ReadonlyArray<FeatureDto>;
    public newsDialog = new DialogModel();

    public info = '';

    public templates =
        this.templatesState.templates.pipe(
            map(x => x.filter(t => t.isStarter)));

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
        public readonly authState: AuthService,
        public readonly uiState: UIState,
        private readonly appsState: AppsState,
        private readonly localStore: LocalStoreService,
        private readonly newsService: NewsService,
        private readonly onboardingService: OnboardingService,
        private readonly teamsState: TeamsState,
        private readonly templatesState: TemplatesState,
        private readonly uiOptions: UIOptions,
    ) {
        if (uiOptions.get('showInfo')) {
            this.info = uiOptions.get('info');
        }
    }

    public ngOnInit() {
        const shouldShowOnboarding = this.onboardingService.shouldShow('dialog');

        this.appsState.apps.pipe(take(1))
            .subscribe(apps => {
                if (shouldShowOnboarding && apps.length === 0) {
                    this.onboardingService.disable('dialog');
                    this.onboardingDialog.show();
                } else if (!this.uiOptions.get('hideNews')) {
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

        this.templatesState.load();
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

    public trackByApp(_index: number, app: AppDto) {
        return app.id;
    }

    public trackByGroup(_index: number, group: GroupedApps) {
        return group.team?.id || '0';
    }
}

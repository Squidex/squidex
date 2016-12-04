/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import {
    AppLanguageDto,
    AppLanguagesService,
    AppsStoreService,
    LanguageDto, 
    LanguageService,
    Notification,
    NotificationService,
    TitleService 
} from 'shared';

@Ng2.Component({
    selector: 'sqx-languages-page',
    styles,
    template
})
export class LanguagesPageComponent implements Ng2.OnInit {
    private appSubscription: any | null = null;
    private appName: string;

    public allLanguages: LanguageDto[] = [];
    public appLanguages: AppLanguageDto[] = [];

    public selectedLanguage: LanguageDto | null = null;

    public get newLanguages() {
        return this.allLanguages.filter(x => !this.appLanguages.find(l => l.iso2Code === x.iso2Code));
    }

    constructor(
        private readonly titles: TitleService,
        private readonly appsStore: AppsStoreService,
        private readonly appLanguagesService: AppLanguagesService,
        private readonly languagesService: LanguageService,
        private readonly notifications: NotificationService
    ) {
    }

    public ngOnDestroy() {
        this.appSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.languagesService.getLanguages().retry(2)
            .subscribe(languages => {
                this.allLanguages = languages;
            }, error => {
                this.notifications.notify(Notification.error(error.displayMessage));
            });

        this.appSubscription =
            this.appsStore.selectedApp.subscribe(app => {
                if (app) {
                    this.appName = app.name;

                    this.titles.setTitle('{appName} | Settings | Languages', { appName: app.name });

                    this.load();
                }
            });
    }

    public load() {
        this.appLanguagesService.getLanguages(this.appName).retry(2)
            .subscribe(appLanguages => {
                this.appLanguages = appLanguages;
            }, error => {
                this.notifications.notify(Notification.error(error.displayMessage));
            });
    }

    public setMasterLanguage(selectedLanguage: AppLanguageDto) {
        for (let language of this.appLanguages) {
            language.isMasterLanguage = false;
        }

        this.appLanguagesService.makeMasterLanguage(this.appName, selectedLanguage.iso2Code)
            .subscribe(() => {
                selectedLanguage.isMasterLanguage = true;
            }, error => {
                this.notifications.notify(Notification.error(error.displayMessage));
            });
    }

    public addLanguage() {
        this.appLanguagesService.postLanguages(this.appName, this.selectedLanguage.iso2Code)
            .subscribe(appLanguage => {
                this.appLanguages.push(appLanguage);
            }, error => {
                this.notifications.notify(Notification.error(error.displayMessage));
            });

        this.selectedLanguage = null;
    }

    public removeLanguage(selectedLanguage: AppLanguageDto) {
        this.appLanguagesService.deleteLanguage(this.appName, selectedLanguage.iso2Code)
            .subscribe(appLanguage => {
                this.appLanguages.splice(this.appLanguages.indexOf(appLanguage), 1);
            }, error => {
                this.notifications.notify(Notification.error(error.displayMessage));
            });
    }
}


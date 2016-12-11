/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import {
    AddAppLanguageDto,
    AppComponentBase,
    AppLanguageDto,
    AppLanguagesService,
    AppsStoreService,
    ImmutableArray,
    LanguageDto,
    LanguageService,
    NotificationService,
    UpdateAppLanguageDto,
    UsersProviderService
} from 'shared';

@Ng2.Component({
    selector: 'sqx-languages-page',
    styles,
    template
})
export class LanguagesPageComponent extends AppComponentBase implements Ng2.OnInit {
    public allLanguages: LanguageDto[] = [];
    public appLanguages = ImmutableArray.empty<AppLanguageDto>();

    public selectedLanguage: LanguageDto | null = null;

    public get newLanguages() {
        return this.allLanguages.filter(x => !this.appLanguages.find(l => l.iso2Code === x.iso2Code));
    }

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly appLanguagesService: AppLanguagesService,
        private readonly languagesService: LanguageService
    ) {
        super(apps, notifications, users);
    }

    public ngOnInit() {
        this.languagesService.getLanguages().retry(2)
            .subscribe(languages => {
                this.allLanguages = languages;
            }, error => {
                this.notifyError(error);
            });

        this.load();
    }

    public load() {
        this.appName()
            .switchMap(app => this.appLanguagesService.getLanguages(app).retry(2))
            .subscribe(dtos => {
                this.appLanguages = ImmutableArray.of(dtos);
            }, error => {
                this.notifyError(error);
            });
    }

    public addLanguage() {
        this.appName()
            .switchMap(app => this.appLanguagesService.postLanguages(app, new AddAppLanguageDto(this.selectedLanguage.iso2Code)))
            .subscribe(dto => {
                this.appLanguages = this.appLanguages.push(dto);
            }, error => {
                this.notifyError(error);
            });

        this.selectedLanguage = null;
    }

    public removeLanguage(language: AppLanguageDto) {
        this.appName()
            .switchMap(app => this.appLanguagesService.deleteLanguage(app, language.iso2Code))
            .subscribe(dto => {
                this.appLanguages = this.appLanguages.remove(dto);
            }, error => {
                this.notifyError(error);
            });
    }

    public setMasterLanguage(language: AppLanguageDto) {
        this.appName()
            .switchMap(app => this.appLanguagesService.updateLanguage(app, language.iso2Code, new UpdateAppLanguageDto(true)))
            .subscribe(() => {
                this.appLanguages = this.appLanguages.map(l => {
                    const isMasterLanguage = l === language;

                    if (isMasterLanguage !== l.isMasterLanguage) {
                        return new AppLanguageDto(l.iso2Code, l.englishName, isMasterLanguage);
                    } else {
                        return l;
                    }
                });
            }, error => {
                this.notifyError(error);
            });
    }
}


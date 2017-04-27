/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import {
    AddAppLanguageDto,
    AppComponentBase,
    AppLanguageDto,
    AppLanguagesService,
    AppsStoreService,
    HistoryChannelUpdated,
    ImmutableArray,
    MessageBus,
    LanguageDto,
    LanguageService,
    NotificationService,
    UpdateAppLanguageDto,
    Version
} from 'shared';

@Component({
    selector: 'sqx-languages-page',
    styleUrls: ['./languages-page.component.scss'],
    templateUrl: './languages-page.component.html'
})
export class LanguagesPageComponent extends AppComponentBase implements OnInit {
    private version = new Version();

    public allLanguages: LanguageDto[] = [];
    public appLanguages = ImmutableArray.empty<AppLanguageDto>();

    public addLanguageForm: FormGroup =
        this.formBuilder.group({
            language: [null,
                Validators.required
            ]
        });

    public get newLanguages(): LanguageDto[] {
        return this.allLanguages.filter(x => !this.appLanguages.find(l => l.iso2Code === x.iso2Code));
    }

    constructor(apps: AppsStoreService, notifications: NotificationService,
        private readonly appLanguagesService: AppLanguagesService,
        private readonly languagesService: LanguageService,
        private readonly messageBus: MessageBus,
        private readonly formBuilder: FormBuilder
    ) {
        super(notifications, apps);
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
            .switchMap(app => this.appLanguagesService.getLanguages(app, this.version).retry(2))
            .subscribe(dtos => {
                this.updateLanguages(ImmutableArray.of(dtos));
            }, error => {
                this.notifyError(error);
            });
    }

    public removeLanguage(language: AppLanguageDto) {
        this.appName()
            .switchMap(app => this.appLanguagesService.deleteLanguage(app, language.iso2Code, this.version))
            .subscribe(dto => {
                this.updateLanguages(this.appLanguages.remove(language));
            }, error => {
                this.notifyError(error);
            });
    }

    public addLanguage() {
        const request = new AddAppLanguageDto(this.addLanguageForm.get('language')!.value.iso2Code);

        this.appName()
            .switchMap(app => this.appLanguagesService.postLanguages(app, request, this.version))
            .subscribe(dto => {
                this.updateLanguages(this.appLanguages.push(dto));
            }, error => {
                this.notifyError(error);
            });
    }

    public setMasterLanguage(language: AppLanguageDto) {
        const request = new UpdateAppLanguageDto(true);

        this.appName()
            .switchMap(app => this.appLanguagesService.updateLanguage(app, language.iso2Code, request, this.version))
            .subscribe(() => {
                this.updateLanguages(this.appLanguages.map(l => {
                    const isMasterLanguage = l === language;

                    if (isMasterLanguage !== l.isMasterLanguage) {
                        return new AppLanguageDto(l.iso2Code, l.englishName, isMasterLanguage);
                    } else {
                        return l;
                    }
                }));
            }, error => {
                this.notifyError(error);
            });

        return false;
    }

    private updateLanguages(languages: ImmutableArray<AppLanguageDto>) {
        this.addLanguageForm.reset();
        this.appLanguages = languages;

        this.messageBus.publish(new HistoryChannelUpdated());
    }
}


/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

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
    LanguagesService,
    NotificationService,
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
    public newLanguages: LanguageDto[] = [];
    public appLanguages = ImmutableArray.empty<AppLanguageDto>();

    public addLanguageForm =
        this.formBuilder.group({
            language: [null,
                Validators.required
            ]
        });

    constructor(apps: AppsStoreService, notifications: NotificationService,
        private readonly appLanguagesService: AppLanguagesService,
        private readonly languagesService: LanguagesService,
        private readonly messageBus: MessageBus,
        private readonly formBuilder: FormBuilder
    ) {
        super(notifications, apps);
    }

    public ngOnInit() {
        this.loadAllLanguages();
        this.load();
    }

    public load() {
        this.appNameOnce()
            .switchMap(app => this.appLanguagesService.getLanguages(app, this.version).retry(2))
            .subscribe(dtos => {
                this.updateLanguages(ImmutableArray.of(dtos));
            }, error => {
                this.notifyError(error);
            });
    }

    public removeLanguage(language: AppLanguageDto) {
        this.appNameOnce()
            .switchMap(app => this.appLanguagesService.deleteLanguage(app, language.iso2Code, this.version))
            .subscribe(dto => {
                this.updateLanguages(this.appLanguages.remove(language));
            }, error => {
                this.notifyError(error);
            });
    }

    public addLanguage() {
        const requestDto = new AddAppLanguageDto(this.addLanguageForm.controls['language'].value.iso2Code);

        this.appNameOnce()
            .switchMap(app => this.appLanguagesService.postLanguages(app, requestDto, this.version))
            .subscribe(dto => {
                this.updateLanguages(this.appLanguages.push(dto));
            }, error => {
                this.notifyError(error);
            });
    }

    public updateLanguage(language: AppLanguageDto) {
        this.appNameOnce()
            .switchMap(app => this.appLanguagesService.updateLanguage(app, language.iso2Code, language, this.version))
            .subscribe(dto => {
                this.updateLanguages(
                    this.appLanguages.replaceAll(
                        l => l.iso2Code === language.iso2Code,
                        l => language),
                    language.isMaster ? language.iso2Code : undefined);
            }, error => {
                this.notifyError(error);
            });
    }

    private loadAllLanguages() {
        this.languagesService.getLanguages().retry(2)
            .subscribe(languages => {
                this.allLanguages = languages;

                this.updateNewLanguages();
            }, error => {
                this.notifyError(error);
            });
    }

    private updateLanguages(languages: ImmutableArray<AppLanguageDto>, masterId?: string) {
        this.addLanguageForm.reset();

        this.appLanguages =
            languages.map(l => {
                return new AppLanguageDto(
                    l.iso2Code,
                    l.englishName,
                    masterId ? l.iso2Code === masterId : l.isMaster,
                    l.isOptional,
                    l.fallback.filter(f => !!languages.find(l2 => l2.iso2Code === f))
                );
            }).sort((a, b) => {
                if (a.isMaster === b.isMaster) {
                    return a.iso2Code.localeCompare(b.iso2Code);
                } else {
                    return (a.isMaster ? 0 : 1) - (b.isMaster ? 0 : 1);
                }
            });

        this.updateNewLanguages();

        this.messageBus.emit(new HistoryChannelUpdated());
    }

    private updateNewLanguages() {
        this.newLanguages = this.allLanguages.filter(x => !this.appLanguages.find(l => l.iso2Code === x.iso2Code));
    }
}


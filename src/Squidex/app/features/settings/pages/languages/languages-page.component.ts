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
    AppLanguagesDto,
    AppLanguagesService,
    AppsStoreService,
    AuthService,
    DialogService,
    HistoryChannelUpdated,
    ImmutableArray,
    MessageBus,
    LanguageDto,
    LanguagesService
} from 'shared';

@Component({
    selector: 'sqx-languages-page',
    styleUrls: ['./languages-page.component.scss'],
    templateUrl: './languages-page.component.html'
})
export class LanguagesPageComponent extends AppComponentBase implements OnInit {
    public allLanguages: LanguageDto[] = [];
    public newLanguages: LanguageDto[] = [];
    public appLanguages: AppLanguagesDto;

    public addLanguageForm =
        this.formBuilder.group({
            language: [null,
                Validators.required
            ]
        });

    constructor(apps: AppsStoreService, dialogs: DialogService, authService: AuthService,
        private readonly appLanguagesService: AppLanguagesService,
        private readonly languagesService: LanguagesService,
        private readonly messageBus: MessageBus,
        private readonly formBuilder: FormBuilder
    ) {
        super(dialogs, apps, authService);
    }

    public ngOnInit() {
        this.loadAllLanguages();
        this.load();
    }

    public load() {
        this.appNameOnce()
            .switchMap(app => this.appLanguagesService.getLanguages(app).retry(2))
            .subscribe(dto => {
                this.updateLanguages(dto);
            }, error => {
                this.notifyError(error);
            });
    }

    public removeLanguage(language: AppLanguageDto) {
        this.appNameOnce()
            .switchMap(app => this.appLanguagesService.deleteLanguage(app, language.iso2Code, this.appLanguages.version))
            .subscribe(dto => {
                this.updateLanguages(this.appLanguages.removeLanguage(language, dto.version));
            }, error => {
                this.notifyError(error);
            });
    }

    public addLanguage() {
        const requestDto = new AddAppLanguageDto(this.addLanguageForm.controls['language'].value.iso2Code);

        this.appNameOnce()
            .switchMap(app => this.appLanguagesService.postLanguages(app, requestDto, this.appLanguages.version))
            .subscribe(dto => {
                this.updateLanguages(this.appLanguages.addLanguage(dto.payload, dto.version));
            }, error => {
                this.notifyError(error);
            });
    }

    public updateLanguage(language: AppLanguageDto) {
        this.appNameOnce()
            .switchMap(app => this.appLanguagesService.updateLanguage(app, language.iso2Code, language, this.appLanguages.version))
            .subscribe(dto => {
                this.updateLanguages(this.appLanguages.updateLanguage(language, dto.version));
            }, error => {
                this.notifyError(error);
            });
    }

    private loadAllLanguages() {
        this.languagesService.getLanguages().retry(2)
            .subscribe(languages => {
                this.allLanguages = ImmutableArray.of(languages).sortByStringAsc(l => l.englishName).values;

                this.updateNewLanguages();
            }, error => {
                this.notifyError(error);
            });
    }

    private updateLanguages(appLanguages: AppLanguagesDto, masterId?: string) {
        this.addLanguageForm.reset();

        this.appLanguages =
            new AppLanguagesDto(
                appLanguages.languages.map(l => {
                    const isMaster = masterId ? l.iso2Code === masterId : l.isMaster;

                    return new AppLanguageDto(
                        l.iso2Code,
                        l.englishName, isMaster,
                        l.isOptional,
                        l.fallback.filter(f => !!appLanguages.languages.find(l2 => l2.iso2Code === f))
                    );
                }).sort((a, b) => {
                    if (a.isMaster === b.isMaster) {
                        return a.iso2Code.localeCompare(b.iso2Code);
                    } else {
                        return (a.isMaster ? 0 : 1) - (b.isMaster ? 0 : 1);
                    }
                }), appLanguages.version);

        this.updateNewLanguages();

        this.messageBus.emit(new HistoryChannelUpdated());
    }

    private updateNewLanguages() {
        if (this.appLanguages) {
            this.newLanguages = this.allLanguages.filter(x => !this.appLanguages.languages.find(l => l.iso2Code === x.iso2Code));
            this.addLanguageForm.controls['language'].setValue(this.newLanguages[0]);
        }
    }
}


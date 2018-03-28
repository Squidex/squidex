/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import {
    AddAppLanguageDto,
    AppLanguageDto,
    AppLanguagesDto,
    AppLanguagesService,
    AppsState,
    DialogService,
    ImmutableArray,
    LanguageDto,
    LanguagesService
} from '@app/shared';

@Component({
    selector: 'sqx-languages-page',
    styleUrls: ['./languages-page.component.scss'],
    templateUrl: './languages-page.component.html'
})
export class LanguagesPageComponent implements OnInit {
    public allLanguages: LanguageDto[] = [];
    public newLanguages: LanguageDto[] = [];
    public appLanguages: AppLanguagesDto;

    public addLanguageForm =
        this.formBuilder.group({
            language: [null,
                [
                    Validators.required
                ]
            ]
        });

    constructor(
        public readonly appsState: AppsState,
        private readonly appLanguagesService: AppLanguagesService,
        private readonly dialogs: DialogService,
        private readonly formBuilder: FormBuilder,
        private readonly languagesService: LanguagesService
    ) {
    }

    public ngOnInit() {
        this.loadAllLanguages();
        this.load();
    }

    public load() {
        this.appLanguagesService.getLanguages(this.appsState.appName)
            .subscribe(dto => {
                this.updateLanguages(dto);
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    public removeLanguage(language: AppLanguageDto) {
        this.appLanguagesService.deleteLanguage(this.appsState.appName, language.iso2Code, this.appLanguages.version)
            .subscribe(dto => {
                this.updateLanguages(this.appLanguages.removeLanguage(language, dto.version));
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    public addLanguage() {
        const requestDto = new AddAppLanguageDto(this.addLanguageForm.controls['language'].value.iso2Code);

        this.appLanguagesService.postLanguages(this.appsState.appName, requestDto, this.appLanguages.version)
            .subscribe(dto => {
                this.updateLanguages(this.appLanguages.addLanguage(dto.payload, dto.version));
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    public updateLanguage(language: AppLanguageDto) {
        this.appLanguagesService.putLanguage(this.appsState.appName, language.iso2Code, language, this.appLanguages.version)
            .subscribe(dto => {
                this.updateLanguages(this.appLanguages.updateLanguage(language, dto.version));
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    private loadAllLanguages() {
        this.languagesService.getLanguages()
            .subscribe(languages => {
                this.allLanguages = ImmutableArray.of(languages).sortByStringAsc(l => l.englishName).values;

                this.updateNewLanguages();
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    private updateLanguages(appLanguages: AppLanguagesDto, masterId?: string) {
        this.addLanguageForm.reset();

        this.appLanguages =
            new AppLanguagesDto(
                appLanguages.languages.sort((a, b) => {
                    if (a.isMaster === b.isMaster) {
                        return a.iso2Code.localeCompare(b.iso2Code);
                    } else {
                        return (a.isMaster ? 0 : 1) - (b.isMaster ? 0 : 1);
                    }
                }), appLanguages.version);

        this.updateNewLanguages();
    }

    private updateNewLanguages() {
        if (this.appLanguages) {
            this.newLanguages = this.allLanguages.filter(x => !this.appLanguages.languages.find(l => l.iso2Code === x.iso2Code));
            this.addLanguageForm.controls['language'].setValue(this.newLanguages[0]);
        }
    }
}


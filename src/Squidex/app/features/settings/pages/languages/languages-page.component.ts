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
    AppContext,
    AppLanguageDto,
    AppLanguagesDto,
    AppLanguagesService,
    HistoryChannelUpdated,
    ImmutableArray,
    LanguageDto,
    LanguagesService
} from 'shared';

@Component({
    selector: 'sqx-languages-page',
    styleUrls: ['./languages-page.component.scss'],
    templateUrl: './languages-page.component.html',
    providers: [
        AppContext
    ]
})
export class LanguagesPageComponent implements OnInit {
    public allLanguages: LanguageDto[] = [];
    public newLanguages: LanguageDto[] = [];
    public appLanguages: AppLanguagesDto;

    public addLanguageForm =
        this.formBuilder.group({
            language: [null,
                Validators.required
            ]
        });

    constructor(public readonly ctx: AppContext,
        private readonly appLanguagesService: AppLanguagesService,
        private readonly languagesService: LanguagesService,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.loadAllLanguages();
        this.load();
    }

    public load() {
        this.appLanguagesService.getLanguages(this.ctx.appName)
            .subscribe(dto => {
                this.updateLanguages(dto);
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public removeLanguage(language: AppLanguageDto) {
        this.appLanguagesService.deleteLanguage(this.ctx.appName, language.iso2Code, this.appLanguages.version)
            .subscribe(dto => {
                this.updateLanguages(this.appLanguages.removeLanguage(language, dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public addLanguage() {
        const requestDto = new AddAppLanguageDto(this.addLanguageForm.controls['language'].value.iso2Code);

        this.appLanguagesService.postLanguages(this.ctx.appName, requestDto, this.appLanguages.version)
            .subscribe(dto => {
                this.updateLanguages(this.appLanguages.addLanguage(dto.payload, dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public updateLanguage(language: AppLanguageDto) {
        this.appLanguagesService.putLanguage(this.ctx.appName, language.iso2Code, language, this.appLanguages.version)
            .subscribe(dto => {
                this.updateLanguages(this.appLanguages.updateLanguage(language, dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    private loadAllLanguages() {
        this.languagesService.getLanguages()
            .subscribe(languages => {
                this.allLanguages = ImmutableArray.of(languages).sortByStringAsc(l => l.englishName).values;

                this.updateNewLanguages();
            }, error => {
                this.ctx.notifyError(error);
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

        this.ctx.bus.emit(new HistoryChannelUpdated());
    }

    private updateNewLanguages() {
        if (this.appLanguages) {
            this.newLanguages = this.allLanguages.filter(x => !this.appLanguages.languages.find(l => l.iso2Code === x.iso2Code));
            this.addLanguageForm.controls['language'].setValue(this.newLanguages[0]);
        }
    }
}


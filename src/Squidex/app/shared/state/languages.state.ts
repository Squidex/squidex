/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable } from 'rxjs';

import '@app/framework/utils/rxjs-extensions';

import {
    DialogService,
    Form,
    ImmutableArray,
    State,
    Version
} from '@app/framework';

import { AddAppLanguageDto, AppLanguageDto, AppLanguagesService, UpdateAppLanguageDto } from './../services/app-languages.service';
import { LanguageDto, LanguagesService } from './../services/languages.service';
import { AppsState } from './apps.state';


export class EditLanguageForm extends Form<FormGroup> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            isMaster: false,
            isOptional: false
        }));

        this.form.controls['isMaster'].valueChanges
            .subscribe(value => {
                if (value) {
                    this.form.controls['isOptional'].setValue(false);
                }
            });

        this.form.controls['isOptional'].valueChanges
            .subscribe(value => {
                if (value) {
                    this.form.controls['isMaster'].setValue(false);
                }
            });
    }
}

export class AddLanguageForm extends Form<FormGroup> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            language: [null,
                [
                    Validators.required
                ]
            ]
        }));
    }
}

interface SnapshotLanguage {
    language: AppLanguageDto;

    fallbackLanguages: ImmutableArray<LanguageDto>;
    fallbackLanguagesNew: ImmutableArray<LanguageDto>;
}

interface Snapshot {
    plainLanguages: ImmutableArray<AppLanguageDto>;

    allLanguages: ImmutableArray<LanguageDto>;
    allLanguagesNew: ImmutableArray<LanguageDto>;

    languages: ImmutableArray<SnapshotLanguage>;

    isLoaded?: boolean;

    version: Version;
}

@Injectable()
export class LanguagesState extends State<Snapshot> {
    public languages =
        this.changes.map(x => x.languages)
            .distinctUntilChanged();

    public newLanguages =
        this.changes.map(x => x.allLanguagesNew)
            .distinctUntilChanged();

    public isLoaded =
        this.changes.map(x => !!x.isLoaded)
            .distinctUntilChanged();

    constructor(
        private readonly appLanguagesService: AppLanguagesService,
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly languagesService: LanguagesService
    ) {
        super({
            plainLanguages: ImmutableArray.empty(),
            allLanguages: ImmutableArray.empty(),
            allLanguagesNew: ImmutableArray.empty(),
            languages: ImmutableArray.empty(),
            version: new Version('')
        });
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return Observable.forkJoin(
                this.languagesService.getLanguages(),
                this.appLanguagesService.getLanguages(this.appName),
                (allLanguages, languages) => ({ allLanguages, languages })
            )
            .do(dtos => {
                if (isReload) {
                    this.dialogs.notifyInfo('Languages reloaded.');
                }

                const sorted = ImmutableArray.of(dtos.allLanguages).sortByStringAsc(x => x.englishName);

                this.replaceLanguages(ImmutableArray.of(dtos.languages.languages), dtos.languages.version, sorted);
            })
            .notify(this.dialogs);
    }

    public add(language: LanguageDto): Observable<any> {
        return this.appLanguagesService.postLanguage(this.appName, new AddAppLanguageDto(language.iso2Code), this.version)
            .do(dto => {
                const languages = this.snapshot.plainLanguages.push(dto.payload).sortByStringAsc(x => x.englishName);

                this.replaceLanguages(languages, dto.version);
            })
            .notify(this.dialogs);
    }

    public remove(language: AppLanguageDto): Observable<any> {
        return this.appLanguagesService.deleteLanguage(this.appName, language.iso2Code, this.version)
            .do(dto => {
                const languages = this.snapshot.plainLanguages.filter(x => x.iso2Code !== language.iso2Code);

                this.replaceLanguages(languages, dto.version);
            })
            .notify(this.dialogs);
    }

    public update(language: AppLanguageDto, request: UpdateAppLanguageDto): Observable<any> {
        return this.appLanguagesService.putLanguage(this.appName, language.iso2Code, request, this.version)
            .do(dto => {
                const languages = this.snapshot.plainLanguages.map(l => {
                    if (l.iso2Code === language.iso2Code) {
                        return update(l, request.isMaster, request.isOptional, request.fallback);
                    } else if (l.isMaster && request.isMaster) {
                        return  update(l, false, l.isOptional, l.fallback);
                    } else {
                        return l;
                    }
                });

                this.replaceLanguages(languages, dto.version);
            })
            .notify(this.dialogs);
    }

    private replaceLanguages(languages: ImmutableArray<AppLanguageDto>, version: Version, allLanguages?: ImmutableArray<LanguageDto>) {
        this.next(s => {
            allLanguages = allLanguages || s.allLanguages;

            return {
                ...s,
                languages: languages.sort((a, b) => {
                    if (a.isMaster === b.isMaster) {
                        return a.englishName.localeCompare(b.englishName);
                    } else {
                        return (a.isMaster ? 0 : 1) - (b.isMaster ? 0 : 1);
                    }
                }).map(x => this.createLanguage(x, languages)),
                plainLanguages: languages,
                allLanguages: allLanguages,
                allLanguagesNew: allLanguages.filter(x => !languages.find(l => l.iso2Code === x.iso2Code)),
                isLoaded: true,
                version: version
            };
        });
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get version() {
        return this.snapshot.version;
    }

    private createLanguage(language: AppLanguageDto, languages: ImmutableArray<AppLanguageDto>): SnapshotLanguage {
        return {
            language,
            fallbackLanguages:
                ImmutableArray.of(
                    language.fallback
                        .map(l => languages.find(x => x.iso2Code === l)).filter(x => !!x)
                        .map(x => <AppLanguageDto>x)),
            fallbackLanguagesNew:
                languages
                    .filter(l =>
                        language.iso2Code !== l.iso2Code &&
                        language.fallback.indexOf(l.iso2Code) < 0)
                    .sortByStringAsc(x => x.englishName)
        };
    }
}



const update = (language: AppLanguageDto, isMaster: boolean, isOptional: boolean, fallback: string[]) =>
    new AppLanguageDto(
        language.iso2Code,
        language.englishName,
        isMaster,
        isOptional,
        fallback);
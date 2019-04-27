/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { forkJoin, Observable } from 'rxjs';
import { distinctUntilChanged, map, share, tap } from 'rxjs/operators';

import {
    DialogService,
    ImmutableArray,
    notify,
    State,
    Version
} from '@app/framework';

import {
    AppLanguageDto,
    AppLanguagesService,
    UpdateAppLanguageDto
} from './../services/app-languages.service';

import { LanguageDto, LanguagesService } from './../services/languages.service';
import { AppsState } from './apps.state';

interface SnapshotLanguage {
    // The language.
    language: AppLanguageDto;

    // All configured fallback languages.
    fallbackLanguages: LanguageList;

    // The fallback languages that have not been added yet.
    fallbackLanguagesNew: LanguageList;
}

interface Snapshot {
    // the configured languages as plan format.
    plainLanguages: AppLanguagesList;

    // All supported languages.
    allLanguages: LanguageList;

    // The languages that have not been added yet.
    allLanguagesNew: LanguageList;

    // The configured languages with extra information.
    languages: LanguageResultList;

    // The app version.
    version: Version;

    // Indicates if the languages are loaded.
    isLoaded?: boolean;
}

type AppLanguagesList = ImmutableArray<AppLanguageDto>;
type LanguageList = ImmutableArray<LanguageDto>;
type LanguageResultList = ImmutableArray<SnapshotLanguage>;

@Injectable()
export class LanguagesState extends State<Snapshot> {
    public languages =
        this.changes.pipe(map(x => x.languages),
            distinctUntilChanged());

    public newLanguages =
        this.changes.pipe(map(x => x.allLanguagesNew),
            distinctUntilChanged());

    public isLoaded =
        this.changes.pipe(map(x => !!x.isLoaded),
            distinctUntilChanged());

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

        const http$ =
            forkJoin(
                this.languagesService.getLanguages(),
                this.appLanguagesService.getLanguages(this.appName)).pipe(
                map(args => {
                    return { allLanguages: args[0], languages: args[1] };
                }),
                share());

        http$.subscribe(response => {
            if (isReload) {
                this.dialogs.notifyInfo('Languages reloaded.');
            }

            const sorted = ImmutableArray.of(response.allLanguages).sortByStringAsc(x => x.englishName);

            this.replaceLanguages(ImmutableArray.of(response.languages.languages), response.languages.version, sorted);

        }, error => {
            this.dialogs.notifyError(error);
        });

        return http$;
    }

    public add(language: LanguageDto): Observable<any> {
        return this.appLanguagesService.postLanguage(this.appName, { language: language.iso2Code }, this.version).pipe(
            tap(dto => {
                const languages = this.snapshot.plainLanguages.push(dto.payload).sortByStringAsc(x => x.englishName);

                this.replaceLanguages(languages, dto.version);
            }),
            notify(this.dialogs));
    }

    public remove(language: AppLanguageDto): Observable<any> {
        return this.appLanguagesService.deleteLanguage(this.appName, language.iso2Code, this.version).pipe(
            tap(dto => {
                const languages = this.snapshot.plainLanguages.filter(x => x.iso2Code !== language.iso2Code);

                this.replaceLanguages(languages, dto.version);
            }),
            notify(this.dialogs));
    }

    public update(language: AppLanguageDto, request: UpdateAppLanguageDto): Observable<any> {
        return this.appLanguagesService.putLanguage(this.appName, language.iso2Code, request, this.version).pipe(
            tap(dto => {
                const languages = this.snapshot.plainLanguages.map(x => {
                    if (x.iso2Code === language.iso2Code) {
                        return update(x, request);
                    } else if (x.isMaster && request.isMaster) {
                        return update(x, { isMaster: false });
                    } else {
                        return x;
                    }
                });

                this.replaceLanguages(languages, dto.version);
            }),
            notify(this.dialogs));
    }

    private replaceLanguages(languages: AppLanguagesList, version: Version, allLanguages?: LanguageList) {
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

    private createLanguage(language: AppLanguageDto, languages: AppLanguagesList): SnapshotLanguage {
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

const update = (language: AppLanguageDto, request: UpdateAppLanguageDto) =>
    language.with(request);
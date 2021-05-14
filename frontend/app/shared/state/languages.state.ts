/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { DialogService, shareMapSubscribed, shareSubscribed, State, Version } from '@app/framework';
import { forkJoin, Observable } from 'rxjs';
import { finalize, map, shareReplay, tap } from 'rxjs/operators';
import { AppLanguageDto, AppLanguagesPayload, AppLanguagesService, UpdateAppLanguageDto } from './../services/app-languages.service';
import { LanguageDto, LanguagesService } from './../services/languages.service';
import { AppsState } from './apps.state';

export interface SnapshotLanguage {
    // The language.
    language: AppLanguageDto;

    // All configured fallback languages.
    fallbackLanguages: LanguageList;

    // The fallback languages that have not been added yet.
    fallbackLanguagesNew: LanguageList;
}

interface Snapshot {
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

    // Indicates if the languages are loading.
    isLoading?: boolean;

    // Inedicates if the user can add a language.
    canCreate?: boolean;
}

type AppLanguagesList = ReadonlyArray<AppLanguageDto>;
type LanguageList = ReadonlyArray<LanguageDto>;
type LanguageResultList = ReadonlyArray<SnapshotLanguage>;

@Injectable()
export class LanguagesState extends State<Snapshot> {
    private cachedLanguage$: Observable<ReadonlyArray<LanguageDto>>;

    public languages =
        this.project(x => x.languages);

    public isoLanguages =
        this.project(x => x.languages.map(y => y.language));

    public isoMasterLanguage =
        this.projectFrom(this.isoLanguages, x => x.find(l => l.isMaster)!);

    public newLanguages =
        this.project(x => x.allLanguagesNew);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public canCreate =
        this.project(x => x.canCreate === true);

    constructor(
        private readonly appLanguagesService: AppLanguagesService,
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly languagesService: LanguagesService,
    ) {
        super({
            allLanguages: [],
            allLanguagesNew: [],
            languages: [],
            version: Version.EMPTY,
        }, 'Languages');
    }

    public load(isReload = false): Observable<any> {
        if (isReload) {
            this.resetState('Loading Success');
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload: boolean): Observable<any> {
        this.next({ isLoading: true }, 'Loading Started');

        return forkJoin([
                this.getAllLanguages(),
                this.getAppLanguages()]).pipe(
            map(args => {
                return { allLanguages: args[0], languages: args[1] };
            }),
            tap(({ allLanguages, languages }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:languages.reloaded');
                }

                const sorted = allLanguages.sortedByString(x => x.englishName);

                this.replaceLanguages(languages.payload, languages.version, sorted);
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
            }),
            shareSubscribed(this.dialogs));
    }

    public add(language: LanguageDto): Observable<any> {
        return this.appLanguagesService.postLanguage(this.appName, { language: language.iso2Code }, this.version).pipe(
            tap(({ version, payload }) => {
                this.replaceLanguages(payload, version);
            }),
            shareMapSubscribed(this.dialogs, x => x.payload));
    }

    public remove(language: AppLanguageDto): Observable<any> {
        return this.appLanguagesService.deleteLanguage(this.appName, language, this.version).pipe(
            tap(({ version, payload }) => {
                this.replaceLanguages(payload, version);
            }),
            shareSubscribed(this.dialogs));
    }

    public update(language: AppLanguageDto, request: UpdateAppLanguageDto): Observable<any> {
        return this.appLanguagesService.putLanguage(this.appName, language, request, this.version).pipe(
            tap(({ version, payload }) => {
                this.replaceLanguages(payload, version);
            }),
            shareMapSubscribed(this.dialogs, x => x.payload));
    }

    private replaceLanguages(payload: AppLanguagesPayload, version: Version, allLanguages?: LanguageList) {
        this.next(s => {
            allLanguages = allLanguages || s.allLanguages;

            const { canCreate, items: languages } = payload;

            return {
                ...s,
                allLanguages,
                allLanguagesNew: allLanguages.filter(x => !languages.find(l => l.iso2Code === x.iso2Code)),
                canCreate,
                isLoaded: true,
                isLoading: false,
                languages: languages.map(x => this.createLanguage(x, languages)),
                version,
            };
        }, 'Loading Success / Updated');
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get version() {
        return this.snapshot.version;
    }

    private getAppLanguages() {
        return this.appLanguagesService.getLanguages(this.appName);
    }

    private getAllLanguages() {
        if (!this.cachedLanguage$) {
            this.cachedLanguage$ =
                this.languagesService.getLanguages().pipe(
                    shareReplay(1));
        }

        return this.cachedLanguage$;
    }

    private createLanguage(language: AppLanguageDto, languages: AppLanguagesList): SnapshotLanguage {
        return {
            language,
            fallbackLanguages:
                language.fallback
                    .map(l => languages.find(x => x.iso2Code === l)).filter(x => !!x)
                    .map(l => l!),
            fallbackLanguagesNew:
                languages
                    .filter(l =>
                        language.iso2Code !== l.iso2Code &&
                        language.fallback.indexOf(l.iso2Code) < 0)
                    .sortByString(x => x.englishName),
        };
    }
}

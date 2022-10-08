/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { forkJoin, Observable } from 'rxjs';
import { finalize, map, shareReplay, tap } from 'rxjs/operators';
import { DialogService, LoadingState, shareMapSubscribed, shareSubscribed, State, Version } from '@app/framework';
import { AppLanguageDto, AppLanguagesPayload, AppLanguagesService, UpdateAppLanguageDto } from './../services/app-languages.service';
import { LanguageDto, LanguagesService } from './../services/languages.service';
import { AppsState } from './apps.state';

export interface SnapshotLanguage {
    // The language.
    language: AppLanguageDto;

    // All configured fallback languages.
    fallbackLanguages: ReadonlyArray<LanguageDto>;

    // The fallback languages that have not been added yet.
    fallbackLanguagesNew: ReadonlyArray<LanguageDto>;
}

interface Snapshot extends LoadingState {
    // All supported languages.
    allLanguages: ReadonlyArray<LanguageDto>;

    // The languages that have not been added yet.
    allLanguagesNew: ReadonlyArray<LanguageDto>;

    // The configured languages with extra information.
    languages: ReadonlyArray<SnapshotLanguage>;

    // The app version.
    version: Version;

    // Inedicates if the user can add a language.
    canCreate?: boolean;
}

@Injectable()
export class LanguagesState extends State<Snapshot> {
    private cachedLanguage$?: Observable<ReadonlyArray<LanguageDto>>;

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

    public get appId() {
        return this.appsState.appId;
    }

    public get appName() {
        return this.appsState.appName;
    }

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

    public add(language: string): Observable<any> {
        return this.appLanguagesService.postLanguage(this.appName, { language }, this.version).pipe(
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

    private replaceLanguages(payload: AppLanguagesPayload, version: Version, allLanguages?: ReadonlyArray<LanguageDto>) {
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

    private createLanguage(language: AppLanguageDto, languages: ReadonlyArray<LanguageDto>): SnapshotLanguage {
        return {
            language,
            fallbackLanguages:
                language.fallback
                    .map(l => languages.find(x => x.iso2Code === l)).defined(),
            fallbackLanguagesNew:
                languages
                    .filter(l => language.iso2Code !== l.iso2Code && !language.fallback.includes(l.iso2Code)).sortByString(x => x.englishName),
        };
    }
}

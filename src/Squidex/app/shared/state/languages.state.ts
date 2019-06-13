/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { forkJoin, Observable } from 'rxjs';
import { distinctUntilChanged, map, tap } from 'rxjs/operators';

import {
    DialogService,
    ImmutableArray,
    ResourceLinks,
    shareMapSubscribed,
    shareSubscribed,
    State,
    Version
} from '@app/framework';

import {
    AppLanguageDto,
    AppLanguagesPayload,
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

    // The links.
    links: ResourceLinks;
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

    public links =
        this.changes.pipe(map(x => x.links),
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
            version: new Version(''),
            links: {}
        });
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return forkJoin(
                this.languagesService.getLanguages(),
                this.appLanguagesService.getLanguages(this.appName)).pipe(
            map(args => {
                return { allLanguages: args[0], languages: args[1] };
            }),
            tap(({ allLanguages, languages }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('Languages reloaded.');
                }

                const sorted = ImmutableArray.of(allLanguages).sortByStringAsc(x => x.englishName);

                this.replaceLanguages(languages.payload, languages.version, sorted);
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

            const languages = ImmutableArray.of(payload.items);

            return {
                ...s,
                languages: languages.map(x => this.createLanguage(x, languages)),
                plainLanguages: payload,
                allLanguages: allLanguages,
                allLanguagesNew: allLanguages.filter(x => !languages.find(l => l.iso2Code === x.iso2Code)),
                isLoaded: true,
                links: payload._links,
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
                        .map(l => l!)),
            fallbackLanguagesNew:
                languages
                    .filter(l =>
                        language.iso2Code !== l.iso2Code &&
                        language.fallback.indexOf(l.iso2Code) < 0)
                    .sortByStringAsc(x => x.englishName)
        };
    }
}
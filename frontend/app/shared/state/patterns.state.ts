/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { DialogService, shareMapSubscribed, shareSubscribed, State, Version } from '@app/framework';
import { Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { EditPatternDto, PatternDto, PatternsPayload, PatternsService } from './../services/patterns.service';
import { AppsState } from './apps.state';

interface Snapshot {
    // The current patterns.
    patterns: PatternsList;

    // The app version.
    version: Version;

    // Indicates if the patterns are loaded.
    isLoaded?: boolean;

    // Indicates if the patterns are loading.
    isLoading?: boolean;

    // Indicates if patterns can be created.
    canCreate?: boolean;
}

type PatternsList = ReadonlyArray<PatternDto>;

@Injectable()
export class PatternsState extends State<Snapshot> {
    public patterns =
        this.project(x => x.patterns);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public canCreate =
        this.project(x => x.canCreate === true);

    constructor(
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly patternsService: PatternsService
    ) {
        super({ patterns: [], version: Version.EMPTY });
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload: boolean): Observable<any> {
        this.next({ isLoading: true });

        return this.patternsService.getPatterns(this.appName).pipe(
            tap(({ version, payload }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:patterns.reloaded');
                }

                this.replacePatterns(payload, version);
            }),
            finalize(() => {
                this.next({ isLoading: false });
            }),
            shareMapSubscribed(this.dialogs, x => x.payload));
    }

    public create(request: EditPatternDto): Observable<any> {
        return this.patternsService.postPattern(this.appName, request, this.version).pipe(
            tap(({ version, payload }) => {
                this.replacePatterns(payload, version);
            }),
            shareSubscribed(this.dialogs));
    }

    public update(pattern: PatternDto, request: EditPatternDto): Observable<any> {
        return this.patternsService.putPattern(this.appName, pattern, request, this.version).pipe(
            tap(({ version, payload }) => {
                this.replacePatterns(payload, version);
            }),
            shareSubscribed(this.dialogs));
    }

    public delete(pattern: PatternDto): Observable<any> {
        return this.patternsService.deletePattern(this.appName, pattern, this.version).pipe(
            tap(({ version, payload }) => {
                this.replacePatterns(payload, version);
            }),
            shareSubscribed(this.dialogs));
    }

    private replacePatterns(payload: PatternsPayload, version: Version) {
        const { canCreate, items: patterns } = payload;

        this.next({
            isLoaded: true,
            isLoading: false,
            patterns,
            version, canCreate
        });
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get version() {
        return this.snapshot.version;
    }
}
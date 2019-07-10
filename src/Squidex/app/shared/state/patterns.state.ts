/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

import {
    DialogService,
    ImmutableArray,
    shareMapSubscribed,
    shareSubscribed,
    State,
    Version
} from '@app/framework';

import { AppsState } from './apps.state';

import {
    EditPatternDto,
    PatternDto,
    PatternsPayload,
    PatternsService
} from './../services/patterns.service';

interface Snapshot {
    // The current patterns.
    patterns: PatternsList;

    // The app version.
    version: Version;

    // Indicates if the patterns are loaded.
    isLoaded?: boolean;

    // Indicates if patterns can be created.
    canCreate?: boolean;
}

type PatternsList = ImmutableArray<PatternDto>;

@Injectable()
export class PatternsState extends State<Snapshot> {
    public patterns =
        this.project(x => x.patterns);

    public isLoaded =
        this.project(x => !!x.isLoaded);

    public canCreate =
        this.project(x => !!x.canCreate);

    constructor(
        private readonly patternsService: PatternsService,
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService
    ) {
        super({ patterns: ImmutableArray.empty(), version: Version.EMPTY });
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return this.patternsService.getPatterns(this.appName).pipe(
            tap(({ version, payload }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('Patterns reloaded.');
                }

                this.replacePatterns(payload, version);
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
        const patterns = ImmutableArray.of(payload.items);

        const { canCreate } = payload;

        this.next(s => {
            return { ...s, patterns, isLoaded: true, version, canCreate };
        });
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get version() {
        return this.snapshot.version;
    }
}
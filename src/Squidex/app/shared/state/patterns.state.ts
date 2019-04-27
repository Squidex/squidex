/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { distinctUntilChanged, map, share } from 'rxjs/operators';

import {
    DialogService,
    ImmutableArray,
    State,
    Version
} from '@app/framework';

import { AppsState } from './apps.state';

import {
    EditPatternDto,
    PatternDto,
    PatternsService
} from './../services/patterns.service';

interface Snapshot {
    // The current patterns.
    patterns: PatternsList;

    // The app version.
    version: Version;

    // Indicates if the patterns are loaded.
    isLoaded?: boolean;
}

type PatternsList = ImmutableArray<PatternDto>;

@Injectable()
export class PatternsState extends State<Snapshot> {
    public patterns =
        this.changes.pipe(map(x => x.patterns),
            distinctUntilChanged());

    public isLoaded =
        this.changes.pipe(map(x => !!x.isLoaded),
            distinctUntilChanged());

    constructor(
        private readonly patternsService: PatternsService,
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService
    ) {
        super({ patterns: ImmutableArray.empty(), version: new Version('') });
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        const update$ =
            this.patternsService.getPatterns(this.appName).pipe(
                share());

        update$.subscribe(({ version, patterns: items }) => {
            if (isReload) {
                this.dialogs.notifyInfo('Patterns reloaded.');
            }

            this.next(s => {
                const patterns = ImmutableArray.of(items).sortByStringAsc(x => x.name);

                return { ...s, patterns, isLoaded: true, version: version };
            });
        }, error => {
            this.dialogs.notifyError(error);
        });

        return update$;
    }

    public create(request: EditPatternDto): Observable<PatternDto> {
        const update$ =
            this.patternsService.postPattern(this.appName, request, this.version).pipe(
                share());

        update$.subscribe(({ version, payload: pattern }) => {
            this.next(s => {
                const patterns = s.patterns.push(pattern).sortByStringAsc(x => x.name);

                return { ...s, patterns, version: version };
            });
        }, error => {
            this.dialogs.notifyError(error);
        });

        return update$.pipe(map(x => x.payload));
    }

    public update(pattern: PatternDto, request: EditPatternDto): Observable<PatternDto> {
        const update$ =
            this.patternsService.putPattern(this.appName, pattern.id, request, this.version).pipe(
                map(({ version }) => ({ version, payload: update(pattern, request) })), share());

        update$.subscribe(({ version, payload }) => {
            this.next(s => {
                const patterns = s.patterns.replaceBy('id', payload).sortByStringAsc(x => x.name);

                return { ...s, patterns, version: version };
            });
        }, error => {
            this.dialogs.notifyError(error);
        });

        return update$.pipe(map(x => x.payload));
    }

    public delete(pattern: PatternDto): Observable<any> {
        const update$ =
            this.patternsService.deletePattern(this.appName, pattern.id, this.version).pipe(
                share());

        update$.subscribe(({ version }) => {
            this.next(s => {
                const patterns = s.patterns.filter(c => c.id !== pattern.id);

                return { ...s, patterns, version: version };
            });
        }, error => {
            this.dialogs.notifyError(error);
        });

        return update$;
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get version() {
        return this.snapshot.version;
    }
}

const update = (pattern: PatternDto, request: EditPatternDto) =>
    pattern.with(request);
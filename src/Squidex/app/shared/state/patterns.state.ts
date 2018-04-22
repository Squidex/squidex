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
    ValidatorsEx,
    Version
} from '@app/framework';

import { AppsState } from './apps.state';

import {
    AppPatternDto,
    AppPatternsService,
    EditAppPatternDto
} from './../services/app-patterns.service';

export class EditPatternForm extends Form<FormGroup> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(100),
                    ValidatorsEx.pattern('[A-z0-9]+[A-z0-9\- ]*[A-z0-9]', 'Name can only contain letters, numbers, dashes and spaces.')
                ]
            ],
            pattern: ['',
                [
                    Validators.required
                ]
            ],
            message: ['',
                [
                    Validators.maxLength(1000)
                ]
            ]
        }));
    }
}

interface Snapshot {
    patterns: ImmutableArray<AppPatternDto>;

    isLoaded?: boolean;

    version: Version;
}

@Injectable()
export class PatternsState extends State<Snapshot> {
    public patterns =
        this.changes.map(x => x.patterns)
            .distinctUntilChanged();

    public isLoaded =
        this.changes.map(x => !!x.isLoaded)
            .distinctUntilChanged();

    constructor(
        private readonly appPatternsService: AppPatternsService,
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService
    ) {
        super({ patterns: ImmutableArray.empty(), version: new Version('') });
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return this.appPatternsService.getPatterns(this.appName)
            .do(dtos => {
                if (isReload) {
                    this.dialogs.notifyInfo('Patterns reloaded.');
                }

                this.next(s => {
                    const patterns = ImmutableArray.of(dtos.patterns).sortByStringAsc(x => x.name);

                    return { ...s, patterns, isLoaded: true, version: dtos.version };
                });
            })
            .notify(this.dialogs);
    }

    public create(request: EditAppPatternDto): Observable<any> {
        return this.appPatternsService.postPattern(this.appName, request, this.version)
            .do(dto => {
                this.next(s => {
                    const patterns = s.patterns.push(dto.payload).sortByStringAsc(x => x.name);

                    return { ...s, patterns, version: dto.version };
                });
            })
            .notify(this.dialogs);
    }

    public update(pattern: AppPatternDto, request: EditAppPatternDto): Observable<any> {
        return this.appPatternsService.putPattern(this.appName, pattern.id, request, this.version)
            .do(dto => {
                this.next(s => {
                    const patterns = s.patterns.replaceBy('id', update(pattern, request)).sortByStringAsc(x => x.name);

                    return { ...s, patterns, version: dto.version };
                });
            })
            .notify(this.dialogs);
    }

    public delete(pattern: AppPatternDto): Observable<any> {
        return this.appPatternsService.deletePattern(this.appName, pattern.id, this.version)
            .do(dto => {
                this.next(s => {
                    const patterns = s.patterns.filter(c => c.id !== pattern.id);

                    return { ...s, patterns, version: dto.version };
                });
            })
            .notify(this.dialogs);
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get version() {
        return this.snapshot.version;
    }
}

const update = (pattern: AppPatternDto, request: EditAppPatternDto) =>
    new AppPatternDto(pattern.id, request.name, request.pattern, request.message);
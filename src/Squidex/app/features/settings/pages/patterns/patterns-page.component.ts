/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';

import {
    AppPatternDto,
    AppsState,
    PatternsState
} from '@app/shared';

@Component({
    selector: 'sqx-patterns-page',
    styleUrls: ['./patterns-page.component.scss'],
    templateUrl: './patterns-page.component.html'
})
export class PatternsPageComponent implements OnInit {
    constructor(
        public readonly appsState: AppsState,
        public readonly patternsState: PatternsState
    ) {
    }

    public ngOnInit() {
        this.patternsState.load().onErrorResumeNext().subscribe();
    }

    public reload() {
        this.patternsState.load(true).onErrorResumeNext().subscribe();
    }

    public trackByPattern(index: number, pattern: AppPatternDto) {
        return pattern.id;
    }
}
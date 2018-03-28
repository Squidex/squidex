/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';

import {
    AppPatternDto,
    AppPatternsDto,
    AppPatternsService,
    AppsState,
    DialogService,
    UpdatePatternDto
} from '@app/shared';

@Component({
    selector: 'sqx-patterns-page',
    styleUrls: ['./patterns-page.component.scss'],
    templateUrl: './patterns-page.component.html'
})
export class PatternsPageComponent implements OnInit {
    public appPatterns: AppPatternsDto;

    constructor(
        public readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly appPatternsService: AppPatternsService
    ) {
    }

    public ngOnInit() {
        this.load();
    }

    public load() {
        this.appPatternsService.getPatterns(this.appsState.appName)
            .subscribe(dtos => {
                this.updatePatterns(dtos);
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    public addPattern(pattern: AppPatternDto) {
        const requestDto = new UpdatePatternDto(pattern.name, pattern.pattern, pattern.message);

        this.appPatternsService.postPattern(this.appsState.appName, requestDto, this.appPatterns.version)
            .subscribe(dto => {
                this.updatePatterns(this.appPatterns.addPattern(dto.payload, dto.version));
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    public updatePattern(pattern: AppPatternDto, update: UpdatePatternDto) {
        this.appPatternsService.putPattern(this.appsState.appName, pattern.patternId, update, this.appPatterns.version)
            .subscribe(dto => {
                this.updatePatterns(this.appPatterns.updatePattern(pattern.update(update), dto.version));
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    public removePattern(pattern: AppPatternDto) {
        this.appPatternsService.deletePattern(this.appsState.appName, pattern.patternId, this.appPatterns.version)
            .subscribe(dto => {
                this.updatePatterns(this.appPatterns.deletePattern(pattern, dto.version));
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    private updatePatterns(patterns: AppPatternsDto) {
        this.appPatterns =
            new AppPatternsDto(
                patterns.patterns.sort((a, b) => {
                    return a.name.localeCompare(b.name);
                }),
                patterns.version);
    }
}
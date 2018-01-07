/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';

import {
    AppContext,
    AppPatternDto,
    AppPatternsDto,
    AppPatternsService,
    HistoryChannelUpdated,
    UpdatePatternDto
} from 'shared';

@Component({
    selector: 'sqx-patterns-page',
    styleUrls: ['./patterns-page.component.scss'],
    templateUrl: './patterns-page.component.html',
    providers: [
        AppContext
    ]
})
export class PatternsPageComponent implements OnInit {
    public appPatterns: AppPatternsDto;

    constructor(public readonly ctx: AppContext,
        private readonly appPatternsService: AppPatternsService
    ) {
    }

    public ngOnInit() {
        this.load();
    }

    public load() {
        this.appPatternsService.getPatterns(this.ctx.appName).retry(2)
            .subscribe(dtos => {
                this.updatePatterns(dtos);
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public addPattern(pattern: AppPatternDto) {
        const requestDto = new UpdatePatternDto(pattern.name, pattern.pattern, pattern.message);

        this.appPatternsService.postPattern(this.ctx.appName, requestDto, this.appPatterns.version)
            .subscribe(dto => {
                this.updatePatterns(this.appPatterns.addPattern(dto.payload, dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public updatePattern(pattern: AppPatternDto, update: UpdatePatternDto) {
        this.appPatternsService.putPattern(this.ctx.appName, pattern.patternId, update, this.appPatterns.version)
            .subscribe(dto => {
                this.updatePatterns(this.appPatterns.updatePattern(pattern.update(update), dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public removePattern(pattern: AppPatternDto) {
        this.appPatternsService.deletePattern(this.ctx.appName, pattern.patternId, this.appPatterns.version)
            .subscribe(dto => {
                this.updatePatterns(this.appPatterns.deletePattern(pattern, dto.version));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    private updatePatterns(patterns: AppPatternsDto) {
        this.appPatterns =
            new AppPatternsDto(
                patterns.patterns.sort((a, b) => {
                    return a.name.localeCompare(b.name);
                }),
                patterns.version);

        this.ctx.bus.emit(new HistoryChannelUpdated());
    }
}
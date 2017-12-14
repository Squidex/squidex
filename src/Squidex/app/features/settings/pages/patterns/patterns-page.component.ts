/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';

import {
    AppContext,
    HistoryChannelUpdated,
    ImmutableArray,
    AppPatternsService,
    AppPatternsSuggestionDto,
    Version
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
    private version = new Version();
    public appPatterns = ImmutableArray.empty<AppPatternsSuggestionDto>();

    constructor(public readonly ctx: AppContext,
        private readonly patternService: AppPatternsService
    ) {
    }

    public ngOnInit() {
        this.load();
    }

    public load() {
        this.patternService.getPatterns(this.ctx.appName).retry(2)
            .subscribe(dtos => {
                this.updatePatterns(ImmutableArray.of(dtos));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public updatePattern(requestDto: AppPatternsSuggestionDto) {
        this.updatePatterns(this.appPatterns.remove(requestDto));
    }

    public addPattern(requestDto: AppPatternsSuggestionDto) {
        this.updatePatterns(this.appPatterns.push(requestDto));
    }

    public removePattern(pattern: AppPatternsSuggestionDto) {
        this.patternService.deletePattern(this.ctx.appName, pattern.id, this.version)
            .subscribe(() => {
                this.updatePatterns(this.appPatterns.remove(pattern));
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    private updatePatterns(patterns: ImmutableArray<AppPatternsSuggestionDto>) {
        this.appPatterns =
            patterns.map(p => {
            return new AppPatternsSuggestionDto(
                    p.id,
                    p.name,
                    p.pattern,
                    p.defaultMessage
                );
            }).sort((a, b) => {
                return a.name.localeCompare(b.name);
            });

        this.ctx.bus.emit(new HistoryChannelUpdated());
    }
}
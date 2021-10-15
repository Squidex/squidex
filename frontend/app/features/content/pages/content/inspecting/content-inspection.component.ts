/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { AppLanguageDto, ContentDto, ContentsService, ContentsState, ErrorDto } from '@app/shared';
import { BehaviorSubject, combineLatest, of } from 'rxjs';
import { filter, map, switchMap } from 'rxjs/operators';

type Mode = 'Content' | 'Data' | 'FlatData';

@Component({
    selector: 'sqx-content-inspection[appName][content][language][languages]',
    styleUrls: ['./content-inspection.component.scss'],
    templateUrl: './content-inspection.component.html',
})
export class ContentInspectionComponent implements OnChanges {
    private languageChanges$ = new BehaviorSubject<AppLanguageDto | null>(null);

    @Input()
    public appName: string;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: ReadonlyArray<AppLanguageDto>;

    @Input()
    public content: ContentDto;

    public mode = new BehaviorSubject<Mode>('Content');

    public contentError?: ErrorDto | null;
    public contentData: any;

    public actualData =
        combineLatest([
            this.languageChanges$,
            this.mode,
        ]).pipe(
            filter(x => !!x[0]),
            switchMap(([language, mode]) => {
                if (mode === 'Content') {
                    return of(this.content);
                } else if (mode === 'Data') {
                    return of(this.content.data);
                } else {
                    return this.contentsService.getContent(this.appName,
                        this.content.schemaName,
                        this.content.id,
                        language?.iso2Code).pipe(
                        map(x => x.data));
                }
            }));

    constructor(
        private readonly contentsService: ContentsService,
        private readonly contentsState: ContentsState,
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['language']) {
            this.languageChanges$.next(this.language);
        }
    }

    public setData(data: any) {
        this.contentData = data;
    }

    public setMode(mode: Mode) {
        this.mode.next(mode);
    }

    public save() {
        if (!this.contentData || this.mode.value !== 'Data') {
            return;
        }

        this.contentsState.update(this.content, this.contentData)
            .subscribe({
                next: () => {
                    this.contentError = null;
                },
                error: error => {
                    this.contentError = error;
                },
            });
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Component, Input, OnChanges, OnDestroy, SimpleChanges } from '@angular/core';
import { BehaviorSubject, combineLatest, of } from 'rxjs';
import { filter, map, switchMap } from 'rxjs/operators';
import { AppLanguageDto, ContentDto, ContentsService, ContentsState, ErrorDto, ToolbarService } from '@app/shared';

type Mode = 'Content' | 'Data' | 'FlatData';

@Component({
    selector: 'sqx-content-inspection[appName][content][language][languages]',
    styleUrls: ['./content-inspection.component.scss'],
    templateUrl: './content-inspection.component.html',
})
export class ContentInspectionComponent implements OnChanges, OnDestroy {
    private languageChanges$ = new BehaviorSubject<AppLanguageDto | null>(null);

    @Input()
    public appName!: string;

    @Input()
    public language!: AppLanguageDto;

    @Input()
    public languages!: ReadonlyArray<AppLanguageDto>;

    @Input()
    public content!: ContentDto;

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
        private readonly changeDetector: ChangeDetectorRef,
        private readonly contentsService: ContentsService,
        private readonly contentsState: ContentsState,
        private toolbar: ToolbarService,
    ) {
    }

    public ngOnDestroy() {
        this.toolbar.remove(this);
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['language']) {
            this.languageChanges$.next(this.language);
        }

        if (changes['content']) {
            this.updateActions();
        }
    }

    private updateActions() {
        if (this.mode.value === 'Data' && this.content.canUpdate) {
            this.toolbar.addButton(this, 'common.save', () => {
                this.save();

                this.changeDetector.detectChanges();
            });
        } else {
            this.toolbar.remove(this);
        }
    }

    public setData(data: any) {
        this.contentData = data;

        this.updateActions();
    }

    public setMode(mode: Mode) {
        this.mode.next(mode);

        this.updateActions();
    }

    private save() {
        if (!this.contentData) {
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

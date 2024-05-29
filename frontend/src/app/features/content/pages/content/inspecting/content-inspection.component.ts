/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectorRef, Component, Input, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BehaviorSubject, combineLatest, of } from 'rxjs';
import { filter, map, switchMap } from 'rxjs/operators';
import { AppLanguageDto, CodeEditorComponent, ContentDto, ContentsService, ContentsState, ErrorDto, FormErrorComponent, ToolbarService, TranslatePipe, TypedSimpleChanges } from '@app/shared';

type Mode = 'Content' | 'Data' | 'FlatData';

@Component({
    standalone: true,
    selector: 'sqx-content-inspection',
    styleUrls: ['./content-inspection.component.scss'],
    templateUrl: './content-inspection.component.html',
    imports: [
        AsyncPipe,
        CodeEditorComponent,
        FormErrorComponent,
        FormsModule,
        TranslatePipe,
    ],
})
export class ContentInspectionComponent implements OnDestroy {
    private languageChanges$ = new BehaviorSubject<AppLanguageDto | null>(null);

    @Input({ required: true })
    public appName!: string;

    @Input({ required: true })
    public language!: AppLanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<AppLanguageDto>;

    @Input({ required: true })
    public content!: ContentDto;

    public mode = new BehaviorSubject<Mode>('Content');

    public contentError?: ErrorDto | null;
    public contentData: any;

    public actualData =
        combineLatest([
            this.languageChanges$.pipe(filter(x => !!x)),
            this.mode,
        ]).pipe(
            switchMap(([language, mode]) => {
                if (mode === 'Data') {
                    return of(this.content.data);
                } else if (mode === 'Content') {
                    return this.contentsService.getRawContent(this.appName,
                        this.content.schemaName,
                        this.content.id);
                } else {
                    return this.contentsService.getRawContent(this.appName,
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

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.language) {
            this.languageChanges$.next(this.language);
        }

        if (changes.content) {
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

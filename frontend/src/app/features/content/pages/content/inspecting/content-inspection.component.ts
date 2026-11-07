/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, computed, DestroyRef, effect, inject, input, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { of } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { AppLanguageDto, CodeEditorComponent, ContentDto, ContentsService, ContentsState, ErrorDto, FormErrorComponent, ToolbarService, TranslatePipe } from '@app/shared';

type Mode = 'Content' | 'Data' | 'FlatData';

@Component({
    selector: 'sqx-content-inspection',
    styleUrls: ['./content-inspection.component.scss'],
    templateUrl: './content-inspection.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeEditorComponent,
        FormErrorComponent,
        FormsModule,
        TranslatePipe,
    ],
})
export class ContentInspectionComponent {
    private readonly changeDetector = inject(ChangeDetectorRef);
    private readonly contentsService = inject(ContentsService);
    private readonly contentsState = inject(ContentsState);
    private readonly toolbar = inject(ToolbarService);

    private readonly request = computed(() => {
        return { language: this.language(), mode: this.mode() };
    });

    private contentData: any;

    /** The name of the app. */
    public readonly appName = input.required<string>();

    /** The language to load the flat data for. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** The content to inspect. */
    public readonly content = input.required<ContentDto>();

    protected readonly mode = signal<Mode>('Content');

    protected readonly contentError = signal<ErrorDto | null | undefined>(undefined);

    protected readonly actualData = toSignal(
        toObservable(this.request).pipe(
            switchMap(({ language, mode }) => {
                const content = this.content();

                if (mode === 'Data') {
                    return of(content.data);
                } else if (mode === 'Content') {
                    return this.contentsService.getRawContent(this.appName(),
                        content.schemaName,
                        content.id);
                } else {
                    return this.contentsService.getRawContent(this.appName(),
                        content.schemaName,
                        content.id,
                        language?.iso2Code).pipe(
                        map(x => x.data));
                }
            })));

    constructor() {
        inject(DestroyRef).onDestroy(() => {
            this.toolbar.remove(this);
        });

        effect(() => {
            this.updateActions();
        });
    }

    private updateActions() {
        if (this.mode() === 'Data' && this.content().canUpdate) {
            this.toolbar.addButton(this, 'common.save', () => {
                this.save();

                this.changeDetector.detectChanges();
            });
        } else {
            this.toolbar.remove(this);
        }
    }

    protected setData(data: any) {
        this.contentData = data;

        this.updateActions();
    }

    protected setMode(mode: Mode) {
        this.mode.set(mode);
    }

    private save() {
        if (!this.contentData) {
            return;
        }

        this.contentsState.update(this.content(), this.contentData)
            .subscribe({
                next: () => {
                    this.contentError.set(null);
                },
                error: error => {
                    this.contentError.set(error);
                },
            });
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, OnChanges, Output, QueryList, SimpleChanges, ViewChildren } from '@angular/core';
import { AppLanguageDto, ContentDto, ContentListFieldComponent, ContentsState, fadeAnimation, ModalModel, PatchContentForm, RootFieldDto, TableField, Types } from '@app/shared';

/* tslint:disable: component-selector */

@Component({
    selector: '[sqxContent][language][listFields]',
    styleUrls: ['./content.component.scss'],
    templateUrl: './content.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContentComponent implements OnChanges {
    @Output()
    public clone = new EventEmitter();

    @Output()
    public delete = new EventEmitter();

    @Output()
    public statusChange = new EventEmitter<string>();

    @Output()
    public selectedChange = new EventEmitter<boolean>();

    @Input()
    public selected = false;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public listFields: ReadonlyArray<TableField>;

    @Input()
    public cloneable?: boolean | null;

    @Input()
    public link: any = null;

    @Input('sqxContent')
    public content: ContentDto;

    @ViewChildren(ContentListFieldComponent)
    public fields: QueryList<ContentListFieldComponent>;

    public patchForm: PatchContentForm;
    public patchAllowed?: boolean | null;

    public dropdown = new ModalModel();

    public get isDirty() {
        return this.patchForm && this.patchForm.form.dirty;
    }

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
        private readonly contentsState: ContentsState,
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['content']) {
            this.patchAllowed = this.content.canUpdate;
        }

        if (this.patchAllowed && (changes['listFields'] || changes['language'])) {
            this.patchForm = new PatchContentForm(this.listFields, this.language);
        }
    }

    public save() {
        if (!this.content.canUpdate) {
            return;
        }

        const value = this.patchForm.submit();

        if (value) {
            this.contentsState.patch(this.content, value)
                .subscribe({
                    next: () => {
                        this.patchForm.submitCompleted({ noReset: true });

                        this.changeDetector.detectChanges();
                    },
                    error: error => {
                        this.patchForm.submitFailed(error);

                        this.changeDetector.detectChanges();
                    },
                });
        }
    }

    public shouldStop(field: TableField) {
        if (Types.is(field, RootFieldDto)) {
            return this.isDirty || (field.isInlineEditable && this.patchAllowed);
        } else {
            return this.isDirty;
        }
    }

    public cancel() {
        this.patchForm.submitCompleted();

        this.fields.forEach(x => x.reset());
    }
}

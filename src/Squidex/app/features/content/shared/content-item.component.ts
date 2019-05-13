/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';

import {
    AppLanguageDto,
    ContentDto,
    ContentsState,
    fadeAnimation,
    FieldDto,
    FieldFormatter,
    fieldInvariant,
    ModalModel,
    PatchContentForm,
    RootFieldDto,
    SchemaDetailsDto,
    Types
} from '@app/shared';

/* tslint:disable:component-selector */

@Component({
    selector: '[sqxContent]',
    styleUrls: ['./content-item.component.scss'],
    templateUrl: './content-item.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentItemComponent implements OnChanges {
    @Output()
    public clone = new EventEmitter();

    @Output()
    public delete = new EventEmitter();

    @Output()
    public archive = new EventEmitter();

    @Output()
    public restore = new EventEmitter();

    @Output()
    public publish = new EventEmitter();

    @Output()
    public unpublish = new EventEmitter();

    @Output()
    public selectedChange = new EventEmitter();

    @Input()
    public selected = false;

    @Input()
    public selectable = true;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public schema: SchemaDetailsDto;

    @Input()
    public isReadOnly = false;

    @Input()
    public isReference = false;

    @Input()
    public isCompact = false;

    @Input('sqxContent')
    public content: ContentDto;

    public patchForm: PatchContentForm;

    public dropdown = new ModalModel();

    public values: any[] = [];

    constructor(
        private readonly contentsState: ContentsState
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['schema'] || changes['language']) {
            this.patchForm = new PatchContentForm(this.schema, this.language);
        }

        if (changes['content'] || changes['language']) {
            this.updateValues();
        }
    }

    public shouldStop(event: Event, field?: FieldDto) {
        if (this.patchForm.form.dirty || (field && field.isInlineEditable)) {
            event.stopPropagation();
            event.stopImmediatePropagation();
        }
    }

    public stop(event: Event) {
        event.stopPropagation();
        event.stopImmediatePropagation();
    }

    public save() {
        const value = this.patchForm.submit();

        if (value) {
            this.contentsState.patch(this.content, value)
                .subscribe(() => {
                    this.patchForm.submitCompleted();
                }, error => {
                    this.patchForm.submitFailed(error);
                });
        }
    }

    public cancel() {
        this.patchForm.submitCompleted();

        this.updateValues();
    }

    public emitDelete() {
        this.delete.emit();
    }

    public emitPublish() {
        this.publish.emit();
    }

    public emitUnpublish() {
        this.unpublish.emit();
    }

    public emitArchive() {
        this.archive.emit();
    }

    public emitRestore() {
        this.unpublish.emit();
    }

    public emitClone() {
        this.clone.emit();
    }

    private updateValues() {
        this.values = [];

        for (let field of this.schema.listFields) {
            const value = this.getRawValue(field);

            if (Types.isUndefined(value)) {
                this.values.push('');
            } else {
                this.values.push(FieldFormatter.format(field, value));
            }

            if (this.patchForm) {
                const formControl = this.patchForm.form.controls[field.name];

                if (formControl) {
                    formControl.setValue(value);
                }
            }
        }
    }

    private getRawValue(field: RootFieldDto): any {
        const contentField = this.content.dataDraft[field.name];

        if (contentField) {
            if (field.isLocalizable) {
                return contentField[this.language.iso2Code];
            } else {
                return contentField[fieldInvariant];
            }
        }

        return undefined;
    }

    public trackByField(index: number, field: FieldDto) {
        return field.fieldId + this.schema.id;
    }
}


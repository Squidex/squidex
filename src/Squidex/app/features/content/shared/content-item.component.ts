/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';

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
    public statusChange = new EventEmitter<string>();

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
    public canClone: boolean;

    @Input()
    public isReadOnly = false;

    @Input()
    public isReference = false;

    @Input()
    public isCompact = false;

    @Input('sqxContent')
    public content: ContentDto;

    public patchForm: PatchContentForm;
    public patchAllowed = false;

    public dropdown = new ModalModel();

    public values: any[] = [];

    public get isDirty() {
        return this.patchForm && this.patchForm.form.dirty;
    }

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
        private readonly contentsState: ContentsState
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['content']) {
            this.patchAllowed = !this.isReadOnly && this.content.canUpdate;
        }

        if (changes['schema'] || changes['language']) {
            if (this.patchAllowed) {
                this.patchForm = new PatchContentForm(this.schema, this.language);
            }
        }

        if (changes['content'] || changes['language']) {
            this.updateValues();
        }
    }

    public save() {
        if (!this.content.canUpdate) {
            return;
        }

        const value = this.patchForm.submit();

        if (value) {
            this.contentsState.patch(this.content, value)
                .subscribe(() => {
                    this.patchForm.submitCompleted({ noReset: true});

                    this.changeDetector.detectChanges();
                }, error => {
                    this.patchForm.submitFailed(error);

                    this.changeDetector.detectChanges();
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

    public emitChangeStatus(status: string) {
        this.statusChange.emit(status);
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
                this.values.push(FieldFormatter.format(field, value, true));
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


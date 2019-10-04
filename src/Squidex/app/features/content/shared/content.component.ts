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
    getContentValue,
    ModalModel,
    PatchContentForm,
    RootFieldDto,
    SchemaDetailsDto
} from '@app/shared';

/* tslint:disable:component-selector */

@Component({
    selector: '[sqxContent]',
    styleUrls: ['./content.component.scss'],
    templateUrl: './content.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
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
    public schema: SchemaDetailsDto;

    @Input()
    public schemaFields: RootFieldDto[];

    @Input()
    public canClone: boolean;

    @Input()
    public isCompact = false;

    @Input()
    public link: any = null;

    @Input('sqxContent')
    public content: ContentDto;

    public trackByFieldFn: Function;

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
        this.trackByFieldFn = this.trackByField.bind(this);
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['content']) {
            this.patchAllowed = this.content.canUpdate;
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

    public emitSelectedChange(isSelected: boolean) {
        this.selectedChange.emit(isSelected);
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

        for (let field of this.schemaFields) {
            const { value, formatted } = getContentValue(this.content, this.language, field);

            this.values.push(formatted);

            if (this.patchForm) {
                const formControl = this.patchForm.form.controls[field.name];

                if (formControl) {
                    formControl.setValue(value);
                }
            }
        }
    }

    public trackByField(index: number, field: FieldDto) {
        return field.fieldId + this.schema.id;
    }
}
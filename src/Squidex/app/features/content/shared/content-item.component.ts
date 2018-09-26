/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';

import {
    AppLanguageDto,
    ContentDto,
    ContentsState,
    fadeAnimation,
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
    ]
})
export class ContentItemComponent implements OnChanges {
    @Output()
    public cloning = new EventEmitter();

    @Output()
    public deleting = new EventEmitter();

    @Output()
    public archiving = new EventEmitter();

    @Output()
    public restoring = new EventEmitter();

    @Output()
    public publishing = new EventEmitter();

    @Output()
    public unpublishing = new EventEmitter();

    @Output()
    public selectedChange = new EventEmitter();

    @Input()
    public selected = false;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public schema: SchemaDetailsDto;

    @Input()
    public isReadOnly = false;

    @Input()
    public isReference = false;

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

    public shouldStop(event: Event) {
        if (this.patchForm.form.dirty) {
            event.stopPropagation();
            event.stopImmediatePropagation();
        }
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
}


/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AddFieldForm,
    createProperties,
    EditFieldForm,
    FieldDto,
    fieldTypes,
    PatternsState,
    RootFieldDto,
    SchemaDetailsDto,
    SchemasState,
    Types,
    UpdateFieldDto
} from '@app/shared';

@Component({
    selector: 'sqx-field-wizard',
    styleUrls: ['./field-wizard.component.scss'],
    templateUrl: './field-wizard.component.html'
})
export class FieldWizardComponent implements OnInit {
    @ViewChild('nameInput')
    public nameInput: ElementRef<HTMLElement>;

    @Input()
    public schema: SchemaDetailsDto;

    @Input()
    public parent: RootFieldDto;

    @Output()
    public completed = new EventEmitter();

    public fieldTypes = fieldTypes;
    public field: FieldDto;

    public addFieldForm = new AddFieldForm(this.formBuilder);

    public editForm = new EditFieldForm(this.formBuilder);

    public isEditing = false;
    public selectedTab = 0;

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly schemasState: SchemasState,
        public readonly patternsState: PatternsState
    ) {}

    public ngOnInit() {
        if (this.parent) {
            this.fieldTypes = this.fieldTypes.filter(x => x.type !== 'Array');
        }
    }

    public complete() {
        this.completed.emit();
    }

    public addField(addNew: boolean, edit: boolean) {
        const value = this.addFieldForm.submit();

        if (value) {
            this.schemasState.addField(this.schema, value, this.parent)
                .subscribe(dto => {
                    this.field = dto;

                    this.addFieldForm.submitCompleted({ type: fieldTypes[0].type });

                    if (addNew) {
                        if (Types.isFunction(this.nameInput.nativeElement.focus)) {
                            this.nameInput.nativeElement.focus();
                        }
                    } else if (edit) {
                        this.selectTab(0);

                        this.isEditing = true;
                    } else {
                        this.complete();
                    }
                }, error => {
                    this.addFieldForm.submitFailed(error);
                });
        }
    }

    public selectTab(tab: number) {
        this.selectedTab = tab;
    }

    public save(addNew: boolean) {
        const value = this.editForm.submit();

        if (value) {
            const properties = createProperties(this.field.properties['fieldType'], value);

            this.schemasState.updateField(this.schema, this.field as RootFieldDto, new UpdateFieldDto(properties))
                .subscribe(() => {
                    this.editForm.submitCompleted();

                    if (addNew) {
                        this.isEditing = false;
                    } else {
                        this.complete();
                    }
                }, error => {
                    this.editForm.submitFailed(error);
                });
        }
    }
}

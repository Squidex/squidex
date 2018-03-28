/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';

import {
    createProperties,
    fadeAnimation,
    FieldDto,
    ModalView,
    SchemaDetailsDto,
    UpdateFieldDto
} from '@app/shared';

import { SchemasState } from './../../state/schemas.state';

@Component({
    selector: 'sqx-field',
    styleUrls: ['./field.component.scss'],
    templateUrl: './field.component.html',
    animations: [
        fadeAnimation
    ]
})
export class FieldComponent implements OnInit {
    @Input()
    public field: FieldDto;

    @Input()
    public schema: SchemaDetailsDto;

    public dropdown = new ModalView(false, true);

    public isEditing = false;
    public selectedTab = 0;

    public editFormSubmitted = false;
    public editForm = new FormGroup({});

    constructor(
        private readonly schemasState: SchemasState
    ) {
    }

    public ngOnInit() {
        this.resetEditForm();
    }

    public toggleEditing() {
        this.isEditing = !this.isEditing;
    }

    public selectTab(tab: number) {
        this.selectedTab = tab;
    }

    public cancel() {
        this.resetEditForm();
    }

    public deleteField() {
        this.schemasState.deleteField(this.schema, this.field).subscribe();
    }

    public enableField() {
        this.schemasState.enableField(this.schema, this.field).subscribe();
    }

    public disableField() {
        this.schemasState.disableField(this.schema, this.field).subscribe();
    }

    public showField() {
        this.schemasState.showField(this.schema, this.field).subscribe();
    }

    public hideField() {
        this.schemasState.hideField(this.schema, this.field).subscribe();
    }

    public lockField() {
        this.schemasState.lockField(this.schema, this.field).subscribe();
    }

    public save() {
        this.editFormSubmitted = true;

        if (this.editForm.valid) {
            const properties = createProperties(this.field.properties['fieldType'], this.editForm.value);

            this.schemasState.updateField(this.schema, this.field, new UpdateFieldDto(properties))
                .subscribe(() => {
                    this.resetEditForm();
                });
        }
    }

    private resetEditForm() {
        this.isEditing = false;
        this.editFormSubmitted = false;
        this.editForm.reset(this.field.properties);

        if (this.field.isLocked) {
            this.editForm.disable();
        }
    }
}


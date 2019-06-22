/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AddPermissionForm,
    AutocompleteComponent,
    AutocompleteSource,
    EditPermissionsForm,
    fadeAnimation,
    RoleDto,
    RolesState
} from '@app/shared';

@Component({
    selector: 'sqx-role',
    styleUrls: ['./role.component.scss'],
    templateUrl: './role.component.html',
    animations: [
        fadeAnimation
    ]
})
export class RoleComponent implements OnChanges {
    @Input()
    public role: RoleDto;

    @Input()
    public allPermissions: AutocompleteSource;

    @ViewChild('addInput', { static: false })
    public addPermissionInput: AutocompleteComponent;

    public isEditing = false;
    public isEditable = false;

    public addPermissionForm = new AddPermissionForm(this.formBuilder);

    public editForm = new EditPermissionsForm();

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly rolesState: RolesState
    ) {
    }

    public ngOnChanges() {
        this.isEditable = this.role.canUpdate;

        this.editForm.load(this.role.permissions);
        this.editForm.setEnabled(this.isEditable);
    }

    public toggleEditing() {
        this.isEditing = !this.isEditing;
    }

    public removePermission(index: number) {
        this.editForm.remove(index);
    }

    public remove() {
        this.rolesState.delete(this.role);
    }

    public addPermission() {
        const value = this.addPermissionForm.submit();

        if (value) {
            this.editForm.add(value.permission);

            this.addPermissionForm.submitCompleted();
            this.addPermissionInput.focus();
        }
    }

    public save() {
        const value = this.editForm.submit();

        if (value) {
            const request = { permissions: value };

            this.rolesState.update(this.role, request)
                .subscribe(() => {
                    this.editForm.submitCompleted();

                    this.toggleEditing();
                }, error => {
                    this.editForm.submitFailed(error);
                });
        }
    }
}


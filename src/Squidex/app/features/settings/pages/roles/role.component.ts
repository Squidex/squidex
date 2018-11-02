/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { onErrorResumeNext } from 'rxjs/operators';

import {
    AddPermissionForm,
    AppRoleDto,
    AutocompleteComponent,
    AutocompleteSource,
    EditPermissionsForm,
    fadeAnimation,
    RolesState,
    UpdateAppRoleDto
} from '@app/shared';

const DEFAULT_ROLES = [
    'Owner',
    'Developer',
    'Editor',
    'Reader'
];

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
    public role: AppRoleDto;

    @Input()
    public allPermissions: AutocompleteSource;

    @ViewChild('addInput')
    public addPermissionInput: AutocompleteComponent;

    public isEditing = false;
    public isDefaultRole = false;

    public addPermissionForm = new AddPermissionForm(this.formBuilder);

    public editForm = new EditPermissionsForm();

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly rolesState: RolesState
    ) {
    }

    public ngOnChanges() {
        this.isDefaultRole = DEFAULT_ROLES.indexOf(this.role.name) >= 0;

        this.editForm.load(this.role.permissions);

        if (this.isDefaultRole) {
            this.editForm.form.disable();
        }
    }

    public toggleEditing() {
        this.isEditing = !this.isEditing;
    }

    public removePermission(index: number) {
        this.editForm.remove(index);
    }

    public remove() {
        this.rolesState.delete(this.role).pipe(onErrorResumeNext()).subscribe();
    }

    public addPermission() {
        const value = this.addPermissionForm.submit();

        if (value) {
            this.editForm.add(value.permission);

            this.addPermissionForm.submitCompleted({});
            this.addPermissionInput.focus();
        }
    }

    public save() {
        const value = this.editForm.submit();

        if (value) {
            const request = new UpdateAppRoleDto(value);

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


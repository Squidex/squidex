/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges } from '@angular/core';
import { onErrorResumeNext } from 'rxjs/operators';

import {
    AppRoleDto,
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

    public isEditing = false;
    public isDefaultRole = false;

    public editForm = new EditPermissionsForm();

    constructor(
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

    public addPermission() {
        this.editForm.add();
    }

    public removePermission(index: number) {
        this.editForm.remove(index);
    }

    public remove() {
        this.rolesState.delete(this.role).pipe(onErrorResumeNext()).subscribe();
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


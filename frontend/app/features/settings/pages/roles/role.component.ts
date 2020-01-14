/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AddPermissionForm,
    AutocompleteComponent,
    AutocompleteSource,
    EditRoleForm,
    RoleDto,
    RolesState
} from '@app/shared';

const Descriptions = {
    Developer: 'Can use the API view, edit assets, contents, schemas, rules, workflows and patterns.',
    Editor: 'Can edit assets and contents and view workflows.',
    Owner: 'Can do everything, including deleting the app.',
    Reader: 'Can only read assets and contents.'
};

@Component({
    selector: 'sqx-role',
    styleUrls: ['./role.component.scss'],
    templateUrl: './role.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class RoleComponent implements OnChanges {
    @Input()
    public role: RoleDto;

    @Input()
    public allPermissions: AutocompleteSource;

    @ViewChild('addInput', { static: false })
    public addPermissionInput: AutocompleteComponent;

    public descriptions = Descriptions;

    public isEditing = false;
    public isEditable = false;

    public addPermissionForm = new AddPermissionForm(this.formBuilder);

    public editForm = new EditRoleForm();

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly rolesState: RolesState
    ) {
    }

    public ngOnChanges() {
        this.isEditable = this.role.canUpdate;

        this.editForm.load(this.role);
        this.editForm.setEnabled(this.isEditable);
    }

    public toggleEditing() {
        this.isEditing = !this.isEditing;
    }

    public delete() {
        this.rolesState.delete(this.role);
    }

    public removePermission(index: number) {
        this.editForm.remove(index);
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
            this.rolesState.update(this.role, value)
                .subscribe(() => {
                    this.editForm.submitCompleted();

                    this.toggleEditing();
                }, error => {
                    this.editForm.submitFailed(error);
                });
        }
    }
}
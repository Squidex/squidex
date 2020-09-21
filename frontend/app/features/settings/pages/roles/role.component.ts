/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { AddPermissionForm, AutocompleteComponent, AutocompleteSource, EditRoleForm, RoleDto, RolesState, Settings } from '@app/shared';

const Descriptions = {
    Developer: 'i18n:roles.defaults.developer',
    Editor: 'i18n:roles.defaults.editor',
    Owner: 'i18n:roles.default.owner',
    Reader: 'i18n:roles.default.reader'
};

@Component({
    selector: 'sqx-role',
    styleUrls: ['./role.component.scss'],
    templateUrl: './role.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class RoleComponent implements OnChanges {
    public readonly standalone = { standalone: true };

    @Input()
    public role: RoleDto;

    @Input()
    public allPermissions: AutocompleteSource;

    @ViewChild('addInput', { static: false })
    public addPermissionInput: AutocompleteComponent;

    public descriptions = Descriptions;

    public propertiesList = Settings.AppProperties;
    public properties: {};

    public isEditing = false;
    public isEditable = false;

    public addPermissionForm = new AddPermissionForm(this.formBuilder);

    public editForm = new EditRoleForm();

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly rolesState: RolesState
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['role']) {
            this.isEditable = this.role.canUpdate;

            this.properties = this.role.properties;

            this.editForm.load(this.role);
            this.editForm.setEnabled(this.isEditable);
        }
    }

    public getProperty(name: string) {
        return this.properties[name];
    }

    public toggleEditing() {
        this.isEditing = !this.isEditing;
    }

    public setProperty(name: string, value: boolean) {
        this.properties[name] = value;
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
            this.rolesState.update(this.role, { ...value, properties: this.properties })
                .subscribe(() => {
                    this.editForm.submitCompleted({ noReset: true });

                    this.toggleEditing();
                }, error => {
                    this.editForm.submitFailed(error);
                });
        }
    }
}
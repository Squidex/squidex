/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { AddPermissionForm, AutocompleteComponent, AutocompleteSource, EditRoleForm, RoleDto, RolesState, SchemaDto, Settings } from '@app/shared';

const DESCRIPTIONS = {
    Developer: 'i18n:roles.defaults.developer',
    Editor: 'i18n:roles.defaults.editor',
    Owner: 'i18n:roles.default.owner',
    Reader: 'i18n:roles.default.reader',
};

type Property = { name: string; key: string };

const SIMPLE_PROPERTIES: ReadonlyArray<Property> = [{
    name: 'i18n:roles.properties.hideSchemas',
    key: Settings.AppProperties.HIDE_SCHEMAS,
}, {
    name: 'i18n:roles.properties.hideAssets',
    key: Settings.AppProperties.HIDE_ASSETS,
}, {
    name: 'i18n:roles.properties.hideSettings',
    key: Settings.AppProperties.HIDE_SETTINGS,
}, {
    name: 'i18n:roles.properties.hideAPI',
    key: Settings.AppProperties.HIDE_API,
}];

@Component({
    selector: 'sqx-role[allPermissions][role][schemas]',
    styleUrls: ['./role.component.scss'],
    templateUrl: './role.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RoleComponent implements OnChanges {
    @Input()
    public role: RoleDto;

    @Input()
    public allPermissions: AutocompleteSource;

    @Input()
    public schemas: ReadonlyArray<SchemaDto>;

    @ViewChild('addInput', { static: false })
    public addPermissionInput: AutocompleteComponent;

    public get halfSchemas() {
        return Math.ceil(this.schemas.length / 2);
    }

    public descriptions = DESCRIPTIONS;

    public propertiesList = Settings.AppProperties;
    public properties: {};
    public propertiesSimple = SIMPLE_PROPERTIES;

    public isEditing = false;
    public isEditable = false;

    public addPermissionForm = new AddPermissionForm(this.formBuilder);

    public editForm = new EditRoleForm();

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly rolesState: RolesState,
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
                .subscribe({
                    next: () => {
                        this.editForm.submitCompleted({ noReset: true });
                    },
                    error: error => {
                        this.editForm.submitFailed(error);
                    },
                });
        }
    }

    public trackByProperty(_index: number, property: Property) {
        return property.key;
    }

    public trackBySchema(_index: number, schema: SchemaDto) {
        return schema.id;
    }
}

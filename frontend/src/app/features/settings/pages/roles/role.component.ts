/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { SlicePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input, ViewChild } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AutocompleteComponent, AutocompleteSource, ConfirmClickDirective, ControlErrorsComponent, EditRoleForm, FormAlertComponent, FormHintComponent, RoleDto, RolesState, SchemaDto, Settings, TranslatePipe, TypedSimpleChanges } from '@app/shared';

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
    standalone: true,
    selector: 'sqx-role',
    styleUrls: ['./role.component.scss'],
    templateUrl: './role.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AutocompleteComponent,
        ConfirmClickDirective,
        ControlErrorsComponent,
        FormAlertComponent,
        FormHintComponent,
        FormsModule,
        ReactiveFormsModule,
        SlicePipe,
        TranslatePipe,
    ],
})
export class RoleComponent {
    @Input({ required: true })
    public role!: RoleDto;

    @Input({ required: true })
    public allPermissions!: AutocompleteSource;

    @Input({ required: true })
    public schemas!: ReadonlyArray<SchemaDto>;

    @ViewChild('addInput', { static: false })
    public addPermissionInput!: AutocompleteComponent;

    public descriptions = DESCRIPTIONS as Record<string, string>;

    public propertiesList = Settings.AppProperties;
    public properties!: Record<string, any>;
    public propertiesSimple = SIMPLE_PROPERTIES;

    public isEditing = false;
    public isEditable = false;

    public editForm = new EditRoleForm();

    public get halfSchemas() {
        return Math.ceil(this.schemas.length / 2);
    }

    constructor(
        private readonly rolesState: RolesState,
    ) {
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.role) {
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

    public addPermission() {
        this.editForm.form.add('');
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
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { SlicePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, input, linkedSignal, signal } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AutocompleteComponent, AutocompleteSource, ConfirmClickDirective, ControlErrorsComponent, EditRoleForm, FormAlertComponent, FormHintComponent, RoleDto, RolesState, SchemaDto, Settings, TranslatePipe, UpdateRoleDto } from '@app/shared';

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
    /** The role to show and edit. */
    public readonly role = input.required<RoleDto>();

    /** The source for the permission suggestions. */
    public readonly allPermissions = input.required<AutocompleteSource>();

    /** The schemas of the app, used to hide individual schemas for the role. */
    public readonly schemas = input.required<ReadonlyArray<SchemaDto>>();

    protected readonly descriptions = DESCRIPTIONS as Record<string, string>;

    protected readonly propertiesList = Settings.AppProperties;
    protected readonly properties = linkedSignal<Record<string, any>>(() => ({ ...this.role().properties }));
    protected readonly propertiesSimple = SIMPLE_PROPERTIES;

    protected readonly isEditing = signal(false);

    protected readonly isEditable = computed(() => {
        return this.role().canUpdate;
    });

    protected readonly editForm = new EditRoleForm();

    protected readonly halfSchemas = computed(() => {
        return Math.ceil(this.schemas().length / 2);
    });

    constructor(
        private readonly rolesState: RolesState,
    ) {
        effect(() => {
            this.editForm.load(this.role());
            this.editForm.setEnabled(this.isEditable());
        });
    }

    protected getProperty(name: string) {
        return this.properties()[name];
    }

    protected toggleEditing() {
        this.isEditing.update(x => !x);
    }

    protected setProperty(name: string, value: boolean) {
        this.properties.update(x => ({ ...x, [name]: value }));
    }

    protected delete() {
        this.rolesState.delete(this.role());
    }

    protected addPermission() {
        this.editForm.form.add('');
    }

    protected save() {
        const value = this.editForm.submit();
        if (!value) {
            return;
        }

        const request = new UpdateRoleDto({ ...value, properties: this.properties() });

        this.rolesState.update(this.role(), request)
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

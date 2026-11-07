/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDragHandle, CdkDropList } from '@angular/cdk/drag-drop';

import { booleanAttribute, ChangeDetectionStrategy, Component, input, linkedSignal, output, inject } from '@angular/core';
import { AppLanguageDto, AppSettingsDto, FieldDto, FieldGroup, LocalStoreService, SchemaDto, Settings } from '@app/shared';
import { FieldComponent } from './field.component';

@Component({
    selector: 'sqx-field-group',
    styleUrls: ['./field-group.component.scss'],
    templateUrl: './field-group.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CdkDrag,
        CdkDragHandle,
        CdkDropList,
        FieldComponent,
    ],
})
export class FieldGroupComponent {
    private readonly localStore = inject(LocalStoreService);

    /** Emits when the user has moved a field within or into this group. */
    public readonly sorted = output<CdkDragDrop<FieldDto[]>>();

    /** The languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** The parent array field, if the fields are nested fields. */
    public readonly parent = input<FieldDto>();

    /** The settings of the app. */
    public readonly settings = input.required<AppSettingsDto>();

    /** Allows the user to sort the fields with drag and drop. */
    public readonly sortable = input(false, { transform: booleanAttribute });

    /** The schema the fields belong to. */
    public readonly schema = input.required<SchemaDto>();

    /** Hides the drop placeholder of an empty group, if there are no fields at all. */
    public readonly fieldsEmpty = input(false, { transform: booleanAttribute });

    /** The group to render, with the optional separator and its fields. */
    public readonly fieldGroup = input.required<FieldGroup>();

    protected readonly isCollapsed = linkedSignal(() => {
        const separator = this.fieldGroup().separator;
        if (!separator) {
            return false;
        }

        return this.localStore.getBoolean(this.isCollapsedKey(separator));
    });

    protected toggle() {
        const isCollapsed = !this.isCollapsed();

        this.isCollapsed.set(isCollapsed);

        const separator = this.fieldGroup().separator;
        if (separator) {
            this.localStore.setBoolean(this.isCollapsedKey(separator), isCollapsed);
        }
    }

    protected trackByField(field: FieldDto) {
        return field.fieldId + this.schema().id;
    }

    private isCollapsedKey(separator: FieldDto) {
        return Settings.Local.FIELD_COLLAPSED(this.schema().id, separator.fieldId);
    }
}

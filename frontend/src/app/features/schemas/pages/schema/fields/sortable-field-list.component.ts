/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDragHandle, CdkDropList, CdkDropListGroup, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';

import { booleanAttribute, Component, computed, forwardRef, input, output } from '@angular/core';
import { AppLanguageDto, AppSettingsDto, FieldDto, FieldGroup, groupFields, SchemaDto } from '@app/shared';
import { FieldGroupComponent } from './field-group.component';

@Component({
    selector: 'sqx-sortable-field-list',
    styleUrls: ['./sortable-field-list.component.scss'],
    templateUrl: './sortable-field-list.component.html',
    imports: [
        CdkDrag,
        CdkDragHandle,
        CdkDropList,
        CdkDropListGroup,
        forwardRef(() => FieldGroupComponent),
    ],
})
export class SortableFieldListComponent {
    /** Emits all fields in the new order after the user has moved a field or group. */
    public readonly sorted = output<ReadonlyArray<FieldDto>>();

    /** The languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** The parent array field, if the fields are nested fields. */
    public readonly parent = input<FieldDto>();

    /** The settings of the app. */
    public readonly settings = input.required<AppSettingsDto>();

    /** The schema the fields belong to. */
    public readonly schema = input.required<SchemaDto>();

    /** Allows the user to sort the fields with drag and drop. */
    public readonly sortable = input(false, { transform: booleanAttribute });

    /** Hides the drop placeholder of empty groups, if there are no fields at all. */
    public readonly fieldsEmpty = input(false, { transform: booleanAttribute });

    /** The fields to render, grouped by their separators. */
    public readonly fields = input.required<ReadonlyArray<FieldDto>>();

    protected readonly fieldGroups = computed(() => {
        return groupFields(this.fields(), true);
    });

    protected sortGroups(event: CdkDragDrop<FieldGroup[]>) {
        this.onSort(event);
    }

    protected sortFields(event: CdkDragDrop<FieldDto[]>) {
        this.onSort(event);
    }

    private onSort<T>(event: CdkDragDrop<T[]>) {
        if (event.previousContainer === event.container) {
            moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
        } else {
            transferArrayItem(
                event.previousContainer.data,
                event.container.data,
                event.previousIndex,
                event.currentIndex);
        }

        const result: FieldDto[] = [];

        for (const group of this.fieldGroups()) {
            if (group.separator) {
                result.push(group.separator);
            }

            for (const field of group.fields) {
                result.push(field);
            }
        }

        this.sorted.emit(result);
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDragHandle, CdkDropList, CdkDropListGroup, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';

import { booleanAttribute, Component, EventEmitter, forwardRef, Input, Output } from '@angular/core';
import { AppSettingsDto, FieldDto, FieldGroup, groupFields, LanguageDto, RootFieldDto, SchemaDto } from '@app/shared';
import { FieldGroupComponent } from './field-group.component';

@Component({
    standalone: true,
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
    @Output()
    public sorted = new EventEmitter<ReadonlyArray<FieldDto>>();

    @Input({ required: true })
    public languages!: ReadonlyArray<LanguageDto>;

    @Input()
    public parent?: RootFieldDto;

    @Input({ required: true })
    public settings!: AppSettingsDto;

    @Input()
    public schema!: SchemaDto;

    @Input({ transform: booleanAttribute })
    public sortable = false;

    @Input({ transform: booleanAttribute })
    public fieldsEmpty = false;

    @Input({ required: true })
    public set fields(value: ReadonlyArray<FieldDto>) {
        this.fieldGroups = groupFields(value, true);
    }

    public fieldGroups: FieldGroup[] = [];

    public sortGroups(event: CdkDragDrop<FieldGroup[]>) {
        this.onSort(event);
    }

    public sortFields(event: CdkDragDrop<FieldDto[]>) {
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

        for (const group of this.fieldGroups) {
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

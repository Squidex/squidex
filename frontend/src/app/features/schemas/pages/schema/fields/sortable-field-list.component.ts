/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AppSettingsDto, FieldDto, FieldGroup, groupFields, LanguageDto, RootFieldDto, SchemaDto } from '@app/shared';

@Component({
    selector: 'sqx-sortable-field-list[fields][languages][settings]',
    styleUrls: ['./sortable-field-list.component.scss'],
    templateUrl: './sortable-field-list.component.html',
})
export class SortableFieldListComponent {
    @Output()
    public sorted = new EventEmitter<ReadonlyArray<FieldDto>>();

    @Input()
    public languages!: ReadonlyArray<LanguageDto>;

    @Input()
    public parent?: RootFieldDto;

    @Input()
    public settings!: AppSettingsDto;

    @Input()
    public schema!: SchemaDto;

    @Input()
    public sortable = false;

    @Input()
    public fieldsEmpty = false;

    @Input()
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
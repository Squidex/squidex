/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

import { CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';

import { FieldDto, SchemaDetailsDto } from '@app/shared';

@Component({
    selector: 'sqx-field-list',
    styleUrls: ['./field-list.component.scss'],
    templateUrl: './field-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FieldListComponent implements OnChanges {
    @Input()
    public emptyText = '';

    @Input()
    public schema: SchemaDetailsDto;

    @Input()
    public fieldNames: ReadonlyArray<string>;

    @Output()
    public fieldNamesChange = new EventEmitter<ReadonlyArray<string>>();

    public fieldsAdded: FieldDto[];
    public fieldsNotAdded: FieldDto[];

    public ngOnChanges() {
        this.fieldsAdded = this.fieldNames.map(x => this.schema.fields.find(y => y.name === x)!);
        this.fieldsNotAdded = this.schema.fields.filter(x => this.fieldNames.indexOf(x.name) < 0);
    }

    public drop(event: CdkDragDrop<FieldDto[]>) {
        if (event.previousContainer === event.container) {
            moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
        } else {
            transferArrayItem(
                event.previousContainer.data,
                event.container.data,
                event.previousIndex,
                event.currentIndex);
        }

        const newNames = this.fieldsAdded.map(x => x.name);

        this.fieldNamesChange.emit(newNames);
    }
}
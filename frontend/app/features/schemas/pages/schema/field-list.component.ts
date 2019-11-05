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
        this.fieldsAdded = this.fieldNames.map(n => this.schema.contentFields.find(y => y.name === n)!).filter(x => !!x);
        this.fieldsNotAdded = this.schema.contentFields.filter(n => this.fieldNames.indexOf(n.name) < 0);
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
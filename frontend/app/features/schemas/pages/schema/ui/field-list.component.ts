/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { MetaFields, SchemaDto } from '@app/shared';

const META_FIELD_NAMES = Object.values(MetaFields);

@Component({
    selector: 'sqx-field-list',
    styleUrls: ['./field-list.component.scss'],
    templateUrl: './field-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FieldListComponent implements OnChanges {
    @Input()
    public emptyText = '';

    @Input()
    public schema: SchemaDto;

    @Input()
    public fieldNames: ReadonlyArray<string>;

    @Input()
    public withMetaFields = false;

    @Output()
    public fieldNamesChange = new EventEmitter<ReadonlyArray<string>>();

    public fieldsAdded: string[];
    public fieldsNotAdded: string[];

    public ngOnChanges() {
        let allFields = this.schema.contentFields.map(x => x.name);

        if (this.withMetaFields) {
            allFields = [...allFields, ...META_FIELD_NAMES];
        }

        this.fieldsAdded = this.fieldNames.filter(n => allFields.indexOf(n) >= 0);
        this.fieldsNotAdded = allFields.filter(n => this.fieldNames.indexOf(n) < 0);
    }

    public drop(event: CdkDragDrop<string[]>) {
        if (event.previousContainer === event.container) {
            moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
        } else {
            transferArrayItem(
                event.previousContainer.data,
                event.container.data,
                event.previousIndex,
                event.currentIndex);
        }

        const newNames = this.fieldsAdded;

        this.fieldNamesChange.emit(newNames);
    }
}

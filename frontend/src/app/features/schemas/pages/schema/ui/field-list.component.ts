/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { META_FIELDS, SchemaDto, TableField } from '@app/shared';

const META_FIELD_NAMES = Object.values(META_FIELDS).filter(x => x !== META_FIELDS.empty);

@Component({
    selector: 'sqx-field-list',
    styleUrls: ['./field-list.component.scss'],
    templateUrl: './field-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FieldListComponent {
    @Input()
    public emptyText = '';

    @Input({ required: true })
    public schema!: SchemaDto;

    @Input({ required: true })
    public fieldNames!: ReadonlyArray<string>;

    @Input({ transform: booleanAttribute })
    public withMetaFields = false;

    @Output()
    public fieldNamesChange = new EventEmitter<ReadonlyArray<string>>();

    public fieldsAdded!: TableField[];
    public fieldsNotAdded!: TableField[];

    public ngOnChanges() {
        let allFields = this.schema.contentFields;

        if (this.withMetaFields) {
            allFields = [...allFields, ...META_FIELD_NAMES];
        }

        this.fieldsAdded = this.fieldNames.map(x => allFields.find(z => z.name === x)).defined() as any;
        this.fieldsNotAdded = allFields.filter(x => !this.fieldNames.includes(x.name));
    }

    public drop(event: CdkDragDrop<TableField[]>) {
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

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDropList, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { FormAlertComponent, META_FIELDS, SchemaDto, TableField, TranslatePipe } from '@app/shared';

const META_FIELD_NAMES = Object.values(META_FIELDS).filter(x => x !== META_FIELDS.empty);

@Component({
    selector: 'sqx-field-list',
    styleUrls: ['./field-list.component.scss'],
    templateUrl: './field-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CdkDrag,
        CdkDropList,
        FormAlertComponent,
        TranslatePipe,
    ],
})
export class FieldListComponent {
    /** The text to show when no field has been added. */
    public readonly emptyText = input('');

    /** The schema to provide the fields. */
    public readonly schema = input.required<SchemaDto>();

    /** The names of the added fields, in the configured order. */
    public readonly fieldNames = input.required<ReadonlyArray<string>>();

    /** Also provides the meta fields like the creation date. */
    public readonly withMetaFields = input(false, { transform: booleanAttribute });

    /** Emits the new field names when fields are added, removed or reordered. */
    public readonly fieldNamesChange = output<string[]>();

    protected readonly fieldsAll = computed<ReadonlyArray<TableField>>(() => {
        const fields = this.schema().contentFields;

        if (this.withMetaFields()) {
            return [...fields, ...META_FIELD_NAMES];
        }

        return fields;
    });

    protected readonly fieldsAdded = computed<TableField[]>(() => {
        const fieldsAll = this.fieldsAll();

        return [...this.fieldNames().map(x => fieldsAll.find(z => z.name === x)).defined()];
    });

    protected readonly fieldsNotAdded = computed<TableField[]>(() => {
        const fieldNames = this.fieldNames();

        return this.fieldsAll().filter(x => !fieldNames.includes(x.name));
    });

    protected drop(event: CdkDragDrop<TableField[]>) {
        if (event.previousContainer === event.container) {
            moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
        } else {
            transferArrayItem(
                event.previousContainer.data,
                event.container.data,
                event.previousIndex,
                event.currentIndex);
        }

        const newNames = this.fieldsAdded().map(x => x.name);

        this.fieldNamesChange.emit(newNames);
    }
}

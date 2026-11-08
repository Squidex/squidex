/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDropList, moveItemInArray } from '@angular/cdk/drag-drop';

import { Component, computed, input, output } from '@angular/core';
import { TableField, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-custom-view-editor',
    styleUrls: ['./custom-view-editor.component.scss'],
    templateUrl: './custom-view-editor.component.html',
    imports: [
        CdkDrag,
        CdkDropList,
        TranslatePipe,
    ],
})
export class CustomViewEditorComponent {
    /** Emits when the user resets the fields to the default. */
    public readonly listFieldsReset = output();

    /** Emits the new fields when the user adds, removes or sorts fields. */
    public readonly listFieldsChange = output<ReadonlyArray<TableField>>();

    /** The fields that are shown in the list. */
    public readonly listFields = input.required<TableField[]>();

    /** All fields that can be shown in the list. */
    public readonly allFields = input.required<ReadonlyArray<TableField>>();

    protected readonly fieldsNotAdded = computed(() => {
        const listFields = this.listFields();

        return this.allFields().filter(lhs => !listFields.find(rhs => rhs.name === lhs.name));
    });

    protected drop(event: CdkDragDrop<TableField[], any>) {
        moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);

        this.updateListFields(event.container.data);
    }

    protected addField(field: TableField) {
        this.updateListFields([...this.listFields(), field]);
    }

    protected removeField(field: TableField) {
        this.updateListFields(this.listFields().removed(field));
    }

    private updateListFields(fields: ReadonlyArray<TableField>) {
        this.listFieldsChange.emit(fields);
    }

    protected resetDefault() {
        this.listFieldsReset.emit();
    }
}

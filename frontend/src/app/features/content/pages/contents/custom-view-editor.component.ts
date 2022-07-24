/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { TableField } from '@app/shared';

@Component({
    selector: 'sqx-custom-view-editor[allFields][listFields]',
    styleUrls: ['./custom-view-editor.component.scss'],
    templateUrl: './custom-view-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomViewEditorComponent implements OnChanges {
    @Output()
    public reset = new EventEmitter();

    @Output()
    public listFieldsChange = new EventEmitter<ReadonlyArray<TableField>>();

    @Input()
    public listFields!: TableField[];

    @Input()
    public allFields!: ReadonlyArray<TableField>;

    public fieldsNotAdded!: ReadonlyArray<TableField>;

    public ngOnChanges() {
        this.fieldsNotAdded = this.allFields.filter(lhs => !this.listFields.find(rhs => rhs.name === lhs.name));
    }

    public drop(event: CdkDragDrop<TableField[], any>) {
        moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);

        this.updateListFields(event.container.data);
    }

    public addField(field: TableField) {
        this.updateListFields([...this.listFields, field]);
    }

    public removeField(field: TableField) {
        this.updateListFields(this.listFields.removed(field));
    }

    private updateListFields(fields: ReadonlyArray<TableField>) {
        this.listFieldsChange.emit(fields);
    }

    public resetDefault() {
        this.reset.emit();
    }
}
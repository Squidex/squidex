/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDropList, moveItemInArray } from '@angular/cdk/drag-drop';

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { TableField, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-custom-view-editor',
    styleUrls: ['./custom-view-editor.component.scss'],
    templateUrl: './custom-view-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CdkDrag,
        CdkDropList,
        TranslatePipe,
    ],
})
export class CustomViewEditorComponent {
    @Output()
    public listFieldsReset = new EventEmitter();

    @Output()
    public listFieldsChange = new EventEmitter<ReadonlyArray<TableField>>();

    @Input({ required: true })
    public listFields!: TableField[];

    @Input({ required: true })
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
        this.listFieldsReset.emit();
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, Input, OnChanges, SimpleChanges } from '@angular/core';

import { TableView } from '@app/shared';

@Component({
    selector: 'sqx-custom-view-editor',
    styleUrls: ['./custom-view-editor.component.scss'],
    templateUrl: './custom-view-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CustomViewEditorComponent implements OnChanges {
    @Input()
    public table: TableView;

    @Input()
    public fieldNames: ReadonlyArray<string>;

    public fieldsNotAdded: ReadonlyArray<string>;

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['fieldNames']) {
            this.fieldsNotAdded = this.table.allFields.filter(n => this.fieldNames.indexOf(n) < 0);
        }
    }

    public random() {
        return Math.random();
    }

    public drop(event: CdkDragDrop<string[]>) {
        moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);

        this.table.updateFields(event.container.data);
    }
}
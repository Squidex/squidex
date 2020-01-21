/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';

@Component({
    selector: 'sqx-custom-view-editor',
    styleUrls: ['./custom-view-editor.component.scss'],
    templateUrl: './custom-view-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CustomViewEditorComponent implements OnChanges {
    @Input()
    public allFields: ReadonlyArray<string>;

    @Input()
    public fieldNames: ReadonlyArray<string>;

    @Output()
    public fieldNamesChange = new EventEmitter<ReadonlyArray<string>>();

    public fieldsNotAdded: ReadonlyArray<string>;

    public ngOnChanges() {
        this.fieldsNotAdded = this.allFields.filter(n => this.fieldNames.indexOf(n) < 0);
    }

    public random() {
        return Math.random();
    }

    public drop(event: CdkDragDrop<string[]>) {
        moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);

        this.updateFields(event.container.data);
    }

    public updateFields(fieldNames: ReadonlyArray<string>) {
        this.fieldNamesChange.emit(fieldNames);
    }

    public resetDefault() {
        this.updateFields([]);
    }

    public addField(field: string) {
        this.updateFields([...this.fieldNames, field]);
    }

    public removeField(field: string) {
        this.updateFields(this.fieldNames.filter(x => x !== field));
    }
}
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';

@Component({
    selector: 'sqx-custom-view-editor[allFields][fieldNames]',
    styleUrls: ['./custom-view-editor.component.scss'],
    templateUrl: './custom-view-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomViewEditorComponent implements OnChanges {
    @Output()
    public fieldNamesChange = new EventEmitter<ReadonlyArray<string>>();

    @Input()
    public fieldNames: string[];

    @Input()
    public allFields: ReadonlyArray<string>;

    public fieldsNotAdded: ReadonlyArray<string>;

    public ngOnChanges() {
        this.fieldsNotAdded = this.allFields.filter(n => this.fieldNames.indexOf(n) < 0);
    }

    public drop(event: CdkDragDrop<string[], any>) {
        moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);

        this.updateFieldNames(event.container.data);
    }

    public resetDefault() {
        this.updateFieldNames([]);
    }

    public addField(field: string) {
        this.updateFieldNames([...this.fieldNames, field]);
    }

    public removeField(field: string) {
        this.updateFieldNames(this.fieldNames.filter(x => x !== field));
    }

    private updateFieldNames(fieldNames: ReadonlyArray<string>) {
        this.fieldNamesChange.emit(fieldNames);
    }
}

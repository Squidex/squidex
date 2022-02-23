/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';

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
    public listFieldsChange = new EventEmitter<ReadonlyArray<string>>();

    @Input()
    public listFields!: string[];

    @Input()
    public allFields!: ReadonlyArray<string>;

    public fieldsNotAdded!: ReadonlyArray<string>;

    public ngOnChanges() {
        this.fieldsNotAdded = this.allFields.filter(n => this.listFields.indexOf(n) < 0);
    }

    public drop(event: CdkDragDrop<string[], any>) {
        moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);

        this.updateFieldNames(event.container.data);
    }

    public resetDefault() {
        this.reset.emit();
    }

    public addField(field: string) {
        this.updateFieldNames([...this.listFields, field]);
    }

    public removeField(field: string) {
        this.updateFieldNames(this.listFields.filter(x => x !== field));
    }

    private updateFieldNames(fieldNames: ReadonlyArray<string>) {
        this.listFieldsChange.emit(fieldNames);
    }
}

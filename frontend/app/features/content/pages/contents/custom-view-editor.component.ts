/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { take } from 'rxjs/operators';

import {
    MetaFields,
    ResourceOwner,
    SchemaDetailsDto,
    TableField,
    UIState
} from '@app/shared';

const META_FIELD_NAMES = Object.values(MetaFields);

@Component({
    selector: 'sqx-custom-view-editor',
    styleUrls: ['./custom-view-editor.component.scss'],
    templateUrl: './custom-view-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CustomViewEditorComponent extends ResourceOwner implements OnChanges {
    private allFields: string[];

    @Input()
    public schema: SchemaDetailsDto;

    @Output()
    public fieldsChange = new EventEmitter<ReadonlyArray<TableField>>();

    public fieldsAdded: string[];
    public fieldsNotAdded: string[];

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
        private readonly uiState: UIState
    ) {
        super();
    }

    public ngOnChanges() {
        this.fieldsChange.emit([]);

        this.allFields = [...this.schema.contentFields.map(x => x.name), ...META_FIELD_NAMES].sorted();

        this.unsubscribeAll();

        this.own(
            this.uiState.getUser<string[]>(`${this.schema.id}.view`, []).pipe(take(1))
                .subscribe(fieldNames => {
                    this.updateFields(fieldNames, true);

                    this.changeDetector.detectChanges();
                }));
    }

    private updateFields(fieldNames: string[], save: boolean) {
        if (fieldNames.length === 0) {
            fieldNames = this.schema.defaultListFields.map(x => x['name'] || x);
        }

        this.fieldsAdded = fieldNames.filter(n => this.allFields.indexOf(n) >= 0);
        this.fieldsNotAdded = this.allFields.filter(n => fieldNames.indexOf(n) < 0);

        if (save) {
            this.uiState.set(`${this.schema.id}.view`, this.fieldsAdded, true);
        }

        this.emitFields();
    }

    private emitFields() {
        if (this.fieldsAdded.length === 0) {
            const fields = this.schema.defaultListFields;

            this.fieldsChange.emit(fields);
        } else {
            let fields: ReadonlyArray<TableField> = this.fieldsAdded.map(n => this.schema.fields.find(f => f.name === n) || n);

            this.fieldsChange.emit(fields);
        }
    }

    public resetDefault() {
        this.updateFields([], true);
    }

    public addField(field: string) {
        this.updateFields([...this.fieldsAdded, field], true);
    }

    public removeField(field: string) {
        this.updateFields(this.fieldsAdded.filter(x => x !== field), true);
    }

    public random() {
        return Math.random();
    }

    public drop(event: CdkDragDrop<string[]>) {
        moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);

        this.updateFields(this.fieldsAdded, true);
    }
}
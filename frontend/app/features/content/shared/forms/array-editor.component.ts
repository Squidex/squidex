/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, Input, OnChanges, QueryList, SimpleChanges, ViewChildren } from '@angular/core';
import { AppLanguageDto, ArrayFieldPropertiesDto, disabled$, EditContentForm, FieldArrayForm, FieldArrayItemForm, sorted } from '@app/shared';
import { combineLatest, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ArrayItemComponent } from './array-item.component';

@Component({
    selector: 'sqx-array-editor',
    styleUrls: ['./array-editor.component.scss'],
    templateUrl: './array-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ArrayEditorComponent implements OnChanges {
    @Input()
    public form: EditContentForm;

    @Input()
    public formContext: any;

    @Input()
    public formModel: FieldArrayForm;

    @Input()
    public canUnset: boolean;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: ReadonlyArray<AppLanguageDto>;

    @ViewChildren(ArrayItemComponent)
    public children: QueryList<ArrayItemComponent>;

    public isDisabled: Observable<boolean>;

    public isFull: Observable<boolean>;

    public get field() {
        return this.formModel.field;
    }

    public get hasFields() {
        return this.field.nested.length > 0;
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['formModel']) {
            const properties = this.field.properties as ArrayFieldPropertiesDto;

            const maxItems = properties.maxItems || Number.MAX_VALUE;

            this.isDisabled = disabled$(this.formModel.form);

            this.isFull = combineLatest([
                this.isDisabled,
                this.formModel.itemChanges
            ]).pipe(map(([disabled, items]) => {
                return disabled || items.length >= maxItems;
            }));
        }
    }

    public removeItem(index: number) {
        this.formModel.removeItemAt(index);
    }

    public addItem(value?: FieldArrayItemForm) {
        this.formModel.addItem(value);
    }

    public clear() {
        this.formModel.reset();
    }

    public sort(event: CdkDragDrop<ReadonlyArray<FieldArrayItemForm>>) {
        this.formModel.sort(sorted(event));

        this.reset();
    }

    public move(item: FieldArrayItemForm, index: number) {
        this.formModel.move(index, item);

        this.reset();
    }

    public collapseAll() {
        this.children.forEach(child => {
            child.collapse();
        });
    }

    public expandAll() {
        this.children.forEach(child => {
            child.expand();
        });
    }

    private reset() {
        this.children.forEach(child => {
            child.reset();
        });
    }
}
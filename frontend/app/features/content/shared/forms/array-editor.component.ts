/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, Component, Input, OnChanges, QueryList, SimpleChanges, ViewChildren } from '@angular/core';
import { AppLanguageDto, ComponentsFieldPropertiesDto, disabled$, EditContentForm, fadeAnimation, FieldArrayForm, LocalStoreService, ModalModel, ObjectForm, SchemaDto, Settings, sorted, Types } from '@app/shared';
import { combineLatest, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ArrayItemComponent } from './array-item.component';

@Component({
    selector: 'sqx-array-editor[form][formContext][formModel][language][languages]',
    styleUrls: ['./array-editor.component.scss'],
    templateUrl: './array-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: [
        fadeAnimation,
    ],
})
export class ArrayEditorComponent implements OnChanges {
    @Input()
    public form: EditContentForm;

    @Input()
    public formContext: any;

    @Input()
    public formModel: FieldArrayForm;

    @Input()
    public canUnset?: boolean | null;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: ReadonlyArray<AppLanguageDto>;

    @ViewChildren(ArrayItemComponent)
    public children: QueryList<ArrayItemComponent>;

    public isArray = false;

    public schemasDropdown = new ModalModel();
    public schemasList: ReadonlyArray<SchemaDto>;

    public isDisabled: Observable<boolean>;
    public isCollapsedInitial = false;

    public isFull: Observable<boolean>;

    public get hasField() {
        return this.formModel.field['nested']?.length > 0;
    }

    constructor(
        private readonly localStore: LocalStoreService,
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['formModel']) {
            const maxItems = this.formModel.field.properties['maxItems'] || Number.MAX_VALUE;

            if (Types.is(this.formModel.field.properties, ComponentsFieldPropertiesDto)) {
                this.schemasList = this.formModel.field.properties.schemaIds?.map(x => this.formModel.globals.schemas[x]).filter(x => !!x) || [];
            } else {
                this.isArray = true;
            }

            this.isDisabled = disabled$(this.formModel.form);

            this.isFull = combineLatest([
                this.isDisabled,
                this.formModel.itemChanges,
            ]).pipe(map(([disabled, items]) => {
                return disabled || items.length >= maxItems;
            }));

            this.isCollapsedInitial = this.localStore.getBoolean(this.expandedKey());
        }
    }

    public removeItem(index: number) {
        this.formModel.removeItemAt(index);
    }

    public addCopy(value: ObjectForm) {
        this.formModel.addCopy(value);
    }

    public addItem() {
        this.formModel.addItem();
    }

    public addComponent(schema: SchemaDto) {
        this.formModel.addComponent(schema.id);
    }

    public clear() {
        this.formModel.reset();
    }

    public sort(event: CdkDragDrop<ReadonlyArray<ObjectForm>>) {
        this.formModel.sort(sorted(event));

        this.reset();
    }

    public move(item: ObjectForm, index: number) {
        this.formModel.move(index, item);

        this.reset();
    }

    public collapseAll() {
        this.children.forEach(child => {
            child.collapse();
        });

        this.localStore.setBoolean(this.expandedKey(), true);
    }

    public expandAll() {
        this.children.forEach(child => {
            child.expand();
        });

        this.localStore.setBoolean(this.expandedKey(), false);
    }

    private reset() {
        this.children.forEach(child => {
            child.reset();
        });
    }

    private expandedKey(): string {
        return Settings.Local.FIELD_COLLAPSED(this.form.schema?.id, this.formModel.field?.fieldId);
    }
}

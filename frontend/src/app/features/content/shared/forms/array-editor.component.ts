/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import { ChangeDetectionStrategy, Component, Input, OnChanges, OnInit, QueryList, SimpleChanges, ViewChildren } from '@angular/core';
import { combineLatest, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AppLanguageDto, ComponentsFieldPropertiesDto, disabled$, EditContentForm, FieldArrayForm, LocalStoreService, ModalModel, ObjectFormBase, SchemaDto, Settings, sorted, Types } from '@app/shared';
import { ArrayItemComponent } from './array-item.component';

@Component({
    selector: 'sqx-array-editor[form][formContext][formLevel][formModel][language][languages]',
    styleUrls: ['./array-editor.component.scss'],
    templateUrl: './array-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArrayEditorComponent implements OnChanges, OnInit {
    @Input()
    public form!: EditContentForm;

    @Input()
    public formContext!: any;

    @Input()
    public formLevel!: number;

    @Input()
    public formModel!: FieldArrayForm;

    @Input()
    public canUnset?: boolean | null;

    @Input()
    public language!: AppLanguageDto;

    @Input()
    public languages!: ReadonlyArray<AppLanguageDto>;

    @ViewChildren(ArrayItemComponent)
    public children!: QueryList<ArrayItemComponent>;

    @ViewChildren(CdkVirtualScrollViewport)
    public viewport?: QueryList<CdkVirtualScrollViewport>;

    public isArray = false;

    public schemasDropdown = new ModalModel();
    public schemasList: ReadonlyArray<SchemaDto> = [];

    public isDisabled?: Observable<boolean>;
    public isCollapsedInitial = false;

    public isFull?: Observable<boolean>;

    public get hasField() {
        return this.formModel.field['nested']?.length > 0;
    }

    constructor(
        private readonly localStore: LocalStoreService,
    ) {
    }

    public ngOnInit() {
        this.isCollapsedInitial = this.formLevel > 0;
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['formModel']) {
            const maxItems = this.formModel.field.properties['maxItems'] || Number.MAX_VALUE;

            if (Types.is(this.formModel.field.properties, ComponentsFieldPropertiesDto)) {
                this.schemasList = this.formModel.field.properties.schemaIds?.map(x => this.formModel.globals.schemas[x]).defined() || [];
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

            if (this.formLevel === 0) {
                this.isCollapsedInitial = this.localStore.getBoolean(this.expandedKey());
            }
        }
    }

    public removeItem(index: number) {
        this.formModel.removeItemAt(index);
    }

    public addCopy(value: ObjectFormBase) {
        this.formModel.addCopy(value);
    }

    public addItem() {
        this.formModel.addItem();
    }

    public addComponent(schema: SchemaDto) {
        this.formModel.addComponent(schema.id);
    }

    public clear() {
        this.formModel.setValue([]);
    }

    public sort(event: CdkDragDrop<ReadonlyArray<ObjectFormBase>>) {
        this.formModel.sort(sorted(event));

        this.reset();
    }

    public move(item: ObjectFormBase, index: number) {
        this.formModel.move(index, item);

        this.reset();
    }

    public onExpanded() {
        this.viewport?.first?.checkViewportSize();
    }

    public collapseAll() {
        this.children.forEach(child => {
            child.collapse();
        });

        if (this.formLevel === 0) {
            this.localStore.setBoolean(this.expandedKey(), true);
        }

        this.onExpanded();
    }

    public expandAll() {
        this.children.forEach(child => {
            child.expand();
        });

        if (this.formLevel === 0) {
            this.localStore.setBoolean(this.expandedKey(), false);
        }

        this.onExpanded();
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

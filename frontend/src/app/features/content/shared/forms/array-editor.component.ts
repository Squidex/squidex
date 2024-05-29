/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDragHandle, CdkDropList } from '@angular/cdk/drag-drop';
import { AsyncPipe } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, Input, numberAttribute, QueryList, ViewChildren } from '@angular/core';
import { VirtualScrollerComponent, VirtualScrollerModule } from '@iharbeck/ngx-virtual-scroller';
import { combineLatest, Observable } from 'rxjs';
import { AppLanguageDto, ComponentsFieldPropertiesDto, ConfirmClickDirective, disabled$, DropdownMenuComponent, EditContentForm, FieldArrayForm, FormHintComponent, LocalStoreService, ModalDirective, ModalModel, ModalPlacementDirective, ObjectFormBase, SchemaDto, Settings, sorted, TooltipDirective, TranslatePipe, TypedSimpleChanges, Types } from '@app/shared';
import { ArrayItemComponent } from './array-item.component';

@Component({
    standalone: true,
    selector: 'sqx-array-editor',
    styleUrls: ['./array-editor.component.scss'],
    templateUrl: './array-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ArrayItemComponent,
        AsyncPipe,
        CdkDrag,
        CdkDragHandle,
        CdkDropList,
        ConfirmClickDirective,
        DropdownMenuComponent,
        FormHintComponent,
        ModalDirective,
        ModalPlacementDirective,
        TooltipDirective,
        TranslatePipe,
        VirtualScrollerModule,
    ],
})
export class ArrayEditorComponent {
    @Input({ required: true })
    public hasChatBot!: boolean;

    @Input({ required: true })
    public form!: EditContentForm;

    @Input({ required: true })
    public formContext!: any;

    @Input({ required: true, transform: numberAttribute })
    public formLevel!: number;

    @Input({ required: true })
    public formModel!: FieldArrayForm;

    @Input({ required: true, transform: booleanAttribute })
    public isComparing = false;

    @Input({ transform: booleanAttribute })
    public isExpanded = false;

    @Input({ required: true })
    public language!: AppLanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<AppLanguageDto>;

    @ViewChildren(ArrayItemComponent)
    public children!: QueryList<ArrayItemComponent>;

    @ViewChildren(VirtualScrollerComponent)
    public scroller?: QueryList<VirtualScrollerComponent>;

    public isArray = false;

    public schemasDropdown = new ModalModel();
    public schemasList: ReadonlyArray<SchemaDto> = [];

    public isDisabledOrFull?: Observable<boolean>;
    public isDisabled?: Observable<boolean>;
    public isCollapsedInitial = false;

    public get hasField() {
        return (this.formModel.field as any)['nested']?.length > 0;
    }

    constructor(
        private readonly localStore: LocalStoreService,
    ) {
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.formModel) {
            const maxItems = (this.formModel.field.properties as any)['maxItems'] || Number.MAX_VALUE;

            if (Types.is(this.formModel.field.properties, ComponentsFieldPropertiesDto)) {
                this.schemasList = this.formModel.field.properties.schemaIds?.map(x => this.formModel.globals.schemas[x]).defined().sortedByString(x => x.displayName) || [];
            } else {
                this.isArray = true;
            }

            this.isDisabled = disabled$(this.formModel.form);
            this.isDisabledOrFull = combineLatest([
                this.isDisabled,
                this.formModel.itemChanges,
            ], (disabled, items) => {
                return disabled || items.length >= maxItems;
            });
        }

        if (changes.formModel || changes.formLevel) {
            this.isCollapsedInitial = this.localStore.getBoolean(this.isCollapsedKey()) || this.formLevel > 0;
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

    public collapseAll() {
        for (const item of this.formModel.items) {
            item.collapse();
        }

        if (this.formLevel === 0) {
            this.localStore.setBoolean(this.isCollapsedKey(), true);
        }

        this.scroller?.first?.invalidateAllCachedMeasurements();
    }

    public expandAll() {
        for (const item of this.formModel.items) {
            item.expand();
        }

        if (this.formLevel === 0) {
            this.localStore.setBoolean(this.isCollapsedKey(), false);
        }

        this.scroller?.first?.invalidateAllCachedMeasurements();
    }

    private reset() {
        this.children.forEach(child => {
            child.reset();
        });
    }

    private isCollapsedKey(): string {
        return Settings.Local.FIELD_COLLAPSED(this.form.schema?.id, this.formModel.field?.fieldId);
    }
}

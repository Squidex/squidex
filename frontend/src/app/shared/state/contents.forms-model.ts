/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed, signal, untracked } from '@angular/core';
import { FieldTree } from '@angular/forms/signals';
import { Types } from '@app/framework';
import { AppLanguageDto, ComponentFieldPropertiesDto, FieldDto, fieldInvariant, LanguageDto } from '../model';
import { AbstractContentForm, AnyFieldDto, ControlArgs, FieldSection, fieldTranslationStatus, groupFields } from './contents.forms-helpers';

export class FieldForm extends AbstractContentForm<FieldDto> {
    private readonly partitions: { [partition: string]: FieldItemForm } = {};
    private readonly initialValue = signal<{ value: any } | null>(null);
    private cachedInitial: any;
    private cachedHash?: string | null;

    public readonly translationStatus = computed(() => {
        return fieldTranslationStatus(this.value() || {});
    });

    public readonly hasChanges = computed(() => {
        const initial = this.initialValue();
        if (!initial) {
            return false;
        }

        return !Types.equals(initial.value, this.value());
    });

    public get initialHash() {
        return (this.cachedHash ??= Types.hashWithId(this.cachedInitial, this.field.name));
    }

    public override readonly disabled = computed(() => {
        return Object.values(this.partitions).every(x => x.disabled());
    });

    public override readonly hidden = computed(() => {
        return Object.values(this.partitions).every(x => x.hidden());
    });

    constructor(args: ControlArgs<FieldDto>, form: FieldTree<any>) {
        super(args, form);

        for (const { key: partition, isOptional } of args.globals.partitions.getAll(args.field)) {
            this.partitions[partition] = buildForm({
                ...args,
                isOptional,
                partition,
                path: this.relativePath(partition),
            }, (form as any)[partition]);
        }
    }

    public get(language: string | LanguageDto | AppLanguageDto) {
        if (this.field.isLocalizable) {
            return this.partitions[(language as any)['iso2Code'] || language];
        } else {
            return this.partitions[fieldInvariant];
        }
    }

    public updateInitial(data: any) {
        this.cachedInitial = data;
        this.cachedHash = null;
        this.initialValue.set({ value: data });
    }
}

export class FieldValueForm extends AbstractContentForm<AnyFieldDto> {
    constructor(args: ControlArgs, form: FieldTree<any>) {
        super(args, form);
    }
}

export class FieldArrayForm extends AbstractContentForm<AnyFieldDto> {
    private readonly itemForms = new WeakMap<object, ObjectFormBase>();

    public readonly items = computed<ReadonlyArray<ObjectFormBase>>(() => {
        const value = this.form().value();
        if (!Types.isArray(value)) {
            return [];
        }

        return untracked(() => value.map((_, index) => this.getItemForm((this.form as any)[index])));
    }, { equal: sameItems });

    constructor(args: ControlArgs, form: FieldTree<any>,
        public readonly isComponents: boolean,
    ) {
        super(args, form);
    }

    public get(index: number) {
        return this.items()[index];
    }

    public addCopy(source: ObjectFormBase) {
        this.add(Types.clone(source.value()));
    }

    public addComponent(schemaId: string) {
        this.add(this.globals.values.createComponent(schemaId, this.partition));
    }

    public addItem() {
        this.add(this.globals.values.createItem(this.field, this.partition));
    }

    public removeItemAt(index: number) {
        this.formValue.update(items => items.filter((_: any, i: number) => i !== index));
    }

    public move(index: number, item: ObjectFormBase) {
        const children = [...this.items()];

        children.splice(children.indexOf(item), 1);
        children.splice(index, 0, item);

        this.sort(children);
    }

    public sort(children: ReadonlyArray<ObjectFormBase>) {
        this.formValue.set(children.map(x => x.value()));
    }

    private add(item: any) {
        this.formValue.update(items => [...items || [], item]);
    }

    private getItemForm(form: FieldTree<any>) {
        // The field tree tracks the identity of items, so the forms can be cached per field.
        let result = this.itemForms.get(form);
        if (!result) {
            result = this.isComponents ?
                new ComponentForm(this.args, form) :
                new ArrayItemForm(this.args, form);

            this.itemForms.set(form, result);
        }

        return result;
    }
}

export type FieldItemForm = ComponentForm | FieldValueForm | FieldArrayForm;

type FieldMap = { [name: string]: FieldItemForm };

export abstract class ObjectFormBase extends AbstractContentForm<AnyFieldDto> {
    public readonly fields = computed<FieldMap>(() => {
        const fields = this.getFields();

        return untracked(() => this.buildFields(fields));
    });

    public readonly sections = computed(() => {
        const fields = this.fields();

        return groupFields(this.getFields()).map(({ separator, fields: group }) => {
            return new FieldSection<AnyFieldDto, FieldItemForm>(separator, group.map(x => fields[x.name]));
        });
    });

    public get(field: string | { name: string }): FieldItemForm | undefined {
        return this.fields()[(field as any)['name'] || field];
    }

    protected abstract getFields(): ReadonlyArray<AnyFieldDto>;

    private buildFields(fields: ReadonlyArray<AnyFieldDto>) {
        const result: FieldMap = {};

        for (const field of fields) {
            if (!field.properties.isContentField) {
                continue;
            }

            result[field.name] = buildForm({
                ...this.args,
                field,
                path: this.relativePath(field.name),
            }, (this.form as any)[field.name]);
        }

        return result;
    }
}

function sameItems(lhs: ReadonlyArray<unknown>, rhs: ReadonlyArray<unknown>) {
    return lhs.length === rhs.length && lhs.every((x, i) => x === rhs[i]);
}

export class ArrayItemForm extends ObjectFormBase {
    protected getFields() {
        if (Types.is(this.field, FieldDto)) {
            return this.field.nested || [];
        } else {
            return [];
        }
    }
}

export class ComponentForm extends ObjectFormBase {
    public readonly schema = computed(() => {
        return this.globals.schemas[this.value()?.schemaId];
    });

    public get properties() {
        return this.field.properties as ComponentFieldPropertiesDto;
    }

    public selectSchema(schemaId: string) {
        this.formValue.set(this.globals.values.createComponent(schemaId, this.partition));
    }

    protected getFields() {
        return this.schema()?.fields || [];
    }
}

export function buildForm(args: ControlArgs, form: FieldTree<any>): FieldItemForm {
    switch (args.field.properties.fieldType) {
        case 'Array':
            return new FieldArrayForm(args, form, false);
        case 'Component':
            return new ComponentForm(args, form);
        case 'Components':
            return new FieldArrayForm(args, form, true);
        default:
            return new FieldValueForm(args, form);
    }
}

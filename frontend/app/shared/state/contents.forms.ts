/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: max-line-length
// tslint:disable: prefer-for-of
// tslint:disable: readonly-array

import { AbstractControl, FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Form, StringFormControl as FormControlForString, Types, value$, getRawValue } from '@app/framework';
import { BehaviorSubject } from 'rxjs';
import { AppLanguageDto } from './../services/app-languages.service';
import { FieldDto, RootFieldDto, SchemaDetailsDto, TableField } from './../services/schemas.service';
import { fieldInvariant } from './../services/schemas.types';
import { FieldDefaultValue, FieldsValidators } from './contents.forms.visitors';

type SaveQueryFormType = { name: string, user: boolean };

export class SaveQueryForm extends Form<FormGroup, SaveQueryFormType> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.required
                ]
            ],
            user: false
        }));
    }
}

const NO_EMIT = { emitEvent: false };
const NO_EMIT_SELF = { emitEvent: false, onlySelf: true };

type Partition = { key: string, isOptional: boolean };

export class PartitionConfig {
    private readonly invariant: ReadonlyArray<Partition> = [{ key: fieldInvariant, isOptional: false }];
    private readonly languages: ReadonlyArray<Partition>;

    constructor(languages: ReadonlyArray<AppLanguageDto>) {
        this.languages = languages.map(l => this.get(l));
    }

    public get(language?: AppLanguageDto) {
        if (!language) {
            return this.invariant[0];
        }

        return { key: language.iso2Code, isOptional: language.isOptional };
    }

    public getAll(field: RootFieldDto) {
        return field.isLocalizable ? this.languages : this.invariant;
    }
}

type ConditionType = 'Disable' | 'Hide';
type Condition = { field: string, type: ConditionType, expression: string };

class CompiledCondition {
    private function: Function;

    public get field() {
        return this.condition.field;
    }

    public get type() {
        return this.condition.type;
    }

    constructor(
        private readonly condition: Condition
    ) {
        this.function = new Function(`return function(data, itemData, user) { return ${condition.expression} }`)();
    }

    public eval(data: any, itemData: any, user: any) {
        try {
            return this.function(data, itemData, user);
        } catch {
            return false;
        }
    }
}

export class EditContentForm extends Form<FormGroup, any> {
    private readonly partitions: PartitionConfig;
    private readonly conditions: ReadonlyArray<CompiledCondition>;
    private readonly fields: FieldControl<RootFieldDto>[];
    private initialData: any;

    public value = new BehaviorSubject<any>(this.form.value);

    constructor(languages: ReadonlyArray<AppLanguageDto>, schema: SchemaDetailsDto, conditions: Condition[] = []) {
        super(new FormGroup({}, {
            updateOn: 'blur'
        }));

        this.conditions = conditions.map(x => new CompiledCondition(x));

        value$(this.form).subscribe(value => {
            this.value.next(value);

            this.updateVisibility();
        });

        this.partitions = new PartitionConfig(languages);

        for (const field of schema.fields) {
            if (field.properties.isContentField) {
                const fieldForm = new FieldControl(field, new FormGroup({}), this.conditions.filter(x => x.field === field.name));
                const fieldDefault = FieldDefaultValue.get(field);

                for (const { key, isOptional } of this.partitions.getAll(field)) {
                    const fieldValidators = FieldsValidators.create(field, isOptional);

                    const control =
                        field.isArray ?
                            new FormArray([], fieldValidators) :
                            new FormControlForString(fieldDefault, fieldValidators);

                    fieldForm.add(new FieldControl(field, control), key);
                }

                this.form.setControl(field.name, fieldForm.form);
            }
        }

        this.updateInitialData();
        this.updateVisibility();
        this.enable();
    }

    public hasChanged() {
        const currentValue = this.form.getRawValue();

        return !Types.equals(this.initialData, currentValue, true);
    }

    public hasChanges(changes: any) {
        const currentValue = this.form.getRawValue();

        return !Types.equals(changes, currentValue, true);
    }

    public arrayItemRemove(field: RootFieldDto, language: AppLanguageDto, index: number) {
        const partitionForm = this.findArrayItemForm(field, language);

        if (partitionForm) {
            this.removeItem(partitionForm, index);
        }
    }

    public arrayItemInsert(field: RootFieldDto, language: AppLanguageDto, source?: FormGroup) {
        const partitionForm = this.findArrayItemForm(field, language);

        if (partitionForm && field.nested.length > 0) {
            this.addArrayItem(partitionForm, this.partitions.get(language), source);
        }
    }

    private removeItem(partitionForm: FieldControl, index: number) {
        partitionForm.removeAt(index);
    }

    private addArrayItem(partitionForm: FieldControl<RootFieldDto>, partition: Partition, source?: FormGroup) {
        const itemForm = new FieldControl(partitionForm.field, new FormGroup({}));

        for (const nestedField of partitionForm.field.nested) {
            if (nestedField.properties.isContentField) {
                let value = FieldDefaultValue.get(nestedField);

                if (source) {
                    const sourceField = source.get(nestedField.name);

                    if (sourceField) {
                        value = sourceField.value;
                    }
                }

                const nestedValidators = FieldsValidators.create(nestedField, partition.isOptional);
                const nestedForm = new FormControlForString(value, nestedValidators);
                const nestedFieldControl = new FieldControl(nestedField, nestedForm,  this.conditions.filter(x => x.field === nestedField.name));

                itemForm.add(nestedFieldControl, nestedField.name);
            }
        }

        itemForm.enable();

        partitionForm.add(itemForm, '_');
    }

    private findArrayItemForm(field: RootFieldDto, language: AppLanguageDto): FieldControl<RootFieldDto> | undefined {
        const fieldForm = this.fields.find(x => x.field.name === field.name);

        if (!fieldForm) {
            return undefined;
        } else if (field.isLocalizable) {
            return fieldForm.get(language.iso2Code) as any;
        } else {
            return fieldForm.get(fieldInvariant) as any;
        }
    }

    public load(value: any, isInitial?: boolean) {
        for (const control of this.fields) {
            const { field } = control;

            if (control.isArray && field.nested.length > 0) {
                const fieldValue = value?.[field.name] || {};

                for (const partition of this.partitions.getAll(field)) {
                    const { key, isOptional } = partition;

                    const partitionValidators = FieldsValidators.create(field, isOptional);
                    const partitionForm = new FieldControl(field, new FormArray([], partitionValidators));

                    const partitionValue = fieldValue[key];

                    if (Types.isArray(partitionValue)) {
                        for (let i = 0; i < partitionValue.length; i++) {
                            this.addArrayItem(partitionForm, partition);
                        }
                    }
                }
            }
        }

        super.load(value);

        if (isInitial) {
            this.updateInitialData();
        }
    }

    public submitCompleted(options?: { newValue?: any, noReset?: boolean }) {
        super.submitCompleted(options);

        this.updateInitialData();
    }

    protected disable() {
        this.form.disable(NO_EMIT);
    }

    protected enable() {
        this.form.enable(NO_EMIT_SELF);

        const data = this.form.getRawValue();

        for (const field of this.fields) {
            field.enable(data);
        }
    }

    protected updateVisibility() {
        for (const field of this.fields) {
            field.updateVisibility();
        }
    }

    private updateInitialData() {
        this.initialData = this.form.getRawValue();
    }
}

export type FormSection<T> = {
    separator?: T;

    fields: ReadonlyArray<T>;
};

export class FieldControl<T extends FieldDto = FieldDto> {
    private readonly hidden$ = new BehaviorSubject<boolean>(false);
    private readonly childrenControls: { name: string, control: FieldControl }[] = [];

    public get hidden() {
        return this.hidden$.value;
    }

    public get children(): ReadonlyArray<FieldControl> {
        return this.children;
    }

    public get isArray() {
        return Types.is(this.field, RootFieldDto) && this.field.isArray;
    }

    public get(name: string) {
        return this.childrenControls.find(x => x.name === name)?.control;
    }

    constructor(
        public readonly field: T,
        public readonly form: AbstractControl,
        private readonly conditions?: CompiledCondition[]
    ) {
    }

    public add(control: FieldControl, name: string) {
        if (Types.is(this.form, FormArray)) {
            this.form.push(control.form);
        } else if (Types.is(this.form, FormGroup)) {
            this.form.setControl(name, control.form);
        }

        this.childrenControls.push({ name, control });
    }

    public removeAt(index: number) {
        if (Types.is(this.form, FormArray)) {
            this.form.removeAt(index);
        }
    }

    public updateVisibility(fromParent = false) {
        if (this.conditions && this.conditions.length > 0) {
            let evaluated = undefined;

            for (const condition of this.conditions) {
                if (condition.type === 'Hide' && (evaluated = condition.eval(this, this))) {
                    break;
                }
            }

            let hidden = fromParent;

            if (Types.isBoolean(evaluated)) {
                hidden = evaluated;
            }

            if (this.hidden !== hidden) {
                this.hidden$.next(true);
            }

            if (!hidden) {
                for (const { control } of this.childrenControls) {
                    control.updateVisibility(fromParent);
                }
            }
        }
    }

    public enable(data: any, user?: any, itemData?: any) {
        if (this.conditions && this.conditions.length > 0) {
            let evaluated = false;

            for (const condition of this.conditions) {
                if (condition.type === 'Disable' && (evaluated = condition.eval(data, itemData, user))) {
                    break;
                }
            }

            if (evaluated) {
                this.form.disable(NO_EMIT);
                return;
            }
        }

        if (this.isArray) {
            for (const partitionForm of this.childrenControls) {
                partitionForm.control.form.enable(NO_EMIT_SELF);

                for (const itemForm of partitionForm.control.childrenControls) {
                    itemForm.control.form.enable(NO_EMIT_SELF);

                    const currentItemData = getRawValue(itemForm.control.form);

                    for (const nestedField of itemForm.control.childrenControls) {
                        nestedField.control.enable(user, itemData);
                    }
                }
            }
        } else {
            if (!this.field.isDisabled) {
                this.form.enable(NO_EMIT);
            } else {
                this.form.disable(NO_EMIT);
            }
        }
    }
}

export class PatchContentForm extends Form<FormGroup, any> {
    private readonly editableFields: ReadonlyArray<RootFieldDto>;

    constructor(
        private readonly listFields: ReadonlyArray<TableField>,
        private readonly language: AppLanguageDto
    ) {
        super(new FormGroup({}));

        this.editableFields = <any>this.listFields.filter(x => Types.is(x, RootFieldDto) && x.isInlineEditable);

        for (const field of this.editableFields) {
            const validators = FieldsValidators.create(field, this.language.isOptional);

            this.form.setControl(field.name, new FormControlForString(undefined, validators));
        }
    }

    public submit() {
        const result = super.submit();

        if (result) {
            const request = {};

            for (const field of this.editableFields) {
                const value = result[field.name];

                if (field.isLocalizable) {
                    request[field.name] = { [this.language.iso2Code]: value };
                } else {
                    request[field.name] = { iv: value };
                }
            }

            return request;
        }

        return result;
    }
}
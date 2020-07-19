/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: prefer-for-of
// tslint:disable: readonly-array

import { AbstractControl, FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Form, StringFormControl as FormControlForString, Types, valueAll$ } from '@app/framework';
import { BehaviorSubject, Observable } from 'rxjs';
import { AppLanguageDto } from './../services/app-languages.service';
import { FieldDto, NestedFieldDto, RootFieldDto, SchemaDetailsDto, TableField } from './../services/schemas.service';
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
    private readonly fields: { [name: string]: FieldForm } = {};
    private initialData: any;

    public readonly sections: ReadonlyArray<FieldSection<RootFieldDto, FieldForm>>;

    public readonly value = new BehaviorSubject<any>(this.form.value);

    constructor(languages: ReadonlyArray<AppLanguageDto>, schema: SchemaDetailsDto, conditions: Condition[] = []) {
        super(new FormGroup({}, {
            updateOn: 'blur'
        }));

        conditions.push({
            field: 'text1', type: 'Hide', expression: 'data.value.iv > 100'
        });
        conditions.push({
            field: 'text2', type: 'Disable', expression: 'data.value.iv > 100'
        });
        conditions.push({
            field: 'nested.text3', type: 'Hide', expression: 'data.value.iv > 100'
        });

        const compiledPartitions = new PartitionConfig(languages);
        const compiledConditions = conditions.map(x => new CompiledCondition(x));

        const sections: FieldSection<RootFieldDto, FieldForm>[] = [];

        let currentSeparator: RootFieldDto | undefined = undefined;
        let currentFields: FieldForm[] = [];

        for (const field of schema.fields) {
            if (field.properties.isContentField) {
                const child = new FieldForm(field, compiledPartitions, compiledConditions);

                currentFields.push(child);

                this.form.setControl(field.name, child.form);

                this.fields[field.name] = child;
            } else {
                sections.push(new FieldSection<RootFieldDto, FieldForm>(currentSeparator, currentFields));

                currentFields = [];
                currentSeparator = field;
            }
        }

        if (currentFields.length > 0) {
            sections.push(new FieldSection<RootFieldDto, FieldForm>(currentSeparator, currentFields));
        }

        this.sections = sections;

        valueAll$(this.form).subscribe(value => {
            this.value.next(value);

            this.updateHidden(value);
            this.updateEnabled(value);
        });

        this.updateInitialData();
    }

    public getFieldForm(name: string): FieldForm | undefined {
        return this.fields[name];
    }

    public hasChanged() {
        const currentValue = this.form.getRawValue();

        return !Types.equals(this.initialData, currentValue, true);
    }

    public hasChanges(changes: any) {
        const currentValue = this.form.getRawValue();

        return !Types.equals(changes, currentValue, true);
    }

    public load(value: any, isInitial?: boolean) {
        for (const section of this.sections) {
            for (const child of section.fields) {
                child.prepareLoad(value[child.field.name]);
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

        this.updateEnabled(this.form.getRawValue());
    }

    private updateEnabled(data: any) {
        for (const section of this.sections) {
            for (const child of section.fields) {
                child.updateEnabled(data, data);
            }
        }
    }

    private updateHidden(data: any) {
        for (const section of this.sections) {
            section.updateHidden(data, data);
        }
    }

    private updateInitialData() {
        this.initialData = this.form.getRawValue();
    }
}

abstract class Hidden {
    private readonly hidden$ = new BehaviorSubject<boolean>(false);

    public get hidden() {
        return this.hidden$.value;
    }

    public get hiddenChanges(): Observable<boolean> {
        return this.hidden$;
    }

    protected setHidden(hidden: boolean) {
        if (hidden !== this.hidden) {
            this.hidden$.next(hidden);
        }
    }

    public abstract updateHidden(user: any, data: any, itemData?: any): void;
}

export abstract class AbstractContentForm<T extends FieldDto, TForm extends AbstractControl> extends Hidden {
    constructor(
        public readonly field: T,
        public readonly form: TForm,
        private readonly rules?: CompiledCondition[]
    ) {
        super();
    }

    public updateEnabled(user: any, data: any, itemData?: any) {
        let disabled = this.field.isDisabled;

        if (this.rules) {
            for (const rule of this.rules) {
                if (rule.type === 'Disable' && rule.eval(data, itemData, user)) {
                    disabled = true;
                    break;
                }
            }
        }

        if (disabled !== this.form.disabled) {
            if (disabled) {
                this.form.disable(NO_EMIT);
                return;
            } else {
                this.form.enable(NO_EMIT_SELF);
            }
        }

        this.updateChildEnabled(user, data, itemData);
    }

    public updateHidden(user: any, data: any, itemData?: any) {
        let hidden = this.field.isHidden;

        if (this.rules) {
            for (const rule of this.rules) {
                if (rule.type === 'Hide' && rule.eval(data, itemData, user)) {
                    hidden = true;
                    break;
                }
            }
        }

        if (hidden !== this.hidden) {
            if (hidden) {
                this.setHidden(true);
                return;
            } else {
                this.setHidden(false);
            }
        }

        this.updateChildHidden(user, data, itemData);

        return !hidden;
    }

    protected updateChildEnabled(user: any, data: any, itemData: any) {
        return;
    }

    protected updateChildHidden(user: any, data: any, itemData: any) {
        return;
    }

    public prepareLoad(value: any) {
        return;
    }
}

export class FieldForm extends AbstractContentForm<RootFieldDto, FormGroup> {
    private readonly childMap: { [partition: string]: (FieldValueForm | FieldArrayForm) } = {};

    constructor(field: RootFieldDto, partitions: PartitionConfig, rules: CompiledCondition[]
    ) {
        super(field, new FormGroup({}), FieldForm.buildRules(field, rules));

        for (const { key, isOptional } of partitions.getAll(field)) {
            const child =
                field.isArray ?
                    new FieldArrayForm(field, isOptional, rules) :
                    new FieldValueForm(field, isOptional);

            this.childMap[key] = child;

            this.form.setControl(key, child.form);
        }
    }

    public copyFrom(source: FieldForm, key: string) {
        this.getField(key)?.form.setValue(source.getField(key)?.form.value);
    }

    public copyAllFrom(source: FieldForm) {
        this.form.setValue(source.form.getRawValue());
    }

    public getField(language: string) {
        if (this.field.isLocalizable) {
            return this.childMap[language];
        } else {
            return this.childMap[fieldInvariant];
        }
    }

    protected updateChildEnabled(user: any, data: any) {
        for (const child of Object.values(this.childMap)) {
            child.updateEnabled(user, data);
        }
    }

    public updateChildHidden(user: any, data: any) {
        for (const child of Object.values(this.childMap)) {
            child.updateHidden(user, data);
        }
    }

    public prepareLoad(value: any) {
        if (Types.isObject(value)) {
            for (const key in this.childMap) {
                if (this.childMap.hasOwnProperty(key)) {
                    const child = this.childMap[key];

                    child.prepareLoad(value[key]);
                }
            }
        }
    }

    private static buildRules(field: RootFieldDto, rules: CompiledCondition[]) {
        return rules.filter(x => x.field === field.name);
    }
}

export class FieldValueForm extends AbstractContentForm<RootFieldDto, FormControlForString> {
    constructor(field: RootFieldDto, isOptional: boolean
    ) {
        super(field, FieldValueForm.buildControl(field, isOptional));
    }

    private static buildControl(field: RootFieldDto, isOptional: boolean) {
        const value = FieldDefaultValue.get(field);

        const validators = FieldsValidators.create(field, isOptional);

        return new FormControlForString(value, validators);
    }
}

export class FieldArrayForm extends AbstractContentForm<RootFieldDto, FormArray> {
    public children: FieldArrayItemForm[] = [];

    constructor(field: RootFieldDto,
        private readonly isOptional: boolean,
        private readonly allRules: CompiledCondition[]
    ) {
        super(field, FieldArrayForm.buildControl(field, isOptional));
    }

    public add(source?: FieldArrayItemForm) {
        const child = new FieldArrayItemForm(this.field, this.isOptional, this.allRules, source);

        this.children.push(child);

        this.form.push(child.form);
    }

    public removeAt(index: number) {
        this.children.splice(index, 1);

        this.form.removeAt(index);
    }

    public move(index: number, item: FieldArrayItemForm) {
        const children = [...this.children];

        children.splice(children.indexOf(item), 1);
        children.splice(index, 0, item);

        this.children = children;

        this.sort(children);
    }

    public sort(children: ReadonlyArray<FieldArrayItemForm>) {
        for (let i = 0; i < children.length; i++) {
            this.form.setControl(i, children[i].form);
        }
    }

    protected updateChildEnabled(user: any, data: any) {
        for (const child of this.children) {
            child.updateEnabled(user, data);
        }
    }

    public updateChildHidden(user: any, data: any) {
        for (const child of this.children) {
            child.updateHidden(user, data);
        }
    }

    public prepareLoad(value: any) {
        if (Types.isArray(value)) {
            while (this.children.length < value.length) {
                this.add();
            }

            while (this.children.length > value.length) {
                this.removeAt(this.children.length - 1);
            }
        }
    }

    private static buildControl(field: RootFieldDto, isOptional: boolean) {
        const validators = FieldsValidators.create(field, isOptional);

        return new FormArray([], validators);
    }
}

export class FieldArrayItemForm extends AbstractContentForm<RootFieldDto, FormGroup>  {
    public readonly sections: ReadonlyArray<FieldSection<NestedFieldDto, FieldArrayItemValueForm>>;

    constructor(field: RootFieldDto, isOptional: boolean, allRules: CompiledCondition[], source?: FieldArrayItemForm
    ) {
        super(field, new FormGroup({}));

        const sections: FieldSection<NestedFieldDto, FieldArrayItemValueForm>[] = [];

        let currentSeparator: NestedFieldDto | undefined = undefined;
        let currentFields: FieldArrayItemValueForm[] = [];

        for (const nestedField of field.nested) {
            if (nestedField.properties.isContentField) {
                const child = new FieldArrayItemValueForm(nestedField, field, isOptional, allRules, source);

                currentFields.push(child);

                this.form.setControl(nestedField.name, child.form);
            } else {
                sections.push(new FieldSection<NestedFieldDto, FieldArrayItemValueForm>(currentSeparator, currentFields));

                currentFields = [];
                currentSeparator = nestedField;
            }
        }

        if (currentFields.length > 0) {
            sections.push(new FieldSection<NestedFieldDto, FieldArrayItemValueForm>(currentSeparator, currentFields));
        }

        this.sections = sections;
    }

    public updateChildHidden(user: any, data: any) {
        const itemData = this.form.getRawValue();

        for (const section of this.sections) {
            section.updateHidden(user, data, itemData);
        }
    }

    protected updateChildEnabled(user: any, data: any) {
        const itemData = this.form.getRawValue();

        for (const section of this.sections) {
            for (const child of section.fields) {
                child.updateEnabled(user, data, itemData);
            }
        }
    }
}

export class FieldArrayItemValueForm extends AbstractContentForm<NestedFieldDto, FormControlForString> {
    constructor(field: NestedFieldDto, parent: RootFieldDto, isOptional: boolean, rules: CompiledCondition[], source?: FieldArrayItemForm
    ) {
        super(field,
            FieldArrayItemValueForm.buildControl(field, isOptional, source),
            FieldArrayItemValueForm.buildRules(field, parent, rules)
        );
    }

    private static buildRules(field: NestedFieldDto, parent: RootFieldDto, rules: CompiledCondition[]) {
        const fullName = `${parent.name}.${field.name}`;

        return rules.filter(x => x.field === fullName);
    }

    private static buildControl(field: NestedFieldDto, isOptional: boolean, source?: FieldArrayItemForm) {
        let value = FieldDefaultValue.get(field);

        if (source) {
            const sourceField = source.form.get(field.name);

            if (sourceField) {
                value = sourceField.value;
            }
        }

        const validators = FieldsValidators.create(field, isOptional);

        return new FormControlForString(value, validators);
    }
}

export class FieldSection<TSeparator, TChild extends Hidden> extends Hidden {
    constructor(
        public readonly separator: TSeparator | undefined,
        public readonly fields: ReadonlyArray<TChild>
    ) {
        super();
    }

    public updateHidden(user: any, data: any, itemData?: any) {
        let visible = false;

        for (const child of this.fields) {
            child.updateHidden(user, data, itemData);

            visible = visible || !child.hidden;
        }

        this.setHidden(!visible);
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
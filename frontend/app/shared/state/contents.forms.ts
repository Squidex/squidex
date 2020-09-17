/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

import { AbstractControl, FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Form, Types, valueAll$ } from '@app/framework';
import { BehaviorSubject } from 'rxjs';
import { AppLanguageDto } from './../services/app-languages.service';
import { LanguageDto } from './../services/languages.service';
import { FieldDto, NestedFieldDto, RootFieldDto, SchemaDetailsDto, TableField } from './../services/schemas.service';
import { fieldInvariant } from './../services/schemas.types';
import { CompiledRule, FieldSection, Hidden, PartitionConfig } from './contents.forms-helpers';
import { FieldDefaultValue, FieldsValidators } from './contents.forms.visitors';

export { FieldSection } from './contents.forms-helpers';

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

            this.form.setControl(field.name, new FormControl(undefined, { validators }));
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

export class EditContentForm extends Form<FormGroup, any> {
    private readonly fields: { [name: string]: FieldForm } = {};
    private initialData: any;

    public readonly sections: ReadonlyArray<FieldSection<RootFieldDto, FieldForm>>;

    public readonly value = new BehaviorSubject<any>(this.form.value);

    constructor(languages: ReadonlyArray<AppLanguageDto>, schema: SchemaDetailsDto,
        private readonly user: any = {}
    ) {
        super(new FormGroup({}));

        const compiledPartitions = new PartitionConfig(languages);
        const compiledConditions = schema.fieldRules.map(x => new CompiledRule(x));

        const sections: FieldSection<RootFieldDto, FieldForm>[] = [];

        let currentSeparator: RootFieldDto | undefined = undefined;
        let currentFields: FieldForm[] = [];

        for (const field of schema.fields) {
            if (field.properties.isContentField) {
                const child = new FieldForm(field, compiledPartitions, compiledConditions);

                currentFields.push(child);

                this.fields[field.name] = child;

                this.form.setControl(field.name, child.form);
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

            this.updateState(value);
        });

        this.updateInitialData();
    }

    public get(field: string | RootFieldDto): FieldForm | undefined {
        if (Types.is(field, RootFieldDto)) {
            return this.fields[field.name];
        } else {
            return this.fields[field];
        }
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

        this.updateState(this.form.getRawValue());
    }

    private updateState(data: any) {
        for (const field of Object.values(this.fields)) {
            field.updateState(this.user, data);
        }

        for (const section of this.sections) {
            section.updateHidden();
        }
    }

    private updateInitialData() {
        this.initialData = this.form.getRawValue();
    }
}

export abstract class AbstractContentForm<T extends FieldDto, TForm extends AbstractControl> extends Hidden {
    constructor(
        public readonly field: T,
        public readonly form: TForm,
        public readonly isOptional: boolean,
        private readonly rules?: CompiledRule[]
    ) {
        super();
    }

    public updateState(user: any, data: any, itemData?: any) {
        const state = {
            isDisabled: this.field.isDisabled,
            isHidden: false,
            isRequired: this.field.properties.isRequired && !this.isOptional
        };

        if (this.rules) {
            for (const rule of this.rules) {
                if (rule.eval(user, data, itemData)) {
                    if (rule.action === 'Disable') {
                        state.isDisabled = true;
                    } else if (rule.action === 'Hide') {
                        state.isHidden = true;
                    } else {
                        state.isRequired = true;
                    }
                }
            }
        }

        this.setHidden(state.isHidden);

        if (state.isDisabled !== this.form.disabled) {
            if (state.isDisabled) {
                this.form.disable(NO_EMIT);
            } else {
                this.form.enable(NO_EMIT_SELF);
            }
        }

        this.updateCustomState(state, user, data, itemData);
    }

    protected updateCustomState(_state: State, _user: any, _data: any, _itemData: any) {
        return;
    }

    public prepareLoad(_data: any) {
        return;
    }
}

export class FieldForm extends AbstractContentForm<RootFieldDto, FormGroup> {
    private readonly partitions: { [partition: string]: (FieldValueForm | FieldArrayForm) } = {};
    private isRequired: boolean;

    constructor(field: RootFieldDto, partitions: PartitionConfig, rules: CompiledRule[]
    ) {
        super(field, new FormGroup({}), false, FieldForm.buildRules(field, rules));

        for (const { key, isOptional } of partitions.getAll(field)) {
            const child =
                field.isArray ?
                    new FieldArrayForm(field, isOptional, rules) :
                    new FieldValueForm(field, isOptional);

            this.partitions[key] = child;

            this.form.setControl(key, child.form);
        }

        this.isRequired = field.properties.isRequired;
    }

    public copyFrom(source: FieldForm, key: string) {
        this.get(key)?.form.setValue(source.get(key)?.form.value);
    }

    public copyAllFrom(source: FieldForm) {
        this.form.setValue(source.form.getRawValue());
    }

    public get(language: string | LanguageDto) {
        if (this.field.isLocalizable) {
            return this.partitions[language['iso2Code'] || language];
        } else {
            return this.partitions[fieldInvariant];
        }
    }

    protected updateCustomState({ isRequired }: State, user: any, data: any) {
        if (this.isRequired !== isRequired) {
            this.isRequired = isRequired;

            for (const partition of Object.values(this.partitions)) {
                if (!partition.isOptional) {
                    let validators = FieldsValidators.create(this.field, false);

                    if (isRequired) {
                        validators.push(Validators.required);
                    } else {
                        validators = validators.filter(x => x !== Validators.required);
                    }

                    partition.form.setValidators(validators);
                    partition.form.updateValueAndValidity();
                }
            }
        }

        for (const partition of Object.values(this.partitions)) {
            partition.updateState(user, data);
        }
    }

    public prepareLoad(value: any) {
        if (Types.isObject(value)) {
            for (const key in this.partitions) {
                if (this.partitions.hasOwnProperty(key)) {
                    const child = this.partitions[key];

                    child.prepareLoad(value[key]);
                }
            }
        }
    }

    private static buildRules(field: RootFieldDto, rules: CompiledRule[]) {
        return rules.filter(x => x.field === field.name);
    }
}

export class FieldValueForm extends AbstractContentForm<RootFieldDto, FormControl> {
    constructor(field: RootFieldDto, isOptional: boolean
    ) {
        super(field, FieldValueForm.buildControl(field, isOptional), isOptional);
    }

    private static buildControl(field: RootFieldDto, isOptional: boolean) {
        const value = FieldDefaultValue.get(field);

        const validators = FieldsValidators.create(field, isOptional);

        return new FormControl(value, { validators });
    }
}

export class FieldArrayForm extends AbstractContentForm<RootFieldDto, FormArray> {
    public items: FieldArrayItemForm[] = [];

    constructor(field: RootFieldDto, isOptional: boolean,
        private readonly allRules: CompiledRule[]
    ) {
        super(field, FieldArrayForm.buildControl(field, isOptional), isOptional);
    }

    public get(index: number) {
        return this.items[index];
    }

    public addItem(source?: FieldArrayItemForm) {
        const child = new FieldArrayItemForm(this.field, this.isOptional, this.allRules, source);

        this.items.push(child);

        this.form.push(child.form);
    }

    public removeItemAt(index: number) {
        this.items.splice(index, 1);

        this.form.removeAt(index);
    }

    public move(index: number, item: FieldArrayItemForm) {
        const children = [...this.items];

        children.splice(children.indexOf(item), 1);
        children.splice(index, 0, item);

        this.items = children;

        this.sort(children);
    }

    public sort(children: ReadonlyArray<FieldArrayItemForm>) {
        for (let i = 0; i < children.length; i++) {
            this.form.setControl(i, children[i].form);
        }
    }

    protected updateCustomState(_: State, user: any, data: any) {
        for (const item of this.items) {
            item.updateState(user, data);
        }
    }

    public prepareLoad(value: any) {
        if (Types.isArray(value)) {
            while (this.items.length < value.length) {
                this.addItem();
            }

            while (this.items.length > value.length) {
                this.removeItemAt(this.items.length - 1);
            }
        }
    }

    private static buildControl(field: RootFieldDto, isOptional: boolean) {
        const validators = FieldsValidators.create(field, isOptional);

        return new FormArray([], validators);
    }
}

export class FieldArrayItemForm extends AbstractContentForm<RootFieldDto, FormGroup>  {
    private fields: { [key: string]: FieldArrayItemValueForm } = {};

    public readonly sections: ReadonlyArray<FieldSection<NestedFieldDto, FieldArrayItemValueForm>>;

    constructor(field: RootFieldDto, isOptional: boolean, allRules: CompiledRule[], source?: FieldArrayItemForm
    ) {
        super(field, new FormGroup({}), isOptional);

        const sections: FieldSection<NestedFieldDto, FieldArrayItemValueForm>[] = [];

        let currentSeparator: NestedFieldDto | undefined = undefined;
        let currentFields: FieldArrayItemValueForm[] = [];

        for (const nestedField of field.nested) {
            if (nestedField.properties.isContentField) {
                const child = new FieldArrayItemValueForm(nestedField, field, allRules, isOptional, source);

                currentFields.push(child);

                this.fields[nestedField.name] = child;

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

    public get(field: string | NestedFieldDto): FieldArrayItemValueForm | undefined {
        return this.fields[field['name'] || field];
    }

    protected updateCustomState(_: State, user: any, data: any) {
        const itemData = this.form.getRawValue();

        for (const field of Object.values(this.fields)) {
            field.updateState(user, data, itemData);
        }

        for (const section of this.sections) {
            section.updateHidden();
        }
    }
}

export class FieldArrayItemValueForm extends AbstractContentForm<NestedFieldDto, FormControl> {
    private isRequired = false;

    constructor(field: NestedFieldDto, parent: RootFieldDto, rules: CompiledRule[], isOptional: boolean, source?: FieldArrayItemForm
    ) {
        super(field,
            FieldArrayItemValueForm.buildControl(field, isOptional, source),
            isOptional,
            FieldArrayItemValueForm.buildRules(field, parent, rules)
        );

        this.isRequired = field.properties.isRequired && !isOptional;
    }

    protected updateCustomState({ isRequired }: State) {
        if (!this.isOptional && this.isRequired !== isRequired) {
            this.isRequired = isRequired;

            let validators = FieldsValidators.create(this.field, true);

            if (isRequired) {
                validators.push(Validators.required);
            } else {
                validators = validators.filter(x => x !== Validators.required);
            }

            this.form.setValidators(validators);
            this.form.updateValueAndValidity();
        }
    }

    private static buildRules(field: NestedFieldDto, parent: RootFieldDto, rules: CompiledRule[]) {
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

        return new FormControl(value, { validators });
    }
}

type State = {
    isRequired: boolean,
    isHidden: boolean,
    isDisabled: boolean
};

const NO_EMIT = { emitEvent: false };
const NO_EMIT_SELF = { emitEvent: false, onlySelf: true };
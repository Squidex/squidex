/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

import { FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { Form, Types, UndefinableFormArray, valueAll$ } from '@app/framework';
import { BehaviorSubject, Observable } from 'rxjs';
import { debounceTime, onErrorResumeNext } from 'rxjs/operators';
import { AppLanguageDto } from './../services/app-languages.service';
import { LanguageDto } from './../services/languages.service';
import { NestedFieldDto, RootFieldDto, SchemaDetailsDto, TableField } from './../services/schemas.service';
import { fieldInvariant } from './../services/schemas.types';
import { AbstractContentForm, AbstractContentFormState, CompiledRule, FieldSection, PartitionConfig } from './contents.forms-helpers';
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
    private readonly valueChange$ = new BehaviorSubject<any>(this.form.value);
    private initialData: any;

    public readonly sections: ReadonlyArray<FieldSection<RootFieldDto, FieldForm>>;

    public get valueChanges(): Observable<any> {
        return this.valueChange$;
    }

    public get value() {
        return this.valueChange$.value;
    }

    constructor(languages: ReadonlyArray<AppLanguageDto>, schema: SchemaDetailsDto,
        private readonly user: any = {}, debounce = 100
    ) {
        super(new FormGroup({}));

        const compiledPartitions = new PartitionConfig(languages);
        const compiledConditions = schema.fieldRules.map(x => new CompiledRule(x));

        const sections: FieldSection<RootFieldDto, FieldForm>[] = [];

        let currentSeparator: RootFieldDto | undefined = undefined;
        let currentFields: FieldForm[] = [];

        for (const field of schema.fields) {
            if (field.properties.isContentField) {
                const child = new FieldForm(field, compiledPartitions, compiledConditions, this.remoteValidator);

                currentFields.push(child);

                this.fields[field.name] = child;

                this.form.setControl(field.name, child.form);
            } else {
                sections.push(new FieldSection<RootFieldDto, FieldForm>(currentSeparator, currentFields, this.remoteValidator));

                currentFields = [];
                currentSeparator = field;
            }
        }

        if (currentFields.length > 0) {
            sections.push(new FieldSection<RootFieldDto, FieldForm>(currentSeparator, currentFields, this.remoteValidator));
        }

        this.sections = sections;

        let change$ = valueAll$(this.form);

        if (debounce > 0) {
            change$ = change$.pipe(debounceTime(debounce), onErrorResumeNext());
        } else {
            change$ = change$.pipe(onErrorResumeNext());
        }

        change$.subscribe(value => {
            this.valueChange$.next(value);

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
        return !Types.equals(this.initialData, this.value, true);
    }

    public hasChanges(changes: any) {
        return !Types.equals(this.initialData, changes, true);
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

    protected disable() {
        this.form.disable();
    }

    protected enable() {
        this.form.enable({ onlySelf: true });

        this.updateState(this.value);
    }

    public submitCompleted(options?: { newValue?: any, noReset?: boolean }) {
        super.submitCompleted(options);

        this.updateInitialData();
    }

    private updateState(data: any) {
        const context = { user: this.user, data };

        for (const field of Object.values(this.fields)) {
            field.updateState(context, { isDisabled: this.form.disabled });
        }

        for (const section of this.sections) {
            section.updateHidden();
        }
    }

    private updateInitialData() {
        this.initialData = this.form.getRawValue();
    }
}

export class FieldForm extends AbstractContentForm<RootFieldDto, FormGroup> {
    private readonly partitions: { [partition: string]: (FieldValueForm | FieldArrayForm) } = {};
    private isRequired: boolean;

    constructor(field: RootFieldDto, partitions: PartitionConfig, rules: CompiledRule[],
        private readonly remoteValidator?: ValidatorFn
    ) {
        super(field, new FormGroup({}), false, FieldForm.buildRules(field, rules));

        for (const { key, isOptional } of partitions.getAll(field)) {
            const child =
                field.isArray ?
                    new FieldArrayForm(field, isOptional, key, rules, this.remoteValidator) :
                    new FieldValueForm(field, isOptional, key, this.remoteValidator);

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

    protected updateCustomState(context: any, state: AbstractContentFormState) {
        const isRequired = state.isRequired === true;

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

                    if (this.remoteValidator) {
                        validators.push(this.remoteValidator);
                    }

                    partition.form.setValidators(validators);
                    partition.form.updateValueAndValidity();
                }
            }
        }

        for (const partition of Object.values(this.partitions)) {
            partition.updateState(context, state);
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
    constructor(field: RootFieldDto, isOptional: boolean, key: string,
        remoteValidator?: ValidatorFn
    ) {
        super(field, FieldValueForm.buildControl(field, isOptional, key, remoteValidator), isOptional);
    }

    private static buildControl(field: RootFieldDto, isOptional: boolean, key: string, remoteValidator?: ValidatorFn) {
        const value = FieldDefaultValue.get(field, key);

        const validators = FieldsValidators.create(field, isOptional);

        if (remoteValidator) {
            validators.push(remoteValidator);
        }

        return new FormControl(value, { validators });
    }
}

export class FieldArrayForm extends AbstractContentForm<RootFieldDto, UndefinableFormArray> {
    private readonly item$ = new BehaviorSubject<ReadonlyArray<FieldArrayItemForm>>([]);

    public get itemChanges(): Observable<ReadonlyArray<FieldArrayItemForm>> {
        return this.item$;
    }

    public get items() {
        return this.item$.value;
    }

    public set items(value: ReadonlyArray<FieldArrayItemForm>) {
        this.item$.next(value);
    }

    constructor(field: RootFieldDto, isOptional: boolean,
        private readonly partition: string,
        private readonly allRules: CompiledRule[],
        private readonly remoteValidator?: ValidatorFn
    ) {
        super(field, FieldArrayForm.buildControl(field, isOptional), isOptional);
    }

    public get(index: number) {
        return this.items[index];
    }

    public addItem(source?: FieldArrayItemForm) {
        const child = new FieldArrayItemForm(this.field, this.isOptional, this.allRules, source, this.partition, this.remoteValidator);

        this.items = [...this.items, child];

        this.form.push(child.form);
    }

    public unset() {
        this.items = [];

        super.unset();

        this.form.clear();
    }

    public reset() {
        this.items = [];

        this.form.clear();
    }

    public removeItemAt(index: number) {
        this.items = this.items.filter((_, i) => i !== index);

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

    protected updateCustomState(context: any, state: AbstractContentFormState) {
        for (const item of this.items) {
            item.updateState(context, state);
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

        return new UndefinableFormArray([], validators);
    }
}

export class FieldArrayItemForm extends AbstractContentForm<RootFieldDto, FormGroup>  {
    private readonly fields: { [key: string]: FieldArrayItemValueForm } = {};

    public readonly sections: ReadonlyArray<FieldSection<NestedFieldDto, FieldArrayItemValueForm>>;

    constructor(field: RootFieldDto, isOptional: boolean, allRules: CompiledRule[], source: FieldArrayItemForm | undefined, partition: string,
        private readonly remoteValidator?: ValidatorFn
    ) {
        super(field, new FormGroup({}), isOptional);

        const sections: FieldSection<NestedFieldDto, FieldArrayItemValueForm>[] = [];

        let currentSeparator: NestedFieldDto | undefined = undefined;
        let currentFields: FieldArrayItemValueForm[] = [];

        for (const nestedField of field.nested) {
            if (nestedField.properties.isContentField) {
                const child = new FieldArrayItemValueForm(nestedField, field, allRules, isOptional, partition, source, this.remoteValidator);

                currentFields.push(child);

                this.fields[nestedField.name] = child;

                this.form.setControl(nestedField.name, child.form);
            } else {
                sections.push(new FieldSection<NestedFieldDto, FieldArrayItemValueForm>(currentSeparator, currentFields, this.remoteValidator));

                currentFields = [];
                currentSeparator = nestedField;
            }
        }

        if (currentFields.length > 0) {
            sections.push(new FieldSection<NestedFieldDto, FieldArrayItemValueForm>(currentSeparator, currentFields, this.remoteValidator));
        }

        this.sections = sections;
    }

    public get(field: string | NestedFieldDto): FieldArrayItemValueForm | undefined {
        return this.fields[field['name'] || field];
    }

    protected updateCustomState(context: any, state: AbstractContentFormState) {
        const itemData = this.form.getRawValue();

        for (const field of Object.values(this.fields)) {
            field.updateState({ ...context, itemData }, state);
        }

        for (const section of this.sections) {
            section.updateHidden();
        }
    }
}

export class FieldArrayItemValueForm extends AbstractContentForm<NestedFieldDto, FormControl> {
    private isRequired = false;

    constructor(field: NestedFieldDto, parent: RootFieldDto, rules: CompiledRule[], isOptional: boolean, partition: string,
        source: FieldArrayItemForm | undefined, remoteValidator?: ValidatorFn
    ) {
        super(field,
            FieldArrayItemValueForm.buildControl(field, isOptional, partition, remoteValidator, source),
            isOptional,
            FieldArrayItemValueForm.buildRules(field, parent, rules)
        );

        this.isRequired = field.properties.isRequired && !isOptional;
    }

    protected updateCustomState(_: any, state: AbstractContentFormState) {
        const isRequired = state.isRequired === true;

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

    private static buildControl(field: NestedFieldDto, isOptional: boolean, partition: string, remoteValidator?: ValidatorFn, source?: FieldArrayItemForm) {
        let value = FieldDefaultValue.get(field, partition);

        if (source) {
            const sourceField = source.form.get(field.name);

            if (sourceField) {
                value = sourceField.value;
            }
        }

        const validators = FieldsValidators.create(field, isOptional);

        if (remoteValidator) {
            validators.push(remoteValidator);
        }

        return new FormControl(value, { validators });
    }
}
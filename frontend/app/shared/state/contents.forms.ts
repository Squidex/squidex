/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { debounceTimeSafe, Form, getRawValue, Types, UndefinableFormArray, UndefinableFormGroup, valueAll$ } from '@app/framework';
import { BehaviorSubject, distinctUntilChanged, Observable } from 'rxjs';
import { AppLanguageDto } from './../services/app-languages.service';
import { LanguageDto } from './../services/languages.service';
import { FieldDto, RootFieldDto, SchemaDto, TableField } from './../services/schemas.service';
import { ComponentFieldPropertiesDto, fieldInvariant } from './../services/schemas.types';
import { AbstractContentForm, AbstractContentFormState, ComponentRulesProvider, FieldSection, FormGlobals, groupFields, PartitionConfig, RootRulesProvider, RulesProvider } from './contents.forms-helpers';
import { FieldDefaultValue, FieldsValidators } from './contents.forms.visitors';

type SaveQueryFormType = { name: string; user: boolean };

export class SaveQueryForm extends Form<FormGroup, SaveQueryFormType> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.required,
                ],
            ],
            user: false,
        }));
    }
}

export class PatchContentForm extends Form<FormGroup, any> {
    private readonly editableFields: ReadonlyArray<RootFieldDto>;

    constructor(
        private readonly listFields: ReadonlyArray<TableField>,
        private readonly language: AppLanguageDto,
    ) {
        super(new FormGroup({}));

        this.editableFields = this.listFields.filter(x => Types.is(x, RootFieldDto) && x.isInlineEditable) as any;

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

    constructor(
        public readonly languages: ReadonlyArray<AppLanguageDto>,
        public readonly schema: SchemaDto, schemas: { [id: string ]: SchemaDto },
        public context: any,
        debounce = 100,
    ) {
        super(new FormGroup({}));

        const globals: FormGlobals = {
            schema,
            schemas,
            partitions: new PartitionConfig(languages),
            remoteValidator: this.remoteValidator,
        };

        const rules = new RootRulesProvider(schema);

        this.sections = groupFields(schema.fields).map(({ separator, fields }) => {
            const forms: FieldForm[] = [];

            for (const field of fields) {
                const childForm =
                    new FieldForm(
                        globals,
                        field,
                        field.name,
                        rules);

                this.form.setControl(field.name, childForm.form);

                forms.push(childForm);

                this.fields[field.name] = childForm;
            }

            return new FieldSection<RootFieldDto, FieldForm>(separator, forms);
        });

        valueAll$(this.form).pipe(debounceTimeSafe(debounce), distinctUntilChanged(Types.equals)).subscribe(value => {
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
        for (const key of Object.keys(this.fields)) {
            this.fields[key].prepareLoad(value?.[key]);
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

    public setContext(context?: any) {
        this.context = context;

        this.updateState(this.value);
    }

    public submitCompleted(options?: { newValue?: any; noReset?: boolean }) {
        super.submitCompleted(options);

        this.updateInitialData();
    }

    private updateState(data: any) {
        const context = { ...this.context || {}, data };

        for (const field of Object.values(this.fields)) {
            field.updateState(context, data[field.field.name], data, { isDisabled: this.form.disabled });
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
    private readonly partitions: { [partition: string]: FieldItemForm } = {};
    private isRequired: boolean;

    constructor(
        globals: FormGlobals,
        field: RootFieldDto,
        fieldPath: string,
        rules: RulesProvider,
    ) {
        super(globals, field, fieldPath, FieldForm.buildForm(), false, rules);

        for (const { key, isOptional } of globals.partitions.getAll(field)) {
            const childForm =
                buildForm(
                    this.globals,
                    field,
                    this.path(key),
                    isOptional,
                    rules,
                    key);

            this.partitions[key] = childForm;

            this.form.setControl(key, childForm.form);
        }

        this.isRequired = field.properties.isRequired;
    }

    public copyFrom(source: FieldForm, key: string) {
        const target = this.get(key);

        if (!target) {
            return;
        }

        const value = source.get(key)?.form.value;

        target.prepareLoad(value);
        target.form.reset(source.get(key)?.form.value);
    }

    public copyAllFrom(source: FieldForm) {
        const value = source.form.getRawValue();

        this.prepareLoad(value);

        this.form.reset(value);
    }

    public get(language: string | LanguageDto) {
        if (this.field.isLocalizable) {
            return this.partitions[language['iso2Code'] || language];
        } else {
            return this.partitions[fieldInvariant];
        }
    }

    public prepareLoad(value: any) {
        for (const key of Object.keys(this.partitions)) {
            this.partitions[key].prepareLoad(value?.[key]);
        }
    }

    protected updateCustomState(context: any, fieldData: any, itemData: any, state: AbstractContentFormState) {
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

                    if (this.globals.remoteValidator) {
                        validators.push(this.globals.remoteValidator);
                    }

                    partition.form.setValidators(validators);
                    partition.form.updateValueAndValidity();
                }
            }
        }

        for (const key of Object.keys(this.partitions)) {
            this.partitions[key].updateState(context, fieldData?.[key], itemData, state);
        }
    }

    private static buildForm() {
        return new FormGroup({});
    }
}

export class FieldValueForm extends AbstractContentForm<FieldDto, FormControl> {
    private isRequired = false;

    constructor(
        globals: FormGlobals,
        field: FieldDto,
        fieldPath: string,
        isOptional: boolean,
        rules: RulesProvider,
        partition: string,
    ) {
        super(globals, field, fieldPath,
            FieldValueForm.buildControl(field, isOptional, partition, globals),
            isOptional, rules);

        this.isRequired = field.properties.isRequired && !isOptional;
    }

    protected updateCustomState(_context: any, _fieldData: any, _itemData: any, state: AbstractContentFormState) {
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

    private static buildControl(field: FieldDto, isOptional: boolean, partition: string, globals: FormGlobals) {
        const value = FieldDefaultValue.get(field, partition);

        const validators = FieldsValidators.create(field, isOptional);

        if (globals.remoteValidator) {
            validators.push(globals.remoteValidator);
        }

        return new FormControl(value, { validators });
    }
}

export class FieldArrayForm extends AbstractContentForm<FieldDto, UndefinableFormArray> {
    private readonly item$ = new BehaviorSubject<ReadonlyArray<ObjectForm>>([]);

    public get itemChanges(): Observable<ReadonlyArray<ObjectForm>> {
        return this.item$;
    }

    public get items() {
        return this.item$.value;
    }

    public set items(value: ReadonlyArray<ObjectForm>) {
        this.item$.next(value);
    }

    constructor(
        globals: FormGlobals,
        field: FieldDto,
        fieldPath: string,
        isOptional: boolean,
        rules: RulesProvider,
        private readonly partition: string,
        private readonly isComponents: boolean,
    ) {
        super(globals, field, fieldPath,
            FieldArrayForm.buildControl(field, isOptional),
            isOptional, rules);
    }

    public get(index: number) {
        return this.items[index];
    }

    public addCopy(source: ObjectForm) {
        if (this.isComponents) {
            const child = this.createComponent();

            child.load(getRawValue(source.form));

            this.addChild(child);
        } else {
            const child = this.createItem();

            child.load(getRawValue(source.form));

            this.addChild(child);
        }
    }

    public addComponent(schemaId?: string) {
        const child = this.createComponent(schemaId);

        this.addChild(child);
    }

    public addItem() {
        const child = this.createItem();

        this.addChild(child);
    }

    public addChild(child: ObjectForm) {
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

    public move(index: number, item: ObjectForm) {
        const children = [...this.items];

        children.splice(children.indexOf(item), 1);
        children.splice(index, 0, item);

        this.items = children;

        this.sort(children);
    }

    public sort(children: ReadonlyArray<ObjectForm>) {
        for (let i = 0; i < children.length; i++) {
            this.form.setControl(i, children[i].form);
        }
    }

    public prepareLoad(value: any) {
        if (Types.isArray(value)) {
            while (this.items.length < value.length) {
                if (this.isComponents) {
                    this.addComponent();
                } else {
                    this.addItem();
                }
            }

            while (this.items.length > value.length) {
                this.removeItemAt(this.items.length - 1);
            }
        }

        for (let i = 0; i < this.items.length; i++) {
            this.items[i].prepareLoad(value?.[i]);
        }
    }

    protected updateCustomState(context: any, fieldData: any, itemData: any, state: AbstractContentFormState) {
        for (let i = 0; i < this.items.length; i++) {
            this.items[i].updateState(context, fieldData?.[i], itemData, state);
        }
    }

    private createItem() {
        return new ArrayItemForm(
            this.globals,
            this.field as RootFieldDto,
            this.fieldPath,
            this.isOptional,
            this.rules,
            this.partition);
    }

    private createComponent(schemaId?: string) {
        return new ComponentForm(
            this.globals,
            this.field as RootFieldDto,
            this.fieldPath,
            this.isOptional,
            this.rules,
            this.partition,
            schemaId);
    }

    private static buildControl(field: FieldDto, isOptional: boolean) {
        const validators = FieldsValidators.create(field, isOptional);

        return new UndefinableFormArray([], validators);
    }
}

export type FieldItemForm = ComponentForm | FieldValueForm | FieldArrayForm;

export class ObjectForm<TField extends FieldDto = FieldDto> extends AbstractContentForm<TField, UndefinableFormGroup> {
    private fields: { [key: string]: FieldItemForm } = {};
    private fieldSections: FieldSection<FieldDto, FieldItemForm>[] = [];

    public get sections() {
        return this.fieldSections;
    }

    constructor(
        globals: FormGlobals,
        field: TField,
        fieldPath: string,
        isOptional: boolean,
        rules: RulesProvider,
        private readonly partition: string,
    ) {
        super(globals, field, fieldPath,
            ObjectForm.buildControl(field, isOptional, false),
            isOptional, rules);
    }

    public get(field: string | { name: string }): FieldItemForm | undefined {
        return this.fields[field['name'] || field];
    }

    protected init(schema?: ReadonlyArray<FieldDto>) {
        this.fields = {};
        this.fieldSections = [];

        for (const key of Object.keys(this.form.controls)) {
            this.form.removeControl(key);
        }

        if (schema) {
            this.form.reset({});

            for (const { separator, fields } of groupFields(schema)) {
                const forms: FieldItemForm[] = [];

                for (const field of fields) {
                    const childForm =
                        buildForm(
                            this.globals,
                            field,
                            this.path(field.name),
                            this.isOptional,
                            this.rules,
                            this.partition);

                    this.form.setControl(field.name, childForm.form);

                    forms.push(childForm);

                    this.fields[field.name] = childForm;
                }

                this.fieldSections.push(new FieldSection<FieldDto, FieldItemForm>(separator, forms));
            }
        } else {
            this.form.reset(undefined);
        }
    }

    public load(data: any) {
        this.prepareLoad(data);

        this.form.reset(data);
    }

    public prepareLoad(value: any) {
        for (const key of Object.keys(this.fields)) {
            this.fields[key].prepareLoad(value?.[key]);
        }
    }

    protected updateCustomState(context: any, fieldData: any, _: any, state: AbstractContentFormState) {
        for (const key of Object.keys(this.fields)) {
            this.fields[key].updateState(context, fieldData?.[key], fieldData, state);
        }

        for (const section of this.sections) {
            section.updateHidden();
        }
    }

    private static buildControl(field: FieldDto, isOptional: boolean, validate: boolean) {
        let validators = [Validators.nullValidator];

        if (validate) {
            validators = FieldsValidators.create(field, isOptional);
        }

        return new UndefinableFormGroup({}, validators);
    }
}

export class ArrayItemForm extends ObjectForm<RootFieldDto> {
    constructor(
        globals: FormGlobals,
        field: RootFieldDto,
        fieldPath: string,
        isOptional: boolean,
        rules: RulesProvider,
        partition: string,
    ) {
        super(globals, field, fieldPath, isOptional, rules, partition);

        this.init(field.nested);
    }
}

export class ComponentForm extends ObjectForm {
    private schemaId?: string;

    public readonly properties: ComponentFieldPropertiesDto;

    public get schema() {
        return this.globals.schemas[this.schemaId!];
    }

    constructor(
        globals: FormGlobals,
        field: FieldDto,
        fieldPath: string,
        isOptional: boolean,
        rules: RulesProvider,
        partition: string,
        schemaId?: string,
    ) {
        super(globals, field, fieldPath, isOptional,
            new ComponentRulesProvider(fieldPath, rules), partition);

        this.properties = field.properties as ComponentFieldPropertiesDto;

        if (schemaId) {
            this.selectSchema(schemaId);
        }
    }

    public selectSchema(schemaId?: string) {
        if (this.schemaId !== schemaId) {
            this.schemaId = schemaId;

            if (this.schema) {
                this.rules.setSchema(this.schema);

                this.init(this.schema.fields);

                this.form.setControl('schemaId', new FormControl(schemaId));
            } else {
                this.init(undefined);
            }
        }
    }

    public unset() {
        this.selectSchema(undefined);

        super.unset();
    }

    public prepareLoad(value: any) {
        this.selectSchema(value?.['schemaId']);

        super.prepareLoad(value);
    }
}

function buildForm(globals: FormGlobals, field: FieldDto, fieldPath: string, isOptional: boolean, rules: RulesProvider, partition: string) {
    switch (field.properties.fieldType) {
        case 'Array':
            return new FieldArrayForm(globals, field, fieldPath, isOptional, rules, partition, false);
        case 'Component':
            return new ComponentForm(globals, field, fieldPath, isOptional, rules, partition);
        case 'Components':
            return new FieldArrayForm(globals, field, fieldPath, isOptional, rules, partition, true);
        default:
            return new FieldValueForm(globals, field, fieldPath, isOptional, rules, partition);
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { debounceTimeSafe, Form, FormArrayTemplate, getRawValue, TemplatedFormArray, Types, value$ } from '@app/framework';
import { FormGroupTemplate, TemplatedFormGroup } from '@app/framework/angular/forms/templated-form-group';
import { BehaviorSubject, distinctUntilChanged, Observable } from 'rxjs';
import { AppLanguageDto } from './../services/app-languages.service';
import { LanguageDto } from './../services/languages.service';
import { FieldDto, RootFieldDto, SchemaDto, TableField } from './../services/schemas.service';
import { ComponentFieldPropertiesDto, fieldInvariant } from './../services/schemas.types';
import { ComponentRulesProvider, RootRulesProvider, RulesProvider } from './contents.form-rules';
import { AbstractContentForm, AbstractContentFormState, FieldSection, FormGlobals, groupFields, PartitionConfig } from './contents.forms-helpers';
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

        value$(this.form).pipe(debounceTimeSafe(debounce), distinctUntilChanged(Types.equals)).subscribe(value => {
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

    public get(language: string | LanguageDto) {
        if (this.field.isLocalizable) {
            return this.partitions[language['iso2Code'] || language];
        } else {
            return this.partitions[fieldInvariant];
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

        for (const [key, partition] of Object.entries(this.partitions)) {
            partition.updateState(context, fieldData?.[key], itemData, state);
        }
    }

    private static buildForm() {
        return new FormGroup({});
    }
}

export class FieldValueForm extends AbstractContentForm<FieldDto, FormControl> {
    private isRequired = false;

    constructor(globals: FormGlobals, field: FieldDto, fieldPath: string, isOptional: boolean, rules: RulesProvider, partition: string) {
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

export class FieldArrayForm extends AbstractContentForm<FieldDto, TemplatedFormArray> {
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

    constructor(globals: FormGlobals, field: FieldDto, fieldPath: string, isOptional: boolean, rules: RulesProvider,
        public readonly partition: string,
        public readonly isComponents: boolean,
    ) {
        super(globals, field, fieldPath,
            FieldArrayForm.buildControl(field, isOptional),
            isOptional, rules);

        this.form.template['form'] = this;
    }

    public get(index: number) {
        return this.items[index];
    }

    public addCopy(source: ObjectForm) {
        this.form.add().reset(getRawValue(source.form));
    }

    public addComponent(schemaId: string) {
        this.form.add().reset({ schemaId });
    }

    public addItem() {
        this.form.add();
    }

    public removeItemAt(index: number) {
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

    protected updateCustomState(context: any, fieldData: any, itemData: any, state: AbstractContentFormState) {
        for (let i = 0; i < this.items.length; i++) {
            this.items[i].updateState(context, fieldData?.[i], itemData, state);
        }
    }

    private static buildControl(field: FieldDto, isOptional: boolean) {
        return new TemplatedFormArray(new ArrayTemplate(), FieldsValidators.create(field, isOptional));
    }
}

class ArrayTemplate implements FormArrayTemplate {
    public form: FieldArrayForm;

    public createControl() {
        const child = this.form.isComponents ?
            this.createComponent() :
            this.createItem();

        this.form.items = [...this.form.items, child];

        return child.form;
    }

    public removeControl(index: number) {
        this.form.items = this.form.items.filter((_, i) => i !== index);
    }

    public clearControls() {
        this.form.items = [];
    }

    private createItem() {
        return new ArrayItemForm(
            this.form.globals,
            this.form.field as RootFieldDto,
            this.form.fieldPath,
            this.form.isOptional,
            this.form.rules,
            this.form.partition);
    }

    private createComponent() {
        return new ComponentForm(
            this.form.globals,
            this.form.field as RootFieldDto,
            this.form.fieldPath,
            this.form.isOptional,
            this.form.rules,
            this.form.partition);
    }
}

export type FieldItemForm = ComponentForm | FieldValueForm | FieldArrayForm;

type FieldMap = { [name: string]: FieldItemForm };

export class ObjectFormBase<TField extends FieldDto = FieldDto> extends AbstractContentForm<TField, TemplatedFormGroup> {
    private readonly fieldSections$ = new BehaviorSubject<ReadonlyArray<FieldSection<FieldDto, FieldItemForm>>>([]);
    private readonly fields$ = new BehaviorSubject<FieldMap>({});

    public get fieldSectionsChanges(): Observable<ReadonlyArray<FieldSection<FieldDto, FieldItemForm>>> {
        return this.fieldSections$;
    }

    public get fieldSections() {
        return this.fieldSections$.value;
    }

    public set fieldSections(value: ReadonlyArray<FieldSection<FieldDto, FieldItemForm>>) {
        this.fieldSections$.next(value);
    }

    public get fieldsChanges(): Observable<FieldMap> {
        return this.fields$;
    }

    public get fields() {
        return this.fields$.value;
    }

    public set fields(value: FieldMap) {
        this.fields$.next(value);
    }

    constructor(globals: FormGlobals, field: TField, fieldPath: string, isOptional: boolean, rules: RulesProvider, template: ObjectTemplate,
        public readonly partition: string,
    ) {
        super(globals, field, fieldPath,
            ObjectForm.buildControl(template),
            isOptional, rules);
    }

    public get(field: string | { name: string }): FieldItemForm | undefined {
        return this.fields[field['name'] || field];
    }

    protected updateCustomState(context: any, fieldData: any, _: any, state: AbstractContentFormState) {
        for (const [key, field] of Object.entries(this.fields)) {
            field.updateState(context, fieldData?.[key], fieldData, state);
        }

        for (const section of this.fieldSections) {
            section.updateHidden();
        }
    }

    private static buildControl(template: ObjectTemplate) {
        return new TemplatedFormGroup(template);
    }
}

abstract class ObjectTemplate<TField extends FieldDto = FieldDto> implements FormGroupTemplate {
    private currentSchema: ReadonlyArray<FieldDto> | undefined;

    constructor(
        protected readonly objectForm: () => ObjectFormBase<TField>,
    ) {
    }

    protected abstract getSchema(value: any, objectForm: ObjectFormBase<TField>): ReadonlyArray<FieldDto> | undefined;

    public setControls(form: FormGroup, value: any) {
        const objectForm = this.objectForm();

        if (!objectForm) {
            return;
        }

        const schema = this.getSchema(value, objectForm);

        if (this.currentSchema !== schema) {
            this.clearControlsCore(objectForm);

            if (schema) {
                this.setControlsCore(schema, objectForm, form);
            }

            this.currentSchema = schema;
        }
    }

    protected setControlsCore(schema: ReadonlyArray<FieldDto>, objectForm: ObjectForm, form: FormGroup) {
        const fieldMap: FieldMap = {};
        const fieldSections: FieldSection<FieldDto, FieldItemForm>[] = [];

        for (const { separator, fields } of groupFields(schema)) {
            const forms: FieldItemForm[] = [];

            for (const field of fields) {
                const childForm = buildForm(
                    objectForm.globals,
                    field,
                    objectForm.path(field.name),
                    objectForm.isOptional,
                    objectForm.rules,
                    objectForm.partition);

                form.setControl(field.name, childForm.form);

                forms.push(childForm);

                fieldMap[field.name] = childForm;
            }

            fieldSections.push(new FieldSection<FieldDto, FieldItemForm>(separator, forms));
        }

        objectForm.fields = fieldMap;
        objectForm.fieldSections = fieldSections;
    }

    public clearControls() {
        const objectForm = this.objectForm();

        if (!objectForm) {
            return;
        }

        this.clearControlsCore(objectForm);

        this.currentSchema = undefined;
    }

    private clearControlsCore(objectForm: ObjectFormBase<TField>) {
        for (const name of Object.keys(objectForm.form.controls)) {
            objectForm.form.removeControl(name);
        }

        objectForm.fields = {};
        objectForm.fieldSections = [];
    }
}

export class ArrayItemForm extends ObjectFormBase<RootFieldDto> {
    constructor(globals: FormGlobals, field: RootFieldDto, fieldPath: string, isOptional: boolean, rules: RulesProvider, partition: string) {
        super(globals, field, fieldPath, isOptional, rules,
            new ArrayItemTemplate(() => this), partition);

        this.form.build({});
    }
}

class ArrayItemTemplate extends ObjectTemplate<RootFieldDto> {
    public getSchema() {
        return this.objectForm()?.field?.nested;
    }
}

export class ObjectForm extends ObjectFormBase {
    constructor(globals: FormGlobals, field: FieldDto, fieldPath: string, isOptional: boolean, rules: RulesProvider, partition: string) {
        super(globals, field, fieldPath, isOptional, rules,
            new ComponentTemplate(() => this),
            partition);

        this.form.build();
    }
}

export class ComponentForm extends ObjectFormBase {
    public get properties() {
        return this.field.properties as ComponentFieldPropertiesDto;
    }

    public get schema() {
        return this.form.template['schema'];
    }

    constructor(globals: FormGlobals, field: FieldDto, fieldPath: string, isOptional: boolean, rules: RulesProvider, partition: string) {
        super(globals, field, fieldPath, isOptional,
            new ComponentRulesProvider(fieldPath, rules, () => this.schema),
            new ComponentTemplate(() => this),
            partition);

        this.form.build();
    }

    public selectSchema(schemaId: string) {
        this.form.reset({ schemaId });
    }
}

class ComponentTemplate extends ObjectTemplate {
    public schema?: SchemaDto;

    public getSchema(value: any, objectForm: ObjectFormBase<FieldDto>) {
        this.schema = objectForm.globals.schemas[value?.schemaId];

        return this.schema?.fields;
    }

    protected setControlsCore(schema: ReadonlyArray<FieldDto>, objectForm: ObjectForm, form: FormGroup) {
        form.setControl('schemaId', new FormControl());

        super.setControlsCore(schema, objectForm, form);
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

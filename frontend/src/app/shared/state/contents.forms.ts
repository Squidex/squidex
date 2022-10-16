/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { UntypedFormControl, UntypedFormGroup, Validators } from '@angular/forms';
import { BehaviorSubject, Observable } from 'rxjs';
import { distinctUntilChanged, map } from 'rxjs/operators';
import { debounceTimeSafe, ExtendedFormGroup, Form, FormArrayTemplate, TemplatedFormArray, Types, value$ } from '@app/framework';
import { FormGroupTemplate, TemplatedFormGroup } from '@app/framework/angular/forms/templated-form-group';
import { AppLanguageDto } from './../services/app-languages.service';
import { LanguageDto } from './../services/languages.service';
import { FieldDto, RootFieldDto, SchemaDto, TableField } from './../services/schemas.service';
import { ComponentFieldPropertiesDto, fieldInvariant } from './../services/schemas.types';
import { ComponentRulesProvider, RootRulesProvider, RulesProvider } from './contents.form-rules';
import { AbstractContentForm, AbstractContentFormState, contentTranslationStatus, FieldSection, fieldTranslationStatus, FormGlobals, groupFields, PartitionConfig } from './contents.forms-helpers';
import { FieldDefaultValue, FieldsValidators } from './contents.forms.visitors';

type SaveQueryFormType = { name: string; user: boolean };

export class SaveQueryForm extends Form<ExtendedFormGroup, SaveQueryFormType> {
    constructor() {
        super(new ExtendedFormGroup({
            name: new UntypedFormControl('',
                Validators.required,
            ),
            user: new UntypedFormControl(false,
                Validators.nullValidator,
            ),
        }));
    }
}

export class PatchContentForm extends Form<ExtendedFormGroup, any> {
    private readonly editableFields: ReadonlyArray<RootFieldDto>;

    constructor(
        private readonly listFields: ReadonlyArray<TableField>,
        private readonly language: AppLanguageDto,
    ) {
        super(new ExtendedFormGroup({}));

        this.editableFields = this.listFields.filter(x => x.rootField?.isInlineEditable).map(x => x.rootField!);

        for (const field of this.editableFields) {
            const validators = FieldsValidators.create(field, this.language.isOptional);

            this.form.setControl(field.name, new UntypedFormControl(undefined, { validators }));
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

export class EditContentForm extends Form<ExtendedFormGroup, any> {
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

    public readonly translationStatus =
        this.valueChange$.pipe(map(x => contentTranslationStatus(x, this.schema, this.languages)));

    constructor(
        public readonly languages: ReadonlyArray<AppLanguageDto>,
        public readonly schema: SchemaDto, schemas: { [id: string ]: SchemaDto },
        public context: any,
        debounce = 100,
    ) {
        super(new ExtendedFormGroup({}));

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
            field.updateState(context, data, { isDisabled: this.form.disabled });
        }

        for (const section of this.sections) {
            section.updateHidden();
        }
    }

    private updateInitialData() {
        this.initialData = this.form.value;
    }
}

export class FieldForm extends AbstractContentForm<RootFieldDto, UntypedFormGroup> {
    private readonly partitions: { [partition: string]: FieldItemForm } = {};
    private isRequired: boolean;

    public readonly translationStatus =
        value$(this.form).pipe(map(x => fieldTranslationStatus(x)));

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

    protected updateCustomState(context: any, itemData: any, state: AbstractContentFormState) {
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

        for (const partition of Object.values(this.partitions)) {
            partition.updateState(context, itemData, state);
        }
    }

    private static buildForm() {
        return new ExtendedFormGroup({});
    }
}

export class FieldValueForm extends AbstractContentForm<FieldDto, UntypedFormControl> {
    private isRequired = false;

    constructor(globals: FormGlobals, field: FieldDto, fieldPath: string, isOptional: boolean, rules: RulesProvider, partition: string) {
        super(globals, field, fieldPath,
            FieldValueForm.buildControl(field, isOptional, partition, globals),
            isOptional, rules);

        this.isRequired = field.properties.isRequired && !isOptional;
    }

    protected updateCustomState(_context: any, _itemData: any, state: AbstractContentFormState) {
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

        return new UntypedFormControl(value, { validators });
    }
}

export class FieldArrayForm extends AbstractContentForm<FieldDto, TemplatedFormArray> {
    private readonly item$ = new BehaviorSubject<ReadonlyArray<ObjectFormBase>>([]);

    public get itemChanges(): Observable<ReadonlyArray<ObjectFormBase>> {
        return this.item$;
    }

    public get items() {
        return this.item$.value;
    }

    public set internalItems(value: ReadonlyArray<ObjectFormBase>) {
        this.item$.next(value);
    }

    constructor(globals: FormGlobals, field: FieldDto, fieldPath: string, isOptional: boolean, rules: RulesProvider,
        public readonly partition: string,
        public readonly isComponents: boolean,
    ) {
        super(globals, field, fieldPath,
            new TemplatedFormArray(new ArrayTemplate(() => this), FieldsValidators.create(field, isOptional)),
            isOptional, rules);

        this.form.template['form'] = this;
    }

    public get(index: number) {
        return this.items[index];
    }

    public addCopy(source: ObjectFormBase) {
        this.form.add().reset(source.form.value);
    }

    public addComponent(schemaId: string) {
        this.form.add().patchValue({ schemaId });
    }

    public addItem() {
        this.form.add();
    }

    public removeItemAt(index: number) {
        this.form.removeAt(index);
    }

    public move(index: number, item: ObjectFormBase) {
        const children = [...this.items];

        children.splice(children.indexOf(item), 1);
        children.splice(index, 0, item);

        this.internalItems = children;

        this.sort(children);
    }

    public sort(children: ReadonlyArray<ObjectFormBase>) {
        for (let i = 0; i < children.length; i++) {
            this.form.setControl(i, children[i].form);
        }
    }

    protected updateCustomState(context: any, itemData: any, state: AbstractContentFormState) {
        for (const item of this.items) {
            item.updateState(context, itemData, state);
        }
    }
}

class ArrayTemplate implements FormArrayTemplate {
    protected get model() {
        return this.modelProvider();
    }

    constructor(
        private readonly modelProvider: () => FieldArrayForm,
    ) {
    }

    public createControl() {
        const child = this.model.isComponents ?
            this.createComponent() :
            this.createItem();

        this.model.internalItems = [...this.model.items, child];

        return child.form;
    }

    public removeControl(index: number) {
        this.model.internalItems = this.model.items.filter((_, i) => i !== index);
    }

    public clearControls() {
        this.model.internalItems = [];
    }

    private createItem() {
        return new ArrayItemForm(
            this.model.globals,
            this.model.field as RootFieldDto,
            this.model.fieldPath,
            this.model.isOptional,
            this.model.rules,
            this.model.partition);
    }

    private createComponent() {
        return new ComponentForm(
            this.model.globals,
            this.model.field as RootFieldDto,
            this.model.fieldPath,
            this.model.isOptional,
            this.model.rules,
            this.model.partition);
    }
}

export type FieldItemForm = ComponentForm | FieldValueForm | FieldArrayForm;

type FieldMap = { [name: string]: FieldItemForm };

export class ObjectFormBase<TField extends FieldDto = FieldDto> extends AbstractContentForm<TField, TemplatedFormGroup> {
    private readonly sections$ = new BehaviorSubject<ReadonlyArray<FieldSection<FieldDto, FieldItemForm>>>([]);
    private readonly fields$ = new BehaviorSubject<FieldMap>({});

    public get sectionsChanges(): Observable<ReadonlyArray<FieldSection<FieldDto, FieldItemForm>>> {
        return this.sections$;
    }

    public get sections() {
        return this.sections$.value;
    }

    public set internalFieldSections(value: ReadonlyArray<FieldSection<FieldDto, FieldItemForm>>) {
        this.sections$.next(value);
    }

    public get fieldsChanges(): Observable<FieldMap> {
        return this.fields$;
    }

    public get fields() {
        return this.fields$.value;
    }

    public set internalFieldByName(value: FieldMap) {
        this.fields$.next(value);
    }

    constructor(globals: FormGlobals, field: TField, fieldPath: string, isOptional: boolean, rules: RulesProvider, template: ObjectTemplate,
        public readonly partition: string,
    ) {
        super(globals, field, fieldPath,
            ObjectFormBase.buildControl(template),
            isOptional, rules);
    }

    public get(field: string | { name: string }): FieldItemForm | undefined {
        return this.fields[field['name'] || field];
    }

    protected updateCustomState(context: any, _: any, state: AbstractContentFormState) {
        for (const field of Object.values(this.fields)) {
            field.updateState(context, this.form.value, state);
        }

        for (const section of this.sections) {
            section.updateHidden();
        }
    }

    private static buildControl(template: ObjectTemplate) {
        return new TemplatedFormGroup(template);
    }
}

abstract class ObjectTemplate<T extends ObjectFormBase = ObjectFormBase> implements FormGroupTemplate {
    private currentSchema: ReadonlyArray<FieldDto> | undefined;

    protected get model() {
        return this.modelProvider();
    }

    constructor(
        private readonly modelProvider: () => T,
    ) {
    }

    protected abstract getSchema(value: any, model: T): ReadonlyArray<FieldDto> | undefined;

    public setControls(form: UntypedFormGroup, value: any) {
        const schema = this.getSchema(value, this.model);

        if (this.currentSchema !== schema) {
            this.clearControlsCore(this.model);

            if (schema) {
                this.setControlsCore(schema, value, this.model, form);
            }

            this.currentSchema = schema;
        }
    }

    public clearControls() {
        if (this.currentSchema !== undefined) {
            this.clearControlsCore(this.model);

            this.currentSchema = undefined;
        }
    }

    protected setControlsCore(schema: ReadonlyArray<FieldDto>, _: any, model: T, form: UntypedFormGroup) {
        const fieldByName: FieldMap = {};
        const fieldSections: FieldSection<FieldDto, FieldItemForm>[] = [];

        for (const { separator, fields } of groupFields(schema)) {
            const forms: FieldItemForm[] = [];

            for (const field of fields) {
                const childForm = buildForm(
                    model.globals,
                    field,
                    model.path(field.name),
                    model.isOptional,
                    model.rules,
                    model.partition);

                form.setControl(field.name, childForm.form);

                forms.push(childForm);

                fieldByName[field.name] = childForm;
            }

            fieldSections.push(new FieldSection<FieldDto, FieldItemForm>(separator, forms));
        }

        model.internalFieldByName = fieldByName;
        model.internalFieldSections = fieldSections;
    }

    protected clearControlsCore(model: T) {
        for (const name of Object.keys(model.form.controls)) {
            model.form.removeControl(name);
        }

        model.internalFieldByName = {};
        model.internalFieldSections = [];
    }
}

export class ArrayItemForm extends ObjectFormBase<RootFieldDto> {
    constructor(globals: FormGlobals, field: RootFieldDto, fieldPath: string, isOptional: boolean, rules: RulesProvider, partition: string) {
        super(globals, field, fieldPath, isOptional, rules,
            new ArrayItemTemplate(() => this), partition);

        this.form.build({});
    }
}

class ArrayItemTemplate extends ObjectTemplate<ArrayItemForm> {
    public getSchema() {
        return this.model.field.nested;
    }
}

export class ComponentForm extends ObjectFormBase {
    private readonly schema$ = new BehaviorSubject<SchemaDto | undefined>(undefined);

    public get schemaChanges(): Observable<SchemaDto | undefined> {
        return this.schema$;
    }

    public get schema() {
        return this.schema$.value;
    }

    public set internalSchema(value: SchemaDto | undefined) {
        this.schema$.next(value);
    }

    public get properties() {
        return this.field.properties as ComponentFieldPropertiesDto;
    }

    constructor(globals: FormGlobals, field: FieldDto, fieldPath: string, isOptional: boolean, rules: RulesProvider, partition: string) {
        super(globals, field, fieldPath, isOptional,
            new ComponentRulesProvider(fieldPath, rules, () => this.schema),
            new ComponentTemplate(() => this),
            partition);

        this.form.reset(undefined);
    }

    public selectSchema(schemaId: string) {
        this.form.patchValue({ schemaId });
    }
}

class ComponentTemplate extends ObjectTemplate<ComponentForm> {
    public getSchema(value: any, model: ComponentForm) {
        return model.globals.schemas[value?.schemaId]?.fields;
    }

    protected setControlsCore(schema: ReadonlyArray<FieldDto>, value: any, model: ComponentForm, form: UntypedFormGroup) {
        form.setControl('schemaId', new UntypedFormControl());

        this.model.internalSchema = model.globals.schemas[value?.schemaId];

        super.setControlsCore(schema, value, model, form);
    }

    protected clearControlsCore(model: ComponentForm) {
        this.model.internalSchema = undefined;

        super.clearControlsCore(model);
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

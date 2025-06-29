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
import { AppLanguageDto, ComponentFieldPropertiesDto, FieldDto, fieldInvariant, LanguageDto, SchemaDto, TableField } from '../model';
import { ComponentRulesProvider, RootRulesProvider } from './contents.form-rules';
import { AbstractContentForm, AbstractContentFormState, AnyFieldDto, contentTranslationStatus, ControlArgs, FieldSection, fieldTranslationStatus, FormGlobals, groupFields, PartitionConfig } from './contents.forms-helpers';
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
    private readonly editableFields: ReadonlyArray<FieldDto>;

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
            const request = {} as Record<string, any>;

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

    public readonly sections: ReadonlyArray<FieldSection<FieldDto, FieldForm>>;

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

        const form = this.form;
        const rules = new RootRulesProvider(schema);

        this.sections = groupFields(schema.fields).map(({ separator, fields }) => {
            const forms: FieldForm[] = [];

            for (const field of fields) {
                const childForm =
                    new FieldForm({
                        field,
                        globals,
                        isOptional: false,
                        partition: '',
                        path: field.name,
                        rules,
                    });

                form.setControl(field.name, childForm.form);
                forms.push(childForm);
                this.fields[field.name] = childForm;
            }

            return new FieldSection<FieldDto, FieldForm>(separator, forms);
        });

        value$(this.form).pipe(debounceTimeSafe(debounce), distinctUntilChanged(Types.equals)).subscribe(value => {
            this.updateValue(value);
        });

        this.updateInitialData();
    }

    public get(field: string | FieldDto): FieldForm | undefined {
        if (Types.is(field, FieldDto)) {
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

        this.updateValue(this.form.value);
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

    private updateValue(value: any) {
        this.valueChange$.next(value);

        this.updateState(value);
    }

    private updateInitialData() {
        this.initialData = this.form.value;
    }
}

export class FieldForm extends AbstractContentForm<FieldDto, UntypedFormGroup> {
    private readonly partitions: { [partition: string]: FieldItemForm } = {};
    private isRequired: boolean;

    public readonly translationStatus =
        value$(this.form).pipe(map(x => fieldTranslationStatus(x)));

    constructor(args: ControlArgs<FieldDto>) {
        super(args, FieldForm.buildForm());

        for (const { key: partition, isOptional } of args.globals.partitions.getAll(args.field)) {
            const childForm =
                buildForm({
                    ...args,
                    isOptional,
                    partition,
                    path: this.relativePath(partition),
                });

            this.partitions[partition] = childForm;

            this.form.setControl(partition, childForm.form);
        }

        this.isRequired = !!args.field.properties.isRequired;
    }

    public get(language: string | LanguageDto | AppLanguageDto) {
        if (this.field.isLocalizable) {
            return this.partitions[(language as any)['iso2Code'] || language];
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

export class FieldValueForm extends AbstractContentForm<AnyFieldDto, UntypedFormControl> {
    private isRequired = false;

    constructor(args: ControlArgs) {
        super(args, FieldValueForm.buildControl(args, args.partition));

        this.isRequired = !!args.field.properties.isRequired && !args.isOptional;
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

    private static buildControl(args: ControlArgs, partition: string) {
        const value = FieldDefaultValue.get(args.field, partition);

        const validators = FieldsValidators.create(args.field, args.isOptional);

        if (args.globals.remoteValidator) {
            validators.push(args.globals.remoteValidator);
        }

        return new UntypedFormControl(value, { validators });
    }
}

export class FieldArrayForm extends AbstractContentForm<AnyFieldDto, TemplatedFormArray> {
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

    constructor(args: ControlArgs,
        public readonly isComponents: boolean,
    ) {
        super(args,
            new TemplatedFormArray(
                new ArrayTemplate(() => this),
                FieldsValidators.create(args.field, args.isOptional)));

        this.form.setValue(FieldDefaultValue.get(args.field, args.partition), { emitEvent: false });

        (this.form.template as any)['form'] = this;
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
        const model = this.model;

        const child = this.model.isComponents ?
            this.createComponent(model) :
            this.createItem(model);

        model.internalItems = [...this.model.items, child];
        return child.form;
    }

    public removeControl(index: number) {
        const model = this.model;
        model.internalItems = model.items.filter((_, i) => i !== index);
    }

    public clearControls() {
        const model = this.model;
        model.internalItems = [];
    }

    private createItem(model: FieldArrayForm) {
        return new ArrayItemForm(model.args);
    }

    private createComponent(model: FieldArrayForm) {
        return new ComponentForm(model.args);
    }
}

export type FieldItemForm = ComponentForm | FieldValueForm | FieldArrayForm;

type FieldMap = { [name: string]: FieldItemForm };

export class ObjectFormBase extends AbstractContentForm<AnyFieldDto, TemplatedFormGroup> {
    private readonly sections$ = new BehaviorSubject<ReadonlyArray<FieldSection<AnyFieldDto, FieldItemForm>>>([]);
    private readonly fields$ = new BehaviorSubject<FieldMap>({});

    public get sectionsChanges(): Observable<ReadonlyArray<FieldSection<AnyFieldDto, FieldItemForm>>> {
        return this.sections$;
    }

    public get sections() {
        return this.sections$.value;
    }

    public set internalFieldSections(value: ReadonlyArray<FieldSection<AnyFieldDto, FieldItemForm>>) {
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

    constructor(args: ControlArgs, template: ObjectTemplate) {
        super(args, ObjectFormBase.buildControl(template));
    }

    public get(field: string | { name: string }): FieldItemForm | undefined {
        return this.fields[(field as any)['name'] || field];
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
    private currentSchema: ReadonlyArray<AnyFieldDto> | undefined;

    protected get model() {
        return this.modelProvider();
    }

    constructor(
        private readonly modelProvider: () => T,
    ) {
    }

    protected abstract getSchema(value: any, model: T): ReadonlyArray<AnyFieldDto> | undefined;

    public setControls(form: UntypedFormGroup, value: any) {
        const schema = this.getSchema(value, this.model);

        if (this.currentSchema !== schema) {
            const model = this.model;

            this.clearControlsCore(model);

            if (schema) {
                this.setControlsCore(schema, value, model, form);
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

    protected setControlsCore(schema: ReadonlyArray<AnyFieldDto>, _: any, model: T, form: UntypedFormGroup) {
        const fieldByName: FieldMap = {};
        const fieldSections: FieldSection<AnyFieldDto, FieldItemForm>[] = [];

        for (const { separator, fields } of groupFields(schema)) {
            const forms: FieldItemForm[] = [];

            for (const field of fields) {
                const childForm = buildForm({
                    ...model.args,
                    field,
                    path: model.relativePath(field.name),
                });

                form.setControl(field.name, childForm.form);
                forms.push(childForm);
                fieldByName[field.name] = childForm;
            }

            fieldSections.push(new FieldSection<AnyFieldDto, FieldItemForm>(separator, forms));
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

export class ArrayItemForm extends ObjectFormBase {
    constructor(args: ControlArgs) {
        super(args, new ArrayItemTemplate(() => this));
        this.form.build({});
    }
}

class ArrayItemTemplate extends ObjectTemplate<ArrayItemForm> {
    public getSchema() {
        if (Types.is(this.model.field, FieldDto)) {
            return this.model.field.nested || [];
        } else {
            return [];
        }
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

    constructor(args: ControlArgs) {
        super({
            ...args,
            rules: new ComponentRulesProvider(args.path, args.rules, () => this.schema),
        }, new ComponentTemplate(() => this));

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

function buildForm(args: ControlArgs) {
    switch (args.field.properties.fieldType) {
        case 'Array':
            return new FieldArrayForm(args, false);
        case 'Component':
            return new ComponentForm(args);
        case 'Components':
            return new FieldArrayForm(args, true);
        default:
            return new FieldValueForm(args);
    }
}

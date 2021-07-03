/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Form, getRawValue, Types, UndefinableFormArray, UndefinableFormGroup, valueAll$ } from '@app/framework';
import { BehaviorSubject, Observable } from 'rxjs';
import { debounceTime, onErrorResumeNext } from 'rxjs/operators';
import { AppLanguageDto } from './../services/app-languages.service';
import { LanguageDto } from './../services/languages.service';
import { FieldDto, RootFieldDto, SchemaDto, TableField } from './../services/schemas.service';
import { ComponentFieldPropertiesDto, fieldInvariant } from './../services/schemas.types';
import { AbstractContentForm, AbstractContentFormState, CompiledRule, FieldSection, FormGlobals, PartitionConfig } from './contents.forms-helpers';
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

    constructor(languages: ReadonlyArray<AppLanguageDto>, schema: SchemaDto, schemas: { [id: string ]: SchemaDto },
        private context: any, debounce = 100,
    ) {
        super(new FormGroup({}));

        const globals: FormGlobals = {
            allRules: schema.fieldRules.map(x => new CompiledRule(x)),
            schema,
            schemas,
            partitions: new PartitionConfig(languages),
            remoteValidator: this.remoteValidator,
        };

        const sections: FieldSection<RootFieldDto, FieldForm>[] = [];

        let currentSeparator: RootFieldDto | undefined;
        let currentFields: FieldForm[] = [];

        for (const field of schema.fields) {
            if (field.properties.isContentField) {
                const childPath = field.name;
                const childForm = new FieldForm(globals, childPath, field);

                currentFields.push(childForm);

                this.fields[field.name] = childForm;

                this.form.setControl(field.name, childForm.form);
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
    private readonly partitions: { [partition: string]: FieldItemForm } = {};
    private isRequired: boolean;

    constructor(globals: FormGlobals, fieldPath: string, field: RootFieldDto) {
        super(globals, fieldPath, field, FieldForm.buildForm(), false);

        for (const { key, isOptional } of globals.partitions.getAll(field)) {
            const childPath = `${fieldPath}.${key}`;
            const childForm = buildForm(this.globals, childPath, field, isOptional, key);

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

                    if (this.globals.remoteValidator) {
                        validators.push(this.globals.remoteValidator);
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
        for (const key of Object.keys(this.partitions)) {
            this.partitions[key].prepareLoad(value?.[key]);
        }
    }

    private static buildForm() {
        return new FormGroup({});
    }
}

export class FieldValueForm extends AbstractContentForm<FieldDto, FormControl> {
    private isRequired = false;

    constructor(globals: FormGlobals, path: string, field: FieldDto,
        isOptional: boolean, partition: string,
    ) {
        super(globals, path, field,
            FieldValueForm.buildControl(field, isOptional, partition, globals),
            isOptional,
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

    constructor(globals: FormGlobals, path: string, field: FieldDto, isOptional: boolean,
        private readonly partition: string,
        private readonly isComponents: boolean,
    ) {
        super(globals, path, field,
            FieldArrayForm.buildControl(field, isOptional),
            isOptional,
        );
    }

    public get(index: number) {
        return this.items[index];
    }

    public addCopy(source: ObjectForm) {
        if (this.isComponents) {
            const child = new ComponentForm(this.globals, this.fieldPath, this.field as RootFieldDto, this.isOptional, this.partition);

            child.load(getRawValue(source.form));

            this.addChild(child);
        } else {
            const child = new ArrayItemForm(this.globals, this.fieldPath, this.field as RootFieldDto, this.isOptional, this.partition);

            child.load(getRawValue(source.form));

            this.addChild(child);
        }
    }

    public addComponent(schemaId?: string) {
        const child = new ComponentForm(this.globals, this.fieldPath, this.field, this.isOptional, this.partition, schemaId);

        this.addChild(child);
    }

    public addItem() {
        const child = new ArrayItemForm(this.globals, this.fieldPath, this.field as RootFieldDto, this.isOptional, this.partition);

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

    protected updateCustomState(context: any, state: AbstractContentFormState) {
        for (const item of this.items) {
            item.updateState(context, state);
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

    constructor(globals: FormGlobals, path: string, field: TField, isOptional: boolean,
        private readonly partition: string,
    ) {
        super(globals, path, field, ObjectForm.buildControl(field, isOptional, false), isOptional);
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

            let currentSeparator: FieldDto | undefined;
            let currentFields: FieldItemForm[] = [];

            for (const field of schema) {
                if (field.properties.isContentField) {
                    const childPath = `${this.fieldPath}.${field.name}`;
                    const childForm = buildForm(this.globals, childPath, field, this.isOptional, this.partition);

                    this.form.setControl(field.name, childForm.form);

                    currentFields.push(childForm);

                    this.fields[field.name] = childForm;
                } else {
                    this.fieldSections.push(new FieldSection<FieldDto, FieldItemForm>(currentSeparator, currentFields));

                    currentFields = [];
                    currentSeparator = field;
                }
            }

            if (currentFields.length > 0) {
                this.fieldSections.push(new FieldSection<FieldDto, FieldItemForm>(currentSeparator, currentFields));
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

    protected updateCustomState(context: any, state: AbstractContentFormState) {
        const itemData = this.form.getRawValue();

        for (const field of Object.values(this.fields)) {
            field.updateState({ ...context, itemData }, state);
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
    constructor(globals: FormGlobals, path: string, field: RootFieldDto, isOptional: boolean, partition: string) {
        super(globals, path, field, isOptional, partition);

        this.init(field.nested);
    }
}

export class ComponentForm extends ObjectForm {
    private schemaId?: string;

    public readonly properties: ComponentFieldPropertiesDto;

    public get schema() {
        return this.globals.schemas[this.schemaId!];
    }

    constructor(globals: FormGlobals, path: string, field: FieldDto, isOptional: boolean, partition: string, schemaId?: string) {
        super(globals, path, field, isOptional, partition);

        this.properties = field.properties as ComponentFieldPropertiesDto;

        if (schemaId) {
            this.selectSchema(schemaId);
        }
    }

    public selectSchema(schemaId?: string) {
        if (this.schemaId !== schemaId) {
            this.schemaId = schemaId;

            if (this.schema) {
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

function buildForm(globals: FormGlobals, path: string, field: FieldDto, isOptional: boolean, partition: string) {
    switch (field.properties.fieldType) {
        case 'Array':
            return new FieldArrayForm(globals, path, field, isOptional, partition, false);
        case 'Component':
            return new ComponentForm(globals, path, field, isOptional, partition);
        case 'Components':
            return new FieldArrayForm(globals, path, field, isOptional, partition, true);
        default:
            return new FieldValueForm(globals, path, field, isOptional, partition);
    }
}

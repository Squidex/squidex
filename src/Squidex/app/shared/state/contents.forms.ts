/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:prefer-for-of

import { FormArray, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';

import {
    DateTime,
    Form,
    formControls,
    ImmutableArray,
    Types,
    ValidatorsEx,
    value$
} from '@app/framework';

import { ContentDto, ContentReferencesValue } from '../services/contents.service';
import { LanguageDto } from '../services/languages.service';
import { AppLanguageDto } from './../services/app-languages.service';
import { FieldDto, RootFieldDto, SchemaDetailsDto } from './../services/schemas.service';
import {
    ArrayFieldPropertiesDto,
    AssetsFieldPropertiesDto,
    BooleanFieldPropertiesDto,
    DateTimeFieldPropertiesDto,
    fieldInvariant,
    FieldPropertiesVisitor,
    GeolocationFieldPropertiesDto,
    JsonFieldPropertiesDto,
    NumberFieldPropertiesDto,
    ReferencesFieldPropertiesDto,
    StringFieldPropertiesDto,
    TagsFieldPropertiesDto,
    UIFieldPropertiesDto
} from './../services/schemas.types';

export class HtmlValue {
    constructor(
        public readonly html: string
    ) {
    }
}

export class SaveQueryForm extends Form<FormGroup, any> {
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

export type FieldValue = string | HtmlValue;

export function getContentValue(content: ContentDto, language: LanguageDto, field: RootFieldDto, allowHtml = true): { value: any, formatted: FieldValue } {
    if (content.referenceData) {
        const reference = content.referenceData[field.name];

        if (reference) {
            let fieldValue: ContentReferencesValue;

            if (field.isLocalizable) {
                fieldValue = reference[language.iso2Code];
            } else {
                fieldValue = reference[fieldInvariant];
            }

            let value: string | undefined =
                fieldValue ?
                fieldValue[language.iso2Code] :
                undefined;

            value = value || '- No Value -';

            return { value, formatted: value };
        }
    }

    const contentField = content.dataDraft[field.name];

    if (contentField) {
        let value: any;

        if (field.isLocalizable) {
            value = contentField[language.iso2Code];
        } else {
            value = contentField[fieldInvariant];
        }

        let formatted: any;

        if (Types.isUndefined(value)) {
            formatted = value || '';
        } else {
            formatted = FieldFormatter.format(field, value, allowHtml);
        }

        return { value, formatted };
    }

    return { value: undefined, formatted: '' };
}

export class FieldFormatter implements FieldPropertiesVisitor<FieldValue> {
    private constructor(
        private readonly value: any,
        private readonly allowHtml: boolean
    ) {
    }

    public static format(field: FieldDto, value: any, allowHtml = true) {
        if (value === null || value === undefined) {
            return '';
        }

        return field.properties.accept(new FieldFormatter(value, allowHtml));
    }

    public visitDateTime(properties: DateTimeFieldPropertiesDto): FieldValue {
        try {
            const parsed = DateTime.parseISO_UTC(this.value);

            if (properties.editor === 'Date') {
                return parsed.toUTCStringFormat('YYYY-MM-DD');
            } else {
                return parsed.toUTCStringFormat('YYYY-MM-DD HH:mm:ss');
            }
        } catch (ex) {
            return this.value;
        }
    }

    public visitArray(_: ArrayFieldPropertiesDto): string {
        if (this.value.length) {
            return `${this.value.length} Item(s)`;
        } else {
            return '0 Items';
        }
    }

    public visitAssets(_: AssetsFieldPropertiesDto): string {
        if (this.value.length) {
            return `${this.value.length} Asset(s)`;
        } else {
            return '0 Assets';
        }
    }

    public visitReferences(_: ReferencesFieldPropertiesDto): string {
        if (this.value.length) {
            return `${this.value.length} Reference(s)`;
        } else {
            return '0 References';
        }
    }

    public visitTags(_: TagsFieldPropertiesDto): string {
        if (this.value.length) {
            return this.value.join(', ');
        } else {
            return '';
        }
    }

    public visitBoolean(_: BooleanFieldPropertiesDto): string {
        return this.value ? 'Yes' : 'No';
    }

    public visitGeolocation(_: GeolocationFieldPropertiesDto): string {
        return `${this.value.longitude}, ${this.value.latitude}`;
    }

    public visitJson(_: JsonFieldPropertiesDto): string {
        return '<Json />';
    }

    public visitNumber(properties: NumberFieldPropertiesDto): FieldValue {
        if (Types.isNumber(this.value) && properties.editor === 'Stars' && this.allowHtml) {
            if (this.value <= 0 || this.value > 6) {
                return new HtmlValue(`&#9733; ${this.value}`);
            } else {
                let html = '';

                for (let i = 0; i < this.value; i++) {
                    html += '&#9733; ';
                }

                return new HtmlValue(html);
            }
        }
        return `${this.value}`;
    }

    public visitString(_: StringFieldPropertiesDto): any {
        return this.value;
    }

    public visitUI(_: UIFieldPropertiesDto): any {
        return '';
    }
}

export class FieldsValidators implements FieldPropertiesVisitor<ValidatorFn[]> {
    private constructor(
        private readonly isOptional: boolean
    ) {
    }

    public static create(field: FieldDto, isOptional: boolean) {
        const validators = field.properties.accept(new FieldsValidators(isOptional));

        if (field.properties.isRequired && !isOptional) {
            validators.push(Validators.required);
        }

        return validators;
    }

    public visitNumber(properties: NumberFieldPropertiesDto): ValidatorFn[] {
        const validators: ValidatorFn[] = [
            ValidatorsEx.between(properties.minValue, properties.maxValue)
        ];

        if (properties.allowedValues && properties.allowedValues.length > 0) {
            const values: (number | null)[] = properties.allowedValues;

            if (properties.isRequired && !this.isOptional) {
                validators.push(ValidatorsEx.validValues(values));
            } else {
                validators.push(ValidatorsEx.validValues(values.concat([null])));
            }
        }

        return validators;
    }

    public visitString(properties: StringFieldPropertiesDto): ValidatorFn[] {
        const validators: ValidatorFn[] = [
            ValidatorsEx.betweenLength(properties.minLength, properties.maxLength)
        ];

        if (properties.pattern && properties.pattern.length > 0) {
            validators.push(ValidatorsEx.pattern(properties.pattern, properties.patternMessage));
        }

        if (properties.allowedValues && properties.allowedValues.length > 0) {
            const values: (string | null)[] = properties.allowedValues;

            if (properties.isRequired && !this.isOptional) {
                validators.push(ValidatorsEx.validValues(values));
            } else {
                validators.push(ValidatorsEx.validValues(values.concat([null])));
            }
        }

        return validators;
    }

    public visitArray(properties: ArrayFieldPropertiesDto): ValidatorFn[] {
        const validators: ValidatorFn[] = [
            ValidatorsEx.betweenLength(properties.minItems, properties.maxItems)
        ];

        return validators;
    }

    public visitAssets(properties: AssetsFieldPropertiesDto): ValidatorFn[] {
        const validators: ValidatorFn[] = [
            ValidatorsEx.betweenLength(properties.minItems, properties.maxItems)
        ];

        if (!properties.allowDuplicates) {
            validators.push(ValidatorsEx.uniqueStrings());
        }

        return validators;
    }

    public visitReferences(properties: ReferencesFieldPropertiesDto): ValidatorFn[] {
        const validators: ValidatorFn[] = [
            ValidatorsEx.betweenLength(properties.minItems, properties.maxItems)
        ];

        if (!properties.allowDuplicates) {
            validators.push(ValidatorsEx.uniqueStrings());
        }

        return validators;
    }

    public visitTags(properties: TagsFieldPropertiesDto): ValidatorFn[] {
        const validators: ValidatorFn[] = [
            ValidatorsEx.betweenLength(properties.minItems, properties.maxItems)
        ];

        if (properties.allowedValues && properties.allowedValues.length > 0) {
            const values: (string | null)[] = properties.allowedValues;

            validators.push(ValidatorsEx.validArrayValues(values));
        }

        return validators;
    }

    public visitBoolean(_: BooleanFieldPropertiesDto): ValidatorFn[] {
        return [];
    }

    public visitDateTime(_: DateTimeFieldPropertiesDto): ValidatorFn[] {
        return [];
    }

    public visitGeolocation(_: GeolocationFieldPropertiesDto): ValidatorFn[] {
        return [];
    }

    public visitJson(_: JsonFieldPropertiesDto): ValidatorFn[] {
        return [];
    }

    public visitUI(_: UIFieldPropertiesDto): ValidatorFn[] {
        return [];
    }
}

export class FieldDefaultValue implements FieldPropertiesVisitor<any> {
    private constructor(
        private readonly now?: DateTime
    ) {
    }

    public static get(field: FieldDto, now?: DateTime) {
        return field.properties.accept(new FieldDefaultValue(now));
    }

    public visitDateTime(properties: DateTimeFieldPropertiesDto): any {
        const now = this.now || DateTime.now();

        if (properties.calculatedDefaultValue === 'Now') {
            return `${now.toUTCStringFormat('YYYY-MM-DDTHH:mm:ss')}Z`;
        } else if (properties.calculatedDefaultValue === 'Today') {
            return `${now.toUTCStringFormat('YYYY-MM-DD')}T00:00:00Z`;
        } else {
            return properties.defaultValue;
        }
    }

    public visitArray(_: ArrayFieldPropertiesDto): any {
        return null;
    }

    public visitAssets(_: AssetsFieldPropertiesDto): any {
        return null;
    }

    public visitBoolean(properties: BooleanFieldPropertiesDto): any {
        return properties.defaultValue;
    }

    public visitGeolocation(_: GeolocationFieldPropertiesDto): any {
        return null;
    }

    public visitJson(_: JsonFieldPropertiesDto): any {
        return null;
    }

    public visitNumber(properties: NumberFieldPropertiesDto): any {
        return properties.defaultValue;
    }

    public visitReferences(_: ReferencesFieldPropertiesDto): any {
        return null;
    }

    public visitString(properties: StringFieldPropertiesDto): any {
        return properties.defaultValue;
    }

    public visitTags(_: TagsFieldPropertiesDto): any {
        return null;
    }

    public visitUI(_: UIFieldPropertiesDto): any {
        return null;
    }
}

const NO_EMIT = { emitEvent: false };
const NO_EMIT_SELF = { emitEvent: false, onlySelf: true };

type Partition = { key: string, isOptional: boolean };

export class PartitionConfig {
    private readonly invariant: Partition[] = [{ key: fieldInvariant, isOptional: false }];
    private readonly languages: Partition[];

    constructor(languages: ImmutableArray<AppLanguageDto>) {
        this.languages = languages.values.map(l => this.get(l));
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

export class EditContentForm extends Form<FormGroup, any> {
    private readonly partitions: PartitionConfig;
    private initialData: any;

    public value = value$(this.form);

    constructor(languages: ImmutableArray<AppLanguageDto>,
        private readonly schema: SchemaDetailsDto
    ) {
        super(new FormGroup({}));

        this.partitions = new PartitionConfig(languages);

        for (const field of schema.fields) {
            if (field.properties.isContentField) {
                const fieldForm = new FormGroup({});
                const fieldDefault = FieldDefaultValue.get(field);

                for (let { key, isOptional } of this.partitions.getAll(field)) {
                    const fieldValidators = FieldsValidators.create(field, isOptional);

                    if (field.isArray) {
                        fieldForm.setControl(key, new FormArray([], fieldValidators));
                    } else {
                        fieldForm.setControl(key, new FormControl(fieldDefault, fieldValidators));
                    }
                }

                this.form.setControl(field.name, fieldForm);
            }
        }

        this.extractPrevData();
        this.enable();
    }

    public hasChanged() {
        const currentValue = this.form.getRawValue();

        return !Types.jsJsonEquals(this.initialData, currentValue);
    }

    public hasChanges(changes: any) {
        const currentValue = this.form.getRawValue();

        return !Types.jsJsonEquals(changes, currentValue);
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
            this.addArrayItem(partitionForm, field, this.partitions.get(language), source);
        }
    }

    private removeItem(partitionForm: FormArray, index: number) {
        partitionForm.removeAt(index);
    }

    private addArrayItem(partitionForm: FormArray, field: RootFieldDto, partition: Partition, source?: FormGroup) {
        const itemForm = new FormGroup({});

        for (let nestedField of field.nested) {
            if (nestedField.properties.isContentField) {
                let value = FieldDefaultValue.get(nestedField);

                if (source) {
                    const sourceField = source.get(nestedField.name);

                    if (sourceField) {
                        value = sourceField.value;
                    }
                }

                const nestedValidators = FieldsValidators.create(nestedField, partition.isOptional);
                const nestedForm = new FormControl(value, nestedValidators);

                if (nestedField.isDisabled) {
                    nestedForm.disable(NO_EMIT);
                }

                itemForm.setControl(nestedField.name, nestedForm);
            }
        }

        partitionForm.push(itemForm);
    }

    private findArrayItemForm(field: RootFieldDto, language: AppLanguageDto): FormArray | null {
        const fieldForm = this.form.get(field.name);

        if (!fieldForm) {
            return null;
        } else if (field.isLocalizable) {
            return fieldForm.get(language.iso2Code) as FormArray;
        } else {
            return fieldForm.get(fieldInvariant) as FormArray;
        }
    }

    public load(value: any, isInitial?: boolean) {
        for (let field of this.schema.fields) {
            if (field.isArray && field.nested.length > 0) {
                const fieldForm = this.form.get(field.name) as FormGroup;

                if (fieldForm) {
                    const fieldValue = value ? value[field.name] || {} : {};

                    for (let partition of this.partitions.getAll(field)) {
                        const { key, isOptional } = partition;

                        const partitionValidators = FieldsValidators.create(field, isOptional);
                        const partitionForm = new FormArray([], partitionValidators);

                        const partitionValue = fieldValue[key];

                        if (Types.isArray(partitionValue)) {
                            for (let i = 0; i < partitionValue.length; i++) {
                                this.addArrayItem(partitionForm, field, partition);
                            }
                        }

                        fieldForm.setControl(key, partitionForm);
                    }
                }
            }
        }

        super.load(value);

        if (isInitial) {
            this.extractPrevData();
        }
    }

    public submitCompleted(options?: { newValue?: any, noReset?: boolean }) {
        super.submitCompleted(options);

        this.extractPrevData();
    }

    protected disable() {
        this.form.disable(NO_EMIT);
    }

    protected enable() {
        this.form.enable(NO_EMIT_SELF);

        for (const field of this.schema.fields) {
            const fieldForm = this.form.get(field.name);

            if (fieldForm) {
                if (field.isArray) {
                    fieldForm.enable(NO_EMIT_SELF);

                    for (let partitionForm of formControls(fieldForm)) {
                        partitionForm.enable(NO_EMIT_SELF);

                        for (let itemForm of formControls(partitionForm)) {
                            itemForm.enable(NO_EMIT_SELF);

                            for (let nestedField of field.nested) {
                                const nestedForm = itemForm.get(nestedField.name);

                                if (nestedForm) {
                                    if (nestedField.isDisabled) {
                                        nestedForm.disable(NO_EMIT);
                                    } else {
                                        nestedForm.enable(NO_EMIT);
                                    }
                                }
                            }
                        }
                    }
                } else if (field.isDisabled) {
                    fieldForm.disable(NO_EMIT);
                } else {
                    fieldForm.enable(NO_EMIT);
                }
            }
        }
    }

    private extractPrevData() {
        this.initialData = this.form.getRawValue();
    }
}

export class PatchContentForm extends Form<FormGroup, any> {
    constructor(
        private readonly schema: SchemaDetailsDto,
        private readonly language: AppLanguageDto
    ) {
        super(new FormGroup({}));

        for (let field of this.schema.listFieldsEditable) {
            const validators = FieldsValidators.create(field, this.language.isOptional);

            this.form.setControl(field.name, new FormControl(undefined, validators));
        }
    }

    public submit() {
        const result = super.submit();

        if (result) {
            const request = {};

            for (let field of this.schema.listFieldsEditable) {
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
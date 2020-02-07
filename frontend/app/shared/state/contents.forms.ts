/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:prefer-for-of

import { FormArray, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { BehaviorSubject } from 'rxjs';

import {
    DateTime,
    Form,
    formControls,
    Types,
    ValidatorsEx
} from '@app/framework';

import { AppLanguageDto } from './../services/app-languages.service';
import { ContentDto, ContentReferencesValue } from './../services/contents.service';
import { LanguageDto } from './../services/languages.service';
import { FieldDto, RootFieldDto, SchemaDetailsDto, TableField } from './../services/schemas.service';
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

export type FieldValue = string | HtmlValue;

export function getContentValue(content: ContentDto, language: LanguageDto, field: RootFieldDto, allowHtml = true): { value: any, formatted: FieldValue } {
    if (content.referenceData) {
        const reference = content.referenceData[field.name];

        const isAssets = field.properties.fieldType === 'Assets';

        if (reference && (!isAssets || allowHtml)) {
            let fieldValue: ContentReferencesValue;

            if (field.isLocalizable) {
                fieldValue = reference[language.iso2Code];
            } else {
                fieldValue = reference[fieldInvariant];
            }

            let value: string | undefined = undefined;

            if (Types.isObject(fieldValue)) {
                value = fieldValue[language.iso2Code];
            } else if (Types.isString(fieldValue)) {
                value = fieldValue;
            }

            let formatted: FieldValue = value!;

            if (value) {
                if (Types.isString(value) && isAssets) {
                    formatted = new HtmlValue(`<img src="${value}?width=50&height=50" />`);
                }
            } else {
                value = formatted = '- No Value -';
            }

            return { value, formatted };
        }
    }

    const contentField = content.data[field.name];

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

    public visitBoolean(_: BooleanFieldPropertiesDto): string {
        return this.value ? 'Yes' : 'No';
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

    public visitString(properties: StringFieldPropertiesDto): any {
        if (properties.editor === 'StockPhoto' && this.allowHtml && this.value) {
            const src = thumbnail(this.value, undefined, 50);

            if (src) {
                return new HtmlValue(`<img src="${src}" />`);
            }
        }

        return this.value;
    }

    public visitUI(_: UIFieldPropertiesDto): any {
        return '';
    }
}

export function thumbnail(url: string, width?: number, height?: number) {
    if (url && url.startsWith('https://images.unsplash.com')) {
        if (width) {
            return `${url}&q=80&fm=jpg&crop=entropy&cs=tinysrgb&w=${width}&fit=max`;
        }

        if (height) {
            return `${url}&q=80&fm=jpg&crop=entropy&cs=tinysrgb&h=${height}&fit=max`;
        }
    }

    return undefined;
}

export class FieldsValidators implements FieldPropertiesVisitor<ReadonlyArray<ValidatorFn>> {
    private constructor(
        private readonly isOptional: boolean
    ) {
    }

    public static create(field: FieldDto, isOptional: boolean) {
        const validators = [...field.properties.accept(new FieldsValidators(isOptional))];

        if (field.properties.isRequired && !isOptional) {
            validators.push(Validators.required);
        }

        return validators;
    }

    public visitArray(properties: ArrayFieldPropertiesDto): ReadonlyArray<ValidatorFn> {
        const validators: ValidatorFn[] = [
            ValidatorsEx.betweenLength(properties.minItems, properties.maxItems)
        ];

        return validators;
    }

    public visitAssets(properties: AssetsFieldPropertiesDto): ReadonlyArray<ValidatorFn> {
        const validators: ValidatorFn[] = [
            ValidatorsEx.betweenLength(properties.minItems, properties.maxItems)
        ];

        if (!properties.allowDuplicates) {
            validators.push(ValidatorsEx.uniqueStrings());
        }

        return validators;
    }

    public visitBoolean(_: BooleanFieldPropertiesDto): ReadonlyArray<ValidatorFn> {
        return [];
    }

    public visitDateTime(_: DateTimeFieldPropertiesDto): ReadonlyArray<ValidatorFn> {
        return [];
    }

    public visitGeolocation(_: GeolocationFieldPropertiesDto): ReadonlyArray<ValidatorFn> {
        return [];
    }

    public visitJson(_: JsonFieldPropertiesDto): ReadonlyArray<ValidatorFn> {
        return [];
    }

    public visitNumber(properties: NumberFieldPropertiesDto): ReadonlyArray<ValidatorFn> {
        const validators: ValidatorFn[] = [
            ValidatorsEx.between(properties.minValue, properties.maxValue)
        ];

        if (properties.allowedValues && properties.allowedValues.length > 0) {
            const values: ReadonlyArray<(number | null)> = properties.allowedValues;

            if (properties.isRequired && !this.isOptional) {
                validators.push(ValidatorsEx.validValues(values));
            } else {
                validators.push(ValidatorsEx.validValues(values.concat([null])));
            }
        }

        return validators;
    }

    public visitReferences(properties: ReferencesFieldPropertiesDto): ReadonlyArray<ValidatorFn> {
        const validators: ValidatorFn[] = [
            ValidatorsEx.betweenLength(properties.minItems, properties.maxItems)
        ];

        if (!properties.allowDuplicates) {
            validators.push(ValidatorsEx.uniqueStrings());
        }

        return validators;
    }

    public visitString(properties: StringFieldPropertiesDto): ReadonlyArray<ValidatorFn> {
        const validators: ValidatorFn[] = [
            ValidatorsEx.betweenLength(properties.minLength, properties.maxLength)
        ];

        if (properties.pattern && properties.pattern.length > 0) {
            validators.push(ValidatorsEx.pattern(properties.pattern, properties.patternMessage));
        }

        if (properties.allowedValues && properties.allowedValues.length > 0) {
            const values: ReadonlyArray<string | null> = properties.allowedValues;

            if (properties.isRequired && !this.isOptional) {
                validators.push(ValidatorsEx.validValues(values));
            } else {
                validators.push(ValidatorsEx.validValues(values.concat([null])));
            }
        }

        return validators;
    }

    public visitTags(properties: TagsFieldPropertiesDto): ReadonlyArray<ValidatorFn> {
        const validators: ValidatorFn[] = [
            ValidatorsEx.betweenLength(properties.minItems, properties.maxItems)
        ];

        if (properties.allowedValues && properties.allowedValues.length > 0) {
            const values: ReadonlyArray<string | null> = properties.allowedValues;

            validators.push(ValidatorsEx.validArrayValues(values));
        }

        return validators;
    }

    public visitUI(_: UIFieldPropertiesDto): ReadonlyArray<ValidatorFn> {
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

export class EditContentForm extends Form<FormGroup, any> {
    private readonly partitions: PartitionConfig;
    private initialData: any;

    public value = new BehaviorSubject<any>(this.form.value);

    constructor(languages: ReadonlyArray<AppLanguageDto>,
        private readonly schema: SchemaDetailsDto
    ) {
        super(new FormGroup({}));

        this.form.valueChanges.subscribe(value => {
            this.value.next(value);
        });

        this.partitions = new PartitionConfig(languages);

        for (const field of schema.fields) {
            if (field.properties.isContentField) {
                const fieldForm = new FormGroup({});
                const fieldDefault = FieldDefaultValue.get(field);

                for (const { key, isOptional } of this.partitions.getAll(field)) {
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

        return !Types.equals(this.initialData, currentValue);
    }

    public hasChanges(changes: any) {
        const currentValue = this.form.getRawValue();

        return !Types.equals(changes, currentValue);
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

        for (const nestedField of field.nested) {
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
        for (const field of this.schema.fields) {
            if (field.isArray && field.nested.length > 0) {
                const fieldForm = this.form.get(field.name) as FormGroup;

                if (fieldForm) {
                    const fieldValue = value ? value[field.name] || {} : {};

                    for (const partition of this.partitions.getAll(field)) {
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

                    for (const partitionForm of formControls(fieldForm)) {
                        partitionForm.enable(NO_EMIT_SELF);

                        for (const itemForm of formControls(partitionForm)) {
                            itemForm.enable(NO_EMIT_SELF);

                            for (const nestedField of field.nested) {
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
    private readonly editableFields: ReadonlyArray<RootFieldDto>;

    constructor(
        private readonly listFields: ReadonlyArray<TableField>,
        private readonly language: AppLanguageDto
    ) {
        super(new FormGroup({}));

        this.editableFields = <any>this.listFields.filter(x => Types.is(x, RootFieldDto) && x.isInlineEditable);

        for (const field of this.editableFields) {
            const validators = FieldsValidators.create(field, this.language.isOptional);

            this.form.setControl(field.name, new FormControl(undefined, validators));
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
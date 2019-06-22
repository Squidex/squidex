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

export class SaveQueryForm extends Form<FormGroup, any> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.required
                ]
            ]
        }));
    }
}

export class FieldFormatter implements FieldPropertiesVisitor<string> {
    constructor(
        private readonly value: any
    ) {
    }

    public static format(field: FieldDto, value: any) {
        if (value === null || value === undefined) {
            return '';
        }

        return field.properties.accept(new FieldFormatter(value));
    }

    public visitDateTime(properties: DateTimeFieldPropertiesDto): string {
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

    public visitArray(properties: ArrayFieldPropertiesDto): string {
        if (this.value.length) {
            return `${this.value.length} Item(s)`;
        } else {
            return '0 Items';
        }
    }

    public visitAssets(properties: AssetsFieldPropertiesDto): string {
        if (this.value.length) {
            return `${this.value.length} Asset(s)`;
        } else {
            return '0 Assets';
        }
    }

    public visitReferences(properties: ReferencesFieldPropertiesDto): string {
        if (this.value.length) {
            return `${this.value.length} Reference(s)`;
        } else {
            return '0 References';
        }
    }

    public visitTags(properties: TagsFieldPropertiesDto): string {
        if (this.value.length) {
            return this.value.join(', ');
        } else {
            return '';
        }
    }

    public visitBoolean(properties: BooleanFieldPropertiesDto): string {
        return this.value ? 'Yes' : 'No';
    }

    public visitGeolocation(properties: GeolocationFieldPropertiesDto): string {
        return `${this.value.longitude}, ${this.value.latitude}`;
    }

    public visitJson(properties: JsonFieldPropertiesDto): string {
        return '<Json />';
    }

    public visitNumber(properties: NumberFieldPropertiesDto): string {
        return this.value;
    }

    public visitString(properties: StringFieldPropertiesDto): string {
        return this.value;
    }

    public visitUI(properties: UIFieldPropertiesDto): string {
        return this.value;
    }
}

export class FieldValidatorsFactory implements FieldPropertiesVisitor<ValidatorFn[]> {
    constructor(
        private readonly isOptional: boolean
    ) {
    }

    public static createValidators(field: FieldDto, isOptional: boolean) {
        const validators = field.properties.accept(new FieldValidatorsFactory(isOptional));

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

    public visitBoolean(properties: BooleanFieldPropertiesDto): ValidatorFn[] {
        return [];
    }

    public visitDateTime(properties: DateTimeFieldPropertiesDto): ValidatorFn[] {
        return [];
    }

    public visitGeolocation(properties: GeolocationFieldPropertiesDto): ValidatorFn[] {
        return [];
    }

    public visitJson(properties: JsonFieldPropertiesDto): ValidatorFn[] {
        return [];
    }

    public visitUI(properties: UIFieldPropertiesDto): ValidatorFn[] {
        return [];
    }
}

export class FieldDefaultValue implements FieldPropertiesVisitor<any> {
    constructor(
        private readonly now?: DateTime
    ) {
    }

    public visitDateTime(properties: DateTimeFieldPropertiesDto): any {
        const now = this.now || DateTime.now();

        if (properties.calculatedDefaultValue === 'Now') {
            return now.toUTCStringFormat('YYYY-MM-DDTHH:mm:ss') + 'Z';
        } else if (properties.calculatedDefaultValue === 'Today') {
            return now.toUTCStringFormat('YYYY-MM-DD');
        } else {
            return properties.defaultValue;
        }
    }

    public static get(field: FieldDto, now?: DateTime) {
        return field.properties.accept(new FieldDefaultValue(now));
    }

    public visitArray(properties: ArrayFieldPropertiesDto): any {
        return null;
    }

    public visitAssets(properties: AssetsFieldPropertiesDto): any {
        return null;
    }

    public visitBoolean(properties: BooleanFieldPropertiesDto): any {
        return properties.defaultValue;
    }

    public visitGeolocation(properties: GeolocationFieldPropertiesDto): any {
        return null;
    }

    public visitJson(properties: JsonFieldPropertiesDto): any {
        return null;
    }

    public visitNumber(properties: NumberFieldPropertiesDto): any {
        return properties.defaultValue;
    }

    public visitReferences(properties: ReferencesFieldPropertiesDto): any {
        return null;
    }

    public visitString(properties: StringFieldPropertiesDto): any {
        return properties.defaultValue;
    }

    public visitTags(properties: TagsFieldPropertiesDto): any {
        return null;
    }

    public visitUI(properties: UIFieldPropertiesDto): any {
        return null;
    }
}

export class EditContentForm extends Form<FormGroup, any> {
    public value =
        value$(this.form);

    constructor(
        private readonly schema: SchemaDetailsDto,
        private readonly languages: ImmutableArray<AppLanguageDto>
    ) {
        super(new FormGroup({}));

        for (const field of schema.fields) {
            if (field.properties.isContentField) {
                const fieldForm = new FormGroup({});
                const fieldDefault = FieldDefaultValue.get(field);

                const createControl = (isOptional: boolean) => {
                    const validators = FieldValidatorsFactory.createValidators(field, isOptional);

                    if (field.isArray) {
                        return new FormArray([], validators);
                    } else {
                        return new FormControl(fieldDefault, validators);
                    }
                };

                if (field.isLocalizable) {
                    for (let language of this.languages.values) {
                        fieldForm.setControl(language.iso2Code, createControl(language.isOptional));
                    }
                } else {
                    fieldForm.setControl(fieldInvariant, createControl(false));
                }

                this.form.setControl(field.name, fieldForm);
            }
        }

        this.enable();
    }

    public removeArrayItem(field: RootFieldDto, language: AppLanguageDto, index: number) {
        this.findArrayItemForm(field, language).removeAt(index);
    }

    public insertArrayItem(field: RootFieldDto, language: AppLanguageDto, source?: FormGroup) {
        if (field.nested.length > 0) {
            const formControl = this.findArrayItemForm(field, language);

            this.addArrayItem(field, language, formControl, source);
        }
    }

    private addArrayItem(field: RootFieldDto, language: AppLanguageDto | null, partitionForm: FormArray, source?: FormGroup) {
        const itemForm = new FormGroup({});

        let isOptional = field.isLocalizable && !!language && language.isOptional;

        for (let nested of field.nested) {
            if (nested.properties.isContentField) {
                const nestedValidators = FieldValidatorsFactory.createValidators(nested, isOptional);

                let value = FieldDefaultValue.get(nested);

                if (source) {
                    const sourceField = source.get(nested.name);

                    if (sourceField) {
                        value = sourceField.value;
                    }
                }

                itemForm.setControl(nested.name, new FormControl(value, nestedValidators));
            }
        }

        partitionForm.push(itemForm);
    }

    private findArrayItemForm(field: RootFieldDto, language: AppLanguageDto): FormArray {
        const fieldForm = this.form.get(field.name)!;

        if (field.isLocalizable) {
            return <FormArray>fieldForm.get(language.iso2Code)!;
        } else {
            return <FormArray>fieldForm.get(fieldInvariant);
        }
    }

    public loadContent(value: any) {
        for (let field of this.schema.fields) {
            if (field.isArray && field.nested.length > 0) {
                const fieldForm = <FormGroup>this.form.get(field.name);

                if (!fieldForm) {
                    continue;
                }

                const fieldValue = value ? value[field.name] || {} : {};

                const addControls = (key: string, language: AppLanguageDto | null) => {
                    const partitionValidators = FieldValidatorsFactory.createValidators(field, !!language && language.isOptional);
                    const partitionForm = new FormArray([], partitionValidators);

                    const partitionValue = fieldValue[key];

                    if (Types.isArray(partitionValue)) {
                        for (let i = 0; i < partitionValue.length; i++) {
                            this.addArrayItem(field, language, partitionForm);
                        }
                    }

                    fieldForm.setControl(key, partitionForm);
                };

                if (field.isLocalizable) {
                    for (let language of this.languages.values) {
                        addControls(language.iso2Code, language);
                    }
                } else {
                    addControls(fieldInvariant, null);
                }
            }
        }

        super.load(value);
    }

    protected enable() {
        if (this.schema.fields.length === 0) {
            this.form.enable();
            return;
        }

        for (const field of this.schema.fields) {
            const fieldForm = this.form.get(field.name);

            if (!fieldForm) {
                continue;
            }

            if (field.isArray) {
                fieldForm.enable();

                for (let partitionForm of formControls(fieldForm)) {
                    for (let itemForm of formControls(partitionForm)) {
                        for (let nested of field.nested) {
                            const nestedForm = itemForm.get(nested.name);

                            if (!nestedForm) {
                                continue;
                            }

                            if (nested.isDisabled) {
                                nestedForm.disable();
                            } else {
                                nestedForm.enable();
                            }
                        }
                    }
                }
            } else if (field.isDisabled) {
                fieldForm.disable();
            } else {
                fieldForm.enable();
            }
        }
    }
}

export class PatchContentForm extends Form<FormGroup, any> {
    constructor(
        private readonly schema: SchemaDetailsDto,
        private readonly language: AppLanguageDto
    ) {
        super(new FormGroup({}));

        for (let field of this.schema.listFieldsEditable) {
            const validators = FieldValidatorsFactory.createValidators(field, this.language.isOptional);

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
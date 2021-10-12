/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-useless-escape */

import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Form, ValidatorsEx, value$ } from '@app/framework';
import { map } from 'rxjs/operators';
import { AddFieldDto, CreateSchemaDto, FieldRule, SchemaDto, SchemaPropertiesDto, SynchronizeSchemaDto, UpdateSchemaDto } from './../services/schemas.service';
import { createProperties, FieldPropertiesDto, FieldPropertiesVisitor } from './../services/schemas.types';

type CreateCategoryFormType = { name: string };

export class CreateCategoryForm extends Form<FormGroup, CreateCategoryFormType> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: [''],
        }));
    }
}

export class CreateSchemaForm extends Form<FormGroup, CreateSchemaDto> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'i18n:schemas.schemaNameValidationMessage'),
                ],
            ],
            type: ['Default',
                [
                    Validators.required,
                ],
            ],
            initialCategory: undefined,
            importing: {},
        }));
    }

    public transformLoad(value: CreateSchemaDto) {
        const { name, type, category, ...importing } = value;

        return { name, type, importing, initialCategory: category };
    }

    public transformSubmit(value: any): CreateSchemaDto {
        const { name, type, importing, initialCategory } = value;

        return { name, type, category: initialCategory, ...importing };
    }
}

export class SynchronizeSchemaForm extends Form<FormGroup, SynchronizeSchemaDto> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            json: {},
            fieldsDelete: false,
            fieldsRecreate: false,
        }));
    }

    public loadSchema(schema: SchemaDto) {
        this.form.get('json')!.setValue(schema.export());
    }

    public transformSubmit(value: any) {
        return {
            ...value.json,
            noFieldDeletion: !value.fieldsDelete,
            noFieldRecreation: !value.fieldsRecreate,
        };
    }
}

export class ConfigureFieldRulesForm extends Form<FormArray, ReadonlyArray<FieldRule>, SchemaDto> {
    public get rulesControls(): ReadonlyArray<FormGroup> {
        return this.form.controls as any;
    }

    constructor(
        private readonly formBuilder: FormBuilder,
    ) {
        super(formBuilder.array([]));
    }

    public add(fieldNames: ReadonlyArray<string>) {
        this.form.push(
            this.formBuilder.group({
                action: ['Disable',
                    [
                        Validators.required,
                    ],
                ],
                field: [fieldNames[0],
                    [
                        Validators.required,
                    ],
                ],
                condition: ['',
                    [
                        Validators.required,
                    ],
                ],
            }));
    }

    public remove(index: number) {
        this.form.removeAt(index);
    }

    public transformLoad(value: Partial<SchemaDto>) {
        const result = value.fieldRules || [];

        while (this.form.controls.length < result.length) {
            this.add([]);
        }

        while (this.form.controls.length > result.length) {
            this.remove(this.form.controls.length - 1);
        }

        return result;
    }
}

type ConfigurePreviewUrlsFormType = { [name: string]: string };

export class ConfigurePreviewUrlsForm extends Form<FormArray, ConfigurePreviewUrlsFormType, SchemaDto> {
    public get previewControls(): ReadonlyArray<FormGroup> {
        return this.form.controls as any;
    }

    constructor(
        private readonly formBuilder: FormBuilder,
    ) {
        super(formBuilder.array([]));
    }

    public add() {
        this.form.push(
            this.formBuilder.group({
                name: ['',
                    [
                        Validators.required,
                    ],
                ],
                url: ['',
                    [
                        Validators.required,
                    ],
                ],
            }));
    }

    public remove(index: number) {
        this.form.removeAt(index);
    }

    public transformLoad(value: Partial<SchemaDto>) {
        const result = [];

        const previewUrls = value.previewUrls || {};

        const length = Object.keys(previewUrls).length;

        while (this.form.controls.length < length) {
            this.add();
        }

        while (this.form.controls.length > length) {
            this.remove(this.form.controls.length - 1);
        }

        for (const key in previewUrls) {
            if (previewUrls.hasOwnProperty(key)) {
                result.push({ name: key, url: previewUrls[key] });
            }
        }

        return result;
    }

    public transformSubmit(value: any) {
        const result = {};

        for (const item of value) {
            result[item.name] = item.url;
        }

        return result;
    }
}

export class EditSchemaScriptsForm extends Form<FormGroup, {}, object> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            query: '',
            create: '',
            change: '',
            delete: '',
            update: '',
        }));
    }
}

export class EditFieldForm extends Form<FormGroup, {}, FieldPropertiesDto> {
    constructor(formBuilder: FormBuilder, properties: FieldPropertiesDto) {
        super(EditFieldForm.buildForm(formBuilder, properties));
    }

    private static buildForm(formBuilder: FormBuilder, properties: FieldPropertiesDto) {
        const config = {
            label: ['',
                [
                    Validators.maxLength(100),
                ],
            ],
            hints: ['',
                [
                    Validators.maxLength(1000),
                ],
            ],
            placeholder: ['',
                [
                    Validators.maxLength(1000),
                ],
            ],
            editor: undefined,
            editorUrl: undefined,
            isRequired: false,
            isRequiredOnPublish: false,
            isHalfWidth: false,
            tags: [],
        };

        const visitor = new EditFieldFormVisitor(config);

        properties.accept(visitor);

        return formBuilder.group(config);
    }
}

export class EditFieldFormVisitor implements FieldPropertiesVisitor<any> {
    constructor(
        private readonly config: { [key: string]: any },
    ) {
    }

    public visitArray() {
        this.config['maxItems'] = undefined;
        this.config['minItems'] = undefined;
        this.config['uniqueFields'] = undefined;
    }

    public visitAssets() {
        this.config['allowDuplicates'] = undefined;
        this.config['allowedExtensions'] = undefined;
        this.config['aspectHeight'] = undefined;
        this.config['aspectHeight'] = undefined;
        this.config['aspectWidth'] = undefined;
        this.config['defaultValue'] = undefined;
        this.config['defaultValues'] = undefined;
        this.config['expectedType'] = undefined;
        this.config['folderId'] = undefined;
        this.config['maxHeight'] = undefined;
        this.config['maxItems'] = undefined;
        this.config['maxSize'] = undefined;
        this.config['maxWidth'] = undefined;
        this.config['minHeight'] = undefined;
        this.config['minItems'] = undefined;
        this.config['minSize'] = undefined;
        this.config['minWidth'] = undefined;
        this.config['previewMode'] = undefined;
        this.config['resolveFirst'] = undefined;
    }

    public visitBoolean() {
        this.config['inlineEditable'] = undefined;
        this.config['defaultValues'] = undefined;
        this.config['defaultValue'] = undefined;
    }

    public visitComponent() {
        this.config['schemaIds'] = undefined;
    }

    public visitComponents() {
        this.config['schemaIds'] = undefined;
        this.config['maxItems'] = undefined;
        this.config['minItems'] = undefined;
        this.config['uniqueFields'] = undefined;
    }

    public visitDateTime() {
        this.config['calculatedDefaultValue'] = undefined;
        this.config['defaultValue'] = undefined;
        this.config['defaultValues'] = undefined;
        this.config['format'] = undefined;
        this.config['maxValue'] = [undefined, ValidatorsEx.validDateTime()];
        this.config['minValue'] = [undefined, ValidatorsEx.validDateTime()];
    }

    public visitNumber() {
        this.config['allowedValues'] = undefined;
        this.config['defaultValue'] = undefined;
        this.config['defaultValues'] = undefined;
        this.config['inlineEditable'] = undefined;
        this.config['isUnique'] = undefined;
        this.config['maxValue'] = undefined;
        this.config['minValue'] = undefined;
    }

    public visitReferences() {
        this.config['allowDuplicates'] = undefined;
        this.config['defaultValue'] = undefined;
        this.config['defaultValues'] = undefined;
        this.config['maxItems'] = undefined;
        this.config['minItems'] = undefined;
        this.config['mustBePublished'] = false;
        this.config['resolveReference'] = false;
        this.config['schemaIds'] = undefined;
    }

    public visitString() {
        this.config['allowedValues'] = undefined;
        this.config['contentType'] = undefined;
        this.config['defaultValue'] = undefined;
        this.config['defaultValues'] = undefined;
        this.config['folderId'] = undefined;
        this.config['inlineEditable'] = undefined;
        this.config['isUnique'] = undefined;
        this.config['maxCharacters'] = undefined;
        this.config['maxLength'] = undefined;
        this.config['maxWords'] = undefined;
        this.config['minCharacters'] = undefined;
        this.config['minLength'] = undefined;
        this.config['minWords'] = undefined;
        this.config['pattern'] = undefined;
        this.config['patternMessage'] = undefined;
    }

    public visitTags() {
        this.config['allowedValues'] = undefined;
        this.config['defaultValue'] = undefined;
        this.config['defaultValues'] = undefined;
        this.config['maxItems'] = undefined;
        this.config['minItems'] = undefined;
    }

    public visitGeolocation() {
        return undefined;
    }

    public visitJson() {
        return undefined;
    }

    public visitUI() {
        return undefined;
    }
}

export class EditSchemaForm extends Form<FormGroup, UpdateSchemaDto, SchemaPropertiesDto> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            label: ['',
                [
                    Validators.maxLength(100),
                ],
            ],
            hints: ['',
                [
                    Validators.maxLength(1000),
                ],
            ],
            contentsSidebarUrl: '',
            contentSidebarUrl: '',
            contentEditorUrl: '',
            validateOnPublish: false,
            tags: [],
        }));
    }
}

export class AddFieldForm extends Form<FormGroup, AddFieldDto> {
    public isContentField = value$(this.form.get('type')!).pipe(map(x => x !== 'UI'));

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            type: ['String',
                [
                    Validators.required,
                ],
            ],
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-zA-Z0-9]+(\\-[a-zA-Z0-9]+)*', 'i18n:schemas.field.nameValidationMessage'),
                ],
            ],
            isLocalizable: false,
        }));
    }

    public transformLoad(value: Partial<AddFieldDto>) {
        const isLocalizable = value.partitioning === 'language';

        const type =
            value.properties ?
            value.properties.fieldType :
            'String';

        return { name: value.name, isLocalizable, type };
    }

    public transformSubmit(value: any) {
        const properties = createProperties(value.type);
        const partitioning = value.isLocalizable ? 'language' : 'invariant';

        return { name: value.name, partitioning, properties };
    }
}

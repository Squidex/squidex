/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-useless-escape */

import { AbstractControl, FormControl, Validators } from '@angular/forms';
import { map } from 'rxjs/operators';
import { ExtendedFormGroup, Form, TemplatedFormArray, ValidatorsEx, value$ } from '@app/framework';
import { AddFieldDto, CreateSchemaDto, FieldRule, SchemaDto, SchemaPropertiesDto, SynchronizeSchemaDto, UpdateSchemaDto } from './../services/schemas.service';
import { createProperties, FieldPropertiesDto, FieldPropertiesVisitor } from './../services/schemas.types';

type CreateCategoryFormType = { name: string };

export class CreateCategoryForm extends Form<ExtendedFormGroup, CreateCategoryFormType> {
    constructor() {
        super(new ExtendedFormGroup({
            name: new FormControl('',
                Validators.nullValidator,
            ),
        }));
    }
}

export class CreateSchemaForm extends Form<ExtendedFormGroup, CreateSchemaDto> {
    constructor() {
        super(new ExtendedFormGroup({
            name: new FormControl('', [
                Validators.required,
                Validators.maxLength(40),
                ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'i18n:schemas.schemaNameValidationMessage'),
            ]),
            type: new FormControl('Default',
                Validators.required,
            ),
            initialCategory: new FormControl(undefined,
                Validators.nullValidator,
            ),
            importing: new FormControl({},
                Validators.nullValidator,
            ),
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

export class SynchronizeSchemaForm extends Form<ExtendedFormGroup, SynchronizeSchemaDto> {
    constructor() {
        super(new ExtendedFormGroup({
            json: new FormControl({},
                Validators.nullValidator,
            ),
            fieldsDelete: new FormControl(false,
                Validators.nullValidator,
            ),
            fieldsRecreate: new FormControl(false,
                Validators.nullValidator,
            ),
        }));
    }

    public loadSchema(schema: SchemaDto) {
        this.form.patchValue({ json: schema.export() });
    }

    public transformSubmit(value: any) {
        return {
            ...value.json,
            noFieldDeletion: !value.fieldsDelete,
            noFieldRecreation: !value.fieldsRecreate,
        };
    }
}

export class ConfigureFieldRulesForm extends Form<TemplatedFormArray, ReadonlyArray<FieldRule>, SchemaDto> {
    public get rulesControls(): ReadonlyArray<ExtendedFormGroup> {
        return this.form.controls as any;
    }

    constructor() {
        super(new TemplatedFormArray(FieldRuleTemplate.INSTANCE));
    }

    public add(fieldNames: ReadonlyArray<string>) {
        this.form.add(fieldNames);
    }

    public remove(index: number) {
        this.form.removeAt(index);
    }

    public transformLoad(value: Partial<SchemaDto>) {
        return value.fieldRules || [];
    }
}

class FieldRuleTemplate {
    public static readonly INSTANCE = new FieldRuleTemplate();

    public createControl(_: any, fieldNames?: ReadonlyArray<string>) {
        return new ExtendedFormGroup({
            action: new FormControl('Disable',
                Validators.required,
            ),
            field: new FormControl(fieldNames?.[0],
                Validators.required,
            ),
            condition: new FormControl('',
                Validators.required,
            ),
        });
    }
}

type ConfigurePreviewUrlsFormType = { [name: string]: string };

export class ConfigurePreviewUrlsForm extends Form<TemplatedFormArray, ConfigurePreviewUrlsFormType, SchemaDto> {
    public get previewControls(): ReadonlyArray<ExtendedFormGroup> {
        return this.form.controls as any;
    }

    constructor() {
        super(new TemplatedFormArray(PreviewUrlTemplate.INSTANCE));
    }

    public transformLoad(value: Partial<SchemaDto>) {
        const result = [];

        if (value.previewUrls) {
            for (const [name, url] of Object.entries(value.previewUrls)) {
                result.push({ name, url });
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

class PreviewUrlTemplate {
    public static readonly INSTANCE = new PreviewUrlTemplate();

    public createControl() {
        return new ExtendedFormGroup({
            name: new FormControl('',
                Validators.required,
            ),
            url: new FormControl('',
                Validators.required,
            ),
        });
    }
}

export class EditSchemaScriptsForm extends Form<ExtendedFormGroup, {}, object> {
    constructor() {
        super(new ExtendedFormGroup({
            query: new FormControl('',
                Validators.nullValidator,
            ),
            queryPre: new FormControl('',
                Validators.nullValidator,
            ),
            create: new FormControl('',
                Validators.nullValidator,
            ),
            change: new FormControl('',
                Validators.nullValidator,
            ),
            delete: new FormControl('',
                Validators.nullValidator,
            ),
            update: new FormControl('',
                Validators.nullValidator,
            ),
        }));
    }
}

export class EditFieldForm extends Form<ExtendedFormGroup, {}, FieldPropertiesDto> {
    constructor(properties: FieldPropertiesDto) {
        super(EditFieldForm.buildForm(properties));
    }

    private static buildForm(properties: FieldPropertiesDto) {
        const config = {
            label: new FormControl('',
                Validators.maxLength(100),
            ),
            hints: new FormControl('',
                Validators.maxLength(1000),
            ),
            placeholder: new FormControl('',
                Validators.maxLength(1000),
            ),
            editor: new FormControl(undefined,
                Validators.nullValidator,
            ),
            editorUrl: new FormControl(undefined,
                Validators.nullValidator,
            ),
            isRequired: new FormControl(false,
                Validators.nullValidator,
            ),
            isRequiredOnPublish: new FormControl(false,
                Validators.nullValidator,
            ),
            isHalfWidth: new FormControl(false,
                Validators.nullValidator,
            ),
            tags: new FormControl([],
                Validators.nullValidator,
            ),
        };

        properties.accept(new EditFieldFormVisitor(config));

        return new ExtendedFormGroup(config);
    }
}

export class EditFieldFormVisitor implements FieldPropertiesVisitor<any> {
    constructor(
        private readonly config: { [key: string]: AbstractControl },
    ) {
    }

    public visitArray() {
        this.config['maxItems'] = new FormControl(undefined);
        this.config['minItems'] = new FormControl(undefined);
        this.config['uniqueFields'] = new FormControl(undefined);
    }

    public visitAssets() {
        this.config['allowDuplicates'] = new FormControl(undefined);
        this.config['allowedExtensions'] = new FormControl(undefined);
        this.config['aspectHeight'] = new FormControl(undefined);
        this.config['aspectHeight'] = new FormControl(undefined);
        this.config['aspectWidth'] = new FormControl(undefined);
        this.config['defaultValue'] = new FormControl(undefined);
        this.config['defaultValues'] = new FormControl(undefined);
        this.config['expectedType'] = new FormControl(undefined);
        this.config['folderId'] = new FormControl(undefined);
        this.config['maxHeight'] = new FormControl(undefined);
        this.config['maxItems'] = new FormControl(undefined);
        this.config['maxSize'] = new FormControl(undefined);
        this.config['maxWidth'] = new FormControl(undefined);
        this.config['minHeight'] = new FormControl(undefined);
        this.config['minItems'] = new FormControl(undefined);
        this.config['minSize'] = new FormControl(undefined);
        this.config['minWidth'] = new FormControl(undefined);
        this.config['previewMode'] = new FormControl(undefined);
        this.config['resolveFirst'] = new FormControl(undefined);
    }

    public visitBoolean() {
        this.config['inlineEditable'] = new FormControl(undefined);
        this.config['defaultValues'] = new FormControl(undefined);
        this.config['defaultValue'] = new FormControl(undefined);
    }

    public visitComponent() {
        this.config['schemaIds'] = new FormControl(undefined);
    }

    public visitComponents() {
        this.config['schemaIds'] = new FormControl(undefined);
        this.config['maxItems'] = new FormControl(undefined);
        this.config['minItems'] = new FormControl(undefined);
        this.config['uniqueFields'] = new FormControl(undefined);
    }

    public visitDateTime() {
        this.config['calculatedDefaultValue'] = new FormControl(undefined);
        this.config['defaultValue'] = new FormControl(undefined);
        this.config['defaultValues'] = new FormControl(undefined);
        this.config['format'] = new FormControl(undefined);
        this.config['maxValue'] = new FormControl(undefined, ValidatorsEx.validDateTime());
        this.config['minValue'] = new FormControl(undefined, ValidatorsEx.validDateTime());
    }

    public visitNumber() {
        this.config['allowedValues'] = new FormControl(undefined);
        this.config['defaultValue'] = new FormControl(undefined);
        this.config['defaultValues'] = new FormControl(undefined);
        this.config['inlineEditable'] = new FormControl(undefined);
        this.config['isUnique'] = new FormControl(undefined);
        this.config['maxValue'] = new FormControl(undefined);
        this.config['minValue'] = new FormControl(undefined);
    }

    public visitReferences() {
        this.config['allowDuplicates'] = new FormControl(undefined);
        this.config['defaultValue'] = new FormControl(undefined);
        this.config['defaultValues'] = new FormControl(undefined);
        this.config['maxItems'] = new FormControl(undefined);
        this.config['minItems'] = new FormControl(undefined);
        this.config['mustBePublished'] = new FormControl(false);
        this.config['resolveReference'] = new FormControl(false);
        this.config['schemaIds'] = new FormControl(undefined);
    }

    public visitString() {
        this.config['allowedValues'] = new FormControl(undefined);
        this.config['contentType'] = new FormControl(undefined);
        this.config['createEnum'] = new FormControl(undefined);
        this.config['defaultValue'] = new FormControl(undefined);
        this.config['defaultValues'] = new FormControl(undefined);
        this.config['folderId'] = new FormControl(undefined);
        this.config['inlineEditable'] = new FormControl(undefined);
        this.config['isEmbeddable'] = new FormControl(undefined);
        this.config['isUnique'] = new FormControl(undefined);
        this.config['maxCharacters'] = new FormControl(undefined);
        this.config['maxLength'] = new FormControl(undefined);
        this.config['maxWords'] = new FormControl(undefined);
        this.config['minCharacters'] = new FormControl(undefined);
        this.config['minLength'] = new FormControl(undefined);
        this.config['minWords'] = new FormControl(undefined);
        this.config['pattern'] = new FormControl(undefined);
        this.config['patternMessage'] = new FormControl(undefined);
        this.config['schemaIds'] = new FormControl(undefined);
    }

    public visitTags() {
        this.config['allowedValues'] = new FormControl(undefined);
        this.config['createEnum'] = new FormControl(undefined);
        this.config['defaultValue'] = new FormControl(undefined);
        this.config['defaultValues'] = new FormControl(undefined);
        this.config['maxItems'] = new FormControl(undefined);
        this.config['minItems'] = new FormControl(undefined);
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

export class EditSchemaForm extends Form<ExtendedFormGroup, UpdateSchemaDto, SchemaPropertiesDto> {
    constructor() {
        super(new ExtendedFormGroup({
            label: new FormControl('',
                Validators.maxLength(100),
            ),
            hints: new FormControl('',
                Validators.maxLength(1000),
            ),
            contentsSidebarUrl: new FormControl('',
                Validators.nullValidator,
            ),
            contentSidebarUrl: new FormControl('',
                Validators.nullValidator,
            ),
            contentEditorUrl: new FormControl('',
                Validators.nullValidator,
            ),
            validateOnPublish: new FormControl(false,
                Validators.nullValidator,
            ),
            tags: new FormControl([],
                Validators.nullValidator,
            ),
        }));
    }
}

export class AddFieldForm extends Form<ExtendedFormGroup, AddFieldDto> {
    public isContentField = value$(this.form.controls['type']).pipe(map(x => x !== 'UI'));

    constructor() {
        super(new ExtendedFormGroup({
            type: new FormControl('String',
                Validators.required,
            ),
            name: new FormControl('', [
                Validators.required,
                Validators.maxLength(40),
                ValidatorsEx.pattern('[a-zA-Z0-9]+(\\-[a-zA-Z0-9]+)*', 'i18n:schemas.field.nameValidationMessage'),
            ]),
            isLocalizable: new FormControl(false,
                Validators.nullValidator,
            ),
        }));
    }

    public transformLoad(value: Partial<AddFieldDto>) {
        const { name, properties, partitioning } = value;

        const isLocalizable = partitioning === 'language';

        const type =
            properties ?
            properties.fieldType :
            'String';

        return { name, isLocalizable, type };
    }

    public transformSubmit(value: any) {
        const { name, type, isLocalizable } = value;

        const properties = createProperties(type);
        const partitioning = isLocalizable ? 'language' : 'invariant';

        return { name, partitioning, properties };
    }
}

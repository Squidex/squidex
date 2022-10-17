/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-useless-escape */

import { AbstractControl, UntypedFormControl, Validators } from '@angular/forms';
import { map } from 'rxjs/operators';
import { ExtendedFormGroup, Form, TemplatedFormArray, ValidatorsEx, value$ } from '@app/framework';
import { AddFieldDto, CreateSchemaDto, FieldRule, SchemaDto, SchemaPropertiesDto, SynchronizeSchemaDto, UpdateSchemaDto } from './../services/schemas.service';
import { createProperties, FieldPropertiesDto, FieldPropertiesVisitor } from './../services/schemas.types';

type CreateCategoryFormType = { name: string };

export class CreateCategoryForm extends Form<ExtendedFormGroup, CreateCategoryFormType> {
    constructor() {
        super(new ExtendedFormGroup({
            name: new UntypedFormControl('',
                Validators.nullValidator,
            ),
        }));
    }
}

export class CreateSchemaForm extends Form<ExtendedFormGroup, CreateSchemaDto> {
    constructor() {
        super(new ExtendedFormGroup({
            name: new UntypedFormControl('', [
                Validators.required,
                Validators.maxLength(40),
                ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'i18n:schemas.schemaNameValidationMessage'),
            ]),
            type: new UntypedFormControl('Default',
                Validators.required,
            ),
            initialCategory: new UntypedFormControl(undefined,
                Validators.nullValidator,
            ),
            importing: new UntypedFormControl({},
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
            json: new UntypedFormControl({},
                Validators.nullValidator,
            ),
            fieldsDelete: new UntypedFormControl(false,
                Validators.nullValidator,
            ),
            fieldsRecreate: new UntypedFormControl(false,
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
            action: new UntypedFormControl('Disable',
                Validators.required,
            ),
            field: new UntypedFormControl(fieldNames?.[0],
                Validators.required,
            ),
            condition: new UntypedFormControl('',
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
            name: new UntypedFormControl('',
                Validators.required,
            ),
            url: new UntypedFormControl('',
                Validators.required,
            ),
        });
    }
}

export class EditSchemaScriptsForm extends Form<ExtendedFormGroup, {}, object> {
    constructor() {
        super(new ExtendedFormGroup({
            query: new UntypedFormControl('',
                Validators.nullValidator,
            ),
            queryPre: new UntypedFormControl('',
                Validators.nullValidator,
            ),
            create: new UntypedFormControl('',
                Validators.nullValidator,
            ),
            change: new UntypedFormControl('',
                Validators.nullValidator,
            ),
            delete: new UntypedFormControl('',
                Validators.nullValidator,
            ),
            update: new UntypedFormControl('',
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
            label: new UntypedFormControl('',
                Validators.maxLength(100),
            ),
            hints: new UntypedFormControl('',
                Validators.maxLength(1000),
            ),
            placeholder: new UntypedFormControl('',
                Validators.maxLength(1000),
            ),
            editor: new UntypedFormControl(undefined,
                Validators.nullValidator,
            ),
            editorUrl: new UntypedFormControl(undefined,
                Validators.nullValidator,
            ),
            isRequired: new UntypedFormControl(false,
                Validators.nullValidator,
            ),
            isRequiredOnPublish: new UntypedFormControl(false,
                Validators.nullValidator,
            ),
            isHalfWidth: new UntypedFormControl(false,
                Validators.nullValidator,
            ),
            tags: new UntypedFormControl([],
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
        this.config['maxItems'] = new UntypedFormControl(undefined);
        this.config['minItems'] = new UntypedFormControl(undefined);
        this.config['uniqueFields'] = new UntypedFormControl(undefined);
    }

    public visitAssets() {
        this.config['allowDuplicates'] = new UntypedFormControl(undefined);
        this.config['allowedExtensions'] = new UntypedFormControl(undefined);
        this.config['aspectHeight'] = new UntypedFormControl(undefined);
        this.config['aspectHeight'] = new UntypedFormControl(undefined);
        this.config['aspectWidth'] = new UntypedFormControl(undefined);
        this.config['defaultValue'] = new UntypedFormControl(undefined);
        this.config['defaultValues'] = new UntypedFormControl(undefined);
        this.config['expectedType'] = new UntypedFormControl(undefined);
        this.config['folderId'] = new UntypedFormControl(undefined);
        this.config['maxHeight'] = new UntypedFormControl(undefined);
        this.config['maxItems'] = new UntypedFormControl(undefined);
        this.config['maxSize'] = new UntypedFormControl(undefined);
        this.config['maxWidth'] = new UntypedFormControl(undefined);
        this.config['minHeight'] = new UntypedFormControl(undefined);
        this.config['minItems'] = new UntypedFormControl(undefined);
        this.config['minSize'] = new UntypedFormControl(undefined);
        this.config['minWidth'] = new UntypedFormControl(undefined);
        this.config['previewMode'] = new UntypedFormControl(undefined);
        this.config['resolveFirst'] = new UntypedFormControl(undefined);
    }

    public visitBoolean() {
        this.config['inlineEditable'] = new UntypedFormControl(undefined);
        this.config['defaultValues'] = new UntypedFormControl(undefined);
        this.config['defaultValue'] = new UntypedFormControl(undefined);
    }

    public visitComponent() {
        this.config['schemaIds'] = new UntypedFormControl(undefined);
    }

    public visitComponents() {
        this.config['schemaIds'] = new UntypedFormControl(undefined);
        this.config['maxItems'] = new UntypedFormControl(undefined);
        this.config['minItems'] = new UntypedFormControl(undefined);
        this.config['uniqueFields'] = new UntypedFormControl(undefined);
    }

    public visitDateTime() {
        this.config['calculatedDefaultValue'] = new UntypedFormControl(undefined);
        this.config['defaultValue'] = new UntypedFormControl(undefined);
        this.config['defaultValues'] = new UntypedFormControl(undefined);
        this.config['format'] = new UntypedFormControl(undefined);
        this.config['maxValue'] = new UntypedFormControl(undefined, ValidatorsEx.validDateTime());
        this.config['minValue'] = new UntypedFormControl(undefined, ValidatorsEx.validDateTime());
    }

    public visitJson() {
        this.config['graphQLSchema'] = new UntypedFormControl(undefined);
    }

    public visitNumber() {
        this.config['allowedValues'] = new UntypedFormControl(undefined);
        this.config['defaultValue'] = new UntypedFormControl(undefined);
        this.config['defaultValues'] = new UntypedFormControl(undefined);
        this.config['inlineEditable'] = new UntypedFormControl(undefined);
        this.config['isUnique'] = new UntypedFormControl(undefined);
        this.config['maxValue'] = new UntypedFormControl(undefined);
        this.config['minValue'] = new UntypedFormControl(undefined);
    }

    public visitReferences() {
        this.config['allowDuplicates'] = new UntypedFormControl(undefined);
        this.config['defaultValue'] = new UntypedFormControl(undefined);
        this.config['defaultValues'] = new UntypedFormControl(undefined);
        this.config['maxItems'] = new UntypedFormControl(undefined);
        this.config['minItems'] = new UntypedFormControl(undefined);
        this.config['mustBePublished'] = new UntypedFormControl(false);
        this.config['resolveReference'] = new UntypedFormControl(false);
        this.config['schemaIds'] = new UntypedFormControl(undefined);
    }

    public visitString() {
        this.config['allowedValues'] = new UntypedFormControl(undefined);
        this.config['contentType'] = new UntypedFormControl(undefined);
        this.config['createEnum'] = new UntypedFormControl(undefined);
        this.config['defaultValue'] = new UntypedFormControl(undefined);
        this.config['defaultValues'] = new UntypedFormControl(undefined);
        this.config['folderId'] = new UntypedFormControl(undefined);
        this.config['inlineEditable'] = new UntypedFormControl(undefined);
        this.config['isEmbeddable'] = new UntypedFormControl(undefined);
        this.config['isUnique'] = new UntypedFormControl(undefined);
        this.config['maxCharacters'] = new UntypedFormControl(undefined);
        this.config['maxLength'] = new UntypedFormControl(undefined);
        this.config['maxWords'] = new UntypedFormControl(undefined);
        this.config['minCharacters'] = new UntypedFormControl(undefined);
        this.config['minLength'] = new UntypedFormControl(undefined);
        this.config['minWords'] = new UntypedFormControl(undefined);
        this.config['pattern'] = new UntypedFormControl(undefined);
        this.config['patternMessage'] = new UntypedFormControl(undefined);
        this.config['schemaIds'] = new UntypedFormControl(undefined);
    }

    public visitTags() {
        this.config['allowedValues'] = new UntypedFormControl(undefined);
        this.config['createEnum'] = new UntypedFormControl(undefined);
        this.config['defaultValue'] = new UntypedFormControl(undefined);
        this.config['defaultValues'] = new UntypedFormControl(undefined);
        this.config['maxItems'] = new UntypedFormControl(undefined);
        this.config['minItems'] = new UntypedFormControl(undefined);
    }

    public visitGeolocation() {
        return undefined;
    }

    public visitUI() {
        return undefined;
    }
}

export class EditSchemaForm extends Form<ExtendedFormGroup, UpdateSchemaDto, SchemaPropertiesDto> {
    constructor() {
        super(new ExtendedFormGroup({
            label: new UntypedFormControl('',
                Validators.maxLength(100),
            ),
            hints: new UntypedFormControl('',
                Validators.maxLength(1000),
            ),
            contentsSidebarUrl: new UntypedFormControl('',
                Validators.nullValidator,
            ),
            contentSidebarUrl: new UntypedFormControl('',
                Validators.nullValidator,
            ),
            contentEditorUrl: new UntypedFormControl('',
                Validators.nullValidator,
            ),
            validateOnPublish: new UntypedFormControl(false,
                Validators.nullValidator,
            ),
            tags: new UntypedFormControl([],
                Validators.nullValidator,
            ),
        }));
    }
}

export class AddFieldForm extends Form<ExtendedFormGroup, AddFieldDto> {
    public isContentField = value$(this.form.controls['type']).pipe(map(x => x !== 'UI'));

    constructor() {
        super(new ExtendedFormGroup({
            type: new UntypedFormControl('String',
                Validators.required,
            ),
            name: new UntypedFormControl('', [
                Validators.required,
                Validators.maxLength(40),
                ValidatorsEx.pattern('[a-zA-Z0-9]+(\\-[a-zA-Z0-9]+)*', 'i18n:schemas.field.nameValidationMessage'),
            ]),
            isLocalizable: new UntypedFormControl(false,
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

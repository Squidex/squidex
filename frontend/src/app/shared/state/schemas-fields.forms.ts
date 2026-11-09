/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injector } from '@angular/core';
import { FieldTree, maxLength } from '@angular/forms/signals';
import { SignalForm } from '@app/framework';
import { ArrayFieldPropertiesDto, AssetsFieldPropertiesDto, BooleanFieldPropertiesDto, DateTimeFieldPropertiesDto, FieldPropertiesDto, FieldPropertiesVisitor, JsonFieldPropertiesDto, NumberFieldPropertiesDto, ReferencesFieldPropertiesDto, RichTextFieldPropertiesDto, StringFieldPropertiesDto, TagsFieldPropertiesDto, UserInfoFieldPropertiesDto } from '../model';
import { EditFieldSchemaVisitor } from './schemas-fields.forms-schema';
import { loadWithDefaults, submitWithoutEmpty } from './schemas.forms-helpers';

type Nullable<T> = Exclude<T, undefined> | null;

export type FieldFormCommonModel = {
    label: string;
    hints: string;
    placeholder: string;
    editor: string;
    editorUrl: string;
    isRequired: boolean;
    isRequiredOnPublish: boolean;
    isCreateOnly: boolean;
    isHalfWidth: boolean;
    tags: Nullable<FieldPropertiesDto['tags']>;
    searchMode: Nullable<FieldPropertiesDto['searchMode']>;
    searchPaths: Nullable<FieldPropertiesDto['searchPaths']>;
};

export type ArrayFieldFormModel = FieldFormCommonModel & {
    calculatedDefaultValue: Nullable<ArrayFieldPropertiesDto['calculatedDefaultValue']>;
    maxItems: number;
    minItems: number;
    uniqueFields: Nullable<ArrayFieldPropertiesDto['uniqueFields']>;
};

export type AssetsFieldFormModel = FieldFormCommonModel & {
    allowDuplicates: boolean;
    allowedExtensions: Nullable<AssetsFieldPropertiesDto['allowedExtensions']>;
    aspectHeight: number;
    aspectWidth: number;
    defaultValue: Nullable<AssetsFieldPropertiesDto['defaultValue']>;
    defaultValues: Nullable<AssetsFieldPropertiesDto['defaultValues']>;
    expectedType: Nullable<AssetsFieldPropertiesDto['expectedType']>;
    folderId: Nullable<AssetsFieldPropertiesDto['folderId']>;
    maxHeight: number;
    maxItems: number;
    maxSize: number;
    maxWidth: number;
    minHeight: number;
    minItems: number;
    minSize: number;
    minWidth: number;
    previewFormat: string;
    previewMode: Nullable<AssetsFieldPropertiesDto['previewMode']>;
    resolveFirst: boolean;
};

export type BooleanFieldFormModel = FieldFormCommonModel & {
    defaultValue: Nullable<BooleanFieldPropertiesDto['defaultValue']>;
    defaultValues: Nullable<BooleanFieldPropertiesDto['defaultValues']>;
    inlineEditable: boolean;
};

export type ComponentFieldFormModel = FieldFormCommonModel & {
    schemaIds: Nullable<ReferencesFieldPropertiesDto['schemaIds']>;
};

export type ComponentsFieldFormModel = FieldFormCommonModel & {
    calculatedDefaultValue: Nullable<ArrayFieldPropertiesDto['calculatedDefaultValue']>;
    maxItems: number;
    minItems: number;
    schemaIds: Nullable<ReferencesFieldPropertiesDto['schemaIds']>;
    uniqueFields: Nullable<ArrayFieldPropertiesDto['uniqueFields']>;
};

export type DateTimeFieldFormModel = FieldFormCommonModel & {
    calculatedDefaultValue: Nullable<DateTimeFieldPropertiesDto['calculatedDefaultValue']>;
    defaultValue: string | null;
    defaultValues: Record<string, string> | null;
    format: string;
    maxValue: string | null;
    minValue: string | null;
};

export type JsonFieldFormModel = FieldFormCommonModel & {
    graphQLSchema: Nullable<JsonFieldPropertiesDto['graphQLSchema']>;
};

export type NumberFieldFormModel = FieldFormCommonModel & {
    allowedValues: Nullable<NumberFieldPropertiesDto['allowedValues']>;
    defaultValue: number;
    defaultValues: Nullable<NumberFieldPropertiesDto['defaultValues']>;
    inlineEditable: boolean;
    isUnique: boolean;
    maxValue: number;
    minValue: number;
};

export type ReferencesFieldFormModel = FieldFormCommonModel & {
    allowDuplicates: boolean;
    defaultValue: Nullable<ReferencesFieldPropertiesDto['defaultValue']>;
    defaultValues: Nullable<ReferencesFieldPropertiesDto['defaultValues']>;
    maxItems: number;
    minItems: number;
    mustBePublished: boolean;
    query: string;
    resolveReference: boolean;
    schemaIds: Nullable<ReferencesFieldPropertiesDto['schemaIds']>;
};

export type RichTextFieldFormModel = FieldFormCommonModel & {
    classNames: Nullable<RichTextFieldPropertiesDto['classNames']>;
    folderId: Nullable<RichTextFieldPropertiesDto['folderId']>;
    maxCharacters: number;
    maxLength: number;
    maxWords: number;
    minCharacters: number;
    minLength: number;
    minWords: number;
    schemaIds: Nullable<RichTextFieldPropertiesDto['schemaIds']>;
};

export type StringFieldFormModel = FieldFormCommonModel & {
    allowedValues: Nullable<StringFieldPropertiesDto['allowedValues']>;
    classNames: Nullable<StringFieldPropertiesDto['classNames']>;
    contentType: Nullable<StringFieldPropertiesDto['contentType']>;
    createEnum: boolean;
    defaultValue: string;
    defaultValues: Nullable<StringFieldPropertiesDto['defaultValues']>;
    folderId: Nullable<StringFieldPropertiesDto['folderId']>;
    inlineEditable: boolean;
    isEmbeddable: boolean;
    isUnique: boolean;
    maxCharacters: number;
    maxLength: number;
    maxWords: number;
    minCharacters: number;
    minLength: number;
    minWords: number;
    pattern: string;
    patternMessage: string;
    schemaIds: Nullable<StringFieldPropertiesDto['schemaIds']>;
};

export type TagsFieldFormModel = FieldFormCommonModel & {
    allowedValues: Nullable<TagsFieldPropertiesDto['allowedValues']>;
    createEnum: boolean;
    defaultValue: Nullable<TagsFieldPropertiesDto['defaultValue']>;
    defaultValues: Nullable<TagsFieldPropertiesDto['defaultValues']>;
    maxItems: number;
    minItems: number;
};

export type UserInfoFieldFormModel = FieldFormCommonModel & {
    defaultRole: Nullable<UserInfoFieldPropertiesDto['defaultRole']>;
};

export type FieldFormModel =
    FieldFormCommonModel |
    ArrayFieldFormModel |
    AssetsFieldFormModel |
    BooleanFieldFormModel |
    ComponentFieldFormModel |
    ComponentsFieldFormModel |
    DateTimeFieldFormModel |
    JsonFieldFormModel |
    NumberFieldFormModel |
    ReferencesFieldFormModel |
    RichTextFieldFormModel |
    StringFieldFormModel |
    TagsFieldFormModel |
    UserInfoFieldFormModel;

export class EditFieldForm extends SignalForm<FieldFormModel, Record<string, unknown>, FieldPropertiesDto> {
    private readonly defaults: FieldFormModel;

    constructor(properties: FieldPropertiesDto, injector?: Injector) {
        const defaults = properties.accept(new EditFieldFormVisitor());

        super(defaults, path => {
            maxLength(path.label, 100);
            maxLength(path.hints, 1000);
            maxLength(path.placeholder, 1000);

            properties.accept(new EditFieldSchemaVisitor(path));
        }, injector);

        this.defaults = defaults;
    }

    protected transformLoad(value: Partial<FieldPropertiesDto>): FieldFormModel {
        return loadWithDefaults(value, this.defaults);
    }

    protected transformSubmit(value: FieldFormModel) {
        const result = submitWithoutEmpty(value);

        // Hidden properties do not apply to the selected editor, so the server defaults are used.
        const fields = this.form as unknown as Record<string, FieldTree<unknown> | undefined>;

        for (const key of Object.keys(result)) {
            if (fields[key]?.().hidden()) {
                result[key] = undefined;
            }
        }

        return result;
    }
}


const COMMON_DEFAULTS: FieldFormCommonModel = {
    label: '',
    hints: '',
    placeholder: '',
    editor: '',
    editorUrl: '',
    isRequired: false,
    isRequiredOnPublish: false,
    isCreateOnly: false,
    isHalfWidth: false,
    tags: [],
    searchMode: 'Default',
    searchPaths: [],
};

class EditFieldFormVisitor implements FieldPropertiesVisitor<FieldFormModel> {
    public visitArray(): ArrayFieldFormModel {
        return {
            ...COMMON_DEFAULTS,
            calculatedDefaultValue: 'EmptyArray',
            maxItems: NaN,
            minItems: NaN,
            uniqueFields: null,
        };
    }

    public visitAssets(): AssetsFieldFormModel {
        return {
            ...COMMON_DEFAULTS,
            allowDuplicates: false,
            allowedExtensions: null,
            aspectHeight: NaN,
            aspectWidth: NaN,
            defaultValue: null,
            defaultValues: null,
            expectedType: null,
            folderId: null,
            maxHeight: NaN,
            maxItems: NaN,
            maxSize: NaN,
            maxWidth: NaN,
            minHeight: NaN,
            minItems: NaN,
            minSize: NaN,
            minWidth: NaN,
            previewFormat: '',
            previewMode: null,
            resolveFirst: false,
        };
    }

    public visitBoolean(): BooleanFieldFormModel {
        return {
            ...COMMON_DEFAULTS,
            defaultValue: null,
            defaultValues: null,
            inlineEditable: false,
        };
    }

    public visitComponent(): ComponentFieldFormModel {
        return {
            ...COMMON_DEFAULTS,
            schemaIds: null,
        };
    }

    public visitComponents(): ComponentsFieldFormModel {
        return {
            ...COMMON_DEFAULTS,
            calculatedDefaultValue: 'EmptyArray',
            maxItems: NaN,
            minItems: NaN,
            schemaIds: null,
            uniqueFields: null,
        };
    }

    public visitDateTime(): DateTimeFieldFormModel {
        return {
            ...COMMON_DEFAULTS,
            calculatedDefaultValue: null,
            defaultValue: null,
            defaultValues: null,
            format: '',
            maxValue: null,
            minValue: null,
        };
    }

    public visitJson(): JsonFieldFormModel {
        return {
            ...COMMON_DEFAULTS,
            graphQLSchema: null,
        };
    }

    public visitNumber(): NumberFieldFormModel {
        return {
            ...COMMON_DEFAULTS,
            allowedValues: null,
            defaultValue: NaN,
            defaultValues: null,
            inlineEditable: false,
            isUnique: false,
            maxValue: NaN,
            minValue: NaN,
        };
    }

    public visitReferences(): ReferencesFieldFormModel {
        return {
            ...COMMON_DEFAULTS,
            allowDuplicates: false,
            defaultValue: null,
            defaultValues: null,
            maxItems: NaN,
            minItems: NaN,
            mustBePublished: false,
            query: '',
            resolveReference: false,
            schemaIds: null,
        };
    }

    public visitRichText(): RichTextFieldFormModel {
        return {
            ...COMMON_DEFAULTS,
            classNames: null,
            folderId: null,
            maxCharacters: NaN,
            maxLength: NaN,
            maxWords: NaN,
            minCharacters: NaN,
            minLength: NaN,
            minWords: NaN,
            schemaIds: null,
        };
    }

    public visitString(): StringFieldFormModel {
        return {
            ...COMMON_DEFAULTS,
            allowedValues: null,
            classNames: null,
            contentType: null,
            createEnum: false,
            defaultValue: '',
            defaultValues: null,
            folderId: null,
            inlineEditable: false,
            isEmbeddable: false,
            isUnique: false,
            maxCharacters: NaN,
            maxLength: NaN,
            maxWords: NaN,
            minCharacters: NaN,
            minLength: NaN,
            minWords: NaN,
            pattern: '',
            patternMessage: '',
            schemaIds: null,
        };
    }

    public visitTags(): TagsFieldFormModel {
        return {
            ...COMMON_DEFAULTS,
            allowedValues: null,
            createEnum: false,
            defaultValue: null,
            defaultValues: null,
            maxItems: NaN,
            minItems: NaN,
        };
    }

    public visitUserInfo(): UserInfoFieldFormModel {
        return {
            ...COMMON_DEFAULTS,
            defaultRole: null,
        };
    }

    public visitGeolocation(): FieldFormCommonModel {
        return { ...COMMON_DEFAULTS };
    }

    public visitUI(): FieldFormCommonModel {
        return { ...COMMON_DEFAULTS };
    }
}

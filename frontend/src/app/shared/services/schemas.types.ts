/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export type FieldType =
    'Array' |
    'Assets' |
    'Boolean' |
    'Component' |
    'Components' |
    'DateTime' |
    'Json' |
    'Geolocation' |
    'Number' |
    'References' |
    'String' |
    'Tags' |
    'UI';

export const fieldTypes: ReadonlyArray<{ type: FieldType; description: string }> = [
    {
        type: 'String',
        description: 'i18n:schemas.fieldTypes.string.description',
    }, {
        type: 'Assets',
        description: 'i18n:schemas.fieldTypes.assets.description',
    }, {
        type: 'Boolean',
        description: 'i18n:schemas.fieldTypes.boolean.description',
    }, {
        type: 'Component',
        description: 'i18n:schemas.fieldTypes.component.description',
    }, {
        type: 'Components',
        description: 'i18n:schemas.fieldTypes.components.description',
    }, {
        type: 'DateTime',
        description: 'i18n:schemas.fieldTypes.dateTime.description',
    }, {
        type: 'Geolocation',
        description: 'i18n:schemas.fieldTypes.geolocation.description',
    }, {
        type: 'Json',
        description: 'i18n:schemas.fieldTypes.json.description',
    }, {
        type: 'Number',
        description: 'i18n:schemas.fieldTypes.number.description',
    }, {
        type: 'References',
        description: 'i18n:schemas.fieldTypes.references.description',
    }, {
        type: 'Tags',
        description: 'i18n:schemas.fieldTypes.tags.description',
    }, {
        type: 'Array',
        description: 'i18n:schemas.fieldTypes.array.description',
    }, {
        type: 'UI',
        description: 'i18n:schemas.fieldTypes.ui.description',
    },
];

export const fieldInvariant = 'iv';

export function createProperties(fieldType: FieldType, values?: any): FieldPropertiesDto {
    let properties: FieldPropertiesDto;

    switch (fieldType) {
        case 'Array':
            properties = new ArrayFieldPropertiesDto();
            break;
        case 'Assets':
            properties = new AssetsFieldPropertiesDto();
            break;
        case 'Boolean':
            properties = new BooleanFieldPropertiesDto();
            break;
        case 'Component':
            properties = new ComponentFieldPropertiesDto();
            break;
        case 'Components':
            properties = new ComponentsFieldPropertiesDto();
            break;
        case 'DateTime':
            properties = new DateTimeFieldPropertiesDto();
            break;
        case 'Geolocation':
            properties = new GeolocationFieldPropertiesDto();
            break;
        case 'Json':
            properties = new JsonFieldPropertiesDto();
            break;
        case 'Number':
            properties = new NumberFieldPropertiesDto();
            break;
        case 'References':
            properties = new ReferencesFieldPropertiesDto();
            break;
        case 'String':
            properties = new StringFieldPropertiesDto();
            break;
        case 'Tags':
            properties = new TagsFieldPropertiesDto();
            break;
        case 'UI':
            properties = new UIFieldPropertiesDto();
            break;
        default:
            throw new Error(`Unknown field type ${fieldType}.`);
    }

    if (values) {
        Object.assign(properties, values);
    }

    return properties;
}

export interface FieldPropertiesVisitor<T> {
    visitArray(properties: ArrayFieldPropertiesDto): T;

    visitAssets(properties: AssetsFieldPropertiesDto): T;

    visitBoolean(properties: BooleanFieldPropertiesDto): T;

    visitComponent(properties: ComponentFieldPropertiesDto): T;

    visitComponents(properties: ComponentsFieldPropertiesDto): T;

    visitDateTime(properties: DateTimeFieldPropertiesDto): T;

    visitGeolocation(properties: GeolocationFieldPropertiesDto): T;

    visitJson(properties: JsonFieldPropertiesDto): T;

    visitNumber(properties: NumberFieldPropertiesDto): T;

    visitReferences(properties: ReferencesFieldPropertiesDto): T;

    visitString(properties: StringFieldPropertiesDto): T;

    visitTags(properties: TagsFieldPropertiesDto): T;

    visitUI(properties: UIFieldPropertiesDto): T;
}

type DefaultValue<T> = { [key: string]: T | undefined | null };

export abstract class FieldPropertiesDto {
    public abstract fieldType: FieldType;

    public readonly editorUrl?: string;
    public readonly hints?: string;
    public readonly isRequired: boolean = false;
    public readonly isRequiredOnPublish: boolean = false;
    public readonly isHalfWidth: boolean = false;
    public readonly label?: string;
    public readonly placeholder?: string;
    public readonly tags?: ReadonlyArray<string>;

    public get isComplexUI() {
        return true;
    }

    public get isSortable() {
        return true;
    }

    public get isContentField() {
        return true;
    }

    public abstract accept<T>(visitor: FieldPropertiesVisitor<T>): T;
}

export class ArrayFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Array';

    public readonly maxItems?: number;
    public readonly minItems?: number;
    public readonly uniqueFields?: ReadonlyArray<string>;

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitArray(this);
    }
}

export type AssetPreviewMode = 'ImageAndFileName' | 'Image' | 'FileName';

export const ASSET_PREVIEW_MODES: ReadonlyArray<AssetPreviewMode> = [
    'ImageAndFileName',
    'Image',
    'FileName',
];

export class AssetsFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Assets';

    public readonly previewMode: AssetPreviewMode = 'FileName';
    public readonly defaultValue?: ReadonlyArray<string>;
    public readonly defaultValues?: DefaultValue<ReadonlyArray<string>>;
    public readonly allowDuplicates?: boolean;
    public readonly allowedExtensions?: ReadonlyArray<string>;
    public readonly resolveFirst = false;
    public readonly aspectHeight?: number;
    public readonly aspectWidth?: number;
    public readonly folderId?: string;
    public readonly maxHeight?: number;
    public readonly maxItems?: number;
    public readonly maxSize?: number;
    public readonly maxWidth?: number;
    public readonly minHeight?: number;
    public readonly minItems?: number;
    public readonly minSize?: number;
    public readonly minWidth?: number;
    public readonly expectedType?: string;

    public get isSortable() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitAssets(this);
    }
}

export type BooleanFieldEditor = 'Checkbox' | 'Toggle';

export const BOOLEAN_FIELD_EDITORS: ReadonlyArray<BooleanFieldEditor> = [
    'Checkbox',
    'Toggle',
];

export class BooleanFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Boolean';

    public readonly defaultValue?: boolean;
    public readonly defaultValues?: DefaultValue<boolean>;
    public readonly editor: BooleanFieldEditor = 'Checkbox';
    public readonly inlineEditable: boolean = false;

    public get isComplexUI() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitBoolean(this);
    }
}

export class ComponentFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Component';

    public readonly schemaIds?: ReadonlyArray<string>;

    public get isComplexUI() {
        return true;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitComponent(this);
    }
}

export class ComponentsFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Components';

    public readonly schemaIds?: ReadonlyArray<string>;
    public readonly maxItems?: number;
    public readonly minItems?: number;
    public readonly uniqueFields?: ReadonlyArray<string>;

    public get isComplexUI() {
        return true;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitComponents(this);
    }
}

export type DateTimeFieldEditor = 'DateTime' | 'Date';

export const DATETIME_FIELD_EDITORS: ReadonlyArray<DateTimeFieldEditor> = [
    'DateTime',
    'Date',
];

export class DateTimeFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'DateTime';

    public readonly calculatedDefaultValue?: string;
    public readonly defaultValue?: string;
    public readonly defaultValues?: DefaultValue<string>;
    public readonly format?: string;
    public readonly editor: DateTimeFieldEditor = 'DateTime';
    public readonly maxValue?: string;
    public readonly minValue?: string;

    public get isComplexUI() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitDateTime(this);
    }
}

export type GeolocationFieldEditor = 'Map';

export class GeolocationFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Geolocation';

    public readonly editor: GeolocationFieldEditor = 'Map';

    public get isSortable() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitGeolocation(this);
    }
}

export class JsonFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Json';

    public get isSortable() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitJson(this);
    }
}

export type NumberFieldEditor = 'Input' | 'Radio' | 'Dropdown' | 'Stars';

export const NUMBER_FIELD_EDITORS: ReadonlyArray<NumberFieldEditor> = [
    'Input',
    'Radio',
    'Dropdown',
    'Stars',
];

export class NumberFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Number';

    public readonly allowedValues?: ReadonlyArray<number>;
    public readonly defaultValue?: number;
    public readonly defaultValues?: DefaultValue<number>;
    public readonly editor: NumberFieldEditor = 'Input';
    public readonly inlineEditable: boolean = false;
    public readonly isUnique: boolean = false;
    public readonly maxValue?: number;
    public readonly minValue?: number;

    public get isComplexUI() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitNumber(this);
    }
}

export type ReferencesFieldEditor = 'List' | 'Dropdown' | 'Checkboxes' | 'Tags' | 'Input';

export const REFERENCES_FIELD_EDITORS: ReadonlyArray<ReferencesFieldEditor> = [
    'List',
    'Dropdown',
    'Checkboxes',
    'Tags',
    'Input',
];

export class ReferencesFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'References';

    public readonly allowDuplicates?: boolean;
    public readonly defaultValue?: ReadonlyArray<string>;
    public readonly defaultValues?: DefaultValue<ReadonlyArray<string>>;
    public readonly editor: ReferencesFieldEditor = 'List';
    public readonly maxItems?: number;
    public readonly minItems?: number;
    public readonly mustBePublished?: boolean;
    public readonly resolveReference?: boolean;
    public readonly schemaIds?: ReadonlyArray<string>;

    public get singleId() {
        return this.schemaIds?.[0] || null;
    }

    public get isSortable() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitReferences(this);
    }
}

export type StringFieldEditor = 'Color' | 'Dropdown' | 'Html' | 'Input' | 'Markdown' | 'Radio' | 'RichText' | 'Slug' | 'StockPhoto' | 'TextArea';
export type StringContentType = 'Unspecified' | 'Markdown' | 'Html';

export const STRING_FIELD_EDITORS: ReadonlyArray<StringFieldEditor> = [
    'Input',
    'TextArea',
    'RichText',
    'Slug',
    'Markdown',
    'Dropdown',
    'Radio',
    'Html',
    'StockPhoto',
    'Color',
];

export const STRING_CONTENT_TYPES: ReadonlyArray<StringContentType> = [
    'Unspecified',
    'Markdown',
    'Html',
];

export class StringFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'String';

    public readonly allowedValues?: ReadonlyArray<string>;
    public readonly contentType?: StringContentType;
    public readonly createEnum: boolean = false;
    public readonly defaultValue?: string;
    public readonly defaultValues?: DefaultValue<string>;
    public readonly editor: StringFieldEditor = 'Input';
    public readonly folderId?: string;
    public readonly inlineEditable: boolean = false;
    public readonly isEmbeddable: boolean = false;
    public readonly isUnique: boolean = false;
    public readonly maxCharacters?: number;
    public readonly maxLength?: number;
    public readonly maxWords?: number;
    public readonly minCharacters?: number;
    public readonly minLength?: number;
    public readonly minWords?: number;
    public readonly pattern?: string;
    public readonly patternMessage?: string;
    public readonly schemaIds?: ReadonlyArray<string>;

    public get isComplexUI() {
        return this.editor !== 'Input' && this.editor !== 'Color' && this.editor !== 'Radio' && this.editor !== 'Slug' && this.editor !== 'TextArea';
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitString(this);
    }
}

export type TagsFieldEditor = 'Tags' | 'Checkboxes' | 'Dropdown';

export const TAGS_FIELD_EDITORS: ReadonlyArray<TagsFieldEditor> = [
    'Tags',
    'Checkboxes',
    'Dropdown',
];

export class TagsFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Tags';

    public readonly allowedValues?: ReadonlyArray<string>;
    public readonly createEnum: boolean = false;
    public readonly defaultValue?: ReadonlyArray<string>;
    public readonly defaultValues?: DefaultValue<ReadonlyArray<string>>;
    public readonly editor: TagsFieldEditor = 'Tags';
    public readonly maxItems?: number;
    public readonly minItems?: number;

    public get isComplexUI() {
        return false;
    }

    public get isSortable() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitTags(this);
    }
}

export class UIFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'UI';

    public readonly editor = 'Separator';

    public get isComplexUI() {
        return false;
    }

    public get isSortable() {
        return false;
    }

    public get isContentField() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitUI(this);
    }
}

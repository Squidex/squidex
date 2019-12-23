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
    'DateTime' |
    'Json' |
    'Geolocation' |
    'Number' |
    'References' |
    'String' |
    'Tags' |
    'UI';

export const fieldTypes: ReadonlyArray<{ type: FieldType, description: string }> = [
    {
        type: 'String',
        description: 'Titles, names, paragraphs.'
    }, {
        type: 'Assets',
        description: 'Images, videos, documents.'
    }, {
        type: 'Boolean',
        description: 'Yes or no, true or false.'
    }, {
        type: 'DateTime',
        description: 'Events date, opening hours.'
    }, {
        type: 'Geolocation',
        description: 'Coordinates: latitude and longitude.'
    }, {
        type: 'Json',
        description: 'Data in JSON format, for developers.'
    }, {
        type: 'Number',
        description: 'ID, order number, rating, quantity.'
    }, {
        type: 'References',
        description: 'Links to other content items.'
    }, {
        type: 'Tags',
        description: 'Special format for tags.'
    }, {
        type: 'Array',
        description: 'List of embedded objects.'
    }, {
        type: 'UI',
        description: 'Separator for editing UI.'
    }
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
            throw 'Invalid properties type';
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

    visitDateTime(properties: DateTimeFieldPropertiesDto): T;

    visitGeolocation(properties: GeolocationFieldPropertiesDto): T;

    visitJson(properties: JsonFieldPropertiesDto): T;

    visitNumber(properties: NumberFieldPropertiesDto): T;

    visitReferences(properties: ReferencesFieldPropertiesDto): T;

    visitString(properties: StringFieldPropertiesDto): T;

    visitTags(properties: TagsFieldPropertiesDto): T;

    visitUI(properties: UIFieldPropertiesDto): T;
}

export abstract class FieldPropertiesDto {
    public abstract fieldType: FieldType;

    public readonly editorUrl?: string;
    public readonly hints?: string;
    public readonly isRequired: boolean = false;
    public readonly label?: string;
    public readonly placeholder?: string;
    public readonly tags?: ReadonlyArray<string>;

    public get isTranslateable() {
        return false;
    }

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

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitArray(this);
    }
}

export class AssetsFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Assets';

    public readonly allowDuplicates?: boolean;
    public readonly resolveImage: boolean;
    public readonly allowedExtensions?: ReadonlyArray<string>;
    public readonly aspectHeight?: number;
    public readonly aspectWidth?: number;
    public readonly maxHeight?: number;
    public readonly maxItems?: number;
    public readonly maxSize?: number;
    public readonly maxWidth?: number;
    public readonly minHeight?: number;
    public readonly minItems?: number;
    public readonly minSize?: number;
    public readonly minWidth?: number;
    public readonly mustBeImage?: boolean;

    public get isSortable() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitAssets(this);
    }
}

export type BooleanFieldEditor = 'Checkbox' | 'Toggle';

export class BooleanFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Boolean';

    public readonly defaultValue?: boolean;
    public readonly editor: BooleanFieldEditor = 'Checkbox';
    public readonly inlineEditable: boolean = false;

    public get isComplexUI() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitBoolean(this);
    }
}

export type DateTimeFieldEditor = 'DateTime' | 'Date';

export class DateTimeFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'DateTime';

    public readonly calculatedDefaultValue?: string;
    public readonly defaultValue?: string;
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

export class NumberFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Number';

    public readonly allowedValues?: ReadonlyArray<number>;
    public readonly defaultValue?: number;
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

export type ReferencesFieldEditor = 'List' | 'Dropdown';

export class ReferencesFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'References';

    public readonly allowDuplicates?: boolean;
    public readonly editor: ReferencesFieldEditor = 'List';
    public readonly maxItems?: number;
    public readonly minItems?: number;
    public readonly resolveReference?: boolean;
    public readonly schemaIds?: ReadonlyArray<string>;

    public get singleId() {
        return this.schemaIds && this.schemaIds.length === 1 ? this.schemaIds[0] : null;
    }

    public get isSortable() {
        return false;
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitReferences(this);
    }
}

export type StringEditor = 'Color' | 'Dropdown' | 'Html' | 'Input' | 'Markdown' | 'Radio' | 'RichText' | 'Slug' | 'StockPhoto' | 'TextArea';

export class StringFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'String';

    public readonly allowedValues?: ReadonlyArray<string>;
    public readonly defaultValue?: string;
    public readonly editor: StringEditor = 'Input';
    public readonly inlineEditable: boolean = false;
    public readonly isUnique: boolean = false;
    public readonly maxLength?: number;
    public readonly minLength?: number;
    public readonly pattern?: string;
    public readonly patternMessage?: string;

    public get isComplexUI() {
        return this.editor !== 'Input' && this.editor !== 'Color' && this.editor !== 'Radio' && this.editor !== 'Slug' && this.editor !== 'TextArea';
    }

    public get isTranslateable() {
        return this.editor === 'Input' || this.editor === 'TextArea';
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitString(this);
    }
}

export type TagsFieldEditor = 'Tags' | 'Checkboxes' | 'Dropdown';

export class TagsFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Tags';

    public readonly allowedValues?: ReadonlyArray<string>;
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
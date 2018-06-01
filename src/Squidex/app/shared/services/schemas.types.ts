/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export const fieldTypes = [
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
    }
];

export const fieldInvariant = 'iv';

export function createProperties(fieldType: string, values: Object | null = null): FieldPropertiesDto {
    let properties: FieldPropertiesDto;

    switch (fieldType) {
        case 'Array':
            properties = new ArrayFieldPropertiesDto();
            break;
        case 'Assets':
            properties = new AssetsFieldPropertiesDto();
            break;
        case 'Boolean':
            properties = new BooleanFieldPropertiesDto('Checkbox');
            break;
        case 'DateTime':
            properties = new DateTimeFieldPropertiesDto('DateTime');
            break;
        case 'Geolocation':
            properties = new GeolocationFieldPropertiesDto();
            break;
        case 'Json':
            properties = new JsonFieldPropertiesDto();
            break;
        case 'Number':
            properties = new NumberFieldPropertiesDto('Input');
            break;
        case 'References':
            properties = new ReferencesFieldPropertiesDto();
            break;
        case 'String':
            properties = new StringFieldPropertiesDto('Input');
            break;
        case 'Tags':
            properties = new TagsFieldPropertiesDto();
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
}

export abstract class FieldPropertiesDto {
    public abstract fieldType: string;

    public readonly editorUrl?: string;
    public readonly label?: string;
    public readonly hints?: string;
    public readonly placeholder?: string;
    public readonly isRequired: boolean = false;
    public readonly isListField: boolean = false;

    constructor(public readonly editor: string,
        props?: Partial<FieldPropertiesDto>
    ) {
        if (props) {
            Object.assign(this, props);
        }
    }

    public abstract accept<T>(visitor: FieldPropertiesVisitor<T>): T;
}

export class ArrayFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Array';

    public readonly minItems?: number;
    public readonly maxItems?: number;

    constructor(
        props?: Partial<ArrayFieldPropertiesDto>
    ) {
        super('Default', props);
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitArray(this);
    }
}

export class AssetsFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Assets';

    public readonly minItems?: number;
    public readonly maxItems?: number;
    public readonly minSize?: number;
    public readonly maxSize?: number;
    public readonly allowedExtensions?: string[];
    public readonly mustBeImage?: boolean;
    public readonly minWidth?: number;
    public readonly maxWidth?: number;
    public readonly minHeight?: number;
    public readonly maxHeight?: number;
    public readonly aspectWidth?: number;
    public readonly aspectHeight?: number;

    constructor(
        props?: Partial<AssetsFieldPropertiesDto>
    ) {
        super('Default', props);
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitAssets(this);
    }
}

export class BooleanFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Boolean';

    public readonly inlineEditable: boolean = false;
    public readonly defaultValue?: boolean;

    constructor(editor: string,
        props?: Partial<BooleanFieldPropertiesDto>
    ) {
        super(editor, props);
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitBoolean(this);
    }
}

export class DateTimeFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'DateTime';

    public readonly defaultValue?: string;
    public readonly maxValue?: string;
    public readonly minValue?: string;
    public readonly calculatedDefaultValue?: string;

    constructor(editor: string,
        props?: Partial<DateTimeFieldPropertiesDto>
    ) {
        super(editor, props);
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitDateTime(this);
    }
}

export class GeolocationFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Geolocation';

    constructor(
        props?: Partial<GeolocationFieldPropertiesDto>
    ) {
        super('Default', props);
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitGeolocation(this);
    }
}

export class JsonFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Json';

    constructor(
        props?: Partial<JsonFieldPropertiesDto>
    ) {
        super('Default', props);
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitJson(this);
    }
}

export class NumberFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Number';

    public readonly inlineEditable: boolean = false;
    public readonly defaultValue?: number;
    public readonly maxValue?: number;
    public readonly minValue?: number;
    public readonly allowedValues?: number[];

    constructor(editor: string,
        props?: Partial<NumberFieldPropertiesDto>
    ) {
        super(editor, props);
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitNumber(this);
    }
}

export class ReferencesFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'References';

    public readonly minItems?: number;
    public readonly maxItems?: number;
    public readonly schemaId?: string;

    constructor(
        props?: Partial<ReferencesFieldPropertiesDto>
    ) {
        super('Default', props);
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitReferences(this);
    }
}

export class StringFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'String';

    public readonly inlineEditable = false;
    public readonly defaultValue?: string;
    public readonly pattern?: string;
    public readonly patternMessage?: string;
    public readonly minLength?: number;
    public readonly maxLength?: number;
    public readonly allowedValues?: string[];

    constructor(editor: string,
        props?: Partial<StringFieldPropertiesDto>
    ) {
        super(editor, props);
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitString(this);
    }
}

export class TagsFieldPropertiesDto extends FieldPropertiesDto {
    public readonly fieldType = 'Tags';

    public readonly minItems?: number;
    public readonly maxItems?: number;

    constructor(
        props?: Partial<TagsFieldPropertiesDto>
    ) {
        super('Default', props);
    }

    public accept<T>(visitor: FieldPropertiesVisitor<T>): T {
        return visitor.visitTags(this);
    }
}
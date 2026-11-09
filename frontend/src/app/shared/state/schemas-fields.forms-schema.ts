/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { hidden, maxLength, SchemaPathTree } from '@angular/forms/signals';
import { SignalValidators } from '@app/framework';
import { FieldPropertiesVisitor } from '../model';
import type { DateTimeFieldFormModel, FieldFormModel, NumberFieldFormModel, StringFieldFormModel } from './schemas-fields.forms';

export class EditFieldSchemaVisitor implements FieldPropertiesVisitor<void> {
    constructor(
        private readonly path: SchemaPathTree<FieldFormModel>,
    ) {
    }

    public visitArray() {}

    public visitAssets() {}

    public visitBoolean() {}

    public visitComponent() {}

    public visitComponents() {}

    public visitDateTime() {
        const path = this.path as SchemaPathTree<DateTimeFieldFormModel>;

        maxLength(path.format, 100);
        SignalValidators.validDateTime(path.maxValue);
        SignalValidators.validDateTime(path.minValue);
    }

    public visitGeolocation() {}

    public visitJson() {}

    public visitNumber() {
        const path = this.path as SchemaPathTree<NumberFieldFormModel>;

        hidden(path.allowedValues,
            {when: ({ valueOf }) => !isChoiceEditor(valueOf(path.editor)) }
        );

        hidden(path.inlineEditable,
            {when: ({ valueOf }) => valueOf(path.editor) === 'Radio' }
        );
    }

    public visitReferences() {}

    public visitRichText() {}

    public visitString() {
        const path = this.path as SchemaPathTree<StringFieldFormModel>;

        hidden(path.allowedValues,
            {when: ({ valueOf }) => !isChoiceEditor(valueOf(path.editor)) }
        );

        hidden(path.classNames,
            {when: ({ valueOf }) => valueOf(path.editor) !== 'RichText' }
        );

        hidden(path.createEnum,
            {when: ({ valueOf }) => !isChoiceEditor(valueOf(path.editor)) }
        );

        hidden(path.inlineEditable,
            {when: ({ valueOf }) => !isInlineEditor(valueOf(path.editor)) }
        );
    }

    public visitTags() {}

    public visitUI() {}

    public visitUserInfo() {}
}

function isChoiceEditor(editor: string) {
    return editor === 'Radio' || editor === 'Dropdown';
}

function isInlineEditor(editor: string) {
    return editor === 'Input' || editor === 'Dropdown' || editor === 'Slug';
}

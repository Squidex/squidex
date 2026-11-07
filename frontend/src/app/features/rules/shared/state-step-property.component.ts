/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CodeEditorComponent, FormRowComponent, RuleElementPropertyDto, Types } from '@app/shared';

type ValueType = 'Raw' | 'Singleline' | 'MultilineAny' | 'MultilineJson';

@Component({
    selector: 'sqx-state-step-property',
    styleUrls: ['./state-step-property.component.scss'],
    templateUrl: './state-step-property.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeEditorComponent,
        FormRowComponent,
        FormsModule,
    ],
})
export class StateStepPropertyComponent {
    private readonly valueParsed = computed(() => {
        return parseValue(this.value());
    });

    /** The property definition, which provides the name and label. */
    public readonly property = input.required<RuleElementPropertyDto>();

    /** The configured value of the property. */
    public readonly value = input<any>();

    protected readonly valueFormatted = computed(() => {
        return this.valueParsed().formatted;
    });

    protected readonly valueType = computed(() => {
        return this.valueParsed().type;
    });
}

function parseValue(value: any): { type: ValueType; formatted: any } {
    if (Types.isNull(value) || Types.isUndefined(value)) {
        return { type: 'Raw', formatted: '-' };
    }

    if (Types.isNumber(value) || Types.isBoolean(value)) {
        return { type: 'Raw', formatted: value };
    }

    if (!Types.isString(value)) {
        return { type: 'MultilineJson', formatted: JSON.stringify(value, undefined, 2) };
    }

    const hasNewLine = value.indexOf('\n') >= 0;
    if (!hasNewLine) {
        return { type: 'Singleline', formatted: value };
    }

    const truncated = value.trim();
    if (truncated.indexOf('{') !== 0 && truncated.indexOf('[') !== 0) {
        return { type: 'MultilineAny', formatted: value };
    }

    try {
        return { type: 'MultilineJson', formatted: JSON.stringify(JSON.parse(value), undefined, 2) };
    } catch {
        return { type: 'MultilineAny', formatted: value };
    }
}

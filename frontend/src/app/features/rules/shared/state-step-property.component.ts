/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CodeEditorComponent, FormRowComponent, RuleElementPropertyDto, Types } from '@app/shared';

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
    @Input()
    public property!: RuleElementPropertyDto;

    @Input()
    public set value(value: any) {
        if (Types.isNull(value) || Types.isUndefined(value)) {
            this.valueType = 'Raw';
            this.valueFormatted = '-';
        } else if (Types.isNumber(value) || Types.isBoolean(value)) {
            this.valueType = 'Raw';
            this.valueFormatted = value;
        } else if (Types.isString(value)) {
            const truncated = value.trim();

            const hasNewLine = value.indexOf('\n') >= 0;
            if (!hasNewLine) {
                this.valueType = 'Singleline';
                this.valueFormatted = value;
            } else if (truncated.indexOf('{') === 0 || truncated.indexOf('[') === 0) {
                try {
                    this.valueType = 'MultilineJson';
                    this.valueFormatted = JSON.stringify(JSON.parse(value), undefined, 2);
                } catch {
                    this.valueType = 'MultilineAny';
                    this.valueFormatted = value;
                }
            } else {
                this.valueType = 'MultilineAny';
                this.valueFormatted = value;
            }
        } else {
            this.valueType = 'MultilineJson';
            this.valueFormatted = JSON.stringify(value, undefined, 2);
        }
    }

    public valueFormatted: any;
    public valueType: 'Raw' | 'Singleline' | 'MultilineAny' | 'MultilineJson' = 'Raw';
}
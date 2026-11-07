/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { FieldTree } from '@angular/forms/signals';
import { LocalizerService, Types } from '@app/framework/internal';
import { ControlErrorsMessagesComponent } from './control-errors-messages.component';
import { formatSignalErrors, getFieldKey } from './signal-validators';

@Component({
    selector: 'sqx-control-errors',
    styleUrls: ['./control-errors.component.scss'],
    templateUrl: './control-errors.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ControlErrorsMessagesComponent,
    ],
})
export class ControlErrorsComponent {
    private readonly localizer = inject(LocalizerService);

    /** The field to show the errors for. */
    public readonly for = input.required<string | FieldTree<unknown>>();

    /** The field name in the error messages. Falls back to a name derived from the control. */
    public readonly fieldName = input<string | null>();

    protected readonly field = computed(() => {
        const control = this.for();
        if (!Types.isFunction(control)) {
            return null;
        }

        return control as FieldTree<unknown>;
    });

    protected readonly controlDisplayName = computed(() => {
        const fieldName = this.fieldName();
        if (fieldName) {
            return fieldName;
        }

        let control = this.for();
        if (Types.isFunction(control)) {
            control = getFieldKey(control as FieldTree<unknown>);
        }

        if (!Types.isString(control) || !control) {
            return this.localizer.get('common.field')!;
        }

        return this.localizer.get(`common.${control}`) || control.substring(0, 1).toUpperCase() + control.substring(1);
    });

    protected readonly errorMessages = computed(() => {
        const field = this.field();
        if (!field) {
            return [];
        }

        const state = field();
        if (!state.touched() && !Types.isArray(state.value())) {
            return [];
        }

        return formatSignalErrors(this.localizer, this.controlDisplayName(), field);
    }, { equal: Types.equals });
}

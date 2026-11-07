/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal, untracked } from '@angular/core';
import { AbstractControl, FormGroupDirective, UntypedFormArray } from '@angular/forms';
import { FieldTree } from '@angular/forms/signals';
import { merge } from 'rxjs';
import { LocalizerService, Types } from '@app/framework/internal';
import { ControlErrorsMessagesComponent } from './control-errors-messages.component';
import { formatError } from './error-formatting';
import { touchedChange$ } from './forms-helper';
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
    private readonly formGroupDirective = inject(FormGroupDirective, { optional: true, host: true });
    private readonly localizer = inject(LocalizerService);

    private readonly field = computed(() => {
        const control = this.for();
        if (!Types.isFunction(control)) {
            return null;
        }

        return control as FieldTree<unknown>;
    });

    private readonly control = computed(() => {
        const control = this.for();
        if (Types.isFunction(control)) {
            return null;
        }

        if (!Types.isString(control)) {
            return control;
        }

        return this.formGroupDirective?.form?.controls[control] ?? null;
    });

    private readonly controlDisplayName = computed(() => {
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

    private readonly controlErrorMessages = signal<ReadonlyArray<string>>([]);

    private readonly fieldErrorMessages = computed(() => {
        const field = this.field();
        if (!field) {
            return [];
        }

        const submitCount = this.submitCount();
        if (Types.isNumber(submitCount) && submitCount === 0) {
            return [];
        }

        const state = field();
        if (!state.touched() && !Types.isArray(state.value())) {
            return [];
        }

        return formatSignalErrors(this.localizer, this.controlDisplayName(), field);
    }, { equal: Types.equals });

    /** The control or the name of the control in the parent form group. */
    public readonly for = input.required<string | AbstractControl | FieldTree<unknown>>();

    /** Shows the errors only when the form has been submitted, if set. */
    public readonly submitCount = input<number | null>();

    /** The field name in the error messages. Falls back to a name derived from the control. */
    public readonly fieldName = input<string | null>();

    protected readonly errorMessages = computed(() => {
        return this.field() ? this.fieldErrorMessages() : this.controlErrorMessages();
    });

    constructor() {
        effect(onCleanup => {
            const control = this.control();
            const controlDisplayName = this.controlDisplayName();
            const submitCount = this.submitCount();

            const update = () => {
                untracked(() => this.updateMessages(control, controlDisplayName, submitCount));
            };

            update();

            if (!control) {
                return;
            }

            const subscription =
                merge(
                    control.valueChanges,
                    control.statusChanges,
                    touchedChange$(control),
                ).subscribe(update);

            onCleanup(() => subscription.unsubscribe());
        });
    }

    private updateMessages(control: AbstractControl | null, controlDisplayName: string, submitCount: number | null | undefined) {
        const errorMessages: string[] = [];

        const isSubmitted = !Types.isNumber(submitCount) || submitCount > 0;
        const isTouched = control?.touched || Types.is(control, UntypedFormArray);

        if (control?.errors && control.invalid && isTouched && isSubmitted) {
            for (const [key, error] of Object.entries(control.errors)) {
                const message = formatError(this.localizer, controlDisplayName, key, error, control.value);

                if (Types.isString(message)) {
                    errorMessages.push(message);
                } else if (Types.isArray(message)) {
                    errorMessages.push(...message);
                }
            }
        }

        if (errorMessages.length === 0 && this.controlErrorMessages().length === 0) {
            return;
        }

        this.controlErrorMessages.set(errorMessages);
    }
}

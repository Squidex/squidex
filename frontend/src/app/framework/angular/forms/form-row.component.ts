/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, input, Optional, SkipSelf } from '@angular/core';
import { AbstractControl, FormGroupDirective } from '@angular/forms';
import { FieldTree } from '@angular/forms/signals';
import { Types } from '@app/framework/internal';
import { MarkdownDirective } from '../markdown.directive';
import { TranslatePipe } from '../pipes/translate.pipe';
import { ControlErrorsComponent } from './control-errors.component';
import { FormAlertComponent } from './form-alert.component';
import { FormHintComponent } from './form-hint.component';
import { getFieldKey } from './signal-validators';

@Component({
    selector: 'sqx-form-row',
    styleUrls: ['./form-row.component.scss'],
    templateUrl: './form-row.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    host: {
        'class': 'form-group',
        '[class]': 'class()',
        '[class.d-block]': 'vertical()',
        '[class.row]': '!vertical()',
        '[class.form-group-aligned]': 'centered()',
    },
    viewProviders: [
        {
            provide: FormGroupDirective,
            useFactory: (directive: FormGroupDirective) => directive,
            deps: [[new Optional(), new SkipSelf(), FormGroupDirective]],
        },
    ],
    imports: [
        ControlErrorsComponent,
        FormAlertComponent,
        FormHintComponent,
        MarkdownDirective,
        TranslatePipe,
    ],
})
export class FormRowComponent {
    /** The control or the name of the control in the parent form group. */
    public readonly for = input.required<string | AbstractControl | FieldTree<unknown>>();

    /** The label of the field. */
    public readonly label = input<string>();

    /** The unit, rendered after the field. */
    public readonly unit = input<string>();

    /** Reserves the space for the unit, even if no unit is defined. */
    public readonly showUnit = input(false, { transform: booleanAttribute });

    /** Shows the errors only when the form has been submitted, if set. */
    public readonly submitCount = input<number | null>();

    /** The ID of the field. Required when the control is not defined by name. */
    public readonly formId = input<string>();

    /** The hint below the field. Supports markdown. */
    public readonly hint = input<string>();

    /** The alert below the field. Supports markdown. */
    public readonly alert = input<string>();

    /** The width of the label. */
    public readonly labelSize = input<'md' | 'lg'>('md');

    /** Hides the validation errors. */
    public readonly hideError = input(false, { transform: booleanAttribute });

    /** Renders the field as checkbox with the label after the field. */
    public readonly check = input(false, { transform: booleanAttribute });

    /** Aligns the label vertically with the field. */
    public readonly centered = input(false, { transform: booleanAttribute });

    /** Renders the label above the field. */
    public readonly vertical = input(false, { transform: booleanAttribute });

    /** Marks the field as required in the label. */
    public readonly required = input(false, { transform: booleanAttribute });

    /** The prefix for the ID of the field, to make it unique. */
    public readonly prefix = input<any>('');

    /** Additional CSS classes for the host element. */
    public readonly class = input('');

    public readonly fieldName = computed(() => {
        const prefix = this.prefix();

        const name = this.getName();
        if (prefix) {
            return `${prefix}_${name}`;
        }

        return name;
    });

    protected readonly hasUnit = computed(() => {
        return !!this.unit() || this.showUnit();
    });

    protected readonly labelClass = computed(() => {
        if (this.vertical()) {
            return '';
        }

        const w = this.labelSize() === 'md' ? 3 : 4;
        return `col-${w} col-form-label`;
    });

    protected readonly offsetClass = computed(() => {
        if (this.vertical() || !this.check()) {
            return '';
        }

        const w = this.labelSize() === 'md' ? 3 : 4;
        return `offset-${w}`;
    });

    protected readonly mainClass = computed(() => {
        if (this.vertical()) {
            return '';
        }

        const w = this.labelSize() === 'md' ? 9 : 8;
        return `col-${w} ${this.offsetClass()}`;
    });

    private getName() {
        const control = this.for();
        if (Types.isString(control)) {
            return control;
        }

        const formId = this.formId();
        if (formId) {
            return formId;
        }

        if (Types.isFunction(control)) {
            return getFieldKey(control);
        }

        throw new Error('Form id is required.');
    }
}
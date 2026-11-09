/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/directive-selector */

import { Directive, forwardRef } from '@angular/core';
import { NG_VALUE_ACCESSOR, SelectControlValueAccessor, SelectMultipleControlValueAccessor } from '@angular/forms';

@Directive({
    selector: 'select:not([multiple])[formField]',
    providers: [
        { provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => SignalSelectDirective), multi: true },
        { provide: SelectControlValueAccessor, useExisting: forwardRef(() => SignalSelectDirective) },
    ],
})
export class SignalSelectDirective extends SelectControlValueAccessor {
}

@Directive({
    selector: 'select[multiple][formField]',
    providers: [
        { provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => SignalSelectMultipleDirective), multi: true },
        { provide: SelectMultipleControlValueAccessor, useExisting: forwardRef(() => SignalSelectMultipleDirective) },
    ],
})
export class SignalSelectMultipleDirective extends SelectMultipleControlValueAccessor {
}

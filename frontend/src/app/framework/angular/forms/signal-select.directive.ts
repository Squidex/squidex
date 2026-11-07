/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/directive-selector */

import { Directive, forwardRef } from '@angular/core';
import { NG_VALUE_ACCESSOR, SelectControlValueAccessor } from '@angular/forms';

@Directive({
    selector: 'select[formField]',
    providers: [
        { provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => SignalSelectDirective), multi: true },
        { provide: SelectControlValueAccessor, useExisting: forwardRef(() => SignalSelectDirective) },
    ],
})
export class SignalSelectDirective extends SelectControlValueAccessor {
}

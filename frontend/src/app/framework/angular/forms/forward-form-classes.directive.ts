/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, inject } from "@angular/core";
import { NgControl } from "@angular/forms";

@Directive({
    selector: '[sqxForwardFormClasses]',
    host: {
        '[class.ng-touched]': '!!ngControl?.touched',
        '[class.ng-untouched]': '!!ngControl?.untouched',
        '[class.ng-dirty]': '!!ngControl?.dirty',
        '[class.ng-pristine]': '!!ngControl?.pristine',
        '[class.ng-valid]': '!!ngControl?.valid',
        '[class.ng-invalid]': '!!ngControl?.invalid',
        '[class.ng-pending]': '!!ngControl?.pending',
    },
})
export class ForwardFormClassesDirective {
    protected readonly ngControl = inject(NgControl, { optional: true });
}

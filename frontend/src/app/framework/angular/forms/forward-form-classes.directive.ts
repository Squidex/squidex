/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, DoCheck, ElementRef, Injector, OnInit } from "@angular/core";
import { NgControl } from "@angular/forms";

@Directive({
    selector: '[sqxForwardFormClasses]',
})
export class ForwardFormClassesDirective implements OnInit, DoCheck {
    private ngControl: NgControl | null = null;

    constructor(
        private readonly injector: Injector,
        private readonly host: ElementRef<HTMLElement>,
    ) {
    }

    public ngOnInit() {
        this.ngControl = this.injector.get(NgControl, null);
    }

    public ngDoCheck() {
        const classes = this.host.nativeElement.classList;
        classes.toggle('ng-touched', !!this.ngControl?.touched);
        classes.toggle('ng-untouched', !!this.ngControl?.untouched);
        classes.toggle('ng-dirty', !!this.ngControl?.dirty);
        classes.toggle('ng-pristine', !!this.ngControl?.pristine);
        classes.toggle('ng-valid', !!this.ngControl?.valid);
        classes.toggle('ng-invalid', !!this.ngControl?.invalid);
        classes.toggle('ng-pending', !!this.ngControl?.pending);
    }
}
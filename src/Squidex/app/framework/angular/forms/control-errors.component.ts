/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights r vbeserved
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Host, Input, OnChanges, OnDestroy, Optional } from '@angular/core';
import { AbstractControl, FormGroupDirective } from '@angular/forms';
import { merge } from 'rxjs';

import {
    fadeAnimation,
    StatefulComponent,
    Types
} from '@app/framework/internal';

import { formatError } from './error-formatting';

interface State {
    errorMessages: string[];
}

@Component({
    selector: 'sqx-control-errors',
    styleUrls: ['./control-errors.component.scss'],
    templateUrl: './control-errors.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ControlErrorsComponent extends StatefulComponent<State> implements OnChanges, OnDestroy {
    private displayFieldName: string;
    private control: AbstractControl;
    private originalMarkAsTouched: any;

    @Input()
    public for: string | AbstractControl;

    @Input()
    public fieldName: string;

    @Input()
    public errors: string;

    @Input()
    public submitted: boolean;

    @Input()
    public submitOnly = false;

    constructor(changeDetector: ChangeDetectorRef,
        @Optional() @Host() private readonly formGroupDirective: FormGroupDirective
    ) {
        super(changeDetector, {
            errorMessages: []
        });
    }

    public ngOnDestroy() {
        super.ngOnDestroy();

        if (this.control && this.originalMarkAsTouched) {
            this.control['markAsTouched'] = this.originalMarkAsTouched;
        }
    }

    public ngOnChanges() {
        if (this.fieldName) {
            this.displayFieldName = this.fieldName;
        } else if (this.for) {
            if (Types.isString(this.for)) {
                this.displayFieldName = this.for.substr(0, 1).toUpperCase() + this.for.substr(1);
            } else {
                this.displayFieldName = 'field';
            }
        }

        let control: AbstractControl | null = null;

        if (Types.isString(this.for)) {
            control = this.formGroupDirective.form.controls[this.for];
        } else {
            control = this.for;
        }

        if (this.control !== control) {
            this.unsubscribeAll();

            this.control = control;

            if (control) {
                this.own(
                    merge(control.valueChanges, control.statusChanges)
                        .subscribe(() => {
                            this.createMessages();
                        }));

                this.originalMarkAsTouched = this.control.markAsTouched;

                const self = this;

                this.control['markAsTouched'] = function () {
                    self.originalMarkAsTouched.apply(this, arguments);

                    self.createMessages();
                };
            }
        }

        this.createMessages();
    }

    private createMessages() {
        const errors: string[] = [];

        if (this.control && this.control.invalid && ((this.control.touched && !this.submitOnly) || this.submitted) && this.control.errors) {
            for (let key in <any>this.control.errors) {
                if (this.control.errors.hasOwnProperty(key)) {
                    const message = formatError(this.displayFieldName, key, this.control.errors[key], this.control.value, this.errors);

                    if (message) {
                        errors.push(message);
                    }
                }
            }
        }

        if (errors.length !== this.snapshot.errorMessages.length || errors.length > 0) {
            this.next(s => ({ ...s, errorMessages: errors }));
        }
    }
}
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights r vbeserved
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Host, Input, OnChanges, OnDestroy, Optional } from '@angular/core';
import { AbstractControl, FormArray, FormGroupDirective } from '@angular/forms';
import { fadeAnimation, LocalizerService, StatefulComponent, Types } from '@app/framework/internal';
import { merge } from 'rxjs';
import { formatError } from './error-formatting';

interface State {
    // The error messages to show.
    errorMessages: ReadonlyArray<string>;
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

    public get isTouched() {
        return this.control.touched || Types.is(this.control, FormArray);
    }

    constructor(changeDetector: ChangeDetectorRef,
        @Optional() @Host() private readonly formGroupDirective: FormGroupDirective,
        private readonly localizer: LocalizerService
    ) {
        super(changeDetector, {
            errorMessages: []
        });
    }

    public ngOnDestroy() {
        super.ngOnDestroy();

        this.unsetCustomMarkAsTouchedFunction();
    }

    public ngOnChanges() {
        if (this.fieldName) {
            this.displayFieldName = this.fieldName;
        } else if (this.for) {
            if (Types.isString(this.for)) {
                let translation = this.localizer.get(`common.${this.for}`)!;

                if (!translation) {
                    translation = this.for.substr(0, 1).toUpperCase() + this.for.substr(1);
                }

                this.displayFieldName = translation;
            } else {
                this.displayFieldName = this.localizer.get(`common.field`)!;
            }
        }

        let control: AbstractControl | null = null;

        if (Types.isString(this.for)) {
            if (this.formGroupDirective && this.formGroupDirective.form) {
                control = this.formGroupDirective.form.controls[this.for];
            }
        } else {
            control = this.for;
        }

        if (this.control !== control && control) {
            this.unsubscribeAll();
            this.unsetCustomMarkAsTouchedFunction();

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

    private unsetCustomMarkAsTouchedFunction() {
        if (this.control && this.originalMarkAsTouched) {
            this.control['markAsTouched'] = this.originalMarkAsTouched;
        }
    }

    private createMessages() {
        const errors: string[] = [];

        if (this.control && this.control.invalid && this.isTouched && this.control.errors) {
            for (const key in <any>this.control.errors) {
                if (this.control.errors.hasOwnProperty(key)) {
                    const message = formatError(this.localizer, this.displayFieldName, key, this.control.errors[key], this.control.value);

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

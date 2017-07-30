/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AfterViewInit, Component, forwardRef, ElementRef, Input, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { ControlValueAccessor, FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Subscription } from 'rxjs';
import * as moment from 'moment';

let Pikaday = require('pikaday/pikaday');

const NOOP = () => { /* NOOP */ };

export const SQX_DATE_TIME_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => DateTimeEditorComponent), multi: true
};

@Component({
    selector: 'sqx-date-time-editor',
    styleUrls: ['./date-time-editor.component.scss'],
    templateUrl: './date-time-editor.component.html',
    providers: [SQX_DATE_TIME_EDITOR_CONTROL_VALUE_ACCESSOR]
})
export class DateTimeEditorComponent implements ControlValueAccessor, OnDestroy, OnInit, AfterViewInit {
    private timeSubscription: Subscription;
    private dateSubscription: Subscription;
    private picker: any;
    private timeValue: any | null = null;
    private dateValue: any | null = null;
    private suppressEvents = false;
    private changeCallback: (value: any) => void = NOOP;
    private touchedCallback: () => void = NOOP;

    @Input()
    public mode: string;

    @Input()
    public enforceTime: boolean;

    public timeControl = new FormControl();

    public dateControl = new FormControl();

    public get showTime() {
        return this.mode === 'DateTime';
    }

    public get hasValue() {
        return this.dateValue !== null;
    }

    @ViewChild('dateInput')
    public dateInput: ElementRef;

    public isDisabled = false;

    public ngOnDestroy() {
        this.dateSubscription.unsubscribe();
        this.timeSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.timeSubscription =
            this.timeControl.valueChanges.subscribe(value => {
                if (!value || value.length === 0) {
                    this.timeValue = null;
                } else {
                    this.timeValue = moment(value, 'HH:mm:ss');
                }

                this.updateValue();
            });

        this.dateSubscription =
            this.dateControl.valueChanges.subscribe(value => {
                if (!value || value.length === 0) {
                    this.dateValue = null;
                } else {
                    this.dateValue = moment(value, 'YYYY-MM-DD');
                }

                this.updateValue();
            });
    }

    public writeValue(value: any) {
        if (!value || value.length === 0) {
            this.timeValue = null;
            this.dateValue = null;
        } else {
            const parsed = moment.parseZone(value);

            this.dateValue = moment(parsed);

            if (this.showTime) {
                this.timeValue = moment(parsed);
            }
        }

        this.updateControls();
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;

        if (isDisabled) {
            this.dateControl.disable({ emitEvent: false });
            this.timeControl.disable({ emitEvent: false });
        } else {
            this.dateControl.enable({ emitEvent: false });
            this.timeControl.enable({ emitEvent: false });
        }
    }

    public registerOnChange(fn: any) {
        this.changeCallback = fn;
    }

    public registerOnTouched(fn: any) {
        this.touchedCallback = fn;
    }

    public ngAfterViewInit() {
        this.picker = new Pikaday({ field: this.dateInput.nativeElement, format: 'YYYY-MM-DD',
            onSelect: () => {
                if (this.suppressEvents) {
                    return;
                }
                this.dateValue = this.picker.getMoment();

                this.updateValue();
                this.touched();
            }
        });

        this.updateControls();
    }

    public touched() {
        this.touchedCallback();
    }

    public writeNow() {
        this.writeValue(new Date().toUTCString());

        this.updateControls();
        this.updateValue();
        this.touched();

        return false;
    }

    public reset() {
        this.timeControl.setValue(null, { emitEvent: false });
        this.dateControl.setValue(null, { emitEvent: false });

        this.dateValue = null;

        this.changeCallback(null);
        this.touchedCallback();

        return false;
    }

    private updateValue() {
        let result: string | null;

        if ((this.dateValue && !this.dateValue.isValid()) || (this.timeValue && !this.timeValue.isValid())) {
            result = 'Invalid DateTime';
        } else if (!this.dateValue && !this.timeValue) {
            result = null;
        } else {
            result = this.dateValue.format('YYYY-MM-DD');

            if (this.showTime && this.timeValue) {
                result += 'T';
                result += this.timeValue.format('HH:mm:ss');
                result += 'Z';
            } else if (this.enforceTime) {
                result += 'T00:00:00Z';
            }
        }

        this.changeCallback(result);
    }

    private updateControls() {
        if (!this.dateValue) {
            return;
        }

        this.suppressEvents = true;

        if (this.timeValue && this.timeValue.isValid()) {
            this.timeControl.setValue(this.timeValue.format('HH:mm:ss'), { emitEvent: false });
        }
        if (this.dateValue && this.dateValue.isValid() && this.picker) {
            this.dateControl.setValue(this.dateValue.format('YYYY-MM-DD'), { emitEvent: false });

            this.picker.setMoment(this.dateValue);
        }

        this.suppressEvents = false;
    }
}
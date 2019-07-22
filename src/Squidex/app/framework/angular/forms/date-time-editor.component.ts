/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, forwardRef, Input, OnInit, ViewChild } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import * as moment from 'moment';

import { StatefulControlComponent, Types } from '@app/framework/internal';

declare module 'pikaday/pikaday';

import * as Pikaday from 'pikaday/pikaday';

export const SQX_DATE_TIME_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => DateTimeEditorComponent), multi: true
};

@Component({
    selector: 'sqx-date-time-editor',
    styleUrls: ['./date-time-editor.component.scss'],
    templateUrl: './date-time-editor.component.html',
    providers: [SQX_DATE_TIME_EDITOR_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class DateTimeEditorComponent extends StatefulControlComponent<{}, string | null> implements OnInit, AfterViewInit {
    private picker: any;
    private timeValue: moment.Moment | null = null;
    private dateValue: moment.Moment | null = null;
    private suppressEvents = false;

    @Input()
    public mode: string;

    @Input()
    public enforceTime: boolean;

    @Input()
    public hideClear: boolean;

    @ViewChild('dateInput', { static: false })
    public dateInput: ElementRef;

    public timeControl = new FormControl();
    public dateControl = new FormControl();

    public get showTime() {
        return this.mode === 'DateTime';
    }

    public get hasValue() {
        return !!this.dateValue;
    }

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {});
    }

    public ngOnInit() {
        this.own(
            this.timeControl.valueChanges.subscribe(value => {
                if (!value || value.length === 0) {
                    this.timeValue = null;
                } else {
                    this.timeValue = moment.utc(value, 'HH:mm:ss');
                }

                this.callChangeFormatted();
            }));

        this.own(
            this.dateControl.valueChanges.subscribe(value => {
                if (!value || value.length === 0) {
                    this.dateValue = null;
                } else {
                    this.dateValue = moment.utc(value, 'YYYY-MM-DD');
                }

                this.callChangeFormatted();
            }));
    }

    public writeValue(obj: any) {
        if (Types.isString(obj) && obj.length > 0) {
            const parsed = moment.parseZone(obj);

            this.dateValue = parsed;

            if (this.showTime) {
                this.timeValue = parsed;
            }
        } else {
            this.timeValue = null;
            this.dateValue = null;
        }

        this.updateControls();
    }

    public setDisabledState(isDisabled: boolean): void {
        super.setDisabledState(isDisabled);

        if (isDisabled) {
            this.dateControl.disable({ emitEvent: false });
            this.timeControl.disable({ emitEvent: false });
        } else {
            this.dateControl.enable({ emitEvent: false });
            this.timeControl.enable({ emitEvent: false });
        }
    }

    public registerOnChange(fn: any) {
        this.callChange = fn;
    }

    public registerOnTouched(fn: any) {
        this.callTouched = fn;
    }

    public ngAfterViewInit() {
        this.picker = new Pikaday({ field: this.dateInput.nativeElement, format: 'YYYY-MM-DD',
            onSelect: () => {
                if (this.suppressEvents) {
                    return;
                }
                this.dateValue = this.picker.getMoment();

                this.callChangeFormatted();
                this.callTouched();
            }
        });

        this.updateControls();
    }

    public writeNow() {
        this.writeValue(new Date().toUTCString());

        this.updateControls();
        this.callChangeFormatted();
        this.callTouched();

        return false;
    }

    public reset() {
        this.timeControl.setValue(null, { emitEvent: false });
        this.dateControl.setValue(null, { emitEvent: false });

        this.dateValue = null;

        this.callChange(null);
        this.callTouched();

        return false;
    }

    private callChangeFormatted() {
        this.callChange(this.getValue());
    }

    private getValue(): string | null {
        if (!this.dateValue || !this.dateValue.isValid()) {
            return null;
        }

        if (this.timeValue && !this.timeValue.isValid()) {
            return null;
        }

        let result = this.dateValue.format('YYYY-MM-DD');

        if (this.showTime && this.timeValue) {
            result += 'T';
            result += this.timeValue.format('HH:mm:ss');
            result += 'Z';
        } else if (this.enforceTime) {
            result += 'T00:00:00Z';
        }

        return result;
    }

    private updateControls() {
        this.suppressEvents = true;

        if (this.timeValue && this.timeValue.isValid()) {
            this.timeControl.setValue(this.timeValue.format('HH:mm:ss'), { emitEvent: false });
        } else {
            this.timeControl.setValue(null, { emitEvent: false });
        }

        if (this.dateValue && this.dateValue.isValid() && this.picker) {
            const dateString = this.dateValue.format('YYYY-MM-DD');
            const dateLocal = moment(dateString);

            this.dateControl.setValue(dateString, { emitEvent: false });

            this.picker.setMoment(dateLocal);
        } else {
            this.dateControl.setValue(null, { emitEvent: false });
        }

        this.suppressEvents = false;
    }
}
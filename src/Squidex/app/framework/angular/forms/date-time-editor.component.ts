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
    private timeValue: any | null = null;
    private dateValue: any | null = null;
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
                    this.timeValue = moment(value, 'HH:mm:ss');
                }

                this.updateValue();
            }));

        this.own(
            this.dateControl.valueChanges.subscribe(value => {
                if (!value || value.length === 0) {
                    this.dateValue = null;
                } else {
                    this.dateValue = moment(value, 'YYYY-MM-DD');
                }

                this.updateValue();
            }));
    }

    public writeValue(obj: any) {
        if (Types.isString(obj) && obj.length > 0) {
            const parsed = moment.parseZone(obj);

            this.dateValue = moment(parsed);

            if (this.showTime) {
                this.timeValue = moment(parsed);
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

                this.updateValue();
                this.callTouched();
            }
        });

        this.updateControls();
    }

    public writeNow() {
        this.writeValue(new Date().toUTCString());

        this.updateControls();
        this.updateValue();
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

    private updateValue() {
        let result: string | null;

        if ((this.dateValue && !this.dateValue.isValid()) || (this.timeValue && !this.timeValue.isValid())) {
            result = null;
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

        this.callChange(result);
    }

    private updateControls() {
        this.suppressEvents = true;

        if (this.timeValue && this.timeValue.isValid()) {
            this.timeControl.setValue(this.timeValue.format('HH:mm:ss'), { emitEvent: false });
        } else {
            this.timeControl.setValue(null, { emitEvent: false });
        }

        if (this.dateValue && this.dateValue.isValid() && this.picker) {
            this.dateControl.setValue(this.dateValue.format('YYYY-MM-DD'), { emitEvent: false });

            this.picker.setMoment(this.dateValue);
        } else {
            this.dateControl.setValue(null, { emitEvent: false });
        }

        this.suppressEvents = false;
    }
}

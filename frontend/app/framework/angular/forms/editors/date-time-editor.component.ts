/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, forwardRef, Input, OnInit, ViewChild } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { DateHelper, DateTime, StatefulControlComponent, UIOptions } from '@app/framework/internal';
import * as Pikaday from 'pikaday/pikaday';
import { FocusComponent } from './../forms-helper';

declare module 'pikaday/pikaday';

export const SQX_DATE_TIME_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => DateTimeEditorComponent), multi: true
};

const NO_EMIT = { emitEvent: false };

@Component({
    selector: 'sqx-date-time-editor',
    styleUrls: ['./date-time-editor.component.scss'],
    templateUrl: './date-time-editor.component.html',
    providers: [
        SQX_DATE_TIME_EDITOR_CONTROL_VALUE_ACCESSOR
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class DateTimeEditorComponent extends StatefulControlComponent<{}, string | null> implements OnInit, AfterViewInit, FocusComponent {
    private picker: any;
    private dateTime: DateTime | null;
    private hideDateButtonsSettings: boolean;
    private hideDateTimeModeButtonSetting: boolean;
    private suppressEvents = false;

    @Input()
    public mode: 'DateTime' | 'Date';

    @Input()
    public enforceTime: boolean;

    @Input()
    public hideClear: boolean;

    @Input()
    public hideDateButtons: boolean;

    @Input()
    public hideDateTimeModeButton: boolean;

    @Input()
    public isCompact: boolean;

    @ViewChild('dateInput', { static: false })
    public dateInput: ElementRef<HTMLInputElement>;

    public timeControl = new FormControl();
    public dateControl = new FormControl();
    public isLocalMode = true;

    public get shouldShowDateButtons() {
        return !this.hideDateButtonsSettings && !this.hideDateButtons;
    }

    public get shouldShowDateTimeModeButton() {
        return !this.hideDateTimeModeButtonSetting && !this.hideDateTimeModeButton;
    }

    public get showTime() {
        return this.mode === 'DateTime';
    }

    public get hasValue() {
        return !!this.dateTime;
    }

    constructor(changeDetector: ChangeDetectorRef, uiOptions: UIOptions) {
        super(changeDetector, {});

        this.hideDateButtonsSettings = !!uiOptions.get('hideDateButtons');
        this.hideDateTimeModeButtonSetting = !!uiOptions.get('hideDateTimeModeButton');
    }

    public ngOnInit() {
        this.own(
            this.timeControl.valueChanges.subscribe(() => {
                this.callChangeFormatted();
            }));

        this.own(
            this.dateControl.valueChanges.subscribe(() => {
                this.callChangeFormatted();
            }));
    }

    public writeValue(obj: any) {
        try {
            this.dateTime = DateTime.parseISO(obj);
        } catch (ex) {
            this.dateTime = null;
        }

        this.updateControls();
    }

    public setDisabledState(isDisabled: boolean): void {
        super.setDisabledState(isDisabled);

        if (isDisabled) {
            this.dateControl.disable(NO_EMIT);
            this.timeControl.disable(NO_EMIT);
        } else {
            this.dateControl.enable(NO_EMIT);
            this.timeControl.enable(NO_EMIT);
        }
    }

    public focus() {
        this.dateInput.nativeElement.focus();
    }

    public ngAfterViewInit() {
        this.picker = new Pikaday({field: this.dateInput.nativeElement, format: 'YYYY-MM-DD',
            onSelect: () => {
                if (this.suppressEvents) {
                    return;
                }

                this.dateControl.setValue(this.picker.toString('YYYY-MM-DD'));

                this.callTouched();
            }
        });

        this.updateControls();
    }

    public writeNow() {
        let datetime = DateTime.parseISO(DateTime.now().toISOString(), !this.isLocalMode);
        this.writeValue(datetime.toISOString());

        this.updateControls();
        this.callChangeFormatted();
        this.callTouched();

        return false;
    }

    public reset() {
        this.dateTime = null;

        this.updateControls();

        this.callChange(null);
        this.callTouched();

        return false;
    }

    private callChangeFormatted() {
        this.callChange(this.getValue());
    }

    private getValue(): string | null {
        if (!this.dateControl.value) {
            return null;
        }

        let result: string | null = null;

        if (this.showTime && this.timeControl.value) {
            const combined = `${this.dateControl.value}T${this.timeControl.value}`;

            const parsed = DateTime.tryParseISO(combined, !this.isLocalMode);

            if (parsed) {
                result = parsed.toISOString();
            }
        }

        if (!result) {
            const parsed = DateTime.tryParseISO(this.dateControl.value);

            if (parsed) {
                result = parsed.toISOString();
            }
        }

        return result;
    }

    private updateControls() {
        this.suppressEvents = true;

        if (this.dateTime && this.mode === 'DateTime') {
            if (this.isLocalMode) {
                this.timeControl.setValue(this.dateTime.toStringFormat('HH:mm:ss'), NO_EMIT);
            } else {
                this.timeControl.setValue(this.dateTime.toStringFormatUTC('HH:mm:ss'), NO_EMIT);
            }
        } else {
            this.timeControl.setValue(null, NO_EMIT);
        }

        if (this.dateTime && this.picker) {
            let dateString: string;

            if (this.showTime && this.isLocalMode) {
                dateString = this.dateTime.toStringFormat('yyyy-MM-dd');
            } else {
                dateString = this.dateTime.toStringFormatUTC('yyyy-MM-dd');
            }

            this.picker.setDate(DateHelper.getLocalDate(this.dateTime.raw), true);

            this.dateControl.setValue(dateString, NO_EMIT);
        } else {
            this.dateControl.setValue(null, NO_EMIT);
        }

        this.suppressEvents = false;
    }

    public setLocalMode(isLocalMode: boolean) {
        this.isLocalMode = isLocalMode;

        if (!this.dateControl.value) {
            return null;
        }

        let parsed: DateTime | null;

        if (this.timeControl.value) {
            const combined = `${this.dateControl.value}T${this.timeControl.value}`;
            parsed = DateTime.tryParseISO(combined, this.isLocalMode);
        } else {
            parsed = DateTime.tryParseISO(`${this.dateControl.value}T00:00:00`, this.isLocalMode);
        }

        if (parsed) {
            if (this.isLocalMode) {
                const dateString = parsed.toStringFormat('yyyy-MM-dd');

                this.dateControl.setValue(dateString, NO_EMIT);
                this.timeControl.setValue(parsed.toStringFormat('HH:mm:ss'), NO_EMIT);

                this.picker.setDate(dateString);
            } else {
                const dateString = parsed.toStringFormatUTC('yyyy-MM-dd');

                this.dateControl.setValue(dateString, NO_EMIT);
                this.timeControl.setValue(parsed.toStringFormatUTC('HH:mm:ss'), NO_EMIT);

                this.picker.setDate(dateString);
            }

            this.callChangeFormatted();
            this.callTouched();
        }
    }

    public setCompact(isCompact: boolean) {
        this.next(s => ({ ...s, isCompact: isCompact }));
    }
}
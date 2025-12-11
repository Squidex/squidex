/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, ElementRef, EventEmitter, forwardRef, inject, Input, Output, ViewChild } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { DateHelper, DateTime, StatefulControlComponent, UIOptions } from '@app/framework/internal';
import { TooltipDirective } from '../../modals/tooltip.directive';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { ResizedDirective } from '../../resized.directive';
import { FocusComponent } from '../forms-helper';

export const SQX_DATE_TIME_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => DateTimeEditorComponent), multi: true,
};

interface State {
    // True when the editor is in local mode.
    isLocal: boolean;

    // The actual value.
    value?: DateTime | null;

    // The value for the input.
    inputValue?: string | null;
}

@Component({
    selector: 'sqx-date-time-editor',
    styleUrls: ['./date-time-editor.component.scss'],
    templateUrl: './date-time-editor.component.html',
    providers: [
        SQX_DATE_TIME_EDITOR_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormsModule,
        ReactiveFormsModule,
        ResizedDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class DateTimeEditorComponent extends StatefulControlComponent<State, string | null> implements FocusComponent {
    private readonly hideDateButtonsSettings: boolean = !!inject(UIOptions).value.hideDateButtons;
    private readonly hideDateTimeModeButtonSetting: boolean = !!inject(UIOptions).value.hideDateTimeModeButton;

    @Output()
    public editorBlur = new EventEmitter();

    @Input()
    public mode: 'DateTime' | 'Date' = 'Date';

    @Input()
    public id = '';

    @Input({ transform: booleanAttribute })
    public hideClear?: boolean | null;

    @Input({ transform: booleanAttribute })
    public hideDateButtons?: boolean | null;

    @Input({ transform: booleanAttribute })
    public hideDateTimeModeButton?: boolean | null;

    @Input()
    public size: 'Normal' | 'Compact' | 'Mini' = 'Normal';

    @Input({ transform: booleanAttribute })
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    @ViewChild('dateInput', { static: false })
    public dateInput!: ElementRef<HTMLInputElement>;

    public get shouldShowDateButtons() {
        return !this.hideDateButtonsSettings && !this.hideDateButtons;
    }

    public get shouldShowDateTimeModeButton() {
        return !this.hideDateTimeModeButtonSetting && !this.hideDateTimeModeButton;
    }

    public get isDateTimeMode() {
        return this.mode === 'DateTime';
    }

    public get hasValue() {
        return !!this.snapshot.value;
    }

    constructor() {
        super({ isLocal: true });
    }

    public callTouched() {
        this.editorBlur.next(true);

        super.callTouched();
    }

    public focus() {
        this.dateInput.nativeElement.focus();
    }

    public writeValue(obj: any) {
        let value: DateTime | null;
        try {
            value = DateTime.parseISO(obj, false);
        } catch (ex) {
            value = null;
        }

        this.update(value, this.snapshot.isLocal, false);
    }

    public writeToday() {
        this.update(new DateTime(DateHelper.getLocalDate(DateTime.today().raw)), this.snapshot.isLocal, true);
        return false;
    }

    public writeNow() {
        this.update(DateTime.now(), this.snapshot.isLocal, true);
        return false;
    }

    public reset() {
        this.update(null, this.snapshot.isLocal, true);
        return false;
    }

    public setLocalMode(isLocal: boolean) {
        this.update(this.snapshot.value, isLocal, true);
        return false;
    }

    public updateValue(source: string) {
        this.update(DateTime.tryParseISO(source, !this.snapshot.isLocal), this.snapshot.isLocal, true);
        return false;
    }

    private update(value: DateTime | null | undefined, isLocal: boolean, emit: boolean) {
        let inputValue: string | null = null;

        if (value) {
            if (this.isDateTimeMode) {
                if (isLocal) {
                    inputValue = value.toStringFormat('yyyy-MM-dd\'T\'HH:mm:ss');
                } else {
                    inputValue = value.toStringFormatUTC('yyyy-MM-dd\'T\'HH:mm:ss');
                }
            } else {
                if (isLocal) {
                    inputValue = value.toStringFormat('yyyy-MM-dd');
                } else {
                    inputValue = value.toStringFormatUTC('yyyy-MM-dd');
                }
            }
        }

        this.next({ inputValue, value, isLocal });

        if (emit) {
            this.callChange(value?.toISOString());
            this.callTouched();
        }
    }

    public setSize(size: DOMRect) {
        if (size.width < 300) {
            this.size = 'Mini';
        } else if (size.width < 350) {
            this.size = 'Compact';
        } else {
            this.size = 'Normal';
        }
    }
}

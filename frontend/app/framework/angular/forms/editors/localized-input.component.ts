/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { fadeAnimation, ModalModel, StatefulControlComponent, Types } from '@app/framework/internal';
import { Language } from './../../language-selector.component';

export const SQX_LOCALIZED_INPUT_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => LocalizedInputComponent), multi: true,
};

const DEFAULT_LANGUAGE = { iso2Code: 'iv', englishName: 'Invariant' };

interface State {
    // The selected language.
    language: Language;
}

@Component({
    selector: 'sqx-localized-input',
    styleUrls: ['./localized-input.component.scss'],
    templateUrl: './localized-input.component.html',
    providers: [
        SQX_LOCALIZED_INPUT_CONTROL_VALUE_ACCESSOR,
    ],
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LocalizedInputComponent extends StatefulControlComponent<State, { [key: string]: any }> {
    private value: { [key: string]: any } | undefined;

    @Input()
    public languages: ReadonlyArray<Language>;

    @Input()
    public type: 'text' | 'boolean' | 'datetime' | 'date' | 'tags' | 'number' = 'text';

    @Input()
    public name: string;

    @Input()
    public id: string;

    @Input()
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    public dropdown = new ModalModel();

    public get currentValue() {
        if (!this.snapshot.language || !this.value) {
            return undefined;
        }

        return this.value[this.snapshot.language.iso2Code];
    }

    public get isEmpty() {
        if (!this.snapshot.language || !this.value) {
            return true;
        }

        return !this.value.hasOwnProperty(this.snapshot.language.iso2Code);
    }

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {
            language: DEFAULT_LANGUAGE,
        });
    }

    public setLanguage(language: Language) {
        this.next({ language });
    }

    public writeValue(obj: any) {
        if (Types.isObject(obj)) {
            this.value = obj;
        } else {
            this.value = {};
        }
    }

    public setValue(value: any) {
        this.value = { ...this.value || {} };
        this.value[this.snapshot.language.iso2Code] = value;

        this.callChange(this.value);
    }

    public unset() {
        this.value = { ...this.value || {} };

        delete this.value[this.snapshot.language.iso2Code];

        if (Object.keys(this.value).length === 0) {
            this.value = undefined;
        }

        this.callChange(this.value);
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, forwardRef, Input } from '@angular/core';
import { ControlValueAccessor, FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';

const KEY_ENTER = 13;
const NOOP = () => { /* NOOP */ };

export interface Converter {
    convert(input: string): any;

    isValid(input: string): boolean;
}

export class IntConverter implements Converter {
    public isValid(input: string): boolean {
        return !!parseInt(input, 10) || input === '0';
    }

    public convert(input: string): any {
        return parseInt(input, 10) || 0;
    }
}

export class FloatConverter implements Converter {
    public isValid(input: string): boolean {
        return !!parseFloat(input) || input === '0';
    }

    public convert(input: string): any {
        return parseFloat(input) || 0;
    }
}

export class NoopConverter implements Converter {
    public isValid(input: string): boolean {
        return input.trim().length > 0;
    }

    public convert(input: string): any {
        return input.trim();
    }
}

export const SQX_TAG_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => TagEditorComponent), multi: true
};

@Component({
    selector: 'sqx-tag-editor',
    styleUrls: ['./tag-editor.component.scss'],
    templateUrl: './tag-editor.component.html',
    providers: [SQX_TAG_EDITOR_CONTROL_VALUE_ACCESSOR]
})
export class TagEditorComponent implements ControlValueAccessor {
    private changeCallback: (value: any) => void = NOOP;
    private touchedCallback: () => void = NOOP;

    @Input()
    public converter: Converter = new NoopConverter();

    @Input()
    public useDefaultValue = true;

    @Input()
    public inputName = 'tag-editor';

    public items: any[] = [];

    public addInput = new FormControl();

    public writeValue(value: any) {
        this.addInput.setValue('');

        if (Array.isArray(value)) {
            this.items = value;
        } else {
            this.items = [];
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        if (isDisabled) {
            this.addInput.disable();
        } else {
            this.addInput.enable();
        }
    }

    public registerOnChange(fn: any) {
        this.changeCallback = fn;
    }

    public registerOnTouched(fn: any) {
        this.touchedCallback = fn;
    }

    public remove(index: number) {
        this.updateItems([...this.items.slice(0, index), ...this.items.splice(index + 1)]);
    }

    public markTouched() {
        this.touchedCallback();
    }

    public onKeyDown(event: KeyboardEvent) {
        if (event.keyCode === KEY_ENTER) {
            const value = <string>this.addInput.value;

            if (this.converter.isValid(value)) {
                const converted = this.converter.convert(value);

                this.updateItems([...this.items, converted]);
                this.addInput.reset();
                return false;
            }
        }

        return true;
    }

    private updateItems(items: string[]) {
        this.items = items;

        if (items.length === 0 && this.useDefaultValue) {
            this.changeCallback(undefined);
        } else {
            this.changeCallback(this.items);
        }
    }
}
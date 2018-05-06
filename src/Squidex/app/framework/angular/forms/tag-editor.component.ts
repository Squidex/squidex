/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, forwardRef, Input } from '@angular/core';
import { ControlValueAccessor, FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';

import { Types } from '@app/framework/internal';

const KEY_ENTER = 13;

export interface Converter {
    convert(input: string): any;

    isValidInput(input: string): boolean;
    isValidValue(value: any): boolean;
}

export class IntConverter implements Converter {
    public isValidInput(input: string): boolean {
        return !!parseInt(input, 10) || input === '0';
    }

    public isValidValue(value: any): boolean {
        return Types.isNumber(value);
    }

    public convert(input: string): any {
        return parseInt(input, 10) || 0;
    }
}

export class FloatConverter implements Converter {
    public isValidInput(input: string): boolean {
        return !!parseFloat(input) || input === '0';
    }

    public isValidValue(value: any): boolean {
        return Types.isNumber(value);
    }

    public convert(input: string): any {
        return parseFloat(input) || 0;
    }
}

export class StringConverter implements Converter {
    public isValidInput(input: string): boolean {
        return input.trim().length > 0;
    }

    public isValidValue(value: any): boolean {
        return Types.isString(value);
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
    private callChange = (v: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };

    @Input()
    public converter: Converter = new StringConverter();

    @Input()
    public useDefaultValue = true;

    @Input()
    public inputName = 'tag-editor';

    public items: any[] = [];

    public addInput = new FormControl();

    public writeValue(obj: any) {
        this.resetForm();

        if (this.converter && Types.isArrayOf(obj, v => this.converter.isValidValue(v))) {
            this.items = obj;
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
        this.callChange = fn;
    }

    public registerOnTouched(fn: any) {
        this.callTouched = fn;
    }

    public remove(index: number) {
        this.updateItems([...this.items.slice(0, index), ...this.items.splice(index + 1)]);
    }

    public markTouched() {
        this.callTouched();
    }

    private resetForm() {
        this.addInput.reset();
    }

    public onKeyDown(event: KeyboardEvent) {
        if (event.keyCode === KEY_ENTER) {
            const value = <string>this.addInput.value;

            if (this.converter.isValidInput(value)) {
                const converted = this.converter.convert(value);

                this.updateItems([...this.items, converted]);
                this.resetForm();
                return false;
            }
        }

        return true;
    }

    private updateItems(items: string[]) {
        this.items = items;

        if (items.length === 0 && this.useDefaultValue) {
            this.callChange(undefined);
        } else {
            this.callChange(this.items);
        }
    }
}
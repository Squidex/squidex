/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, forwardRef, Input, OnInit, ViewChild } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { distinctUntilChanged, map, tap } from 'rxjs/operators';

import { StatefulControlComponent, Types } from '@app/framework/internal';

const KEY_COMMA = 188;
const KEY_DELETE = 8;
const KEY_ENTER = 13;
const KEY_UP = 38;
const KEY_DOWN = 40;

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

const CACHED_SIZES: { [key: string]: number } = {};

let CACHED_FONT: string;

interface State {
    hasFocus: boolean;

    suggestedItems: string[];
    suggestedIndex: number;

    items: any[];
}

@Component({
    selector: 'sqx-tag-editor',
    styleUrls: ['./tag-editor.component.scss'],
    templateUrl: './tag-editor.component.html',
    providers: [SQX_TAG_EDITOR_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class TagEditorComponent extends StatefulControlComponent<State, any[]> implements AfterViewInit, OnInit {
    @Input()
    public converter: Converter = new StringConverter();

    @Input()
    public undefinedWhenEmpty = true;

    @Input()
    public acceptEnter = false;

    @Input()
    public allowDuplicates = true;

    @Input()
    public suggestions: string[] = [];

    @Input()
    public singleLine = false;

    @Input()
    public class: string;

    @Input()
    public placeholder = ', to add tag';

    @Input()
    public inputName = 'tag-editor';

    @ViewChild('form')
    public formElement: ElementRef<HTMLElement>;

    @ViewChild('input')
    public inputElement: ElementRef<HTMLInputElement>;

    public addInput = new FormControl();

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {
            hasFocus: false,
            suggestedItems: [],
            suggestedIndex: 0,
            items: []
        });
    }

    public ngAfterViewInit() {
        if (!CACHED_FONT) {
            const style = window.getComputedStyle(this.inputElement.nativeElement);

            CACHED_FONT = `${style.getPropertyValue('font-size')} ${style.getPropertyValue('font-family')}`;
        }

        this.resetSize();
    }

    public ngOnInit() {
        this.own(
            this.addInput.valueChanges.pipe(
                    tap(() => {
                        this.resetSize();
                    }),
                    map(query => <string>query),
                    map(query => query ? query.trim().toLowerCase() : query),
                    tap(query => {
                        if (!query) {
                            this.resetAutocompletion();
                        }
                    }),
                    distinctUntilChanged(),
                    map(query => {
                        if (Types.isArray(this.suggestions) && query && query.length > 0) {
                            return this.suggestions.filter(s => s.toLowerCase().indexOf(query) >= 0 && this.snapshot.items.indexOf(s) < 0);
                        } else {
                            return [];
                        }
                    }))
                .subscribe(items => {
                    this.next(s => ({
                        ...s,
                        suggestedIndex: -1,
                        suggestedItems: items || []
                    }));
                }));
    }

    public writeValue(obj: any) {
        this.resetForm();
        this.resetSize();

        if (this.converter && Types.isArrayOf(obj, v => this.converter.isValidValue(v))) {
            this.next(s => ({ ...s, items: obj }));
        } else {
            this.next(s => ({ ...s, items: [] }));
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        super.setDisabledState(isDisabled);

        if (isDisabled) {
            this.addInput.disable();
        } else {
            this.addInput.enable();
        }
    }

    public focus() {
        if (this.addInput.enabled) {
            this.next(s => ({ ...s, hasFocus: true }));
        }
    }

    public markTouched() {
        this.selectValue(this.addInput.value, true);

        this.resetAutocompletion();
        this.resetFocus();

        this.callTouched();
    }

    public remove(index: number) {
        this.updateItems(this.snapshot.items.filter((_, i) => i !== index));
    }

    public resetSize() {
        if (!CACHED_FONT) {
            return;
        }

        if (!canvas) {
            canvas = document.createElement('canvas');
        }

        if (canvas) {
            const ctx = canvas.getContext('2d');

            if (ctx) {
                ctx.font = CACHED_FONT;

                const text = this.inputElement.nativeElement.value;
                const textKey = `${text}§${this.placeholder}§${ctx.font}`;

                let width = CACHED_SIZES[textKey];

                if (!width) {
                    const widthText = ctx.measureText(text).width;
                    const widthPlaceholder = ctx.measureText(this.placeholder).width;

                    width = Math.max(widthText, widthPlaceholder);

                    CACHED_SIZES[textKey] = width;
                }

                this.inputElement.nativeElement.style.width = <any>((width + 5) + 'px');
            }
        }

        if (this.singleLine) {
            setTimeout(() => {
                this.formElement.nativeElement.scrollLeft = this.formElement.nativeElement.scrollWidth;
            }, 0);
        }
    }

    public onKeyDown(event: KeyboardEvent) {
        const key = event.keyCode;

        if (key === KEY_COMMA) {
            if (this.selectValue(this.addInput.value)) {
                return false;
            }
        } else if (key === KEY_DELETE) {
            const value = <string>this.addInput.value;

            if (!value || value.length === 0) {
                this.updateItems(this.snapshot.items.slice(0, this.snapshot.items.length - 1));

                return false;
            }
        } else if (key === KEY_UP) {
            this.up();
            return false;
        } else if (key === KEY_DOWN) {
            this.down();
            return false;
        } else if (key === KEY_ENTER) {
            if (this.snapshot.suggestedIndex >= 0) {
                if (this.selectValue(this.snapshot.suggestedItems[this.snapshot.suggestedIndex])) {
                    return false;
                }
            } else if (this.acceptEnter) {
                if (this.selectValue(this.addInput.value)) {
                    return false;
                }
            }
        }

        return true;
    }

    public selectValue(value: string, noFocus?: boolean) {
        if (!noFocus) {
            this.inputElement.nativeElement.focus();
        }

        if (value && this.converter.isValidInput(value)) {
            const converted = this.converter.convert(value);

            if (this.allowDuplicates || this.snapshot.items.indexOf(converted) < 0) {
                this.updateItems([...this.snapshot.items, converted]);
            }

            this.resetForm();
            this.resetAutocompletion();
            return true;
        }
    }

    private resetAutocompletion() {
        this.next(s => ({
            ...s,
            suggestedItems: [],
            suggestedIndex: -1
        }));
    }

    public selectIndex(suggestedIndex: number) {
        if (suggestedIndex < 0) {
            suggestedIndex = 0;
        }

        if (suggestedIndex >= this.snapshot.suggestedItems.length) {
            suggestedIndex = this.snapshot.suggestedItems.length - 1;
        }

        this.next(s => ({ ...s, suggestedIndex }));
    }

    public resetFocus(): any {
        this.next(s => ({ ...s, hasFocus: false }));
    }

    private resetForm() {
        this.addInput.reset();
    }

    private up() {
        this.selectIndex(this.snapshot.suggestedIndex - 1);
    }

    private down() {
        this.selectIndex(this.snapshot.suggestedIndex + 1);
    }

    public onCut(event: ClipboardEvent) {
        if (!this.hasSelection()) {
            this.onCopy(event);

            this.updateItems([]);
        }
    }

    public onCopy(event: ClipboardEvent) {
        if (!this.hasSelection()) {
            event.clipboardData.setData('text/plain', this.snapshot.items.filter(x => !!x).join(','));

            event.preventDefault();
        }
    }

    public onPaste(event: ClipboardEvent) {
        const value = event.clipboardData.getData('text/plain');

        if (value) {
            this.resetForm();

            const values = [...this.snapshot.items];

            for (let part of value.split(',')) {
                const converted = this.converter.convert(part);

                if (converted) {
                    values.push(converted);
                }
            }

            this.updateItems(values);
        }

        event.preventDefault();
    }

    private hasSelection() {
        const s = this.inputElement.nativeElement.selectionStart;
        const e = this.inputElement.nativeElement.selectionEnd;

        return s && e && (e - s) > 0;
    }

    private updateItems(items: any[]) {
        this.next(s => ({ ...s, items }));

        if (items.length === 0 && this.undefinedWhenEmpty) {
            this.callChange(undefined);
        } else {
            this.callChange(items);
        }

        this.resetSize();
    }
}

let canvas: HTMLCanvasElement | null = null;
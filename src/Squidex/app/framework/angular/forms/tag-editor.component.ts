/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:template-use-track-by-function

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, forwardRef, Input, OnInit, ViewChild } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { distinctUntilChanged, map, tap } from 'rxjs/operators';

import {
    fadeAnimation,
    Keys,
    StatefulControlComponent,
    Types
} from '@app/framework/internal';

export const CONVERSION_FAILED = {};

export class TagValue<T = any> {
    public readonly lowerCaseName: string;

    constructor(
        public readonly id: any,
        public readonly name: string,
        public readonly value: T
    ) {
        this.lowerCaseName = name.toLowerCase();
    }

    public toString() {
        return this.name;
    }
}

export interface Converter {
    convertInput(input: string): TagValue | null;

    convertValue(value: any): TagValue | null;
}

export class IntConverter implements Converter {
    private static ZERO = new TagValue(0, '0', 0);

    public convertInput(input: string): TagValue<number> | null {
        if (input === '0') {
            return IntConverter.ZERO;
        }

        const parsed = parseInt(input, 10);

        if (parsed) {
            return new TagValue(parsed, input, parsed);
        }

        return null;
    }

    public convertValue(value: any): TagValue<number> | null {
        if (Types.isNumber(value)) {
            return new TagValue(value, `${value}`, value);
        }

        return null;
    }
}

export class FloatConverter implements Converter {
    private static ZERO = new TagValue(0, '0', 0);

    public convertInput(input: string): TagValue<number> | null {
        if (input === '0') {
            return FloatConverter.ZERO;
        }

        const parsed = parseFloat(input);

        if (parsed) {
            return new TagValue(parsed, input, parsed);
        }

        return null;
    }

    public convertValue(value: any): TagValue<number> | null {
        if (Types.isNumber(value)) {
            return new TagValue(value, `${value}`, value);
        }

        return null;
    }
}

export class StringConverter implements Converter {
    public convertInput(input: string): TagValue<string> | null {
        if (input) {
            const trimmed = input.trim();

            if (trimmed.length > 0) {
                return new TagValue(trimmed, trimmed, trimmed);
            }
        }

        return null;
    }

    public convertValue(value: any): TagValue<string> | null {
        if (Types.isString(value)) {
            const trimmed = value.trim();

            return new TagValue(trimmed, trimmed, trimmed);
        }

        return null;
    }
}

export const SQX_TAG_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => TagEditorComponent), multi: true
};

const CACHED_SIZES: { [key: string]: number } = {};

let CACHED_FONT: string;

interface State {
    hasFocus: boolean;

    suggestedItems: TagValue[];
    suggestedIndex: number;

    items: TagValue[];
}

@Component({
    selector: 'sqx-tag-editor',
    styleUrls: ['./tag-editor.component.scss'],
    templateUrl: './tag-editor.component.html',
    providers: [SQX_TAG_EDITOR_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: [
        fadeAnimation
    ]
})
export class TagEditorComponent extends StatefulControlComponent<State, any[]> implements AfterViewInit, OnInit {
    @ViewChild('form', { static: false })
    public formElement: ElementRef<HTMLElement>;

    @ViewChild('input', { static: false })
    public inputElement: ElementRef<HTMLInputElement>;

    @Input()
    public suggestedValues: TagValue[] = [];

    @Input()
    public converter: Converter = new StringConverter();

    @Input()
    public undefinedWhenEmpty = true;

    @Input()
    public acceptEnter = false;

    @Input()
    public allowDuplicates = true;

    @Input()
    public singleLine = false;

    @Input()
    public styleBlank = false;

    @Input()
    public styleGray = false;

    @Input()
    public placeholder = ', to add tag';

    @Input()
    public inputName = 'tag-editor';

    @Input()
    public set suggestions(value: string[]) {
        if (value) {
            this.suggestedValues = value.map(x => new TagValue(x, x, x));
        } else {
            this.suggestedValues = [];
        }
    }

    @Input()
    public set disabled(value: boolean) {
        this.setDisabledState(value);
    }

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
                        if (Types.isArray(this.suggestedValues) && query && query.length > 0) {
                            return this.suggestedValues.filter(s => s.lowerCaseName.indexOf(query) >= 0 && !this.snapshot.items.find(x => x.id === s.id));
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

        const items: any[] = [];

        if (this.converter && Types.isArray(obj)) {
            for (let value of obj) {
                if (Types.is(value, TagValue)) {
                    items.push(value);
                } else {
                    const converted = this.converter.convertValue(value);

                    if (converted) {
                        items.push(converted);
                    }
                }
            }
        }

        this.next(s => ({ ...s, items }));
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
        if (!CACHED_FONT ||
            !this.inputElement ||
            !this.inputElement.nativeElement) {
            return;
        }

        if (!canvas) {
            canvas = document.createElement('canvas');
        }

        if (canvas) {
            const ctx = canvas.getContext('2d');

            if (ctx) {
                ctx.font = CACHED_FONT;

                const textValue = this.inputElement.nativeElement.value;
                const textKey = `${textValue}§${this.placeholder}§${ctx.font}`;

                let width = CACHED_SIZES[textKey];

                if (!width) {
                    const widthText = ctx.measureText(textValue).width;
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

        if (key === Keys.COMMA) {
            if (this.selectValue(this.addInput.value)) {
                return false;
            }
        } else if (key === Keys.DELETE) {
            const value = <string>this.addInput.value;

            if (!value || value.length === 0) {
                this.updateItems(this.snapshot.items.slice(0, this.snapshot.items.length - 1));

                return false;
            }
        } else if (key === Keys.UP) {
            this.up();
            return false;
        } else if (key === Keys.DOWN) {
            this.down();
            return false;
        } else if (key === Keys.ENTER) {
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

    public selectValue(value: TagValue | string, noFocus?: boolean) {
        if (!noFocus) {
            this.inputElement.nativeElement.focus();
        }

        let tagValue: TagValue | null;

        if (Types.isString(value)) {
            tagValue = this.converter.convertInput(value);
        } else {
            tagValue = value;
        }

        if (tagValue) {
            if (this.allowDuplicates || !this.snapshot.items.find(x => x.id === tagValue!.id)) {
                this.updateItems([...this.snapshot.items, tagValue]);
            }

            this.resetForm();
            this.resetAutocompletion();
            return true;
        }

        return false;
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
            if (event.clipboardData) {
                event.clipboardData.setData('text/plain', this.snapshot.items.map(x => x.name).join(','));
            }

            event.preventDefault();
        }
    }

    public onPaste(event: ClipboardEvent) {
        if (event.clipboardData) {
            const value = event.clipboardData.getData('text/plain');

            if (value) {
                this.resetForm();

                const values = [...this.snapshot.items];

                for (let part of value.split(',')) {
                    const converted = this.converter.convertInput(part);

                    if (converted) {
                        values.push(converted);
                    }
                }

                this.updateItems(values);
            }

            event.preventDefault();
        }
    }

    private hasSelection() {
        const s = this.inputElement.nativeElement.selectionStart;
        const e = this.inputElement.nativeElement.selectionEnd;

        return s && e && (e - s) > 0;
    }

    private updateItems(items: TagValue[]) {
        this.next(s => ({ ...s, items }));

        if (items.length === 0 && this.undefinedWhenEmpty) {
            this.callChange(undefined);
        } else {
            this.callChange(items.map(x => x.value));
        }

        this.resetSize();
    }
}

let canvas: HTMLCanvasElement | null = null;
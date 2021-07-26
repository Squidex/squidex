/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, EventEmitter, forwardRef, Input, OnChanges, OnInit, Output, SimpleChanges, ViewChild } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { fadeAnimation, getTagValues, Keys, ModalModel, StatefulControlComponent, StringConverter, TagValue, Types } from '@app/framework/internal';
import { distinctUntilChanged, map, tap } from 'rxjs/operators';

export const SQX_TAG_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => TagEditorComponent), multi: true,
};

let CACHED_FONT: string;

interface State {
    // True, when the item has the focus.
    hasFocus: boolean;

    // The suggested item.
    suggestedItems: ReadonlyArray<TagValue>;

    // The index of the selected suggested items.
    suggestedIndex: number;

    // All available tag values.
    items: ReadonlyArray<TagValue>;
}

@Component({
    selector: 'sqx-tag-editor',
    styleUrls: ['./tag-editor.component.scss'],
    templateUrl: './tag-editor.component.html',
    providers: [
        SQX_TAG_EDITOR_CONTROL_VALUE_ACCESSOR,
    ],
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TagEditorComponent extends StatefulControlComponent<State, ReadonlyArray<any>> implements AfterViewInit, OnChanges, OnInit {
    private latestValue: any;

    @ViewChild('form', { static: false })
    public formElement: ElementRef<HTMLElement>;

    @ViewChild('input', { static: false })
    public inputElement: ElementRef<HTMLInputElement>;

    @Output()
    public blur = new EventEmitter();

    @Input()
    public converter = StringConverter.INSTANCE;

    @Input()
    public undefinedWhenEmpty?: boolean | null = true;

    @Input()
    public acceptEnter?: boolean | null;

    @Input()
    public allowDuplicates?: boolean | null = true;

    @Input()
    public dashed?: boolean | null;

    @Input()
    public separated?: boolean | null;

    @Input()
    public singleLine?: boolean | null;

    @Input()
    public readonly?: boolean | null;

    @Input()
    public styleBlank?: boolean | null;

    @Input()
    public placeholder = 'i18n:common.tagAdd';

    @Input()
    public inputName = 'tag-editor';

    @Input()
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    @Input()
    public set suggestions(value: ReadonlyArray<string | TagValue> | undefined | null) {
        this.suggestionsSorted = getTagValues(value);
    }

    public suggestionsSorted: ReadonlyArray<TagValue> = [];
    public suggestionsModal = new ModalModel();

    public addInput = new FormControl();

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {
            hasFocus: false,
            suggestedItems: [],
            suggestedIndex: 0,
            items: [],
        });
    }

    public ngAfterViewInit() {
        this.resetSize();
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['converter']) {
            this.writeValue(this.latestValue);
        }
    }

    public ngOnInit() {
        this.own(
            this.addInput.valueChanges.pipe(
                    tap(() => {
                        this.resetSize();
                    }),
                    map(query => {
                        if (Types.isString(query)) {
                            return query.trim().toLowerCase();
                        } else {
                            return '';
                        }
                    }),
                    tap(query => {
                        if (!query) {
                            this.resetAutocompletion();
                        }
                    }),
                    distinctUntilChanged(),
                    map(query => {
                        if (!query) {
                            return [];
                        } else if (Types.isArray(this.suggestionsSorted)) {
                            return this.suggestionsSorted.filter(s => s.lowerCaseName.indexOf(query) >= 0 && !this.snapshot.items.find(x => x.id === s.id));
                        } else {
                            return [];
                        }
                    }))
                .subscribe(items => {
                    this.next({
                        suggestedIndex: -1,
                        suggestedItems: items || [],
                    });
                }));
    }

    public writeValue(obj: any) {
        this.latestValue = obj;

        this.resetForm();
        this.resetSize();

        const items: any[] = [];

        if (this.converter && Types.isArray(obj)) {
            for (const value of obj) {
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

        this.next({ items });
    }

    public onDisabled(isDisabled: boolean) {
        if (isDisabled) {
            this.addInput.disable({ emitEvent: false });
        } else {
            this.addInput.enable({ emitEvent: false });
        }
    }

    public focus() {
        if (this.addInput.enabled) {
            this.next({ hasFocus: true });
        }
    }

    public markTouched() {
        this.selectValue(this.addInput.value, true);

        this.resetAutocompletion();
        this.resetFocus();

        this.callTouched();
    }

    public remove(index: number) {
        this.updateItems(this.snapshot.items.filter((_, i) => i !== index), true);
    }

    public resetSize() {
        this.calculateStyle();

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

                const widthText = ctx.measureText(textValue).width;
                const widthPlaceholder = ctx.measureText(this.placeholder).width;

                const width = Math.max(widthText, widthPlaceholder);

                this.inputElement.nativeElement.style.width = `${width + 5}px`;
            }
        }

        if (this.singleLine) {
            setTimeout(() => {
                this.formElement.nativeElement.scrollLeft = this.formElement.nativeElement.scrollWidth;
            }, 0);
        }
    }

    private calculateStyle() {
        if (CACHED_FONT ||
            !this.inputElement ||
            !this.inputElement.nativeElement) {
            return;
        }

        const style = window.getComputedStyle(this.inputElement.nativeElement);

        const fontSize = style.getPropertyValue('font-size');
        const fontFamily = style.getPropertyValue('font-family');

        if (!fontSize || !fontFamily) {
            return;
        }

        CACHED_FONT = `${fontSize} ${fontFamily}`;
    }

    public onKeyDown(event: KeyboardEvent) {
        if (Keys.isComma(event)) {
            return !this.selectValue(this.addInput.value);
        } else if (Keys.isDelete(event)) {
            const value = this.addInput.value as string;

            if (!value || value.length === 0) {
                this.updateItems(this.snapshot.items.slice(0, this.snapshot.items.length - 1), false);

                return false;
            }
        } else if (Keys.isEscape(event) && this.suggestionsModal.isOpen) {
            this.suggestionsModal.hide();
            return false;
        } else if (Keys.isUp(event)) {
            this.selectPrevIndex();
            return false;
        } else if (Keys.isDown(event)) {
            this.selectNextIndex();
            return false;
        } else if (Keys.isEnter(event)) {
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
            if (this.allowDuplicates || !this.isSelected(tagValue)) {
                this.updateItems([...this.snapshot.items, tagValue], true);
            }

            this.resetForm();
            this.resetAutocompletion();
            return true;
        }

        return false;
    }

    public toggleValue(isSelected: boolean, tagValue: TagValue) {
        if (isSelected) {
            this.updateItems([...this.snapshot.items, tagValue], true);
        } else {
            this.updateItems(this.snapshot.items.filter(x => x.id !== tagValue.id), true);
        }
    }

    public selectPrevIndex() {
        this.selectIndex(this.snapshot.suggestedIndex - 1);
    }

    public selectNextIndex() {
        this.selectIndex(this.snapshot.suggestedIndex + 1);
    }

    public selectIndex(suggestedIndex: number) {
        if (suggestedIndex < 0) {
            suggestedIndex = 0;
        }

        if (suggestedIndex >= this.snapshot.suggestedItems.length) {
            suggestedIndex = this.snapshot.suggestedItems.length - 1;
        }

        this.next({ suggestedIndex });
    }

    public resetFocus(): any {
        this.next({ hasFocus: false });
    }

    private resetAutocompletion() {
        this.next({ suggestedItems: [], suggestedIndex: -1 });
    }

    private resetForm() {
        this.addInput.reset();
    }

    public isSelected(tagValue: TagValue) {
        return this.snapshot.items.find(x => x.id === tagValue.id);
    }

    public callTouched() {
        this.blur.next(true);

        super.callTouched();
    }

    public onCut(event: ClipboardEvent) {
        if (!this.hasSelection()) {
            this.onCopy(event);

            this.updateItems([], false);
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

                for (const part of value.split(',')) {
                    const converted = this.converter.convertInput(part);

                    if (converted) {
                        values.push(converted);
                    }
                }

                this.updateItems(values, false);
            }

            event.preventDefault();
        }
    }

    private hasSelection() {
        const s = this.inputElement.nativeElement.selectionStart;
        const e = this.inputElement.nativeElement.selectionEnd;

        return s && e && (e - s) > 0;
    }

    private updateItems(items: ReadonlyArray<TagValue>, touched: boolean) {
        this.next({ items });

        if (items.length === 0 && this.undefinedWhenEmpty) {
            this.callChange(undefined);
        } else {
            this.callChange(items.map(x => x.value));
        }

        if (touched) {
            this.callTouched();
        }

        this.resetSize();
    }
}

let canvas: HTMLCanvasElement | null = null;

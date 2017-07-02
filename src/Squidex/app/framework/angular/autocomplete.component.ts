/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, forwardRef, Input, OnDestroy, OnInit } from '@angular/core';
import { ControlValueAccessor, FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';

export interface AutocompleteSource {
    find(query: string): Observable<AutocompleteItem[]>;
}

export class AutocompleteItem {
    constructor(
        public readonly title: string,
        public readonly description: string,
        public readonly image: string,
        public readonly model: any
    ) {
    }
}

const KEY_ENTER = 13;
const KEY_ESCAPE = 27;
const KEY_UP = 38;
const KEY_DOWN = 40;
const NOOP = () => { /* NOOP */ };

export const SQX_AUTOCOMPLETE_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => AutocompleteComponent), multi: true
};

@Component({
    selector: 'sqx-autocomplete',
    styleUrls: ['./autocomplete.component.scss'],
    templateUrl: './autocomplete.component.html',
    providers: [SQX_AUTOCOMPLETE_CONTROL_VALUE_ACCESSOR]
})
export class AutocompleteComponent implements ControlValueAccessor, OnDestroy, OnInit {
    private subscription: Subscription;
    private changeCallback: (value: any) => void = NOOP;
    private touchedCallback: () => void = NOOP;

    @Input()
    public source: AutocompleteSource;

    @Input()
    public inputName = 'autocompletion';

    @Input()
    public placeholder = '';

    public items: AutocompleteItem[] = [];
    public itemSelection = -1;

    public queryInput = new FormControl();

    public writeValue(value: any) {
        if (!value) {
            this.resetValue();
        } else {
            let item: AutocompleteItem | null = null;

            if (value instanceof AutocompleteItem) {
                item = value;
            } else {
                item = this.items.find(i => i.model === value);
            }

            if (item) {
                this.queryInput.setValue(value.title || '');
            }
        }

        this.reset();
    }

    public setDisabledState(isDisabled: boolean): void {
        if (isDisabled) {
            this.reset();
            this.queryInput.disable();
        } else {
            this.queryInput.enable();
        }
    }

    public registerOnChange(fn: any) {
        this.changeCallback = fn;
    }

    public registerOnTouched(fn: any) {
        this.touchedCallback = fn;
    }

    public ngOnDestroy() {
        this.subscription.unsubscribe();
    }

    public ngOnInit() {
        this.subscription =
            this.queryInput.valueChanges
                .map(query => <string>query)
                .map(query => query ? query.trim() : query)
                .distinctUntilChanged()
                .debounceTime(200)
                .do(query => {
                    if (!query) {
                        this.reset();
                    }
                })
                .filter(query => !!query && !!this.source)
                .switchMap(query => this.source.find(query)).catch(_ => Observable.of([]))
                .subscribe(items => {
                    this.reset();
                    this.items = items || [];
                });
    }

    public onKeyDown(event: KeyboardEvent) {
        switch (event.keyCode) {
            case KEY_UP:
                this.up();
                return false;
            case KEY_DOWN:
                this.down();
                return false;
            case KEY_ESCAPE:
                this.resetValue();
                this.reset();
                return false;
            case KEY_ENTER:
                if (this.items.length > 0) {
                    this.chooseItem();
                    return false;
                }
                break;
        }
    }

    public blur() {
        // this.reset();
        this.touchedCallback();
    }

    public chooseItem(selection: AutocompleteItem | null = null) {
        if (!selection) {
            selection = this.items[this.itemSelection];
        }

        if (!selection && this.items.length === 1) {
            selection = this.items[0];
        }

        if (selection) {
            try {
                this.queryInput.setValue(selection.title);
                this.changeCallback(selection);
            } finally {
                this.reset();
            }
        }
    }

    private up() {
        this.selectIndex(this.itemSelection - 1);
    }

    private down() {
        this.selectIndex(this.itemSelection + 1);
    }

    private resetValue() {
        this.queryInput.setValue('');
    }

    private selectIndex(selection: number) {
        if (selection < 0) {
            selection = 0;
        }

        if (selection >= this.items.length) {
            selection = this.items.length - 1;
        }

        this.itemSelection = selection;
    }

    private reset() {
        this.items = [];
        this.itemSelection = -1;
    }
}
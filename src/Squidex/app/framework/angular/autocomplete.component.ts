/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, ContentChild, forwardRef, Input, OnDestroy, OnInit, TemplateRef } from '@angular/core';
import { ControlValueAccessor, FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';

export interface AutocompleteSource {
    find(query: string): Observable<any[]>;
}

const KEY_ENTER = 13;
const KEY_ESCAPE = 27;
const KEY_UP = 38;
const KEY_DOWN = 40;

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
    private callChange = (v: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };

    @Input()
    public source: AutocompleteSource;

    @Input()
    public inputName = 'autocompletion';

    @Input()
    public displayProperty = '';

    @Input()
    public placeholder = '';

    @ContentChild(TemplateRef)
    public itemTemplate: TemplateRef<any>;

    public items: any[] = [];

    public selectedIndex = -1;

    public queryInput = new FormControl();

    public writeValue(value: any) {
        if (!value) {
            this.resetForm();
        } else {
            const item = this.items.find(i => i === value);

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
        this.callChange = fn;
    }

    public registerOnTouched(fn: any) {
        this.callTouched = fn;
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
                .switchMap(query => this.source.find(query)).catch(error => Observable.of([]))
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
                this.resetForm();
                this.reset();
                return false;
            case KEY_ENTER:
                if (this.items.length > 0) {
                    this.selectItem();
                    return false;
                }
                break;
        }

        return true;
    }

    public blur() {
        this.reset();
        this.callTouched();
    }

    public selectItem(selection: any | null = null) {
        if (!selection) {
            selection = this.items[this.selectedIndex];
        }

        if (!selection && this.items.length === 1) {
            selection = this.items[0];
        }

        if (selection) {
            try {
                if (this.displayProperty && this.displayProperty.length > 0) {
                    this.queryInput.setValue(selection[this.displayProperty], { emitEvent: false });
                } else {
                    this.queryInput.setValue(selection.toString(), { emitEvent: false });
                }
                this.callChange(selection);
            } finally {
                this.reset();
            }
        }
    }

    public selectIndex(selection: number) {
        if (selection < 0) {
            selection = 0;
        }

        if (selection >= this.items.length) {
            selection = this.items.length - 1;
        }

        this.selectedIndex = selection;
    }

    private up() {
        this.selectIndex(this.selectedIndex - 1);
    }

    private down() {
        this.selectIndex(this.selectedIndex + 1);
    }

    private resetForm() {
        this.queryInput.setValue('');
    }

    private reset() {
        this.items = [];
        this.selectedIndex = -1;
    }
}
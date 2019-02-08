/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ContentChild, ElementRef, forwardRef, Input, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Observable, of } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, filter, map, switchMap, tap } from 'rxjs/operators';

import { StatefulControlComponent } from '@app/framework/internal';

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

interface State {
    suggestedItems: any[];
    suggestedIndex: number;
}

@Component({
    selector: 'sqx-autocomplete',
    styleUrls: ['./autocomplete.component.scss'],
    templateUrl: './autocomplete.component.html',
    providers: [SQX_AUTOCOMPLETE_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AutocompleteComponent extends StatefulControlComponent<State, any[]> implements OnInit {
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

    @ViewChild('input')
    public inputControl: ElementRef<HTMLInputElement>;

    public queryInput = new FormControl();

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {
            suggestedItems: [],
            suggestedIndex: -1
        });
    }

    public ngOnInit() {
        this.own(
            this.queryInput.valueChanges.pipe(
                    tap(query => {
                        this.callChange(query);
                    }),
                    map(query => <string>query),
                    map(query => query ? query.trim() : query),
                    tap(query => {
                        if (!query) {
                            this.reset();
                        }
                    }),
                    debounceTime(200),
                    distinctUntilChanged(),
                    filter(query => !!query && !!this.source),
                    switchMap(query => this.source.find(query)), catchError(() => of([])))
                .subscribe(items => {
                    this.next(s => ({
                        ...s,
                        suggestedIndex: -1,
                        suggestedItems: items || []
                    }));
                }));
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
                if (this.snapshot.suggestedItems.length > 0 && this.selectItem()) {
                    return false;
                }
                break;
        }

        return true;
    }

    public writeValue(obj: any) {
        if (!obj) {
            this.resetForm();
        } else {
            if (this.displayProperty && this.displayProperty.length > 0) {
                this.queryInput.setValue(obj[this.displayProperty]);
            } else {
                this.queryInput.setValue(obj.toString());
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

    public focus() {
        this.inputControl.nativeElement.focus();
    }

    public blur() {
        this.reset();
        this.callTouched();
    }

    public selectItem(selection: any | null = null): boolean {
        if (!selection) {
            selection = this.snapshot.suggestedItems[this.snapshot.suggestedIndex];
        }

        if (!selection && this.snapshot.suggestedItems.length === 1) {
            selection = this.snapshot.suggestedItems[0];
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

            return true;
        }

        return false;
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

    private up() {
        this.selectIndex(this.snapshot.suggestedIndex - 1);
    }

    private down() {
        this.selectIndex(this.snapshot.suggestedIndex + 1);
    }

    private resetForm() {
        this.queryInput.setValue('');
    }

    private reset() {
        this.next(s => ({
            ...s,
            suggestedItems: [],
            suggestedIndex: -1
        }));
    }
}
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ContentChild, ElementRef, forwardRef, Input, OnDestroy, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { fadeAnimation, Keys, StatefulControlComponent, Types } from '@app/framework/internal';
import { Observable, of } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, finalize, map, switchMap, tap } from 'rxjs/operators';

export interface AutocompleteSource {
    find(query: string): Observable<ReadonlyArray<any>>;
}

export const SQX_AUTOCOMPLETE_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => AutocompleteComponent), multi: true
};

interface State {
    // The suggested items.
    suggestedItems: ReadonlyArray<any>;

    // The selected suggest item index.
    suggestedIndex: number;

    // True, when the searching is in progress.
    isSearching?: boolean;

    // Indicates whether the loading is in progress.
    isLoading?: boolean;
}

const NO_EMIT = { emitEvent: false };

@Component({
    selector: 'sqx-autocomplete',
    styleUrls: ['./autocomplete.component.scss'],
    templateUrl: './autocomplete.component.html',
    providers: [
        SQX_AUTOCOMPLETE_CONTROL_VALUE_ACCESSOR
    ],
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AutocompleteComponent extends StatefulControlComponent<State, ReadonlyArray<any>> implements OnInit, OnDestroy {
    private timer: any;

    @Input()
    public source: AutocompleteSource;

    @Input()
    public inputName = 'autocompletion';

    @Input()
    public inputStyle: 'underlined' | 'empty';

    @Input()
    public displayProperty: string;

    @Input()
    public placeholder = '';

    @Input()
    public icon: string;

    @Input()
    public autoFocus = false;

    @Input()
    public debounceTime = 300;

    @Input()
    public dropdownWidth = '18rem';

    @ContentChild(TemplateRef, { static: false })
    public itemTemplate: TemplateRef<any>;

    @ViewChild('input', { static: false })
    public inputControl: ElementRef<HTMLInputElement>;

    public queryInput = new FormControl();

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {
            suggestedItems: [],
            suggestedIndex: -1
        });
    }

    public ngOnDestroy() {
        clearTimeout(this.timer);
    }

    public ngOnInit() {
        this.own(
            this.queryInput.valueChanges.pipe(
                    tap(query => {
                        this.callChange(query);
                    }),
                    map(query => {
                        if (Types.isString(query)) {
                            return query.trim();
                        } else {
                            return '';
                        }
                    }),
                    debounceTime(this.debounceTime),
                    distinctUntilChanged(),
                    switchMap(query => {
                        if (!query || !this.source) {
                            return of([]);
                        } else {
                            this.setLoading(true);

                            return this.source.find(query).pipe(
                                finalize(() => {
                                    this.setLoading(false);
                                }),
                                catchError(() => of([]))
                            );
                        }
                    }))
                .subscribe(items => {
                    this.next(s => ({
                        ...s,
                        suggestedIndex: -1,
                        suggestedItems: items || [],
                        isSearching: false
                    }));
                }));
    }

    public onKeyDown(event: KeyboardEvent) {
        switch (event.keyCode) {
            case Keys.UP:
                this.up();
                return false;
            case Keys.DOWN:
                this.down();
                return false;
            case Keys.ESCAPE:
                this.resetForm();
                this.reset();
                return false;
            case Keys.ENTER:
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
                this.queryInput.setValue(obj[this.displayProperty], NO_EMIT);
            } else {
                this.queryInput.setValue(obj.toString(), NO_EMIT);
            }
        }

        this.resetState();
    }

    public setDisabledState(isDisabled: boolean): void {
        if (isDisabled) {
            this.resetState();
            this.queryInput.disable(NO_EMIT);
        } else {
            this.queryInput.enable(NO_EMIT);
        }
    }

    public reset() {
        this.resetState();

        this.queryInput.setValue('', NO_EMIT);
    }

    public focus() {
        this.resetState();

        this.inputControl.nativeElement.focus();
    }

    public blur() {
        this.resetState();

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
                    this.queryInput.setValue(selection[this.displayProperty], NO_EMIT);
                } else {
                    this.queryInput.setValue(selection.toString(), NO_EMIT);
                }

                this.callChange(selection);
                this.callTouched();
            } finally {
                this.resetState();
            }

            return true;
        }

        return false;
    }

    private setLoading(value: boolean) {
        clearTimeout(this.timer);

        if (value) {
            this.next(s => ({ ...s, isLoading: true }));
        } else {
            this.timer = setTimeout(() => {
                this.next(s => ({ ...s, isLoading: false }));
            }, 250);
        }
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
        this.queryInput.setValue('', NO_EMIT);
    }
}
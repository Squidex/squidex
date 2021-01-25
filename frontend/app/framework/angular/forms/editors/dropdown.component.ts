/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: prefer-for-of

import { AfterContentInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ContentChildren, forwardRef, Input, OnChanges, OnInit, QueryList, SimpleChanges, TemplateRef } from '@angular/core';
import { ControlValueAccessor, FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Keys, ModalModel, StatefulControlComponent, Types } from '@app/framework/internal';
import { map } from 'rxjs/operators';

export const SQX_DROPDOWN_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => DropdownComponent), multi: true
};

const NO_EMIT = { emitEvent: false };

interface State {
    // The suggested item.
    suggestedItems: ReadonlyArray<any>;

    // The selected suggested index.
    suggestedIndex: number;

    // The selected item.
    selectedItem?: number;

    // The current search query.
    query?: RegExp;
}

@Component({
    selector: 'sqx-dropdown',
    styleUrls: ['./dropdown.component.scss'],
    templateUrl: './dropdown.component.html',
    providers: [
        SQX_DROPDOWN_CONTROL_VALUE_ACCESSOR
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class DropdownComponent extends StatefulControlComponent<State, ReadonlyArray<any>> implements AfterContentInit, ControlValueAccessor, OnChanges, OnInit {
    private value: any;

    @Input()
    public items: ReadonlyArray<any> = [];

    @Input()
    public searchProperty = 'name';

    @Input()
    public valueProperty?: string;

    @Input()
    public canSearch = true;

    @Input()
    public separated = false;

    @ContentChildren(TemplateRef)
    public templates: QueryList<any>;

    public dropdown = new ModalModel();

    public templateSelection: TemplateRef<any>;
    public templateItem: TemplateRef<any>;

    public queryInput = new FormControl();

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {
            selectedItem: undefined,
            suggestedIndex: -1,
            suggestedItems: []
        });
    }

    public ngOnInit() {
        this.own(
            this.queryInput.valueChanges.pipe(
                    map((queryText: string) => {
                        if (!this.items || !queryText) {
                            return { query: undefined, items: this.items };
                        } else {
                            const query = new RegExp(queryText, 'i');

                            const items = this.items.filter(x => {
                                if (Types.isString(x)) {
                                    return query.test(x);
                                } else {
                                    return query.test(x[this.searchProperty]);
                                }
                            });

                            return { query, items };
                        }
                    }))
                .subscribe(({ query, items }) => {
                    this.next({
                        suggestedItems: items || [],
                        query
                    });
                }));
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['items']) {
            this.items = this.items || [];

            this.resetSearch();

            this.next({
                suggestedIndex: this.getSelectedIndex(this.value),
                suggestedItems: this.items || []
            });
        }
    }

    public ngAfterContentInit() {
        if (this.templates.length === 1) {
            this.templateItem = this.templates.first;
            this.templateSelection = this.templates.first;
        } else {
            this.templateItem = this.templates.first;
            this.templateSelection = this.templates.last;
        }

        if (this.templateItem) {
            this.detectChanges();
        }
    }

    public writeValue(obj: any) {
        this.value = obj;

        this.selectIndex(this.getSelectedIndex(obj), false);
    }

    public setDisabledState(isDisabled: boolean): void {
        super.setDisabledState(isDisabled);

        if (isDisabled) {
            this.queryInput.disable(NO_EMIT);
        } else {
            this.queryInput.enable(NO_EMIT);
        }
    }

    public onKeyDown(event: KeyboardEvent) {
        if (Keys.isEscape(event) && this.dropdown.isOpen) {
            this.close();
            return false;
        } else if (Keys.isUp(event)) {
            this.selectPrevIndex();
            return false;
        } else if (Keys.isDown(event)) {
            this.selectNextIndex();
            return false;
        } else if (Keys.isEnter(event)) {
            this.selectIndexAndClose(this.snapshot.suggestedIndex);
            return false;
        }

        return true;
    }

    public open() {
        if (!this.dropdown.isOpen) {
            this.resetSearch();
        }

        this.dropdown.show();

        this.callTouched();
    }

    public selectIndexAndClose(selectedIndex: number) {
        this.selectIndex(selectedIndex, true);

        this.close();
    }

    private close() {
        this.dropdown.hide();
    }

    private resetSearch() {
        this.queryInput.setValue('');
    }

    public selectPrevIndex() {
        this.selectIndex(this.snapshot.suggestedIndex - 1, true);
    }

    public selectNextIndex() {
        this.selectIndex(this.snapshot.suggestedIndex + 1, true);
    }

    public selectIndex(suggestedIndex: number, fromUserAction: boolean) {
        const items = this.snapshot.suggestedItems || [];

        if (suggestedIndex < 0) {
            suggestedIndex = 0;
        }

        if (suggestedIndex >= items.length) {
            suggestedIndex = items.length - 1;
        }

        const selectedItem = items[suggestedIndex];

        if (fromUserAction) {
            let selectedValue = selectedItem;

            if (this.valueProperty && this.valueProperty.length > 0 && selectedValue) {
                selectedValue = selectedValue[this.valueProperty];
            }

            if (this.value !== selectedValue) {
                this.value = selectedValue;

                this.callChange(selectedValue);
                this.callTouched();
            }
        }

        this.next({ suggestedIndex, selectedItem });
    }

    private getSelectedIndex(value: any) {
        if (!value) {
            return -1;
        }

        if (this.valueProperty && this.valueProperty.length > 0) {
            for (let i = 0; i < this.items.length; i++) {
                const item = this.items[i];

                if (item && item[this.valueProperty] === value) {
                    return i;
                }
            }
        } else {
            return this.items.indexOf(value);
        }

        return -1;
    }
}
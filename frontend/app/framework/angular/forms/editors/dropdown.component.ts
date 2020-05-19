/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterContentInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ContentChildren, forwardRef, Input, OnChanges, OnInit, QueryList, SimpleChanges, TemplateRef } from '@angular/core';
import { ControlValueAccessor, FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Keys, ModalModel, StatefulControlComponent, Types } from '@app/framework/internal';
import { map } from 'rxjs/operators';

export const SQX_DROPDOWN_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => DropdownComponent), multi: true
};

interface State {
    // The suggested item.
    suggestedItems: ReadonlyArray<any>;

    // The selected suggested item.
    selectedItem: any;

    // The selected suggested index.
    selectedIndex: number;

    // The current search query.
    query?: RegExp;
}

const NO_EMIT = { emitEvent: false };

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
    @Input()
    public items: ReadonlyArray<any> = [];

    @Input()
    public searchProperty = 'name';

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
            selectedIndex: -1,
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
                    this.next(s => ({
                        ...s,
                        suggestedIndex: 0,
                        suggestedItems: items || [],
                        query
                    }));
                }));
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['items']) {
            this.resetSearch();

            this.next(s => ({
                ...s,
                suggestedIndex: 0,
                suggestedItems: this.items || []
            }));
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
        this.selectIndex(this.items && obj ? this.items.indexOf(obj) : -1, false);
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
        switch (event.keyCode) {
            case Keys.UP:
                this.selectPrevIndex();
                return false;
            case Keys.DOWN:
                this.selectNextIndex();
                return false;
            case Keys.ENTER:
                this.selectIndexAndClose(this.snapshot.selectedIndex);
                return false;
            case Keys.ESCAPE:
                if (this.dropdown.isOpen) {
                    this.close();
                    return false;
                }
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
        this.selectIndex(this.snapshot.selectedIndex - 1, true);
    }

    public selectNextIndex() {
        this.selectIndex(this.snapshot.selectedIndex + 1, true);
    }

    public selectIndex(selectedIndex: number, fromUserAction: boolean) {
        if (selectedIndex < 0 && fromUserAction) {
            selectedIndex = 0;
        }

        const items = this.snapshot.suggestedItems || [];

        if (selectedIndex >= items.length && fromUserAction) {
            selectedIndex = items.length - 1;
        }

        const value = items[selectedIndex];

        if (value !== this.snapshot.selectedItem) {
            if (fromUserAction) {
                this.callChange(value);
                this.callTouched();
            }

            this.next(s => ({ ...s, selectedIndex, selectedItem: value }));
        }
    }
}
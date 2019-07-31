/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterContentInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ContentChildren, forwardRef, Input, OnChanges, OnInit, QueryList, SimpleChanges, TemplateRef } from '@angular/core';
import { ControlValueAccessor, FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { map } from 'rxjs/operators';

import {
    Keys,
    ModalModel,
    StatefulControlComponent,
    Types
} from '@app/framework/internal';

export const SQX_DROPDOWN_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => DropdownComponent), multi: true
};

interface State {
    suggestedItems: any[];
    selectedItem: any;
    selectedIndex: number;
    query?: string;
}

@Component({
    selector: 'sqx-dropdown',
    styleUrls: ['./dropdown.component.scss'],
    templateUrl: './dropdown.component.html',
    providers: [SQX_DROPDOWN_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class DropdownComponent extends StatefulControlComponent<State, any[]> implements AfterContentInit, ControlValueAccessor, OnChanges, OnInit {
    @Input()
    public items: any[] = [];

    @Input()
    public searchProperty = 'name';

    @Input()
    public canSearch = true;

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
                    map((query: string) => {
                        if (!this.items || !query) {
                            return { query, items: this.items };
                        } else {
                            query = query.trim().toLocaleLowerCase();

                            const items = this.items.filter(x => {
                                if (Types.isString(x)) {
                                    return x.toLocaleLowerCase().indexOf(query) >= 0;
                                } else {
                                    const value: string = x[this.searchProperty];

                                    return value && value.toLocaleLowerCase().indexOf(query) >= 0;
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
            this.templateItem = this.templateSelection = this.templates.first;
        } else {
            this.templates.forEach(template => {
                if (template.name === 'selection') {
                    this.templateSelection = template;
                } else {
                    this.templateItem = template;
                }
            });
        }

        if (this.templateItem) {
            this.detectChanges();
        }
    }

    public writeValue(obj: any) {
        this.selectIndex(this.items && obj ? this.items.indexOf(obj) : 0, false);
    }

    public onKeyDown(event: KeyboardEvent) {
        switch (event.keyCode) {
            case Keys.UP:
                this.up();
                return false;
            case Keys.DOWN:
                this.down();
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

    public selectIndex(selectedIndex: number, emitEvents: boolean) {
        if (selectedIndex < 0) {
            selectedIndex = 0;
        }

        const items = this.snapshot.suggestedItems || [];

        if (selectedIndex >= items.length) {
            selectedIndex = items.length - 1;
        }

        const value = items[selectedIndex];

        if (value !== this.snapshot.selectedItem) {
            if (emitEvents) {
                this.callChange(value);
                this.callTouched();
            }

            this.next(s => ({ ...s, selectedIndex, selectedItem: value }));
        }

    }

    private up() {
        this.selectIndex(this.snapshot.selectedIndex - 1, true);
    }

    private down() {
        this.selectIndex(this.snapshot.selectedIndex + 1, true);
    }
}
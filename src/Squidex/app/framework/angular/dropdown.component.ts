/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AfterContentInit, Component, ContentChildren, forwardRef, Input, QueryList, TemplateRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

const KEY_ENTER = 13;
const KEY_ESCAPE = 27;
const KEY_UP = 38;
const KEY_DOWN = 40;

import { ModalView } from './../utils/modal-view';

export const SQX_DROPDOWN_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => DropdownComponent), multi: true
};

@Component({
    selector: 'sqx-dropdown',
    styleUrls: ['./dropdown.component.scss'],
    templateUrl: './dropdown.component.html',
    providers: [SQX_DROPDOWN_CONTROL_VALUE_ACCESSOR]
})
export class DropdownComponent implements AfterContentInit, ControlValueAccessor {
    private callChange = (v: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };

    @Input()
    public items: any[] = [];

    @ContentChildren(TemplateRef)
    public templates: QueryList<any>;

    public dropdown = new ModalView();

    public selectedItem: any;
    public selectedIndex = -1;
    public selectionTemplate: TemplateRef<any>;

    public itemTemplate: TemplateRef<any>;

    public isDisabled = false;

    public ngAfterContentInit() {
        if (this.templates.length === 1) {
            this.itemTemplate = this.selectionTemplate = this.templates.first;
        } else {
            this.templates.forEach(template => {
                if (template.name === 'selection') {
                    this.selectionTemplate = template;
                } else {
                    this.itemTemplate = template;
                }
            });
        }
    }

    public writeValue(value: any) {
        this.selectIndex(this.items && value ? this.items.indexOf(value) : 0);
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;
    }

    public registerOnChange(fn: any) {
        this.callChange = fn;
    }

    public registerOnTouched(fn: any) {
        this.callTouched = fn;
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
            case KEY_ENTER:
                this.close();
                return false;
        }

        return true;
    }

    public open() {
        this.dropdown.show();
        this.callTouched();
    }

    public selectIndexAndClose(selectedIndex: number) {
        this.selectIndex(selectedIndex);
        this.close();
    }

    private close() {
        this.dropdown.hide();
    }

    private up() {
        this.selectIndex(this.selectedIndex - 1);
    }

    private down() {
        this.selectIndex(this.selectedIndex + 1);
    }

    private selectIndex(selectedIndex: number) {
        if (selectedIndex < 0) {
            selectedIndex = 0;
        }

        const items = this.items || [];

        if (selectedIndex >= items.length) {
            selectedIndex = items.length - 1;
        }

        const value = items[selectedIndex];

        if (value !== this.selectedItem) {
            this.selectedIndex = selectedIndex;
            this.selectedItem = value;

            this.callChange(value);
        }
    }
}
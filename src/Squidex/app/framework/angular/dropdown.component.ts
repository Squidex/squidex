/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, ContentChild, forwardRef, Input, TemplateRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

const KEY_ENTER = 13;
const KEY_ESCAPE = 27;
const KEY_UP = 38;
const KEY_DOWN = 40;
const NOOP = () => { /* NOOP */ };

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
export class DropdownComponent implements ControlValueAccessor {
    private changeCallback: (value: any) => void = NOOP;
    private touchedCallback: () => void = NOOP;

    @Input()
    public items: any[] = [];

    @ContentChild(TemplateRef)
    public itemTemplate: TemplateRef<any>;

    public modalView = new ModalView();

    public selectedItem: any;
    public selectedIndex = -1;

    public isDisabled = false;

    private get safeItems(): any[] {
        return this.items || [];
    }

    public writeValue(value: any) {
        this.selectIndex(this.items && value ? this.items.indexOf(value) : 0);
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;
    }

    public registerOnChange(fn: any) {
        this.changeCallback = fn;
    }

    public registerOnTouched(fn: any) {
        this.touchedCallback = fn;
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
    }

    public open() {
        this.modalView.show();
        this.touchedCallback();
    }

    public selectIndexAndClose(selectedIndex: number) {
        this.selectIndex(selectedIndex);
        this.close();
    }

    private close() {
        this.modalView.hide();
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

            this.changeCallback(value);
        }
    }
}
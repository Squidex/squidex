/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterContentInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ContentChildren, forwardRef, Input, QueryList, TemplateRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

import { Keys, ModalModel, StatefulControlComponent } from '@app/framework/internal';

export const SQX_DROPDOWN_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => DropdownComponent), multi: true
};

interface State {
    selectedItem: any;
    selectedIndex: number;
}

@Component({
    selector: 'sqx-dropdown',
    styleUrls: ['./dropdown.component.scss'],
    templateUrl: './dropdown.component.html',
    providers: [SQX_DROPDOWN_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class DropdownComponent extends StatefulControlComponent<State, any[]> implements AfterContentInit, ControlValueAccessor {
    @Input()
    public items: any[] = [];

    @ContentChildren(TemplateRef)
    public templates: QueryList<any>;

    public dropdown = new ModalModel();

    public templateSelection: TemplateRef<any>;
    public templateItem: TemplateRef<any>;

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {
            selectedItem: undefined,
            selectedIndex: -1
        });
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
        this.selectIndex(this.items && obj ? this.items.indexOf(obj) : 0);
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
            case Keys.ENTER:
                if (this.dropdown.isOpen) {
                    this.close();
                    return false;
                }
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

    public selectIndex(selectedIndex: number) {
        if (selectedIndex < 0) {
            selectedIndex = 0;
        }

        const items = this.items || [];

        if (selectedIndex >= items.length) {
            selectedIndex = items.length - 1;
        }

        const value = items[selectedIndex];

        if (value !== this.snapshot.selectedItem) {
            selectedIndex = selectedIndex;

            this.callChange(value);
            this.callTouched();

            this.next(s => ({ ...s, selectedIndex, selectedItem: value }));
        }

    }

    private up() {
        this.selectIndex(this.snapshot.selectedIndex - 1);
    }

    private down() {
        this.selectIndex(this.snapshot.selectedIndex + 1);
    }
}
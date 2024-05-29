/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { AfterContentInit, booleanAttribute, ChangeDetectionStrategy, Component, ContentChildren, EventEmitter, forwardRef, Input, OnInit, Output, QueryList, TemplateRef } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { map } from 'rxjs/operators';
import { FloatingPlacement, Keys, ModalModel, StatefulControlComponent, Subscriptions, TypedSimpleChanges, Types } from '@app/framework/internal';
import { DropdownMenuComponent } from '../../dropdown-menu.component';
import { LoaderComponent } from '../../loader.component';
import { ModalPlacementDirective } from '../../modals/modal-placement.directive';
import { ModalDirective } from '../../modals/modal.directive';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { ScrollActiveDirective } from '../../scroll-active.directive';
import { TemplateWrapperDirective } from '../../template-wrapper.directive';
import { FocusOnInitDirective } from '../focus-on-init.directive';

export const SQX_DROPDOWN_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => DropdownComponent), multi: true,
};

interface State {
    // The suggested item.
    suggestedItems: ReadonlyArray<any>;

    // The selected suggested index.
    suggestedIndex: number;

    // The selected item.
    selectedItem?: any;

    // The current search query.
    query?: RegExp;
}

@Component({
    standalone: true,
    selector: 'sqx-dropdown',
    styleUrls: ['./dropdown.component.scss'],
    templateUrl: './dropdown.component.html',
    providers: [
        SQX_DROPDOWN_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        DropdownMenuComponent,
        FocusOnInitDirective,
        FormsModule,
        LoaderComponent,
        ModalDirective,
        ModalPlacementDirective,
        ReactiveFormsModule,
        ScrollActiveDirective,
        TemplateWrapperDirective,
        TranslatePipe,
    ],
})
export class DropdownComponent extends StatefulControlComponent<State, ReadonlyArray<any>> implements AfterContentInit, OnInit {
    private readonly subscriptions = new Subscriptions();
    private value: any;

    @Output()
    public dropdownOpen = new EventEmitter();

    @Output()
    public dropdownClose = new EventEmitter();

    @Input({ transform: booleanAttribute })
    public itemsLoading?: boolean | null;

    @Input()
    public itemsEmptyText = 'i18n:common.empty';

    @Input()
    public items: ReadonlyArray<any> | undefined | null = [];

    @Input({ transform: booleanAttribute })
    public itemSeparator?: boolean | null;

    @Input()
    public searchProperty = 'name';

    @Input()
    public valueProperty?: string;

    @Input({ transform: booleanAttribute })
    public canSearch?: boolean | null = true;

    @Input()
    public dropdownPosition: FloatingPlacement = 'bottom-start';

    @Input({ transform: booleanAttribute })
    public dropdownFullWidth = false;

    @Input()
    public dropdownStyles: any = {};

    @Input({ transform: booleanAttribute })
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    @ContentChildren(TemplateRef)
    public templates!: QueryList<any>;

    public dropdown = new ModalModel();

    public templateSelection!: TemplateRef<any>;
    public templateItem!: TemplateRef<any>;

    public queryInput = new UntypedFormControl();

    constructor() {
        super({
            selectedItem: undefined,
            suggestedIndex: -1,
            suggestedItems: [],
        });
    }

    public ngOnInit() {
        this.subscriptions.add(
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
                        query,
                    });
                }));
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.items) {
            this.items = this.items || [];

            this.next({ suggestedItems: this.items });

            this.selectSearch('');
            this.selectIndex(this.getSelectedIndex(this.value), false);
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

    public onDisabled(isDisabled: boolean) {
        if (isDisabled) {
            this.queryInput.disable({ emitEvent: false });
        } else {
            this.queryInput.enable({ emitEvent: false });
        }
    }

    public onKeyDown(event: KeyboardEvent) {
        if (Keys.isEscape(event) && this.dropdown.isOpen) {
            this.closeModal();
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

    public openModal() {
        if (!this.dropdown.isOpen) {
            this.selectSearch('');
        }

        if (!this.dropdown.isOpen) {
            this.dropdownOpen.emit();

            this.dropdown.show();
        }

        this.callTouched();
    }

    public selectIndexAndClose(selectedIndex: number) {
        this.selectIndex(selectedIndex, true);

        this.closeModal();
    }

    public closeModal() {
        if (this.dropdown.isOpen) {
            this.dropdownClose.emit();

            this.dropdown.hide();
        }
    }

    private selectSearch(value: string) {
        this.queryInput.setValue(value);
    }

    public selectPrevIndex() {
        this.selectIndex(this.snapshot.suggestedIndex - 1, true);
    }

    public selectNextIndex() {
        this.selectIndex(this.snapshot.suggestedIndex + 1, true);
    }

    public selectIndex(suggestedIndex: number, fromUserAction: boolean) {
        const items = this.snapshot.suggestedItems || [];

        const selectedItem = items[suggestedIndex];

        if (suggestedIndex < 0) {
            suggestedIndex = 0;
        }

        if (suggestedIndex >= items.length) {
            suggestedIndex = items.length - 1;
        }

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
        if (!value || !this.items) {
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

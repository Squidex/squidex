/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, Component, computed, contentChildren, effect, input, model, output, signal, TemplateRef, untracked } from '@angular/core';
import { FloatingPlacement, Keys, ModalModel, StringHelper, Types } from '@app/framework/internal';
import { DropdownMenuComponent } from '../../dropdown-menu.component';
import { LoaderComponent } from '../../loader.component';
import { ModalPlacementDirective } from '../../modals/modal-placement.directive';
import { ModalDirective } from '../../modals/modal.directive';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { ScrollActiveDirective } from '../../scroll-active.directive';
import { TemplateWrapperDirective } from '../../template-wrapper.directive';
import { FocusOnInitDirective } from '../focus-on-init.directive';
import { FormValueControlBase, syncExternalValue } from '../form-value-control';

@Component({
    selector: 'sqx-dropdown',
    styleUrls: ['./dropdown.component.scss'],
    templateUrl: './dropdown.component.html',
    imports: [
        DropdownMenuComponent,
        FocusOnInitDirective,
        LoaderComponent,
        ModalDirective,
        ModalPlacementDirective,
        ScrollActiveDirective,
        TemplateWrapperDirective,
        TranslatePipe,
    ],
})
export class DropdownComponent extends FormValueControlBase<any> {
    private readonly valueSetter = syncExternalValue(() => this.value, value => {
        this.selectIndex(this.getSelectedIndex(value), false);
    });

    /** The value of the control. */
    public readonly value = model<any>(null);

    /** Emits when the dropdown is opened. */
    public readonly dropdownOpen = output();

    /** Emits when the dropdown is closed. */
    public readonly dropdownClose = output();

    /** Shows a loading indicator instead of the empty text when there are no items. */
    public readonly itemsLoading = input(false, { transform: booleanAttribute });

    /** The text to show when there are no items. */
    public readonly itemsEmptyText = input('i18n:common.empty');

    /** The items to choose from. */
    public readonly items = input<ReadonlyArray<any> | undefined | null>([]);

    /** Renders a separator between the items. */
    public readonly itemSeparator = input(false, { transform: booleanAttribute });

    /** The ID of the inner input. */
    public readonly formId = input<string>();

    /** The name of the inner input. */
    public readonly formName = input<string>();

    /** The property of complex items to search in. */
    public readonly searchProperty = input('name');

    /** The property of the selected item to use as value. */
    public readonly valueProperty = input<string>();

    /** Shows a search input in the dropdown. */
    public readonly canSearch = input(true, { transform: booleanAttribute });

    /** The position of the dropdown. */
    public readonly dropdownPosition = input<FloatingPlacement>('bottom-start');

    /** Makes the dropdown as wide as the input. */
    public readonly dropdownFullWidth = input(false, { transform: booleanAttribute });

    /** Additional styles for the dropdown. */
    public readonly dropdownStyles = input<any>({});

    protected readonly templates = contentChildren(TemplateRef);

    protected readonly suggestedItems = signal<ReadonlyArray<any>>([]);

    protected readonly suggestedIndex = signal(-1);

    protected readonly selectedItem = signal<any>(undefined);

    protected readonly query = signal<RegExp | undefined>(undefined);

    protected readonly queryText = signal('');

    protected readonly templateItem = computed(() => {
        return this.templates()[0];
    });

    protected readonly templateSelection = computed(() => {
        const templates = this.templates();

        return templates[templates.length - 1];
    });

    protected readonly dropdown = new ModalModel();

    constructor() {
        super();

        effect(() => {
            const items = this.items() || [];

            untracked(() => {
                this.suggestedItems.set(items);

                this.selectSearch('');
                this.selectIndex(this.getSelectedIndex(this.value()), false);
            });
        });
    }

    protected onKeyDown(event: KeyboardEvent) {
        if (Keys.isEscape(event) && this.dropdown.isOpen()) {
            this.closeModal();
            return false;
        } else if (Keys.isUp(event)) {
            this.selectPrevIndex();
            return false;
        } else if (Keys.isDown(event)) {
            this.selectNextIndex();
            return false;
        } else if (Keys.isEnter(event)) {
            this.selectIndexAndClose(this.suggestedIndex());
            return false;
        }

        return true;
    }

    protected openModal() {
        if (!this.dropdown.isOpen()) {
            this.selectSearch('');
        }

        if (!this.dropdown.isOpen()) {
            this.dropdownOpen.emit();

            this.dropdown.show();
        }

        this.markTouched();
    }

    protected onSearch(queryText: string) {
        this.selectSearch(queryText);
    }

    protected selectIndexAndClose(selectedIndex: number) {
        this.selectIndex(selectedIndex, true);

        this.closeModal();
    }

    private closeModal() {
        if (this.dropdown.isOpen()) {
            this.dropdownClose.emit();

            this.dropdown.hide();
        }
    }

    private selectPrevIndex() {
        this.selectIndex(this.suggestedIndex() - 1, true);
    }

    private selectNextIndex() {
        this.selectIndex(this.suggestedIndex() + 1, true);
    }

    private selectIndex(suggestedIndex: number, fromUserAction: boolean) {
        const items = this.suggestedItems();

        const selectedItem = items[suggestedIndex];

        if (suggestedIndex < 0) {
            suggestedIndex = 0;
        }

        if (suggestedIndex >= items.length) {
            suggestedIndex = items.length - 1;
        }

        if (fromUserAction) {
            this.selectValue(selectedItem);
        }

        this.suggestedIndex.set(suggestedIndex);
        this.selectedItem.set(selectedItem);
    }

    private selectValue(selectedItem: any) {
        const valueProperty = this.valueProperty();

        let selectedValue = selectedItem;
        if (valueProperty && valueProperty.length > 0 && selectedValue) {
            selectedValue = selectedValue[valueProperty];
        }

        selectedValue ??= null;

        if (this.value() === selectedValue) {
            return;
        }

        this.valueSetter(selectedValue);
        this.markTouched();
    }

    private selectSearch(queryText: string) {
        this.queryText.set(queryText);
        this.search(queryText);
    }

    private search(queryText: string) {
        const items = this.items();
        if (!items || !queryText) {
            this.query.set(undefined);
            this.suggestedItems.set(items || []);
            return;
        }

        const query = StringHelper.searchRegex(queryText);
        const searchProperty = this.searchProperty();

        this.query.set(query);
        this.suggestedItems.set(items.filter(x => {
            if (Types.isString(x)) {
                return query.test(x);
            } else {
                return query.test(x[searchProperty]);
            }
        }));
    }

    private getSelectedIndex(value: any) {
        const items = this.items();
        if (!value || !items) {
            return -1;
        }

        const valueProperty = this.valueProperty();
        if (!valueProperty || valueProperty.length === 0) {
            return items.indexOf(value);
        }

        for (let i = 0; i < items.length; i++) {
            const item = items[i];

            if (item && item[valueProperty] === value) {
                return i;
            }
        }

        return -1;
    }
}

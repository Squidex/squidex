/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { afterNextRender, booleanAttribute, ChangeDetectionStrategy, Component, computed, effect, ElementRef, inject, Injector, input, model, output, signal, untracked, viewChild } from '@angular/core';
import { FormValueControl } from '@angular/forms/signals';
import { getTagValues, Keys, ModalModel, StringConverter, TagConverter, TagValue, TextMeasurer, Types } from '@app/framework/internal';
import { DropdownMenuComponent } from '../../dropdown-menu.component';
import { LoaderComponent } from '../../loader.component';
import { ModalPlacementDirective } from '../../modals/modal-placement.directive';
import { ModalDirective } from '../../modals/modal.directive';
import { TooltipDirective } from '../../modals/tooltip.directive';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { ScrollActiveDirective } from '../../scroll-active.directive';
import { StopClickDirective } from '../../stop-click.directive';

function normalizeQuery(query: unknown) {
    return Types.isString(query) ? query.trim().toLowerCase() : '';
}

function convertTags(obj: any, converter: TagConverter) {
    const tags: TagValue[] = [];

    if (!converter || !Types.isArray(obj)) {
        return tags;
    }

    for (const value of obj) {
        if (Types.is(value, TagValue)) {
            tags.push(value);
        } else {
            const converted = converter.convertValue(value);

            if (converted) {
                tags.push(converted);
            }
        }
    }

    return tags;
}

@Component({
    selector: 'sqx-tag-editor',
    styleUrls: ['./tag-editor.component.scss'],
    templateUrl: './tag-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        DropdownMenuComponent,
        LoaderComponent,
        ModalDirective,
        ModalPlacementDirective,
        ScrollActiveDirective,
        StopClickDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class TagEditorComponent implements FormValueControl<any> {
    private readonly formElement = viewChild.required<ElementRef<HTMLElement>>('form');
    private readonly inputElement = viewChild<ElementRef<HTMLInputElement>>('input');
    private readonly textMeasurer = new TextMeasurer(() => this.inputElement());
    private readonly injector = inject(Injector);
    private lastValue: any;
    private latestInput?: string;

    /** The value of the control. */
    public readonly value = model<any>(null);

    /** Disables the control. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** Indicates whether the value is invalid, provided by the form. */
    public readonly invalid = input(false, { transform: booleanAttribute });

    /** Indicates whether the control has been touched. */
    public readonly touched = model(false);

    /** Emits when the editor loses the focus. */
    public readonly editorBlur = output<boolean>();

    /** Emits when the dropdown with the suggestions is opened. */
    public readonly dropdownOpen = output();

    /** Emits when the dropdown with the suggestions is closed. */
    public readonly dropdownClose = output();

    /** The ID of the inner input. */
    public readonly id = input('');

    /** Converts between the values and the tags. */
    public readonly itemConverter = input<TagConverter>(StringConverter.INSTANCE);

    /** Emits null instead of an empty array when all tags are removed. */
    public readonly nullWhenEmpty = input(true, { transform: booleanAttribute });

    /** Adds the typed text as tag when enter is pressed. */
    public readonly acceptEnter = input(false, { transform: booleanAttribute });

    /** Shows a button to open a dropdown with all suggestions. */
    public readonly allowOpen = input(false, { transform: booleanAttribute });

    /** Allows to add the same tag multiple times. */
    public readonly allowDuplicates = input(true, { transform: booleanAttribute });

    /** Renders the editor in read-only style and hides the dropdown button. */
    public readonly readonly = input(false, { transform: booleanAttribute });

    /** Renders a dashed border when there are no tags. */
    public readonly styleDashed = input(false, { transform: booleanAttribute });

    /** Renders the editor without a border. */
    public readonly styleBlank = input(false, { transform: booleanAttribute });

    /** Renders the tags in a single scrollable line. */
    public readonly styleScrollable = input(false, { transform: booleanAttribute });

    /** The placeholder of the input. */
    public readonly placeholder = input('i18n:common.tagAdd');

    /** The minimum width of the suggestions dropdown. */
    public readonly dropdownWidth = input('18rem');

    /** Renders a separator between the suggestions. */
    public readonly itemSeparator = input(false, { transform: booleanAttribute });

    /** Shows a loading indicator in the dropdown when there are no suggestions. */
    public readonly itemsSourceLoading = input(false, { transform: booleanAttribute });

    /** The text to show in the dropdown when there are no suggestions. */
    public readonly itemsSourceEmptyText = input('i18n:common.empty');

    /** The suggested tags. */
    public readonly itemsSource = input<ReadonlyArray<string | TagValue> | undefined | null>();

    protected readonly itemsSorted = computed(() => {
        return getTagValues(this.itemsSource());
    });

    protected readonly itemsModal = new ModalModel();
    protected readonly hasFocus = signal(false);
    protected readonly suggestedItems = signal<ReadonlyArray<TagValue>>([]);
    protected readonly suggestedIndex = signal(0);
    protected readonly tags = signal<ReadonlyArray<TagValue>>([]);
    protected readonly addText = signal('');

    constructor() {
        afterNextRender(() => {
            this.resetSize();
        });

        effect(() => {
            const value = this.value();

            untracked(() => {
                // Values that have been set by this component are already shown as tags.
                if (value === this.lastValue) {
                    return;
                }

                this.lastValue = value;
                this.resetForm();
                this.resetSize();
                this.tags.set(convertTags(value, this.itemConverter()));
            });
        });

        effect(() => {
            const converter = this.itemConverter();

            untracked(() => {
                this.tags.set(convertTags(this.value(), converter));
            });
        });

        effect(() => {
            const itemsSorted = this.itemsSorted();

            untracked(() => {
                const query = this.addText();
                if (!query) {
                    return;
                }

                this.suggestedIndex.set(-1);
                this.suggestedItems.set(this.findSuggestions(itemsSorted, query));
            });
        });
    }

    public focus() {
        this.inputElement()?.nativeElement.focus();
    }

    protected onFocus() {
        if (!this.disabled()) {
            this.hasFocus.set(true);
        }
    }

    protected markTouched() {
        this.selectValue(this.addText(), true);

        this.resetAutocompletion();
        this.resetFocus();

        this.setTouched();
    }

    protected remove(index: number) {
        this.updateItems(this.tags().filter((_, i) => i !== index), true);
    }

    protected setText(text: string) {
        this.addText.set(text);
        this.resetSize();

        const query = normalizeQuery(text);
        const queryPrevious = this.latestInput;

        if (!query) {
            this.resetAutocompletion();
        } else if (!queryPrevious) {
            this.dropdownOpen.emit();
        }

        this.latestInput = query;

        if (query === queryPrevious) {
            return;
        }

        this.suggestedIndex.set(-1);
        this.suggestedItems.set(query ? this.findSuggestions(this.itemsSorted(), query) : []);
    }

    private resetSize() {
        const input = this.inputElement()?.nativeElement;
        if (!input) {
            return;
        }

        const widthTextValue = this.textMeasurer.getTextSize(this.addText());
        const widthPlaceholder = this.textMeasurer.getTextSize(this.placeholder());

        const width = Math.max(widthTextValue, widthPlaceholder);
        if (width < 0) {
            return;
        }

        input.style.width = `${width + 5}px`;

        if (this.styleScrollable()) {
            afterNextRender(() => {
                const form = this.formElement().nativeElement;

                form.scrollLeft = form.scrollWidth;
            }, { injector: this.injector });
        }
    }

    protected onKeyDown(event: KeyboardEvent) {
        if (Keys.isComma(event)) {
            return !this.selectValue(this.addText());
        } else if (Keys.isDelete(event)) {
            const value = this.addText();

            if (!value || value.length === 0) {
                this.updateItems(this.tags().slice(0, this.tags().length - 1), false);

                return false;
            }
        } else if (Keys.isEscape(event) && this.itemsModal.isOpen()) {
            this.closeModal();
            return false;
        } else if (Keys.isUp(event)) {
            this.selectIndex(this.suggestedIndex() - 1);
            return false;
        } else if (Keys.isDown(event)) {
            this.selectIndex(this.suggestedIndex() + 1);
            return false;
        } else if (Keys.isEnter(event)) {
            return this.onEnter();
        }

        return true;
    }

    private onEnter() {
        const suggestedIndex = this.suggestedIndex();
        if (suggestedIndex >= 0) {
            return !this.selectValue(this.suggestedItems()[suggestedIndex]);
        }

        if (this.acceptEnter()) {
            return !this.selectValue(this.addText());
        }

        return true;
    }

    protected selectValue(value: TagValue | string, noFocus?: boolean) {
        if (!noFocus) {
            this.inputElement()!.nativeElement.focus();
        }

        let tagValue: TagValue | null;
        if (Types.isString(value)) {
            tagValue = this.itemConverter().convertInput(value);
        } else {
            tagValue = value;
        }

        if (!tagValue) {
            return false;
        }

        if (this.allowDuplicates() || !this.isSelected(tagValue)) {
            this.updateItems([...this.tags(), tagValue], true);
        }

        this.resetForm();
        this.resetAutocompletion();
        return true;
    }

    protected toggleValue(isSelected: boolean, tagValue: TagValue) {
        if (isSelected) {
            this.updateItems([...this.tags(), tagValue], true);
        } else {
            this.updateItems(this.tags().filter(x => x.id !== tagValue.id), true);
        }
    }

    protected selectIndex(suggestedIndex: number) {
        const suggestedItems = this.suggestedItems();

        if (suggestedIndex < 0) {
            suggestedIndex = 0;
        }

        if (suggestedIndex >= suggestedItems.length) {
            suggestedIndex = suggestedItems.length - 1;
        }

        this.suggestedIndex.set(suggestedIndex);
    }

    private resetFocus() {
        this.hasFocus.set(false);
    }

    protected isSelected(tagValue: TagValue) {
        return this.tags().find(x => x.id === tagValue.id);
    }

    private closeModal() {
        if (this.itemsModal.isOpen()) {
            this.dropdownClose.emit();

            this.itemsModal.hide();
        }
    }

    protected openModal() {
        if (!this.itemsModal.isOpen()) {
            this.dropdownOpen.emit();

            this.itemsModal.show();
        }
    }

    private setTouched() {
        this.editorBlur.emit(true);

        this.touched.set(true);
    }

    protected focusInput(event: Event) {
        this.inputElement()!.nativeElement.focus();

        event?.preventDefault();
    }

    protected onCut(event: ClipboardEvent) {
        if (!this.hasSelection()) {
            this.onCopy(event);

            this.updateItems([], false);
        }
    }

    protected onCopy(event: ClipboardEvent) {
        if (!this.hasSelection()) {
            if (event.clipboardData) {
                event.clipboardData.setData('text/plain', this.tags().map(x => x.name).join(','));
            }

            event.preventDefault();
        }
    }

    protected onPaste(event: ClipboardEvent) {
        if (!event.clipboardData) {
            return;
        }

        const value = event.clipboardData.getData('text/plain');
        if (value) {
            this.resetForm();

            const values = [...this.tags()];

            for (const part of value.split(',')) {
                const converted = this.itemConverter().convertInput(part);

                if (converted) {
                    values.push(converted);
                }
            }

            this.updateItems(values, false);
        }

        event.preventDefault();
    }

    private findSuggestions(itemsSorted: ReadonlyArray<TagValue>, query: string) {
        if (!Types.isArray(itemsSorted)) {
            return [];
        }

        const tags = this.tags();

        return itemsSorted.filter(s => s.lowerCaseName.includes(query) && !tags.find(x => x.id === s.id));
    }

    private resetAutocompletion() {
        this.suggestedItems.set([]);
        this.suggestedIndex.set(-1);
    }

    private resetForm() {
        this.setText('');
    }

    private hasSelection() {
        const s = this.inputElement()!.nativeElement.selectionStart;
        const e = this.inputElement()!.nativeElement.selectionEnd;

        return s && e && (e - s) > 0;
    }

    private updateItems(tags: ReadonlyArray<TagValue>, touched: boolean) {
        this.tags.set(tags);

        let value: ReadonlyArray<any> | null = tags.map(x => x.value);
        if (tags.length === 0 && this.nullWhenEmpty()) {
            value = null;
        }

        this.lastValue = value;
        this.value.set(value);

        if (touched) {
            this.setTouched();
        }

        this.resetSize();
    }
}

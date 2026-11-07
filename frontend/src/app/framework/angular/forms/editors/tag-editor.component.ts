/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { afterNextRender, booleanAttribute, ChangeDetectionStrategy, Component, computed, effect, ElementRef, forwardRef, input, output, signal, untracked, viewChild } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { distinctUntilChanged, map, tap } from 'rxjs/operators';
import { getTagValues, Keys, ModalModel, SignalControlComponent, StringConverter, TagConverter, TagValue, TextMeasurer, Types } from '@app/framework/internal';
import { DropdownMenuComponent } from '../../dropdown-menu.component';
import { LoaderComponent } from '../../loader.component';
import { ModalPlacementDirective } from '../../modals/modal-placement.directive';
import { ModalDirective } from '../../modals/modal.directive';
import { TooltipDirective } from '../../modals/tooltip.directive';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { ScrollActiveDirective } from '../../scroll-active.directive';
import { StopClickDirective } from '../../stop-click.directive';
import { ForwardFormClassesDirective } from '../forward-form-classes.directive';

export const SQX_TAG_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => TagEditorComponent), multi: true,
};

const NO_EMIT = { emitEvent: false };

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
    providers: [
        SQX_TAG_EDITOR_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        DropdownMenuComponent,
        FormsModule,
        LoaderComponent,
        ForwardFormClassesDirective,
        ModalDirective,
        ModalPlacementDirective,
        ReactiveFormsModule,
        ScrollActiveDirective,
        StopClickDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class TagEditorComponent extends SignalControlComponent<ReadonlyArray<any>> {
    private readonly formElement = viewChild.required<ElementRef<HTMLElement>>('form');
    private readonly inputElement = viewChild<ElementRef<HTMLInputElement>>('input');
    private readonly textMeasurer = new TextMeasurer(() => this.inputElement());
    private latestValue: any;
    private latestInput?: string;

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

    /** Emits undefined instead of an empty array when all tags are removed. */
    public readonly undefinedWhenEmpty = input(true, { transform: booleanAttribute });

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

    protected readonly addInput = new UntypedFormControl();

    constructor() {
        super();

        afterNextRender(() => {
            this.resetSize();
        });

        effect(() => {
            if (this.isDisabled()) {
                this.addInput.disable(NO_EMIT);
            } else {
                this.addInput.enable(NO_EMIT);
            }
        });

        effect(() => {
            const converter = this.itemConverter();

            untracked(() => {
                this.tags.set(convertTags(this.latestValue, converter));
            });
        });

        effect(() => {
            const itemsSorted = this.itemsSorted();

            untracked(() => {
                const query = this.addInput.value;
                if (!query) {
                    return;
                }

                this.suggestedIndex.set(-1);
                this.suggestedItems.set(this.findSuggestions(itemsSorted, query));
            });
        });

        effect(onCleanup => {
            const subscription =
                this.addInput.valueChanges.pipe(
                    tap(() => {
                        this.resetSize();
                    }),
                    map(normalizeQuery),
                    tap(query => {
                        if (!query) {
                            this.resetAutocompletion();
                        } else if (!this.latestInput) {
                            this.dropdownOpen.emit();
                        }

                        this.latestInput = query;
                    }),
                    distinctUntilChanged(),
                    map(query => {
                        if (!query) {
                            return [];
                        }

                        return this.findSuggestions(this.itemsSorted(), query);
                    }))
                .subscribe(suggestedItems => {
                    this.suggestedIndex.set(-1);
                    this.suggestedItems.set(suggestedItems);
                });

            onCleanup(() => subscription.unsubscribe());
        });
    }

    public writeValue(obj: any) {
        this.latestValue = obj;

        this.resetForm();
        this.resetSize();

        this.tags.set(convertTags(obj, this.itemConverter()));
    }

    protected focus() {
        if (this.addInput.enabled) {
            this.hasFocus.set(true);
        }
    }

    protected markTouched() {
        this.selectValue(this.addInput.value, true);

        this.resetAutocompletion();
        this.resetFocus();

        this.callTouched();
    }

    protected remove(index: number) {
        this.updateItems(this.tags().filter((_, i) => i !== index), true);
    }

    private resetSize() {
        const input = this.inputElement()?.nativeElement;
        if (!input) {
            return;
        }

        const widthTextValue = this.textMeasurer.getTextSize(input.value);
        const widthPlaceholder = this.textMeasurer.getTextSize(this.placeholder());

        const width = Math.max(widthTextValue, widthPlaceholder);
        if (width < 0) {
            return;
        }

        input.style.width = `${width + 5}px`;

        if (this.styleScrollable()) {
            setTimeout(() => {
                const form = this.formElement().nativeElement;

                form.scrollLeft = form.scrollWidth;
            }, 0);
        }
    }

    protected onKeyDown(event: KeyboardEvent) {
        if (Keys.isComma(event)) {
            return !this.selectValue(this.addInput.value);
        } else if (Keys.isDelete(event)) {
            const value = this.addInput.value as string;

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
            return !this.selectValue(this.addInput.value);
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

    public callTouched() {
        this.editorBlur.emit(true);

        super.callTouched();
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
        this.addInput.reset();
    }

    private hasSelection() {
        const s = this.inputElement()!.nativeElement.selectionStart;
        const e = this.inputElement()!.nativeElement.selectionEnd;

        return s && e && (e - s) > 0;
    }

    private updateItems(tags: ReadonlyArray<TagValue>, touched: boolean) {
        this.tags.set(tags);

        if (tags.length === 0 && this.undefinedWhenEmpty()) {
            this.callChange(undefined);
        } else {
            this.callChange(tags.map(x => x.value));
        }

        if (touched) {
            this.callTouched();
        }

        this.resetSize();
    }
}

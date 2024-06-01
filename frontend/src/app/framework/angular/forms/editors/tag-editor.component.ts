/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { AfterViewInit, booleanAttribute, ChangeDetectionStrategy, Component, ElementRef, EventEmitter, forwardRef, Input, OnInit, Output, ViewChild } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { distinctUntilChanged, map, tap } from 'rxjs/operators';
import { getTagValues, Keys, ModalModel, StatefulControlComponent, StringConverter, Subscriptions, TagValue, TextMeasurer, TypedSimpleChanges, Types } from '@app/framework/internal';
import { DropdownMenuComponent } from '../../dropdown-menu.component';
import { LoaderComponent } from '../../loader.component';
import { ModalPlacementDirective } from '../../modals/modal-placement.directive';
import { ModalDirective } from '../../modals/modal.directive';
import { TooltipDirective } from '../../modals/tooltip.directive';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { ScrollActiveDirective } from '../../scroll-active.directive';
import { StopClickDirective } from '../../stop-click.directive';

export const SQX_TAG_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => TagEditorComponent), multi: true,
};

interface State {
    // True, when the item has the focus.
    hasFocus: boolean;

    // The suggested items.
    suggestedItems: ReadonlyArray<any>;

    // The selected suggest item index.
    suggestedIndex: number;

    // All available tag values.
    tags: ReadonlyArray<TagValue>;
}

@Component({
    standalone: true,
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
        ModalDirective,
        ModalPlacementDirective,
        ReactiveFormsModule,
        ScrollActiveDirective,
        StopClickDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class TagEditorComponent extends StatefulControlComponent<State, ReadonlyArray<any>> implements AfterViewInit, OnInit {
    private readonly subscriptions = new Subscriptions();
    private readonly textMeasurer: TextMeasurer;
    private latestValue: any;
    private latestInput?: string;

    @ViewChild('form', { static: false })
    public formElement!: ElementRef<HTMLElement>;

    @ViewChild('input', { static: false })
    public inputElement!: ElementRef<HTMLInputElement>;

    @Output()
    public dropdownOpen = new EventEmitter();

    @Output()
    public dropdownClose = new EventEmitter();

    @Output()
    public editorBlur = new EventEmitter();

    @Input()
    public itemConverter = StringConverter.INSTANCE;

    @Input({ transform: booleanAttribute })
    public undefinedWhenEmpty?: boolean | null = true;

    @Input({ transform: booleanAttribute })
    public acceptEnter?: boolean | null;

    @Input({ transform: booleanAttribute })
    public allowOpen?: boolean | null = false;

    @Input({ transform: booleanAttribute })
    public allowDuplicates?: boolean | null = true;

    @Input({ transform: booleanAttribute })
    public readonly?: boolean | null;

    @Input({ transform: booleanAttribute })
    public styleDashed?: boolean | null;

    @Input({ transform: booleanAttribute })
    public styleBlank?: boolean | null;

    @Input({ transform: booleanAttribute })
    public styleScrollable?: boolean | null;

    @Input()
    public placeholder = 'i18n:common.tagAdd';

    @Input()
    public dropdownWidth = '18rem';

    @Input({ transform: booleanAttribute })
    public itemSeparator?: boolean | null;

    @Input({ transform: booleanAttribute })
    public itemsSourceLoading?: boolean | null;

    @Input()
    public itemsSourceEmptyText = 'i18n:common.empty';

    @Input({ transform: booleanAttribute })
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    @Input()
    public set itemsSource(value: ReadonlyArray<string | TagValue> | undefined | null) {
        this.itemsSorted = getTagValues(value);

        if (this.addInput.value) {
            const query = this.addInput.value;

            const items = this.itemsSorted.filter(s => s.lowerCaseName.includes(query) && !this.snapshot.tags.find(x => x.id === s.id));

            this.next({
                suggestedIndex: -1,
                suggestedItems: items || [],
            });
        }
    }

    public itemsSorted: ReadonlyArray<TagValue> = [];
    public itemsModal = new ModalModel();

    public addInput = new UntypedFormControl();

    constructor() {
        super({
            hasFocus: false,
            suggestedIndex: 0,
            suggestedItems: [],
            tags: [],
        });

        this.textMeasurer = new TextMeasurer(() => this.inputElement);
    }

    public ngAfterViewInit() {
        this.resetSize();
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.itemConverter) {
            this.writeValue(this.latestValue, true);
        }
    }

    public ngOnInit() {
        this.subscriptions.add(
            this.addInput.valueChanges.pipe(
                    tap(() => {
                        this.resetSize();
                    }),
                    map(query => {
                        if (Types.isString(query)) {
                            return query.trim().toLowerCase();
                        } else {
                            return '';
                        }
                    }),
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
                        } else if (Types.isArray(this.itemsSorted)) {
                            return this.itemsSorted.filter(s => s.lowerCaseName.includes(query) && !this.snapshot.tags.find(x => x.id === s.id));
                        } else {
                            return [];
                        }
                    }))
                .subscribe(suggestedItems => {
                    this.next({
                        suggestedIndex: -1,
                        suggestedItems,
                    });
                }));
    }

    public writeValue(obj: any, noForm = false) {
        this.latestValue = obj;

        if (!noForm) {
            this.resetForm();
            this.resetSize();
        }

        const tags: any[] = [];

        if (this.itemConverter && Types.isArray(obj)) {
            for (const value of obj) {
                if (Types.is(value, TagValue)) {
                    tags.push(value);
                } else {
                    const converted = this.itemConverter.convertValue(value);

                    if (converted) {
                        tags.push(converted);
                    }
                }
            }
        }

        this.next({ tags });
    }

    public onDisabled(isDisabled: boolean) {
        if (isDisabled) {
            this.addInput.disable({ emitEvent: false });
        } else {
            this.addInput.enable({ emitEvent: false });
        }
    }

    public focus() {
        if (this.addInput.enabled) {
            this.next({ hasFocus: true });
        }
    }

    public markTouched() {
        this.selectValue(this.addInput.value, true);

        this.resetAutocompletion();
        this.resetFocus();

        this.callTouched();
    }

    public remove(index: number) {
        this.updateItems(this.snapshot.tags.filter((_, i) => i !== index), true);
    }

    public resetSize() {
        if (!this.inputElement?.nativeElement) {
            return;
        }

        const textValue = this.inputElement.nativeElement.value;

        const widthTextValue = this.textMeasurer.getTextSize(textValue);
        const widthPlaceholder = this.textMeasurer.getTextSize(this.placeholder);

        const width = Math.max(widthTextValue, widthPlaceholder);

        if (width < 0) {
            return;
        }

        this.inputElement.nativeElement.style.width = `${width + 5}px`;

        if (this.styleScrollable) {
            setTimeout(() => {
                this.formElement.nativeElement.scrollLeft = this.formElement.nativeElement.scrollWidth;
            }, 0);
        }
    }

    public onKeyDown(event: KeyboardEvent) {
        if (Keys.isComma(event)) {
            return !this.selectValue(this.addInput.value);
        } else if (Keys.isDelete(event)) {
            const value = this.addInput.value as string;

            if (!value || value.length === 0) {
                this.updateItems(this.snapshot.tags.slice(0, this.snapshot.tags.length - 1), false);

                return false;
            }
        } else if (Keys.isEscape(event) && this.itemsModal.isOpen) {
            this.closeModal();
            return false;
        } else if (Keys.isUp(event)) {
            this.selectPrevIndex();
            return false;
        } else if (Keys.isDown(event)) {
            this.selectNextIndex();
            return false;
        } else if (Keys.isEnter(event)) {
            if (this.snapshot.suggestedIndex >= 0) {
                if (this.selectValue(this.snapshot.suggestedItems[this.snapshot.suggestedIndex])) {
                    return false;
                }
            } else if (this.acceptEnter) {
                if (this.selectValue(this.addInput.value)) {
                    return false;
                }
            }
        }

        return true;
    }

    public selectValue(value: TagValue | string, noFocus?: boolean) {
        if (!noFocus) {
            this.inputElement.nativeElement.focus();
        }

        let tagValue: TagValue | null;

        if (Types.isString(value)) {
            tagValue = this.itemConverter.convertInput(value);
        } else {
            tagValue = value;
        }

        if (tagValue) {
            if (this.allowDuplicates || !this.isSelected(tagValue)) {
                this.updateItems([...this.snapshot.tags, tagValue], true);
            }

            this.resetForm();
            this.resetAutocompletion();
            return true;
        }

        return false;
    }

    public toggleValue(isSelected: boolean, tagValue: TagValue) {
        if (isSelected) {
            this.updateItems([...this.snapshot.tags, tagValue], true);
        } else {
            this.updateItems(this.snapshot.tags.filter(x => x.id !== tagValue.id), true);
        }
    }

    public selectPrevIndex() {
        this.selectIndex(this.snapshot.suggestedIndex - 1);
    }

    public selectNextIndex() {
        this.selectIndex(this.snapshot.suggestedIndex + 1);
    }

    public selectIndex(suggestedIndex: number) {
        if (suggestedIndex < 0) {
            suggestedIndex = 0;
        }

        if (suggestedIndex >= this.snapshot.suggestedItems.length) {
            suggestedIndex = this.snapshot.suggestedItems.length - 1;
        }

        this.next({ suggestedIndex });
    }

    public resetFocus(): any {
        this.next({ hasFocus: false });
    }

    private resetAutocompletion() {
        this.next({ suggestedItems: [], suggestedIndex: -1 });
    }

    private resetForm() {
        this.addInput.reset();
    }

    public isSelected(tagValue: TagValue) {
        return this.snapshot.tags.find(x => x.id === tagValue.id);
    }

    public closeModal() {
        if (this.itemsModal.isOpen) {
            this.dropdownClose.emit();

            this.itemsModal.hide();
        }
    }

    public openModal() {
        if (!this.itemsModal.isOpen) {
            this.dropdownOpen.emit();

            this.itemsModal.show();
        }
    }

    public callTouched() {
        this.editorBlur.next(true);

        super.callTouched();
    }

    public focusInput(event: Event) {
        this.inputElement.nativeElement.focus();

        event?.preventDefault();
    }

    public onCut(event: ClipboardEvent) {
        if (!this.hasSelection()) {
            this.onCopy(event);

            this.updateItems([], false);
        }
    }

    public onCopy(event: ClipboardEvent) {
        if (!this.hasSelection()) {
            if (event.clipboardData) {
                event.clipboardData.setData('text/plain', this.snapshot.tags.map(x => x.name).join(','));
            }

            event.preventDefault();
        }
    }

    public onPaste(event: ClipboardEvent) {
        if (event.clipboardData) {
            const value = event.clipboardData.getData('text/plain');

            if (value) {
                this.resetForm();

                const values = [...this.snapshot.tags];

                for (const part of value.split(',')) {
                    const converted = this.itemConverter.convertInput(part);

                    if (converted) {
                        values.push(converted);
                    }
                }

                this.updateItems(values, false);
            }

            event.preventDefault();
        }
    }

    private hasSelection() {
        const s = this.inputElement.nativeElement.selectionStart;
        const e = this.inputElement.nativeElement.selectionEnd;

        return s && e && (e - s) > 0;
    }

    private updateItems(tags: ReadonlyArray<TagValue>, touched: boolean) {
        this.next({ tags });

        if (tags.length === 0 && this.undefinedWhenEmpty) {
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

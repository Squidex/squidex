/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, contentChild, DestroyRef, effect, ElementRef, inject, input, model, numberAttribute, output, signal, TemplateRef, untracked, viewChild } from '@angular/core';
import { FormValueControl } from '@angular/forms/signals';
import { merge, Observable, of, Subject } from 'rxjs';
import { catchError, debounceTime, finalize, map, switchMap } from 'rxjs/operators';
import getCaretCoordinates from 'textarea-caret';
import { FloatingPlacement, Keys, ModalModel, Types } from '@app/framework/internal';
import { AutosizeDirective } from '../../autosize.directive';
import { DropdownMenuComponent } from '../../dropdown-menu.component';
import { LoaderComponent } from '../../loader.component';
import { ModalPlacementDirective } from '../../modals/modal-placement.directive';
import { ModalDirective } from '../../modals/modal.directive';
import { ScrollActiveDirective } from '../../scroll-active.directive';
import { StopClickDirective } from '../../stop-click.directive';
import { TemplateWrapperDirective } from '../../template-wrapper.directive';
import { FocusOnInitDirective } from '../focus-on-init.directive';

export interface AutocompleteSource {
    find(query: string): Observable<ReadonlyArray<any>>;
}

export const NOOP_DATASOURCE = {
    find() {
        return of([]);
    },
};

interface Query {
    // The query text.
    text: string;

    // The range.
    range?: { from: number; to: number };
}

const NO_QUERY: Query = { text: '' };
const RANGE_LIMIT = 60;

function parseQuery(text: unknown, startCharacter: string, cursor: number): Query {
    if (!Types.isString(text)) {
        return NO_QUERY;
    }

    if (!startCharacter) {
        return { text: text.trim() };
    }

    const rangeTo = Math.min(cursor, text.length);
    let rangeFrom = -1;
    for (let j = rangeTo - 1; j >= Math.max(0, rangeTo - RANGE_LIMIT); j--) {
        const char = text[j];
        if (char === startCharacter) {
            rangeFrom = j;
        }

        if (/[\s]/.test(char)) {
            break;
        }
    }

    if (rangeFrom >= 0 && rangeFrom < rangeTo) {
        return { text: text.substring(rangeFrom + 1, rangeTo + 1), range: { from: rangeFrom, to: rangeTo } };
    }

    return { text: '' };
}

@Component({
    selector: 'sqx-autocomplete',
    styleUrls: ['./autocomplete.component.scss'],
    templateUrl: './autocomplete.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AutosizeDirective,
        DropdownMenuComponent,
        FocusOnInitDirective,
        LoaderComponent,
        ModalDirective,
        ModalPlacementDirective,
        ScrollActiveDirective,
        StopClickDirective,
        TemplateWrapperDirective,
    ],
})
export class AutocompleteComponent implements FormValueControl<any> {
    private readonly modalStream = new Subject<Query>();
    private readonly queryStream = new Subject<string>();
    private readonly anchor = viewChild.required<ElementRef<HTMLDivElement>>('anchor');
    private readonly inputControl = viewChild.required<ElementRef<HTMLInputElement>>('input');
    private lastCursor = 0;
    private lastQuery?: Query;
    private lastValue: any = null;
    private timer: any;

    /** The value of the control. */
    public readonly value = model<any>(null);

    /** Disables the control. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** Indicates whether the value is invalid, provided by the form. */
    public readonly invalid = input(false, { transform: booleanAttribute });

    /** Indicates whether the control has been touched. */
    public readonly touched = model(false);

    /** Emits when the input loses the focus. */
    public readonly editorBlur = output();

    /** Emits the keydown events of the input. */
    public readonly editorKeyDown = output<KeyboardEvent>();

    /** Emits the keypress events of the input. */
    public readonly editorKeyPress = output<KeyboardEvent>();

    /** The source to query the suggestions from. */
    public readonly itemsSource = input.required<AutocompleteSource>();

    /** The visual style of the input. */
    public readonly inputStyle = input<'underlined' | 'empty'>();

    /** Shows a button to open the suggestions with an empty query. */
    public readonly allowOpen = input(false, { transform: booleanAttribute });

    /** The ID of the inner input. */
    public readonly formId = input<string>();

    /** The name of the inner input. */
    public readonly formName = input<string>();

    /** Renders a multiline text area instead of an input. */
    public readonly textArea = input(false, { transform: booleanAttribute });

    /** The property of the selected item to display in the input. */
    public readonly displayProperty = input('');

    /** The property of the selected item to use as value. */
    public readonly valueProperty = input('');

    /** The placeholder of the input. */
    public readonly placeholder = input('');

    /** Only queries the text after this character, for example "@" for mentions. */
    public readonly startCharacter = input('');

    /** The icon to show in the input. */
    public readonly icon = input('');

    /** Focuses the input when the component is rendered. */
    public readonly autoFocus = input(false, { transform: booleanAttribute });

    /** Selects the only suggestion on enter, when no suggestion is active. */
    public readonly autoSelectFirst = input(true, { transform: booleanAttribute });

    /** The time in milliseconds to wait after typing before querying the source. */
    public readonly debounceTime = input(300, { transform: numberAttribute });

    /** The position of the suggestions dropdown. */
    public readonly dropdownPosition = input<FloatingPlacement>('bottom-start');

    /** Makes the suggestions dropdown as wide as the input. */
    public readonly dropdownFullWidth = input(true, { transform: booleanAttribute });

    /** Additional styles for the suggestions dropdown. */
    public readonly dropdownStyles = input<any>({});

    protected readonly itemTemplate = contentChild(TemplateRef);

    protected readonly suggestionsModal = new ModalModel();
    protected readonly suggestedItems = signal<ReadonlyArray<any>>([]);
    protected readonly suggestedIndex = signal(-1);
    protected readonly isLoading = signal(false);

    protected readonly queryText = signal('');

    constructor() {
        inject(DestroyRef).onDestroy(() => {
            clearTimeout(this.timer);
        });

        effect(() => {
            const value = this.value();

            untracked(() => {
                // Values that have been set by this component must not override the text in progress.
                if (value === this.lastValue) {
                    return;
                }

                this.lastValue = value;
                this.queryText.set(this.getDisplayText(value));
                this.resetState();
            });
        });

        effect(() => {
            if (this.disabled()) {
                untracked(() => this.resetState());
            }
        });

        effect(onCleanup => {
            const startCharacter = this.startCharacter();

            const queryStream =
                this.queryStream.pipe(
                    map(text => parseQuery(text, startCharacter, this.lastCursor)),
                    debounceTime(this.debounceTime()));

            const subscription =
                merge(queryStream, this.modalStream).pipe(
                    switchMap(query => this.find(query)))
                .subscribe(items => {
                    this.updateAnchor();
                    this.suggestedIndex.set(-1);
                    this.setSuggestedItems(items || []);
                });

            onCleanup(() => subscription.unsubscribe());
        });
    }

    private find(query: Query) {
        this.lastQuery = query;

        if (query.text === '') {
            return of([]);
        }

        this.setLoading(true);

        return this.itemsSource().find(query.text).pipe(
            finalize(() => {
                this.setLoading(false);
            }),
            catchError(() => of([])),
        );
    }

    protected onInput(text: string) {
        this.queryText.set(text);
        this.lastQuery = undefined;
        this.setValue(text);
        this.queryStream.next(text);
    }

    protected onKeyDown(event: KeyboardEvent) {
        this.lastCursor = this.inputControl().nativeElement.selectionEnd || 0;
        this.editorKeyDown.emit(event);

        if (Keys.isEscape(event)) {
            this.resetForm();
            this.reset();
        }

        if (this.suggestedItems().length === 0) {
            return true;
        }

        if (Keys.isUp(event)) {
            this.selectIndex(this.suggestedIndex() - 1);
            return false;
        } else if (Keys.isDown(event)) {
            this.selectIndex(this.suggestedIndex() + 1);
            return false;
        } else if (Keys.isEnter(event)) {
            return !this.selectItem();
        }

        return true;
    }

    protected openModal() {
        this.modalStream.next({ text: '' });
    }

    public reset() {
        this.resetState();
        this.resetForm();
    }

    public focus() {
        this.resetState();
        this.inputControl().nativeElement.focus();
    }

    protected blur() {
        this.resetState();
        this.touched.set(true);
        this.editorBlur.emit();
    }

    protected selectItem(selection: any | null = null): boolean {
        const suggestedItems = this.suggestedItems();

        if (!selection) {
            selection = suggestedItems[this.suggestedIndex()];
        }

        if (!selection && suggestedItems.length === 1 && this.autoSelectFirst()) {
            selection = suggestedItems[0];
        }

        if (!selection) {
            return false;
        }

        try {
            const displayProperty = this.displayProperty();

            let displayString: string;
            if (displayProperty && displayProperty.length > 0) {
                displayString = selection[displayProperty];
            } else {
                displayString = selection.toString();
            }

            const query = this.lastQuery;
            if (query?.range) {
                const input = this.queryText();
                const textBefore = input.substring(0, query.range.from);
                const textAfter = input.substring(query.range.to + 1);

                displayString = `${textBefore}${this.startCharacter()}${displayString}${textAfter}`;
            }

            this.queryText.set(displayString);

            let value = selection;
            if (this.valueProperty()) {
                value = selection[this.valueProperty()];
            }

            this.setValue(value);
            this.touched.set(true);
        } finally {
            this.resetState();
        }

        return true;
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

    private setValue(value: any) {
        value ??= null;

        this.lastValue = value;
        this.value.set(value);
    }

    private getDisplayText(value: any): string {
        if (!value) {
            return '';
        }

        const displayProperty = this.displayProperty();
        if (displayProperty && displayProperty.length > 0) {
            return value[displayProperty] ?? '';
        }

        return value.toString();
    }

    private setLoading(value: boolean) {
        clearTimeout(this.timer);

        if (value) {
            this.isLoading.set(true);
        } else {
            this.timer = setTimeout(() => {
                this.isLoading.set(false);
            }, 250);
        }
    }

    private setSuggestedItems(suggestedItems: ReadonlyArray<any>) {
        this.suggestedItems.set(suggestedItems);

        if (suggestedItems.length > 0) {
            this.suggestionsModal.show();
        } else {
            this.suggestionsModal.hide();
        }
    }

    private resetState() {
        this.lastQuery = undefined;
        this.suggestedIndex.set(-1);
        this.setSuggestedItems([]);
        this.isLoading.set(false);
    }

    private resetForm() {
        this.queryText.set('');
    }

    private updateAnchor() {
        if (!this.startCharacter()) {
            return;
        }

        const query = this.lastQuery;
        if (!query?.range) {
            return;
        }

        const input = this.inputControl().nativeElement;
        const inputWidth = input.getBoundingClientRect().width;
        const coordsStart = getCaretCoordinates(input, query.range.from);
        const coordsEnd = getCaretCoordinates(input, query.range.to + 1);

        let w = coordsEnd.left - coordsStart.left;
        let x = coordsStart.left;
        if (coordsEnd.left > inputWidth) {
            x = inputWidth - w;
        }

        x = Math.max(0, x);
        w = Math.min(w, inputWidth);

        const anchor = this.anchor().nativeElement;
        anchor.style.top = `${coordsStart.top - 2}px`;
        anchor.style.left = `${x}px`;
        anchor.style.width = `${w}px`;
    }
}

/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, contentChild, DestroyRef, effect, ElementRef, forwardRef, inject, input, numberAttribute, output, signal, TemplateRef, viewChild } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { merge, Observable, of, Subject } from 'rxjs';
import { catchError, debounceTime, finalize, map, switchMap, tap } from 'rxjs/operators';
import getCaretCoordinates from 'textarea-caret';
import { FloatingPlacement, Keys, ModalModel, SignalControlComponent, Types } from '@app/framework/internal';
import { AutosizeDirective } from '../../autosize.directive';
import { DropdownMenuComponent } from '../../dropdown-menu.component';
import { LoaderComponent } from '../../loader.component';
import { ModalPlacementDirective } from '../../modals/modal-placement.directive';
import { ModalDirective } from '../../modals/modal.directive';
import { ScrollActiveDirective } from '../../scroll-active.directive';
import { StopClickDirective } from '../../stop-click.directive';
import { TemplateWrapperDirective } from '../../template-wrapper.directive';
import { FocusOnInitDirective } from '../focus-on-init.directive';
import { ForwardFormClassesDirective } from '../forward-form-classes.directive';

export interface AutocompleteSource {
    find(query: string): Observable<ReadonlyArray<any>>;
}

export const NOOP_DATASOURCE = {
    find() {
        return of([]);
    },
};

export const SQX_AUTOCOMPLETE_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => AutocompleteComponent), multi: true,
};

interface Query {
    // The query text.
    text: string;

    // The range.
    range?: { from: number; to: number };
}

const NO_EMIT = { emitEvent: false };
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
    providers: [
        SQX_AUTOCOMPLETE_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AutosizeDirective,
        DropdownMenuComponent,
        FocusOnInitDirective,
        FormsModule,
        ForwardFormClassesDirective,
        LoaderComponent,
        ModalDirective,
        ModalPlacementDirective,
        ReactiveFormsModule,
        ScrollActiveDirective,
        StopClickDirective,
        TemplateWrapperDirective,
    ],
})
export class AutocompleteComponent extends SignalControlComponent<ReadonlyArray<any>> {
    private readonly modalStream = new Subject<Query>();
    private readonly anchor = viewChild.required<ElementRef<HTMLDivElement>>('anchor');
    private readonly inputControl = viewChild.required<ElementRef<HTMLInputElement>>('input');
    private lastCursor = 0;
    private lastQuery?: Query;
    private timer: any;

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

    public readonly itemTemplate = contentChild(TemplateRef);

    public readonly suggestionsModal = new ModalModel();
    public readonly suggestedItems = signal<ReadonlyArray<any>>([]);
    public readonly suggestedIndex = signal(-1);
    public readonly isLoading = signal(false);

    public readonly queryInput = new UntypedFormControl();

    constructor() {
        super();

        inject(DestroyRef).onDestroy(() => {
            clearTimeout(this.timer);
        });

        effect(() => {
            if (this.isDisabled()) {
                this.resetState();

                this.queryInput.disable(NO_EMIT);
            } else {
                this.queryInput.enable(NO_EMIT);
            }
        });

        effect(onCleanup => {
            const queryStream =
                this.queryInput.valueChanges.pipe(
                    tap(query => {
                        this.callChange(query);
                        this.lastQuery = undefined;
                    }),
                    map(text => parseQuery(text, this.startCharacter(), this.lastCursor)),
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

    public onKeyDown(event: KeyboardEvent) {
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

    public writeValue(obj: any) {
        const displayProperty = this.displayProperty();

        if (!obj) {
            this.resetForm();
        } else if (displayProperty && displayProperty.length > 0) {
            this.queryInput.setValue(obj[displayProperty], NO_EMIT);
        } else {
            this.queryInput.setValue(obj.toString(), NO_EMIT);
        }

        this.resetState();
    }

    public openModal() {
        this.modalStream.next({ text: '' });
    }

    public reset() {
        this.resetState();
        this.queryInput.setValue('', NO_EMIT);
    }

    public focus() {
        this.resetState();
        this.inputControl().nativeElement.focus();
    }

    public blur() {
        this.resetState();
        this.callTouched();
        this.editorBlur.emit();
    }

    public selectItem(selection: any | null = null): boolean {
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
                const input = this.queryInput.value;
                const textBefore = input.substring(0, query.range.from);
                const textAfter = input.substring(query.range.to + 1);

                displayString = `${textBefore}${this.startCharacter()}${displayString}${textAfter}`;
            }

            this.queryInput.setValue(displayString, NO_EMIT);

            let value = selection;
            if (this.valueProperty()) {
                value = selection[this.valueProperty()];
            }

            this.callChange(value);
            this.callTouched();
        } finally {
            this.resetState();
        }

        return true;
    }

    public selectIndex(suggestedIndex: number) {
        const suggestedItems = this.suggestedItems();

        if (suggestedIndex < 0) {
            suggestedIndex = 0;
        }

        if (suggestedIndex >= suggestedItems.length) {
            suggestedIndex = suggestedItems.length - 1;
        }

        this.suggestedIndex.set(suggestedIndex);
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
        this.queryInput.setValue('', NO_EMIT);
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

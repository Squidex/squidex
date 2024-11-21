/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, ContentChild, ElementRef, forwardRef, Input, numberAttribute, OnDestroy, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { merge, Observable, of, Subject } from 'rxjs';
import { catchError, debounceTime, finalize, map, switchMap, tap } from 'rxjs/operators';
import getCaretCoordinates from 'textarea-caret';
import { FloatingPlacement, Keys, ModalModel, StatefulControlComponent, Subscriptions, Types } from '@app/framework/internal';
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

export const SQX_AUTOCOMPLETE_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => AutocompleteComponent), multi: true,
};

interface Query {
    // The query text.
    text: string;

    // The range.
    range?: { from: number; to: number };
}

interface State {
    // The suggested items.
    suggestedItems: ReadonlyArray<any>;

    // The selected suggest item index.
    suggestedIndex: number;

    // True, when the searching is in progress.
    isSearching?: boolean;

    // The last query.
    lastQuery?: Query;

    // Indicates whether the loading is in progress.
    isLoading?: boolean;
}

const NO_EMIT = { emitEvent: false };

@Component({
    standalone: true,
    selector: 'sqx-autocomplete',
    styleUrls: ['./autocomplete.component.scss'],
    templateUrl: './autocomplete.component.html',
    providers: [
        SQX_AUTOCOMPLETE_CONTROL_VALUE_ACCESSOR,
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
        StopClickDirective,
        TemplateWrapperDirective,
    ],
})
export class AutocompleteComponent extends StatefulControlComponent<State, ReadonlyArray<any>> implements OnInit, OnDestroy {
    private readonly subscriptions = new Subscriptions();
    private readonly modalStream = new Subject<Query>();
    private lastCursor: number | null = null;
    private lastStart: number | null = null;
    private timer: any;

    @Input({ transform: booleanAttribute })
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    @Input({ required: true })
    public itemsSource!: AutocompleteSource;

    @Input()
    public inputStyle?: 'underlined' | 'empty';

    @Input({ transform: booleanAttribute })
    public allowOpen?: boolean | null = false;

    @Input()
    public formId?: string;

    @Input()
    public formName?: string;

    @Input()
    public textArea?: boolean;

    @Input()
    public displayProperty = '';

    @Input()
    public valueProperty = '';

    @Input()
    public placeholder = '';

    @Input()
    public startCharacter = '';

    @Input()
    public icon = '';

    @Input({ transform: booleanAttribute })
    public autoFocus?: boolean | null;

    @Input({ transform: numberAttribute })
    public debounceTime = 300;

    @Input()
    public dropdownPosition: FloatingPlacement = 'bottom-start';

    @Input({ transform: booleanAttribute })
    public dropdownFullWidth = true;

    @Input()
    public dropdownStyles: any = {};

    @ContentChild(TemplateRef, { static: false })
    public itemTemplate!: TemplateRef<any>;

    @ViewChild('anchor', { static: false })
    public anchor!: ElementRef<HTMLDivElement>;

    @ViewChild('measurer', { static: false })
    public measurer!: ElementRef<HTMLDivElement>;

    @ViewChild('input', { static: false })
    public inputControl!: ElementRef<HTMLInputElement>;

    public suggestionsModal = new ModalModel();

    public queryInput = new UntypedFormControl();

    constructor() {
        super({
            suggestedItems: [],
            suggestedIndex: -1,
        });
    }

    public ngOnDestroy() {
        clearTimeout(this.timer);
    }

    public ngOnInit() {
        this.project(x => x.suggestedItems).subscribe(suggestedItems => {
            if (suggestedItems.length > 0) {
                this.suggestionsModal.show();
            } else {
                this.suggestionsModal.hide();
            }
        });

        const queryStream: Observable<Query> =
            this.queryInput.valueChanges.pipe(
                tap(query => {
                    this.callChange(query);
                    this.next({ lastQuery: undefined });
                }),
                map((text: string) => {
                    if (!Types.isString(text)) {
                        return { text: '' };
                    }

                    if (!this.startCharacter || !Types.isNumber(this.lastStart)) {
                        return { text: text.trim() };
                    }

                    const rangeFrom = this.lastStart;
                    const rangeTo = text.length;
                    if (text[this.lastStart] !== this.startCharacter || rangeTo <= rangeFrom) {
                        return { text: '' };
                    } else {
                        text = text.substring(rangeFrom + 1, rangeTo);
                    }

                    return { text, range: { from: rangeFrom, to: rangeTo } };
                }),
                debounceTime(this.debounceTime));

        this.subscriptions.add(
            merge(queryStream, this.modalStream).pipe(
                switchMap(query => {
                    this.next({ lastQuery: query });

                    if (!this.itemsSource || query.text === '') {
                        return of([]);
                    } else {
                        this.setLoading(true);

                        return this.itemsSource.find(query.text ).pipe(
                            finalize(() => {
                                this.setLoading(false);
                            }),
                            catchError(() => of([])),
                        );
                    }
                }))
            .subscribe(items => {
                this.updateAnchor();
                this.next({
                    suggestedIndex: -1,
                    suggestedItems: items || [],
                    isSearching: false,
                });
            }));
    }

    public onKeyDown(event: KeyboardEvent) {
        this.lastCursor = this.inputControl.nativeElement.selectionStart;

        if (Keys.isEscape(event)) {
            this.resetForm();
            this.reset();
        } else if (Keys.isUp(event)) {
            this.selectPrevIndex();
            return false;
        } else if (Keys.isDown(event)) {
            this.selectNextIndex();
            return false;
        } else if (Keys.isEnter(event)) {
            return !(this.snapshot.suggestedItems.length > 0 && this.selectItem());
        } else if (this.startCharacter) {
            if (event.key === this.startCharacter) {
                this.lastStart = this.lastCursor;
            } else if (event.key === ' ') {
                this.lastStart = null;
            }
        }

        return true;
    }

    public writeValue(obj: any) {
        if (!obj) {
            this.resetForm();
        } else if (this.displayProperty && this.displayProperty.length > 0) {
            this.queryInput.setValue(obj[this.displayProperty], NO_EMIT);
        } else {
            this.queryInput.setValue(obj.toString(), NO_EMIT);
        }

        this.resetState();
    }

    public onDisabled(isDisabled: boolean) {
        if (isDisabled) {
            this.resetState();

            this.queryInput.disable(NO_EMIT);
        } else {
            this.queryInput.enable(NO_EMIT);
        }
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
        this.inputControl.nativeElement.focus();
    }

    public blur() {
        this.resetState();
        this.callTouched();
    }

    public selectItem(selection: any | null = null): boolean {
        if (!selection) {
            selection = this.snapshot.suggestedItems[this.snapshot.suggestedIndex];
        }

        if (!selection && this.snapshot.suggestedItems.length === 1) {
            selection = this.snapshot.suggestedItems[0];
        }

        if (!selection) {
            return false;
        }

        try {
            let displayString: string;
            if (this.displayProperty && this.displayProperty.length > 0) {
                displayString = selection[this.displayProperty];
            } else {
                displayString = selection.toString();
            }

            const query = this.snapshot.lastQuery;
            if (query?.range) {
                const input = this.queryInput.value;
                const textBefore = input.substring(0, query.range.from);
                const textAfter = input.substring(query.range.to);

                displayString = `${textBefore}${this.startCharacter}${displayString}${textAfter}`;
            }

            this.queryInput.setValue(displayString, NO_EMIT);

            let value = selection;
            if (this.valueProperty) {
                value = selection[this.valueProperty];
            }

            this.callChange(value);
            this.callTouched();
        } finally {
            this.resetState();
        }

        return true;
    }

    private setLoading(value: boolean) {
        clearTimeout(this.timer);

        if (value) {
            this.next({ isLoading: true });
        } else {
            this.timer = setTimeout(() => {
                this.next({ isLoading: false });
            }, 250);
        }
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

    public getRows() {
        return Math.max(2, this.queryInput.value?.split('\n')?.length);
    }

    private selectPrevIndex() {
        this.selectIndex(this.snapshot.suggestedIndex - 1);
    }

    private selectNextIndex() {
        this.selectIndex(this.snapshot.suggestedIndex + 1);
    }

    private resetForm() {
        this.queryInput.setValue('', NO_EMIT);
    }

    private updateAnchor() {
        if (!this.startCharacter) {
            return;
        }

        const query = this.snapshot.lastQuery;
        if (!query?.range) {
            return;
        }

        const coords = getCaretCoordinates(this.inputControl.nativeElement, query.range.from);
        this.anchor.nativeElement.style.top = `${coords.top}px`;
        this.anchor.nativeElement.style.left = `${coords.left}px`;
    }
}

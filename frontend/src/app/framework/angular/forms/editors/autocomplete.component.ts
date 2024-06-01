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

interface State {
    // The suggested items.
    suggestedItems: ReadonlyArray<any>;

    // The selected suggest item index.
    suggestedIndex: number;

    // True, when the searching is in progress.
    isSearching?: boolean;

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
    private readonly modalStream = new Subject<string>();
    private timer: any;

    @Input({ required: true })
    public itemsSource!: AutocompleteSource;

    @Input()
    public inputStyle?: 'underlined' | 'empty';

    @Input({ transform: booleanAttribute })
    public allowOpen?: boolean | null = false;

    @Input()
    public displayProperty = '';

    @Input()
    public valueProperty = '';

    @Input()
    public placeholder = '';

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

    @Input({ transform: booleanAttribute })
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    @ContentChild(TemplateRef, { static: false })
    public itemTemplate!: TemplateRef<any>;

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

        const inputStream =
            this.queryInput.valueChanges.pipe(
                tap(query => {
                    this.callChange(query);
                }),
                map(query => {
                    if (Types.isString(query)) {
                        return query.trim();
                    } else {
                        return '';
                    }
                }),
                debounceTime(this.debounceTime));

        this.subscriptions.add(
            merge(inputStream, this.modalStream).pipe(
                switchMap(query => {
                    if (!this.itemsSource) {
                        return of([]);
                    } else {
                        this.setLoading(true);

                        return this.itemsSource.find(query).pipe(
                            finalize(() => {
                                this.setLoading(false);
                            }),
                            catchError(() => of([])),
                        );
                    }
                }))
            .subscribe(items => {
                this.next({
                    suggestedIndex: -1,
                    suggestedItems: items || [],
                    isSearching: false,
                });
            }));
    }

    public onKeyDown(event: KeyboardEvent) {
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
        this.modalStream.next('');
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
            if (this.displayProperty && this.displayProperty.length > 0) {
                this.queryInput.setValue(selection[this.displayProperty], NO_EMIT);
            } else {
                this.queryInput.setValue(selection.toString(), NO_EMIT);
            }

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

    private selectPrevIndex() {
        this.selectIndex(this.snapshot.suggestedIndex - 1);
    }

    private selectNextIndex() {
        this.selectIndex(this.snapshot.suggestedIndex + 1);
    }

    private resetForm() {
        this.queryInput.setValue('', NO_EMIT);
    }
}

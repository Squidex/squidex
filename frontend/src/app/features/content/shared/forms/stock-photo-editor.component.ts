/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, effect, forwardRef, signal, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, NG_VALUE_ACCESSOR, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { BehaviorSubject, of } from 'rxjs';
import { debounceTime, map, switchMap, tap } from 'rxjs/operators';
import { DialogModel, ExternalLinkDirective, FocusOnInitDirective, LoaderComponent, ModalDialogComponent, ModalDirective, SignalControlComponent, StockPhotoDto, StockPhotoService, StopClickDirective, thumbnail, TooltipDirective, TranslatePipe, Types, value$, valueProjection$ } from '@app/shared';

export const SQX_STOCK_PHOTO_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => StockPhotoEditorComponent), multi: true,
};

type Request = { search?: string; page: number };

@Component({
    selector: 'sqx-stock-photo-editor',
    styleUrls: ['./stock-photo-editor.component.scss'],
    templateUrl: './stock-photo-editor.component.html',
    providers: [
        SQX_STOCK_PHOTO_EDITOR_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ExternalLinkDirective,
        FocusOnInitDirective,
        FormsModule,
        LoaderComponent,
        ModalDialogComponent,
        ModalDirective,
        ReactiveFormsModule,
        StopClickDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class StockPhotoEditorComponent extends SignalControlComponent<string> {
    private readonly stockPhotoService = inject(StockPhotoService);
    private readonly stockPhotoRequests = new BehaviorSubject<Request>({ page: 1 });

    protected readonly valueControl = new UntypedFormControl('');

    protected readonly value = toSignal(value$<string>(this.valueControl), { initialValue: '' });

    protected readonly stockPhotoThumbnail = toSignal(valueProjection$(this.valueControl, x => thumbnail(x, undefined, 300) || x));

    protected readonly stockPhotoSearch = new UntypedFormControl('');

    protected readonly stockPhotos = signal<ReadonlyArray<StockPhotoDto>>([]);

    protected readonly stockPhotosHasMore = signal(false);

    protected readonly stockPhotosLoading = signal(false);

    protected readonly thumbnailStatus = signal<'Loaded' | 'Failed' | undefined>(undefined);

    protected readonly searchDialog = new DialogModel();

    constructor() {
        super();

        effect(() => {
            if (this.isDisabled()) {
                this.stockPhotoSearch.disable({ emitEvent: false });
            } else {
                this.stockPhotoSearch.enable({ emitEvent: false });
            }
        });

        effect(onCleanup => {
            const subscriptions = [
                value$(this.valueControl)
                    .subscribe(() => {
                        this.thumbnailStatus.set(undefined);
                    }),
                value$(this.stockPhotoSearch)
                    .subscribe(search => {
                        this.stockPhotoRequests.next({ search, page: 1 });
                    }),
                this.valueControl.valueChanges
                    .subscribe(value => {
                        this.callChange(value);
                        this.callTouched();
                    }),
                this.stockPhotoRequests.pipe(
                    debounceTime(500),
                    tap(request => {
                        if (request.search && request.search.length > 0) {
                            this.stockPhotosLoading.set(true);
                        }
                    }),
                    switchMap(request => {
                        if (request.search && request.search.length > 0) {
                            return this.stockPhotoService.getImages(request.search, request.page).pipe(map(result => ({ request, result })));
                        } else {
                            return of(({ request, result: [] }));
                        }
                    }))
                .subscribe(({ request, result }) => {
                    this.stockPhotosLoading.set(false);
                    this.stockPhotos.update(x => request.page > 1 ? [...x, ...result] : result);
                    this.stockPhotosHasMore.set(result.length === 20);
                }),
            ];

            onCleanup(() => {
                for (const subscription of subscriptions) {
                    subscription.unsubscribe();
                }
            });
        });
    }

    public writeValue(obj: string) {
        if (Types.isString(obj)) {
            this.valueControl.setValue(obj);
        } else {
            this.valueControl.setValue('');
        }
    }

    protected selectPhoto(photo: StockPhotoDto) {
        if (!this.isDisabled()) {
            this.valueControl.setValue(photo.url);

            this.searchDialog.hide();
        }
    }

    public reset() {
        if (!this.isDisabled()) {
            this.valueControl.setValue('');
        }
    }

    protected loadMore() {
        const request = this.stockPhotoRequests.value;

        this.stockPhotoRequests.next({ search: request.search, page: request.page + 1 });
    }

    protected onThumbnailLoaded() {
        this.thumbnailStatus.set('Loaded');
    }

    protected onThumbnailFailed() {
        this.thumbnailStatus.set('Failed');
    }

    protected isSelected(photo: StockPhotoDto) {
        return photo.url === this.value();
    }
}

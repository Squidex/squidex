/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, effect, inject, input, linkedSignal, model, signal } from '@angular/core';
import { FormValueControl } from '@angular/forms/signals';
import { BehaviorSubject, of } from 'rxjs';
import { debounceTime, map, switchMap, tap } from 'rxjs/operators';
import { DialogModel, ExternalLinkDirective, FocusOnInitDirective, LoaderComponent, ModalDialogComponent, ModalDirective, StockPhotoDto, StockPhotoService, StopClickDirective, thumbnail, TooltipDirective, TranslatePipe } from '@app/shared';

type Request = { search?: string; page: number };

@Component({
    selector: 'sqx-stock-photo-editor',
    styleUrls: ['./stock-photo-editor.component.scss'],
    templateUrl: './stock-photo-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ExternalLinkDirective,
        FocusOnInitDirective,
        LoaderComponent,
        ModalDialogComponent,
        ModalDirective,
        StopClickDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class StockPhotoEditorComponent implements FormValueControl<string | null> {
    private readonly stockPhotoService = inject(StockPhotoService);
    private readonly stockPhotoRequests = new BehaviorSubject<Request>({ page: 1 });

    /** The value of the control. */
    public readonly value = model<string | null>(null);

    /** Disables the control. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** Indicates whether the control has been touched. */
    public readonly touched = model(false);

    protected readonly stockPhotoThumbnail = computed(() => {
        const value = this.value();

        return value ? thumbnail(value, undefined, 300) || value : null;
    });

    protected readonly stockPhotoSearch = signal('');

    protected readonly stockPhotos = signal<ReadonlyArray<StockPhotoDto>>([]);

    protected readonly stockPhotosHasMore = signal(false);

    protected readonly stockPhotosLoading = signal(false);

    protected readonly thumbnailStatus = linkedSignal<string | null, 'Loaded' | 'Failed' | undefined>({
        source: this.value,
        computation: () => undefined,
    });

    protected readonly searchDialog = new DialogModel();

    constructor() {
        effect(() => {
            this.stockPhotoRequests.next({ search: this.stockPhotoSearch(), page: 1 });
        });

        effect(onCleanup => {
            const subscription =
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
                });

            onCleanup(() => subscription.unsubscribe());
        });
    }

    protected selectPhoto(photo: StockPhotoDto) {
        if (!this.disabled()) {
            this.updateValue(photo.url);

            this.searchDialog.hide();
        }
    }

    public reset() {
        if (!this.disabled()) {
            this.updateValue(null);
        }
    }

    protected search(event: Event) {
        this.stockPhotoSearch.set((event.target as HTMLInputElement).value);
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

    private updateValue(value: string | null) {
        this.value.set(value);
        this.touched.set(true);
    }
}
